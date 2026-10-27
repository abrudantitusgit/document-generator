"""Local Romanian ID card reader: OpenCV card detection + PaddleOCR (PP-OCRv6)."""
import logging
import os
import re
import threading
import time
import unicodedata
from dataclasses import dataclass
from datetime import datetime

import cv2
import numpy as np
from flask import Flask, jsonify, request
from flask_cors import CORS

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(name)s: %(message)s")
log = logging.getLogger("id-ocr")

# ID-1 cards measure 85.60 x 53.98 mm; detected cards are warped to this size.
CARD_WIDTH, CARD_HEIGHT = 1600, 1009
MAX_INPUT_SIDE = 2400
LOW_CONFIDENCE = 0.85
# Text tilted more than this (degrees) is straightened and read a second time.
SKEW_TOLERANCE = 3.0
OCR_SETTINGS = {
    "lang": "ro",
    "ocr_version": "PP-OCRv6",
    # Fixes cards photographed sideways or upside down; other angles are handled with OpenCV.
    "use_doc_orientation_classify": True,
    "use_doc_unwarping": False,
    "use_textline_orientation": False,
    # Bounds GPU memory on large photos (the RTX 2050 has 4 GB).
    "text_det_limit_side_len": 1920,
    "text_det_limit_type": "max",
}

app = Flask(__name__)
app.config["MAX_CONTENT_LENGTH"] = 10 * 1024 * 1024
CORS(app, resources={r"/*": {"origins": os.environ.get("OCR_CORS_ORIGINS", "http://localhost:3000,http://127.0.0.1:3000").split(",")}})


# --- OCR engine -------------------------------------------------------------

def select_device():
    requested = os.environ.get("OCR_DEVICE", "auto").strip().lower()
    if requested != "auto":
        return requested
    import paddle

    if paddle.device.is_compiled_with_cuda() and paddle.device.cuda.device_count() > 0:
        return "gpu:0"
    return "cpu"


@dataclass
class Token:
    text: str
    score: float
    left: float
    top: float
    right: float
    bottom: float
    angle: float = 0.0  # baseline slope in degrees, positive when sloping down to the right
    length: float = 0.0


def baseline(poly):
    """Angle and length of a detected text box's long edge."""
    points = np.asarray(poly, dtype=np.float32).reshape(-1, 2)
    edges = [points[1] - points[0], points[2] - points[1]]
    edge = max(edges, key=np.linalg.norm)
    angle = float(np.degrees(np.arctan2(edge[1], edge[0])))
    if angle > 90:
        angle -= 180
    elif angle <= -90:
        angle += 180
    return angle, float(np.linalg.norm(edge))


class OcrEngine:
    def __init__(self):
        from paddleocr import PaddleOCR

        self.device = select_device()
        started = time.perf_counter()
        # Paddle 3.4's oneDNN CPU path fails on PP-OCRv6 (unsupported PIR attribute).
        cpu_options = {"enable_mkldnn": False} if self.device == "cpu" else {}
        self._ocr = PaddleOCR(device=self.device, **OCR_SETTINGS, **cpu_options)
        # Paddle predictors are not thread-safe; requests share one model instance.
        self._lock = threading.Lock()
        self.read(np.full((CARD_HEIGHT, CARD_WIDTH, 3), 255, np.uint8))
        log.info("PaddleOCR %s ready on %s in %.1fs", OCR_SETTINGS["ocr_version"], self.device, time.perf_counter() - started)

    def read(self, image):
        """OCR an image; returns the tokens and the upright image their coordinates refer to."""
        with self._lock:
            result = self._ocr.predict(image)[0]
            if self.device.startswith("gpu"):
                import paddle

                # Give cached memory back so large photos do not starve the 4 GB card.
                paddle.device.cuda.empty_cache()
        tokens = [
            Token(text.translate(ROMANIAN_LOOKALIKES), float(score), *map(float, box), *baseline(poly))
            for text, score, box, poly in zip(result["rec_texts"], result["rec_scores"], result["rec_boxes"], result["rec_polys"])
            if text.strip()
        ]
        upright = (result.get("doc_preprocessor_res") or {}).get("output_img")
        return tokens, image if upright is None else upright


_engine = None
_engine_error = None
_engine_lock = threading.Lock()


def get_engine():
    global _engine, _engine_error
    if _engine is None:
        with _engine_lock:
            if _engine is None:
                try:
                    _engine = OcrEngine()
                    _engine_error = None
                except Exception as error:
                    _engine_error = str(error)
                    log.exception("PaddleOCR failed to start")
                    raise
    return _engine


