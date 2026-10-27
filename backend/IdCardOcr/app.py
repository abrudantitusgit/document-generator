import io
import re
import threading
import unicodedata
from datetime import datetime

import pytesseract
from flask import Flask, jsonify, request
from flask_cors import CORS
from PIL import Image, ImageOps, UnidentifiedImageError

app = Flask(__name__)
app.config["MAX_CONTENT_LENGTH"] = 10 * 1024 * 1024
CORS(app, resources={r"/*": {"origins": ["http://localhost:3000", "http://127.0.0.1:3000"]}})
_easy_reader = None
_easy_reader_lock = threading.Lock()


def easy_reader():
    global _easy_reader
    if _easy_reader is None:
        with _easy_reader_lock:
            if _easy_reader is None:
                import easyocr
                _easy_reader = easyocr.Reader(["ro", "en"], gpu=False, model_storage_directory="/root/.EasyOCR", verbose=False)
    return _easy_reader


def read_easyocr_lines(image):
    import numpy as np

    results = easy_reader().readtext(np.asarray(image.convert("RGB")), detail=1, paragraph=False, min_size=6)
    tokens = []
    for bounds, value, confidence in results:
        if not clean(value):
            continue
        center_y = sum(point[1] for point in bounds) / len(bounds)
        left_x = min(point[0] for point in bounds)
        height = max(point[1] for point in bounds) - min(point[1] for point in bounds)
        tokens.append((center_y, left_x, max(height, 1), clean(value), confidence))
    tokens.sort(key=lambda token: token[0])
    rows = []
    for token in tokens:
        nearest = min(rows, key=lambda row: abs(row[0] - token[0]), default=None)
        if nearest and abs(nearest[0] - token[0]) <= max(12, min(nearest[1], token[2]) * 0.65):
            nearest[2].append(token)
            nearest[0] = sum(item[0] for item in nearest[2]) / len(nearest[2])
            nearest[1] = sum(item[2] for item in nearest[2]) / len(nearest[2])
        else:
            rows.append([token[0], token[2], [token]])
    return "\n".join(" ".join(item[3] for item in sorted(row[2], key=lambda item: item[1])) for row in sorted(rows, key=lambda row: row[0])), results


def read_easyocr_cnp(image):
    import numpy as np

    left, top, right, bottom = card_bounds(image)
    width, height = right - left, bottom - top
    crop = image.crop((left + int(width * 0.27), top + int(height * 0.21), left + int(width * 0.61), top + int(height * 0.30)))
    crop = ImageOps.autocontrast(crop.convert("L")).resize((crop.width * 6, crop.height * 6))
    pieces = easy_reader().readtext(np.asarray(crop), detail=0, paragraph=False, min_size=4, allowlist="0123456789")
    digit_text = re.sub(r"\D", "", "".join(pieces))
    match = re.search(r"\d{13}", digit_text)
    candidate = match.group(0) if match else ""
    return candidate if is_valid_cnp(candidate) else ""


def normalize(value):
    value = unicodedata.normalize("NFKD", value or "")
    return "".join(char for char in value if not unicodedata.combining(char)).upper()


def clean(value):
    return re.sub(r"\s+", " ", value).strip(" \t:;,.|-")


def is_valid_cnp(value):
    if not re.fullmatch(r"\d{13}", value or ""):
        return False
    checksum = sum(int(digit) * int(weight) for digit, weight in zip(value[:12], "279146358279")) % 11
    if checksum == 10:
        checksum = 1
    return checksum == int(value[12])


def date_to_iso(value):
    value = value.strip().replace("O", "0").replace("I", "1").replace("|", "1").replace(",", ".")
    for date_format in ("%d.%m.%Y", "%d/%m/%Y", "%d-%m-%Y", "%d %m %Y", "%d.%m.%y", "%d/%m/%y", "%d-%m-%y"):
        try:
            return datetime.strptime(value, date_format).date().isoformat()
        except ValueError:
            continue
    return None


def next_value(lines, index):
    for line in lines[index + 1:index + 4]:
        candidate = clean(line)
        if candidate and not re.search(r"(CNP|SERIA|NUMAR|NUMBER|DATA|EMITERII|EXPIRARII|ADRESA|DOMICILIU)", normalize(candidate)):
            return candidate
    return ""