# Load the model in the background so the first upload does not pay for it.
threading.Thread(target=lambda: get_engine() if os.environ.get("OCR_PRELOAD", "1") == "1" else None, daemon=True).start()


# --- Image preparation (OpenCV) ---------------------------------------------

def decode_image(data):
    """Decode an upload, honouring the EXIF orientation phones write."""
    image = cv2.imdecode(np.frombuffer(data, np.uint8), cv2.IMREAD_COLOR)
    if image is None:
        return None
    scale = MAX_INPUT_SIDE / max(image.shape[:2])
    if scale < 1:
        image = cv2.resize(image, None, fx=scale, fy=scale, interpolation=cv2.INTER_AREA)
    return image


def order_corners(points):
    """Order corners clockwise with a long card edge first.

    Sorting by angle around the centre works for any rotation; which long edge
    ends up on top does not matter, as the orientation model turns the warped
    card upright afterwards.
    """
    points = points.reshape(4, 2).astype(np.float32)
    center = points.mean(axis=0)
    corners = points[np.argsort(np.arctan2(points[:, 1] - center[1], points[:, 0] - center[0]))]
    if np.linalg.norm(corners[1] - corners[0]) < np.linalg.norm(corners[2] - corners[1]):
        corners = np.roll(corners, -1, axis=0)
    return corners


def find_card(image):
    """Return the card's four corners, or None when its edges are not visible."""
    scale = min(1.0, 900 / max(image.shape[:2]))
    small = cv2.resize(image, None, fx=scale, fy=scale, interpolation=cv2.INTER_AREA)
    gray = cv2.GaussianBlur(cv2.cvtColor(small, cv2.COLOR_BGR2GRAY), (5, 5), 0)
    edges = cv2.Canny(gray, 30, 110)
    edges = cv2.morphologyEx(edges, cv2.MORPH_CLOSE, np.ones((7, 7), np.uint8), iterations=2)
    contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    image_area = small.shape[0] * small.shape[1]
    for contour in sorted(contours, key=cv2.contourArea, reverse=True)[:5]:
        area = cv2.contourArea(contour)
        # A card covering the whole frame has no visible edges to detect; below 3%
        # the text is too small to read anyway.
        if area < image_area * 0.03 or area > image_area * 0.97:
            continue
        hull = cv2.convexHull(contour)
        approx = cv2.approxPolyDP(hull, 0.02 * cv2.arcLength(hull, True), True)
        if len(approx) != 4:
            approx = cv2.boxPoints(cv2.minAreaRect(hull))
            if cv2.contourArea(hull) < 0.85 * cv2.contourArea(approx.astype(np.float32)):
                continue
        corners = order_corners(np.asarray(approx)) / scale
        width = (np.linalg.norm(corners[1] - corners[0]) + np.linalg.norm(corners[2] - corners[3])) / 2
        height = (np.linalg.norm(corners[3] - corners[0]) + np.linalg.norm(corners[2] - corners[1])) / 2
        # 1.59 for a card seen straight on (a tilted phone shortens one side); the
        # portrait photo printed on the card is about 1.3.
        if 1.4 <= width / max(height, 1) <= 2.1:
            return corners
    return None


def prepare_card(image, enhance=True):
    """Crop and deskew the card, then even out lighting for OCR.

    Returns the image and how the card was found: "detected", "cropped" (the
    upload already has card proportions) or "not-found".
    """
    corners = find_card(image)
    if corners is not None:
        target = np.float32([[0, 0], [CARD_WIDTH - 1, 0], [CARD_WIDTH - 1, CARD_HEIGHT - 1], [0, CARD_HEIGHT - 1]])
        image = cv2.warpPerspective(image, cv2.getPerspectiveTransform(corners, target), (CARD_WIDTH, CARD_HEIGHT), flags=cv2.INTER_CUBIC)
        status = "detected"
    else:
        height, width = image.shape[:2]
        status = "cropped" if 1.3 <= max(width, height) / min(width, height) <= 2.0 else "not-found"
        if max(width, height) < CARD_WIDTH:
            # Small images (e.g. from the web) have tiny labels; read them at card scale.
            scale = CARD_WIDTH / max(width, height)
            image = cv2.resize(image, None, fx=scale, fy=scale, interpolation=cv2.INTER_CUBIC)
    return (enhance_contrast(image) if enhance else image), status


def enhance_contrast(image):
    """Even out lighting (shadows, glare falloff) on the lightness channel only."""
    lab = cv2.cvtColor(image, cv2.COLOR_BGR2LAB)
    lab[..., 0] = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8)).apply(lab[..., 0])
    return cv2.cvtColor(lab, cv2.COLOR_LAB2BGR)


def text_skew(tokens):
    """Dominant text-line angle in degrees: the length-weighted median of the text boxes."""
    lines = [(token.angle, token.length) for token in tokens if len(token.text) >= 4 and token.length > 0]
    if len(lines) < 3:
        return 0.0
    lines.sort()
    halfway = sum(length for _, length in lines) / 2
    running = 0.0
    for angle, length in lines:
        running += length
        if running >= halfway:
            return angle
    return 0.0


def rotate_bound(image, angle):
    """Rotate counter-clockwise by `angle` degrees, growing the canvas so nothing is cut off."""
    height, width = image.shape[:2]
    matrix = cv2.getRotationMatrix2D((width / 2, height / 2), angle, 1.0)
    cos, sin = abs(matrix[0, 0]), abs(matrix[0, 1])
    new_width, new_height = int(height * sin + width * cos), int(height * cos + width * sin)
    matrix[0, 2] += new_width / 2 - width / 2
    matrix[1, 2] += new_height / 2 - height / 2
    # Replicated borders avoid drawing a fake rectangle that could pass for the card.
    return cv2.warpAffine(image, matrix, (new_width, new_height), flags=cv2.INTER_CUBIC, borderMode=cv2.BORDER_REPLICATE)


# When one of these is missing from a blurry photo, the user is asked to retake it;
# otherwise they are asked to check every field.
KEY_FIELDS = ("nume", "cnp", "localitatea", "strada")
# Laplacian variance of the card at 1000 px wide; sharp phone photos score 65+.
BLUR_THRESHOLD = 45


@dataclass
class Reading:
    status: str
    tokens: list
    upright: np.ndarray
    lines: list
    fields: dict
    warnings: list


def sharpness(image):
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    gray = cv2.resize(gray, (1000, max(1, round(1000 * gray.shape[0] / gray.shape[1]))), interpolation=cv2.INTER_AREA)
    return float(cv2.Laplacian(gray, cv2.CV_64F).var())


def read_card(engine, image):
    """OCR the card, levelling tilted text and reading again when needed.

    The first pass reads the photo as taken (the card straightened by OpenCV when
    its edges are visible). If the text is still tilted, the photo is rotated
    level and read again, keeping the reading with more fields.
    Returns (best reading, number of passes, sharpness of the photo).
    """
    def attempt(card, status):
        tokens, upright = engine.read(card)
        lines = group_lines(tokens)
        return Reading(status, tokens, upright, lines, *extract_fields(lines))

    located, status = prepare_card(image, enhance=False)
    best, passes = attempt(enhance_contrast(located), status), 1
    skew = text_skew(best.tokens)
    if abs(skew) > SKEW_TOLERANCE:
        # Rows are rebuilt from vertical positions, which tilted text scrambles. The
        # levelled photo may also expose the card edges.
        straight = attempt(*prepare_card(rotate_bound(best.upright, skew), enhance=False))
        log.info("text tilted %.1f degrees: %d fields as photographed, %d levelled", skew, len(best.fields), len(straight.fields))
        best, passes = (straight if len(straight.fields) >= len(best.fields) else best), passes + 1
    return best, passes, sharpness(located)


# --- Text helpers -----------------------------------------------------------

def normalize(value):
    """Uppercase without diacritics, keeping one output character per input character."""
    result = []
    for char in value or "":
        base = "".join(c for c in unicodedata.normalize("NFKD", char) if not unicodedata.combining(c)).upper()
        result.append(base if len(base) == 1 else char.upper()[:1] or char)
    return "".join(result)


# Letters the multilingual model sometimes emits instead of Romanian ones.
ROMANIAN_LOOKALIKES = str.maketrans("äÄãÃşŞţŢ", "ăĂăĂșȘțȚ")


def clean(value):
    return re.sub(r"\s+", " ", value or "").strip(" \t:;,.|-")


@dataclass
class Line:
    text: str
    score: float
    # The row without its label boxes, for rows where a value and a label merged.
    values: str = None
    values_score: float = None

    @property
    def norm(self):
        return normalize(self.text)


def is_label_text(text):
    # Card labels are trilingual and joined by slashes ("Nume/Nom/Last name").
    norm = normalize(text)
    return bool(LABEL.search(norm) or re.search(r"[A-Z]\s?/\s?[A-Z]{3}", norm) and " / " not in text)