def card_bounds(image):
    """Estimate the card bounds from its colored printed area, ignoring the blank photo background."""
    width, height = image.size
    pixels = image.load()
    points = []
    # Sampling keeps this inexpensive for large phone photos.
    for y in range(0, height, 3):
        for x in range(0, width, 3):
            red, green, blue = pixels[x, y]
            if max(red, green, blue) - min(red, green, blue) > 20 and max(red, green, blue) > 100:
                points.append((x, y))
    if len(points) < 1000:
        return (0, 0, width, height)
    left = max(0, min(x for x, _ in points) - 40)
    top = max(0, min(y for _, y in points) - 30)
    right = min(width, max(x for x, _ in points) + 40)
    bottom = min(height, max(y for _, y in points) + 145)
    return left, top, right, bottom


def extract_fields(text, card_image=None, address_text=""):
    fields = {}
    lines = [clean(line) for line in text.splitlines() if clean(line)]
    normalized_lines = [normalize(line) for line in lines]

    # Prefer digits read from the printed CNP region; the MRZ contains similar
    # digit sequences and must not be mistaken for the CNP.
    if card_image is not None:
        card_left, card_top, card_right, card_bottom = card_bounds(card_image)
        card_width, card_height = card_right - card_left, card_bottom - card_top
        cnp_crop = card_image.crop((
            card_left + int(card_width * 0.27), card_top + int(card_height * 0.21),
            card_left + int(card_width * 0.61), card_top + int(card_height * 0.30),
        )).convert("L")
        cnp_crop = ImageOps.autocontrast(cnp_crop).resize((cnp_crop.width * 6, cnp_crop.height * 6))
        cnp_text = pytesseract.image_to_string(cnp_crop, lang="eng", config="--psm 7 -c tessedit_char_whitelist=0123456789")
        cnp_match = re.search(r"\d{13}", re.sub(r"\s+", "", cnp_text))
        if cnp_match and is_valid_cnp(cnp_match.group(0)):
            fields["cnp"] = cnp_match.group(0)

    for index, line in enumerate(normalized_lines):
        original = lines[index]
        match = re.search(r"(?:\bNUME\b(?:\s*/\s*NOM)?(?:\s*/\s*LAST\s*NAME)?|\bSURNAME\b)\s*[:\-]?\s*(.*)", line)
        if match:
            value = clean(match.group(1) or "") or next_value(lines, index)
            value = re.split(r"(?:PRENUME|GIVEN NAME)\b", value, maxsplit=1, flags=re.I)[0]
            if value and not re.search(r"(PRENUME|GIVEN NAME)", normalize(value)):
                fields["nume"] = value.title()
        match = re.search(r"(?:\bPRENUME\b(?:\s*/\s*PRENOM)?(?:\s*/\s*FIRST\s*NAME)?|\bGIVEN\s+NAME\b|\bFIRST\s*NAME\b)\s*[:\-]?\s*(.*)", line)
        if match:
            value = clean(match.group(1) or "") or next_value(lines, index)
            if value and len(value) > 2 and not re.search(r"(CNP|SEX|NATIONALITY|CETATENIE)", normalize(value)):
                first_name = value.title()
                fields["nume"] = f"{fields.get('nume', '')} {first_name}".strip()
        match = re.search(r"(?:SERIA|SERIE|SERIES)\s*[:\-]?\s*([A-Z]{1,3})\s*(?:NR\.?|NO\.?|NUMAR)?\s*[:\-]?\s*(\d{4,8})", line)
        if match:
            fields["seria"] = match.group(1)
            fields["nrSeria"] = match.group(2)
        elif not fields.get("seria"):
            match = re.search(r"\b([A-Z]{2})\s+(\d{6})\b", line)
            if match:
                fields["seria"], fields["nrSeria"] = match.group(1), match.group(2)
        match = re.search(r"(?:DATA\s*(?:EMITERII|ISSUE)|ISSUED\s*ON)\s*[:\-]?\s*(\d{1,2}[./ -]\d{1,2}[./ -]\d{4})", line)
        if match:
            parsed_date = date_to_iso(match.group(1))
            if parsed_date:
                fields["dataBuletin"] = parsed_date
        match = re.search(r"(?:ELIBERAT(?:A)?\s*DE|EMISA\s*DE|ISSUED\s*BY)\s*[:\-]?\s*(.+)", line)
        if match:
            issuer = clean(match.group(1))
            issuer = re.split(r"/|VALABILITATE|VALIDITY", issuer, maxsplit=1, flags=re.I)[0].strip()
            if not issuer or any(label in normalize(match.group(1)) for label in ("DELIVREE", "ISSUED BY", "VALABILITATE")):
                issuer = next_value(lines, index)
            issuer = re.split(r"\s+\d{1,2}[./-]\d", issuer, maxsplit=1)[0]
            if issuer:
                fields["eliberat"] = issuer

    # Trust the MRZ for stable name and card-number values when the printed
    # label region is noisy or the text is bilingual.
    mrz = [normalize(line).replace(" ", "") for line in lines if "<" in line and len(line) >= 25]
    for line in mrz:
        name_match = re.search(r"IDROU([A-Z]+)<<([A-Z<]+)", line)
        if name_match:
            surname = name_match.group(1).title()
            given_names = " ".join(part.title() for part in name_match.group(2).split("<") if part)
            if given_names and not fields.get("nume"):
                fields["nume"] = f"{surname} {given_names}"
        document_match = re.search(r"([Z27][A-Z0-9]\d{6})<", line)
        if document_match:
            document_number = document_match.group(1).replace("2H", "ZH", 1).replace("7H", "ZH", 1)
            fields["seria"], fields["nrSeria"] = document_number[:2], document_number[2:]

    # Romanian ID OCR frequently prints the issue date and issuer on adjacent lines.
    for index, line in enumerate(normalized_lines):
        if "DATA EMITERII" in line and not fields.get("dataBuletin"):
            match = re.search(r"(\d{1,2}[./ -]\d{1,2}[./ -]\d{4})", lines[index])
            if match and (parsed_date := date_to_iso(match.group(1))):
                fields["dataBuletin"] = parsed_date
        if ("ELIBERAT DE" in line or "EMISA DE" in line or "ISSUED BY" in line) and not fields.get("eliberat"):
            issuer_line = next_value(lines, index)
            issuer_line = re.split(r"\s+\d{1,2}[./-]\d", issuer_line, maxsplit=1)[0]
            if issuer_line:
                fields["eliberat"] = issuer_line

    # Address lines on Romanian ID cards are often split across several OCR lines.
    # Parse the normalized full text and stop each value at the next known label.
    normalized_text = " ".join(normalized_lines)
    next_address_label = r"(?=\s*(?:JUD(?:ET(?:UL)?)?|MUN(?:ICIPIUL)?\.?|ORAS(?:UL)?|COMUNA|SAT(?:UL)?|LOCALITATEA|DOMICILIU|ADRESSE|ADDRESS|STR(?:ADA)?\.?|NR\.?|NUMAR|BLOC|BL\.?|SCARA|SC\.?|ETAJ|ET\.?|APARTAMENT|AP\.?|COD\s+POSTAL)\b|[,;]|$)"

    def address_value(label_pattern, stop_pattern=next_address_label):
        match = re.search(rf"(?<![A-Z])(?:{label_pattern})\s*[:\-]?\s*([A-Z0-9 /.'-]+?){stop_pattern}", normalized_text)
        return clean(match.group(1)) if match else ""

    address_fields = {
        "judet": address_value(r"JUD(?:ET(?:UL)?)?\.?|COUNTY"),
        "localitatea": address_value(r"MUNICIPIUL|MUN\.?|ORAS(?:UL)?|LOCALITATEA"),
        "comuna": address_value(r"COMUNA"),
        "strada": address_value(r"STR(?:ADA)?\.?|STREET"),
        "nrStrada": address_value(r"NR\.?|NUMAR"),
        "bloc": address_value(r"BLOC|BL\."),
        "scara": address_value(r"SCARA|SC\."),
        "etaj": address_value(r"ETAJ|ET\."),
        "apartament": address_value(r"APARTAMENT|AP\."),
        "codPostal": address_value(r"COD\s+POSTAL"),
    }
    fields.update({key: value.title() if key in {"judet", "localitatea", "comuna", "strada"} else value.upper() for key, value in address_fields.items() if value})
    if address_text:
        address_only = normalize(address_text)
        city_match = re.search(r"(?<![A-Z])(?:MUNICIPIUL|MUN\.?|ORAS(?:UL)?|LOCALITATEA)\s*[:\-]?\s*([A-Z0-9 /.'-]+?)(?=\s*(?:STR(?:ADA)?\.?|NR\.?|BLOC|BL\.?|SCARA|SC\.?|ETAJ|ET\.?|APARTAMENT|AP\.?|COD\s+POSTAL)|[,;\n]|$)", address_only)
        if city_match:
            city = clean(city_match.group(1))
            city = re.sub(r"\s+[A-Z]{1,2}$", "", city)
            if city:
                fields["localitatea"] = city.title()
    for line in normalized_lines:
        if re.search(r"\bSTR(?:ADA)?\.?\s*", line):
            for field, label in (("nrStrada", r"NR\.?"), ("bloc", r"BL(?:OC)?\.?"), ("scara", r"SC(?:ARA)?\.?"), ("etaj", r"ET(?:AJ)?\.?"), ("apartament", r"AP(?:ARTAMENT)?\.?")):
                value_match = re.search(rf"(?<![A-Z])(?:{label})\s*([A-Z0-9/-]+)\b", line)
                if value_match:
                    fields[field] = value_match.group(1)
    county_codes = {"AB": "Alba", "AR": "Arad", "AG": "Argeș", "BC": "Bacău", "BH": "Bihor", "BN": "Bistrița-Năsăud", "BT": "Botoșani", "BV": "Brașov", "BR": "Brăila", "B": "București", "BZ": "Buzău", "CS": "Caraș-Severin", "CL": "Călărași", "CJ": "Cluj", "CT": "Constanța", "CV": "Covasna", "DB": "Dâmbovița", "DJ": "Dolj", "GL": "Galați", "GR": "Giurgiu", "GJ": "Gorj", "HR": "Harghita", "HD": "Hunedoara", "IL": "Ialomița", "IS": "Iași", "IF": "Ilfov", "MM": "Maramureș", "MH": "Mehedinți", "MS": "Mureș", "NT": "Neamț", "OT": "Olt", "PH": "Prahova", "SM": "Satu Mare", "SJ": "Sălaj", "SB": "Sibiu", "SV": "Suceava", "TR": "Teleorman", "TM": "Timiș", "TL": "Tulcea", "VS": "Vaslui", "VL": "Vâlcea", "VN": "Vrancea"}
    county_value = fields.get("judet", "").upper().replace("JUDET", "").strip(" .")
    if county_value in county_codes:
        fields["judet"] = county_codes[county_value]

    if not fields.get("dataBuletin"):
        for line in normalized_lines:
            if "VALABILITATE" in line or "VALIDITY" in line:
                validity_start = re.search(r"(\d{1,2}[./, -]\d{1,2}[./, -]\d{2,4})", line)
                if validity_start and (parsed_date := date_to_iso(validity_start.group(1))):
                    fields["dataBuletin"] = parsed_date
                    fields["_dataBuletinInferred"] = True
                    break
    if not fields.get("dataBuletin"):
        date_range = re.search(r"(\d{1,2}[./,]\d{1,2}[./,]\d{2,4})\s*[-–]\s*\d{1,2}[./,]\d{1,2}[./,]\d{4}", normalized_text)
        if date_range and (parsed_date := date_to_iso(date_range.group(1))):
            fields["dataBuletin"] = parsed_date
            fields["_dataBuletinInferred"] = True

    return fields