def group_lines(tokens):
    """Merge OCR boxes into reading-order rows."""
    def overlaps(a, b):
        # Boxes stacked above each other belong to different rows.
        return min(a.right, b.right) - max(a.left, b.left) > 0.3 * min(a.right - a.left, b.right - b.left)

    rows = []
    for token in sorted(tokens, key=lambda item: (item.top + item.bottom) / 2):
        center, height = (token.top + token.bottom) / 2, max(token.bottom - token.top, 1)
        candidates = [
            row for row in rows[-3:]
            if abs(row["center"] - center) <= min(row["height"], height) * 0.5 and not any(overlaps(token, t) for t in row["tokens"])
        ]
        row = min(candidates, key=lambda row: abs(row["center"] - center), default=None)
        if row:
            row["tokens"].append(token)
            row["center"] = sum((t.top + t.bottom) / 2 for t in row["tokens"]) / len(row["tokens"])
            row["height"] = sum(t.bottom - t.top for t in row["tokens"]) / len(row["tokens"])
        else:
            rows.append({"center": center, "height": height, "tokens": [token]})
    lines = []
    for row in rows:
        ordered = sorted(row["tokens"], key=lambda t: t.left)
        # Drop stray glyphs from the coat of arms and hologram (no letters or digits).
        values = [t for t in ordered if not is_label_text(t.text) and re.search(r"[A-Za-z0-9]", t.text)]
        lines.append(Line(
            " ".join(t.text for t in ordered), min(t.score for t in ordered),
            " ".join(t.text for t in values), min((t.score for t in values), default=0.0),
        ))
    return lines


LABEL = re.compile(r"(\bNUME\b|\bNOM\b|LAST\s*NAME|PRENUME|PRENOM|FIRST\s*NAME|GIVEN|CETATENIE|NATIONAL|\bSEXE?\b|LOC\s*NASTERE|LIEU\s*DE|PLACE\s*OF|DOMICILIU|ADRESSE|ADDRESS|EMIS[AE]\s*DE|ELIBERAT|DELIVREE|ISSUED|VALABILITATE|VALIDITE|VALIDITY|\bCNP\b|\bSERIA\b|\bNR\b\s*$|DATA\s*(EMITERII|NASTERII|EXPIRARII)|CARTE\s*DE\s*IDENTITATE|IDENTITY\s*CARD|ROMANIA|ROUMANIE)")
ADDRESS_START = re.compile(r"DOMICILIU|ADRESSE|ADDRESS")
ADDRESS_END = re.compile(r"EMIS[AE]\s*DE|ELIBERAT|DELIVREE|ISSUED|VALABILITATE|VALIDITY")
COUNTY_CODES = {"AB": "Alba", "AR": "Arad", "AG": "Argeș", "BC": "Bacău", "BH": "Bihor", "BN": "Bistrița-Năsăud", "BT": "Botoșani", "BV": "Brașov", "BR": "Brăila", "B": "București", "BZ": "Buzău", "CS": "Caraș-Severin", "CL": "Călărași", "CJ": "Cluj", "CT": "Constanța", "CV": "Covasna", "DB": "Dâmbovița", "DJ": "Dolj", "GL": "Galați", "GR": "Giurgiu", "GJ": "Gorj", "HR": "Harghita", "HD": "Hunedoara", "IL": "Ialomița", "IS": "Iași", "IF": "Ilfov", "MM": "Maramureș", "MH": "Mehedinți", "MS": "Mureș", "NT": "Neamț", "OT": "Olt", "PH": "Prahova", "SM": "Satu Mare", "SJ": "Sălaj", "SB": "Sibiu", "SV": "Suceava", "TR": "Teleorman", "TM": "Timiș", "TL": "Tulcea", "VS": "Vaslui", "VL": "Vâlcea", "VN": "Vrancea"}
COUNTY_NAMES = {normalize(name): name for name in COUNTY_CODES.values()}


def restore_place_names(text):
    """Put back diacritics the OCR dropped on county and capital names ("Brasov" -> "Brașov")."""
    norm = normalize(text)
    for plain, name in COUNTY_NAMES.items():
        for match in re.finditer(rf"(?<![A-Z]){re.escape(plain)}(?![A-Z])", norm):
            text = text[:match.start()] + name + text[match.end():]
    return text


def is_valid_cnp(value):
    if not re.fullmatch(r"\d{13}", value or ""):
        return False
    checksum = sum(int(digit) * int(weight) for digit, weight in zip(value[:12], "279146358279")) % 11
    return (1 if checksum == 10 else checksum) == int(value[12])


def date_to_iso(value):
    value = value.strip().replace("O", "0").replace("I", "1").replace("|", "1").replace(",", ".")
    for date_format in ("%d.%m.%Y", "%d/%m/%Y", "%d-%m-%Y", "%d %m %Y", "%d.%m.%y", "%d/%m/%y", "%d-%m-%y"):
        try:
            return datetime.strptime(value, date_format).date().isoformat()
        except ValueError:
            continue
    return None


def next_value(lines, index):
    """The value printed after a label, ignoring label boxes merged into the same row."""
    for line in lines[index + 1:index + 4]:
        values = line.text if line.values is None else line.values
        if clean(values) and not LABEL.search(normalize(values)):
            return Line(values, line.score if line.values_score is None else line.values_score)
    return None


# --- MRZ --------------------------------------------------------------------

def mrz_check(value):
    total = 0
    for position, char in enumerate(value):
        digit = int(char) if char.isdigit() else 0 if char == "<" else ord(char) - 55
        total += digit * (7, 3, 1)[position % 3]
    return str(total % 10)


def fix_document_number(value):
    """Undo common letter/digit OCR swaps in a series + number field (e.g. XB123456)."""
    letters = str.maketrans("0125867", "OIZSBGT")
    digits = str.maketrans("OQDIZSBG", "00012586")
    return value[:2].translate(letters) + value[2:].translate(digits)


def parse_mrz(lines):
    rows = []
    for line in lines:
        compact = re.sub(r"[^A-Z0-9<]", "", line.norm.replace("«", "<<").replace("‹", "<").replace(" ", ""))
        # The second line of old cards has a single "<" (after the document number).
        if "<" in compact and len(compact) >= 25:
            rows.append(compact)
    found = {}
    for row in rows:
        # Names: after "IDROU" on old cards ("IDROM" before 2009), or a line of their own on electronic cards.
        prefix = re.match(r"I[DO0]R[O0][UM]", row)
        names = re.search(r"^([A-Z]+(?:<[A-Z]+)*)<<([A-Z<]+?)<*$", row[prefix.end():] if prefix else row)
        if names and not re.search(r"\d", row) and (prefix or not re.match(r"I[DO0]R", row)):
            found["surname"] = names.group(1).replace("<", "-").title()
            found["given"] = " ".join(part.title() for part in names.group(2).split("<") if part)
        # Old cards (TD2): document number, birth date and the CNP's first and last digits.
        td2 = re.search(r"([A-Z0-9]{2}[A-Z0-9]{6,7})(<*)(\d)R[O0][UM](\d{6})(\d)([MF<])(\d{6})(\d)(\d{7})(\d)", row)
        if td2:
            document = fix_document_number(td2.group(1))
            if mrz_check((document + "<" * 9)[:9]) == td2.group(3):
                found["seria"], found["nrSeria"] = document[:2], document[2:]
            birth, optional = td2.group(4), td2.group(9)
            cnp = optional[0] + birth + optional[1:]
            if mrz_check(birth) == td2.group(5) and is_valid_cnp(cnp):
                found["cnp"] = cnp
            continue
        # Electronic cards (TD1): document number in the first line.
        td1 = re.search(r"I[DO0]R[O0][UM]([A-Z0-9]{8,9})<*(\d)", row)
        if td1:
            document = fix_document_number(td1.group(1))
            if mrz_check((document + "<" * 9)[:9]) == td1.group(2):
                found["seria"], found["nrSeria"] = document[:2], document[2:]
    return found


# --- Printed fields ---------------------------------------------------------

def parse_cnp(lines):
    candidates = []
    for line in lines:
        if "<" in line.text:
            continue
        # Join neighbouring digit groups ("1991204 055083"), but never letters-and-digit
        # codes such as "L3G5U" printed next to the CNP.
        parts = [part.translate(str.maketrans("OQDIL|", "000111")) if sum(c.isdigit() for c in part) * 2 > len(part) else part
                 for part in re.split(r"\s+", line.norm.replace("CNP", " "))]
        for start in range(len(parts)):
            for end in range(start + 1, min(start + 4, len(parts)) + 1):
                value = "".join(parts[start:end])
                if is_valid_cnp(value):
                    candidates.append(("CNP" in line.norm, value, line.score))
    return max(candidates)[1:] if candidates else None


STREET_LINE = re.compile(r"(?<![A-Z])(?:STR|BD|BUL|CALEA|SOS|ALEEA|INTR|PTA|SPL)\.|(?<![A-Z])(?:NR|BL|SC|ET|AP)\.\s*[A-Z0-9]")
LOCALITY_LINE = re.compile(r"(?<![A-Z])(?:(?:JUD|MUN|ORS|COM|SAT|SEC)\.|(?:JUDETUL|MUNICIPIUL|ORAS(?:UL)?|COMUNA|SAT(?:UL)?|SECTOR(?:UL)?)\b)")