@app.get("/health")
def health():
    return jsonify({"ok": True, "service": "romanian-id-ocr", "secondaryEngine": "easyocr-ro-en"})


@app.post("/extract")
def extract():
    upload = request.files.get("file")
    if not upload or not upload.filename:
        return jsonify({"message": "Selectează o fotografie a cărții de identitate."}), 400
    try:
        image = Image.open(io.BytesIO(upload.read()))
        image = ImageOps.exif_transpose(image).convert("RGB")
    except (UnidentifiedImageError, OSError):
        return jsonify({"message": "Fișierul nu este o imagine JPEG, PNG sau WebP validă."}), 400
    try:
        left, top, right, bottom = card_bounds(image)
        card_crop = image.crop((left, top, right, bottom))
        card_crop = card_crop.resize((card_crop.width * 3, card_crop.height * 3))
        text = pytesseract.image_to_string(card_crop, lang="ron+eng", config="--psm 6")
        width, height = right - left, bottom - top
        name_crop = image.crop((left + int(width * 0.27), top + int(height * 0.17), left + int(width * 0.69), top + int(height * 0.50)))
        name_crop = ImageOps.autocontrast(name_crop.convert("L")).resize((name_crop.width * 3, name_crop.height * 3))
        name_text = pytesseract.image_to_string(name_crop, lang="ron+eng", config="--psm 6")
        address_crop = image.crop((left + int(width * 0.27), top + int(height * 0.53), right - int(width * 0.02), top + int(height * 0.74)))
        address_crop = ImageOps.autocontrast(address_crop.convert("L")).resize((address_crop.width * 4, address_crop.height * 4))
        address_text = pytesseract.image_to_string(address_crop, lang="ron+eng", config="--psm 6")
        mrz_crop = image.crop((left + int(width * 0.01), top + int(height * 0.73), right - int(width * 0.04), top + int(height * 0.89)))
        mrz_crop = ImageOps.autocontrast(mrz_crop.convert("L")).resize((mrz_crop.width * 4, mrz_crop.height * 4))
        mrz_text = pytesseract.image_to_string(mrz_crop, lang="eng", config="--psm 6")
        text = "\n".join((text, name_text, address_text, mrz_text))
    except pytesseract.TesseractNotFoundError:
        return jsonify({"message": "Serviciul OCR local nu este configurat corect."}), 503
    warnings = []

    try:
        easy_text, _easy_results = read_easyocr_lines(card_crop)
        easy_cnp = read_easyocr_cnp(image)
        easy_fields = extract_fields(easy_text, address_text=easy_text)
    except Exception:
        # A secondary-engine failure should not hide valid Tesseract output.
        easy_text, easy_cnp, easy_fields = "", "", {}
        warnings.append("Motorul OCR secundar nu a răspuns; rezultatele provin doar din primul motor.")

    fields = extract_fields(text, image, address_text)
    disagreements = []
    for field, value in easy_fields.items():
        if field.startswith("_") or field == "cnp" or not value:
            continue
        if not fields.get(field):
            fields[field] = value
        elif normalize(str(fields[field])) != normalize(str(value)):
            disagreements.append(field)

    tesseract_cnp = fields.get("cnp", "")
    if tesseract_cnp and easy_cnp and tesseract_cnp != easy_cnp:
        fields.pop("cnp", None)
        disagreements.append("cnp")
    elif not tesseract_cnp and easy_cnp:
        fields["cnp"] = easy_cnp

    if disagreements:
        field_labels = {"nume": "Nume complet", "cnp": "CNP", "seria": "Serie act", "nrSeria": "Număr act", "dataBuletin": "Data emiterii", "eliberat": "Eliberat de", "judet": "Județ", "localitatea": "Localitate", "comuna": "Comună", "strada": "Stradă", "nrStrada": "Număr", "sector": "Sector", "bloc": "Bloc", "scara": "Scară", "etaj": "Etaj", "apartament": "Apartament", "codPostal": "Cod poștal"}
        fields_to_review = ", ".join(dict.fromkeys(field_labels.get(field, field) for field in disagreements))
        warnings.append(f"Motoarele OCR au rezultate diferite pentru: {fields_to_review}. Verifică manual aceste câmpuri.")
    if fields.pop("_dataBuletinInferred", False):
        warnings.append("Data emiterii a fost estimată din începutul perioadei de valabilitate; verific-o.")
    if "cnp" not in fields:
        warnings.append("CNP-ul nu a fost detectat; completează-l manual.")
    if not fields:
        return jsonify({"fields": {}, "warnings": warnings, "message": "Nu au fost detectate date lizibile."}), 200
    return jsonify({"fields": fields, "warnings": warnings}), 200


@app.errorhandler(413)
def too_large(_error):
    return jsonify({"message": "Imaginea depășește limita de 10 MB."}), 413


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001)