def address_block(lines):
    """Lines between the Domiciliu label and the issuer label."""
    start = next((i for i, line in enumerate(lines) if ADDRESS_START.search(line.norm)), None)
    if start is not None:
        block = []
        # Stop at the first row that is not an address, in case the issuer label is unreadable.
        for line in lines[start + 1:start + 4]:
            # Ignore label boxes (e.g. "Emisă de/Issued by") that ended up in an address row.
            values = Line(line.values, line.values_score) if line.values is not None else line
            if ADDRESS_END.search(values.norm) or not (LOCALITY_LINE.search(values.norm) or STREET_LINE.search(values.norm)):
                break
            block.append(values)
        if block:
            return block
    # Labels are too small to read in low-resolution photos: take the street
    # line and the locality line printed above it (not the place of birth).
    for index, line in enumerate(lines):
        if STREET_LINE.search(line.norm) and "<" not in line.text:
            above = lines[index - 1] if index else None
            return [above, line] if above and LOCALITY_LINE.search(above.norm) and not STREET_LINE.search(above.norm) else [line]
    return []


ADDRESS_LABELS = {
    "judet": r"JUD(?:ETUL|ET)?\.?",
    "sector": r"SECTOR(?:UL)?|SEC\.?|SECT\.?",
    "localitatea": r"MUNICIPIUL|MUN\.?|ORAS(?:UL)?|ORS\.?|SAT(?:UL)?|LOC(?:ALITATEA)?\.?",
    "comuna": r"COMUNA|COM\.?",
    "strada": r"STR(?:ADA)?\.?|B(?:UL)?D\.?|BULEVARDUL|CALEA|CAL\.?|SOS(?:EAUA)?\.?|ALEEA|AL\.|PIATA|PTA\.?|SPLAIUL|SPL\.?|INTRAREA|INTR?\.?",
    "nrStrada": r"NR\.?|NUMAR",
    "bloc": r"BLOC|BL\.?",
    "scara": r"SCARA|SC\.?",
    "etaj": r"ETAJ|ET\.?",
    "apartament": r"APARTAMENT|AP\.?",
    "codPostal": r"COD\s+POSTAL",
}
ANY_ADDRESS_LABEL = "|".join(f"(?:{pattern})" for pattern in ADDRESS_LABELS.values())
STREET_PREFIX = re.compile(r"^(STR(ADA)?\.?)\s*", re.I)


def parse_address(text):
    """Split a Romanian address ("Jud.BH Mun.Oradea Str.X nr.1 bl.A ...") into form fields."""
    norm = normalize(text)
    fields = {}
    for field, pattern in ADDRESS_LABELS.items():
        # Short values may be split by OCR ("bl.X 5"); the spaces are removed below.
        value_pattern = r"([A-Z0-9/-]+(?:\s+[0-9]{1,3})?)" if field in {"nrStrada", "bloc", "scara", "etaj", "apartament", "codPostal", "sector"} else r"([A-Z0-9 .'/-]+?)"
        match = re.search(rf"(?<![A-Z])(?:{pattern})\s*[:\-]?\s*{value_pattern}(?=\s*(?:(?<![A-Z])(?:{ANY_ADDRESS_LABEL})(?![A-Z])|[,;]|$))", norm)
        if not match:
            continue
        # Slice the original text so diacritics survive (normalize keeps positions).
        value = clean(text[match.start(1):match.end(1)])
        if field == "strada" and not STREET_PREFIX.match(clean(text[match.start():match.end(1)])):
            value = clean(text[match.start():match.end(1)])  # keep "Bd."/"Calea" for non-"Str." streets
        if field in {"nrStrada", "bloc", "scara", "etaj", "apartament", "codPostal", "sector"}:
            value = re.sub(r"\s+", "", value).upper()
        if value:
            fields[field] = value
    if "judet" in fields:
        county = normalize(fields["judet"]).replace(".", "").strip()
        fields["judet"] = COUNTY_CODES.get(county) or COUNTY_NAMES.get(county) or fields["judet"].title()
    for field in ("localitatea", "comuna"):
        if field in fields:
            # Place names never contain digits; OCR sometimes reads "Oradea" as "0radea".
            fields[field] = fields[field].replace("0", "O")
    for field in ("localitatea", "comuna", "strada"):
        if field in fields:
            fields[field] = re.sub(r"[^\W\d_]+", lambda word: word.group(0).capitalize(), fields[field])
    for field in ("localitatea", "comuna"):
        if field in fields:
            fields[field] = restore_place_names(fields[field])
    if fields.get("localitatea") == "București":
        fields.setdefault("judet", "București")
    return fields


def parse_printed(lines):
    """Read labelled values; returns (fields, confidence per field)."""
    fields, scores = {}, {}

    def put(field, value, score):
        if value:
            fields[field], scores[field] = value, score

    for index, line in enumerate(lines):
        norm = line.norm
        # Values are printed on the row below their label.
        if re.search(r"(?<![A-Z])N[UV]ME\b|LAST\s*NAME|\bSURNAME\b", norm) and "surname" not in fields:
            value_line = next_value(lines, index)
            if value_line:
                put("surname", clean(value_line.text).title(), value_line.score)
        if re.search(r"PR[EO]N[UV]ME|FIRST\s*NAME|GIVEN\s*NAME", norm) and not re.search(r"PARINT|PARENT", norm) and "given" not in fields:
            value_line = next_value(lines, index)
            if value_line:
                put("given", clean(value_line.text).title(), value_line.score)
        series = re.search(r"SERI[AE]\s*[:\-]?\s*([A-Z]{2})\s*(?:NR\.?|NO\.?|NUMAR)?\s*[:\-]?\s*(\d{6,7})", norm)
        if series:
            put("seria", series.group(1), line.score)
            put("nrSeria", series.group(2), line.score)
        if re.search(r"DATA\s*(EMITERII|ISSUE)|ISSUED\s*ON", norm):
            match = re.search(r"(\d{1,2}[./ -]\d{1,2}[./ -]\d{2,4})", line.text) or re.search(r"(\d{1,2}[./ -]\d{1,2}[./ -]\d{2,4})", (next_value(lines, index) or Line("", 0)).text)
            if match and (parsed := date_to_iso(match.group(1))):
                put("dataBuletin", parsed, line.score)
        if re.search(r"EMIS[AE]\s*DE|ELIBERAT[AE]?\s*DE|ISSUED\s*BY", norm):
            value_line = next_value(lines, index)
            if value_line:
                issuer = re.split(r"\s+\d{1,2}[./,-]\d", clean(value_line.text), maxsplit=1)[0]
                put("eliberat", restore_place_names(clean(issuer)), value_line.score)

    if "eliberat" not in fields:
        # Issuer labels are tiny and often unreadable; the issuer is printed either on
        # its own (SPCLEP ...) or just before the validity period on the same row.
        for line in lines:
            values = line.text if line.values is None else line.values
            before_dates = re.split(r"\s*\d{1,2}[./,]\d{1,2}[./,]\d{2,4}\s*[-–]", values, maxsplit=1)
            if re.match(r"(SPCLEP|SPCEP|SPCJEP|DEPABD|POLITIA|POL\.)", normalize(values)) or (len(before_dates) > 1 and re.search(r"[A-Za-z]{3}", before_dates[0])):
                put("eliberat", restore_place_names(clean(re.split(r"\s+\d{1,2}[./,-]\d", values, maxsplit=1)[0])), line.score if line.values_score is None else line.values_score)
                break

    if "dataBuletin" not in fields:
        # Old cards print only the validity period; it starts on the issue date.
        for line in lines:
            match = re.search(r"(\d{1,2}[./,]\d{1,2}[./,]\d{2,4})\s*[-–]\s*\d{1,2}[./,]\d{1,2}[./,]\d{2,4}", line.text)
            if match and (parsed := date_to_iso(match.group(1))):
                put("dataBuletin", parsed, line.score)
                fields["_dataBuletinInferred"] = True
                break

    block = address_block(lines)
    if block:
        score = min(line.score for line in block)
        # ";" ends a value at the line break, so text printed beside the photo
        # (issuer code, logo) cannot run into the locality.
        for field, value in parse_address("; ".join(line.text for line in block)).items():
            put(field, value, score)
    return fields, scores


FIELD_LABELS = {"nume": "Nume complet", "cnp": "CNP", "seria": "Serie act", "nrSeria": "Număr act", "dataBuletin": "Data emiterii", "eliberat": "Eliberat de", "judet": "Județ", "localitatea": "Localitate", "comuna": "Comună", "strada": "Stradă", "nrStrada": "Număr", "sector": "Sector", "bloc": "Bloc", "scara": "Scară", "etaj": "Etaj", "apartament": "Apartament", "codPostal": "Cod poștal"}


def extract_fields(lines):
    """Combine printed values with the check-digit-verified MRZ; returns (fields, warnings)."""
    fields, scores = parse_printed(lines)
    mrz = parse_mrz(lines)
    warnings = []

    for part in ("surname", "given"):
        if part not in mrz:
            continue
        # The MRZ is read most reliably but has no diacritics or hyphens, so use the
        # printed line that matches it; fall back to the MRZ when the label was missed.
        wanted = re.sub(r"[\s-]+", " ", mrz[part].upper())
        printed = next((line for line in lines if re.sub(r"[\s-]+", " ", normalize(line.values or line.text)).strip() == wanted), None)
        if printed:
            fields[part], scores[part] = clean(printed.values or printed.text).title(), 1.0
        elif part not in fields:
            fields[part], scores[part] = mrz[part], 1.0
    name_parts = [fields.pop(part) for part in ("surname", "given") if fields.get(part)]
    if name_parts:
        fields["nume"] = " ".join(name_parts)
        scores["nume"] = min(scores.pop("surname", 1.0), scores.pop("given", 1.0))

    # MRZ values passed their check digits, so they win over the printed text.
    if "seria" in mrz:
        fields["seria"], fields["nrSeria"] = mrz["seria"], mrz["nrSeria"]
        scores["seria"] = scores["nrSeria"] = 1.0

    printed_cnp = parse_cnp(lines)
    if printed_cnp and "cnp" in mrz and printed_cnp[0] != mrz["cnp"]:
        warnings.append("CNP-ul tipărit diferă de cel din zona MRZ; s-a folosit varianta din MRZ. Verifică-l.")
    if "cnp" in mrz:
        fields["cnp"], scores["cnp"] = mrz["cnp"], 1.0
    elif printed_cnp:
        fields["cnp"], scores["cnp"] = printed_cnp

    if fields.pop("_dataBuletinInferred", False):
        warnings.append("Data emiterii a fost estimată din începutul perioadei de valabilitate; verific-o.")
    unsure = [FIELD_LABELS.get(field, field) for field, score in scores.items() if field in fields and score < LOW_CONFIDENCE]
    if unsure:
        warnings.append(f"Citire nesigură pentru: {', '.join(unsure)}. Verifică manual aceste câmpuri.")
    if "cnp" not in fields:
        warnings.append("CNP-ul nu a fost detectat; completează-l manual.")
    return fields, warnings


# --- HTTP -------------------------------------------------------------------

@app.get("/health")
def health():
    if _engine is None:
        status = {"ok": False, "status": "error" if _engine_error else "loading", "error": _engine_error}
        return jsonify({**status, "service": "romanian-id-ocr"}), 503
    return jsonify({"ok": True, "status": "ready", "service": "romanian-id-ocr", "engine": f"paddleocr-{OCR_SETTINGS['ocr_version']}", "device": _engine.device})


@app.post("/extract")
def extract():
    upload = request.files.get("file")
    if not upload or not upload.filename:
        return jsonify({"message": "Selectează o fotografie a cărții de identitate."}), 400
    image = decode_image(upload.read())
    if image is None:
        return jsonify({"message": "Fișierul nu este o imagine JPEG, PNG sau WebP validă."}), 400
    try:
        engine = get_engine()
    except Exception:
        return jsonify({"message": "Serviciul OCR local nu este configurat corect."}), 503

    started = time.perf_counter()
    reading, passes, sharp = read_card(engine, image)
    lines, fields, warnings, card_status = reading.lines, reading.fields, reading.warnings, reading.status
    elapsed_ms = round((time.perf_counter() - started) * 1000)
    log.info("extract: %d lines, %d fields, card=%s, passes=%d, sharpness=%.0f, %dms on %s", len(lines), len(fields), card_status, passes, sharp, elapsed_ms, engine.device)

    if sharp < BLUR_THRESHOLD and any(field not in fields for field in KEY_FIELDS):
        warnings.append("Fotografia este neclară (mișcată sau nefocalizată); pentru date complete, refă poza ținând telefonul nemișcat.")
    elif sharp < BLUR_THRESHOLD:
        # Blur can turn letters into other letters without lowering OCR confidence.
        warnings.append("Fotografia este neclară; verifică atent toate câmpurile citite sau refă poza.")
    elif card_status == "not-found" and len(fields) < 8:
        warnings.append("Marginile cărții nu au fost detectate; pentru rezultate mai bune, fotografiază cartea întreagă pe un fundal închis.")
    meta = {"engine": f"paddleocr-{OCR_SETTINGS['ocr_version']}", "device": engine.device, "card": card_status, "passes": passes, "sharpness": round(sharp), "elapsedMs": elapsed_ms}
    body = {"fields": fields, "warnings": warnings, "meta": meta}
    if request.args.get("debug") == "1":
        body["lines"] = [{"text": line.text, "score": round(line.score, 3)} for line in lines]
    if not fields:
        body["message"] = "Nu au fost detectate date lizibile."
    return jsonify(body), 200


@app.errorhandler(413)
def too_large(_error):
    return jsonify({"message": "Imaginea depășește limita de 10 MB."}), 413


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)
