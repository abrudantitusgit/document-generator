using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using InterfataActe.Drawings.Core;

namespace InterfataActe.Drawings.Exporters
{
    // Minimal WordprocessingML package written directly (no DocX dependency): page 1 holds the
    // drawing at full sheet size, page 2 the equipment / circuit table as an editable Word table.
    public class DocxExporter : IDrawingExporter
    {
        private const string W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const double TwipsPerMm = 1440 / 25.4;
        private const long EmuPerMm = 36000;
        private const double MarginMm = 10;
        private readonly PngExporter _png;
        private readonly int _dpi;

        public DocxExporter(PngExporter png, int dpi)
        {
            _png = png;
            _dpi = dpi;
        }

        public string Extension => "docx";
        public string ContentType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public void Export(Drawing d, Stream output)
        {
            var image = _png.Render(d, _dpi);
            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
            Add(zip, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Default Extension=\"png\" ContentType=\"image/png\"/>" +
                "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
                "<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>" +
                "</Types>");
            Add(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
                "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>" +
                "</Relationships>");
            Add(zip, "docProps/core.xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
                "xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
                $"<dc:title>{X(d.Code + " " + d.Title)}</dc:title><dc:subject>{X(d.UseCase)}</dc:subject><dc:creator>{X(d.Author)}</dc:creator>" +
                $"<dcterms:created xsi:type=\"dcterms:W3CDTF\">{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</dcterms:created>" +
                "</cp:coreProperties>");
            Add(zip, "word/_rels/document.xml.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rIdImg\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"media/desen.png\"/>" +
                "</Relationships>");
            using (var s = zip.CreateEntry("word/media/desen.png", CompressionLevel.NoCompression).Open())
                s.Write(image, 0, image.Length);
            Add(zip, "word/document.xml", Document(d));
        }

        private static void Add(ZipArchive zip, string name, string content)
        {
            using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false));
            writer.Write(content);
        }

        private static string X(string value) => SecurityElement.Escape(value ?? "");

        private static string Document(Drawing d)
        {
            // Image fitted inside the margins (1 mm spare so Word keeps it on page 1).
            double scale = Math.Min((d.Width - 2 * MarginMm) / d.Width, (d.Height - 2 * MarginMm - 1) / d.Height);
            long cx = (long)(d.Width * scale * EmuPerMm), cy = (long)(d.Height * scale * EmuPerMm);

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append($"<w:document xmlns:w=\"{W}\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" " +
                "xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\" " +
                "xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" " +
                "xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><w:body>");

            sb.Append("<w:p><w:pPr><w:spacing w:before=\"0\" w:after=\"0\"/><w:jc w:val=\"center\"/></w:pPr><w:r><w:drawing>");
            sb.Append($"<wp:inline distT=\"0\" distB=\"0\" distL=\"0\" distR=\"0\"><wp:extent cx=\"{cx}\" cy=\"{cy}\"/>");
            sb.Append($"<wp:docPr id=\"1\" name=\"{X(d.FileName)}\" descr=\"{X(d.Title)}\"/>");
            sb.Append("<a:graphic><a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:pic>");
            sb.Append("<pic:nvPicPr><pic:cNvPr id=\"0\" name=\"desen.png\"/><pic:cNvPicPr/></pic:nvPicPr>");
            sb.Append("<pic:blipFill><a:blip r:embed=\"rIdImg\"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill>");
            sb.Append($"<pic:spPr><a:xfrm><a:off x=\"0\" y=\"0\"/><a:ext cx=\"{cx}\" cy=\"{cy}\"/></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom></pic:spPr>");
            sb.Append("</pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>");

            if (d.Table.Count > 0)
            {
                sb.Append($"<w:p><w:pPr><w:pageBreakBefore/><w:spacing w:after=\"120\"/></w:pPr>{Run($"{d.Code} - {d.Title}", 28, true)}</w:p>");
                sb.Append(Table(d, (int)((d.Width - 2 * MarginMm) * TwipsPerMm)));
            }

            int w = (int)(d.Width * TwipsPerMm), h = (int)(d.Height * TwipsPerMm), m = (int)(MarginMm * TwipsPerMm);
            sb.Append($"<w:sectPr><w:pgSz w:w=\"{w}\" w:h=\"{h}\"{(w > h ? " w:orient=\"landscape\"" : "")}/>");
            sb.Append($"<w:pgMar w:top=\"{m}\" w:right=\"{m}\" w:bottom=\"{m}\" w:left=\"{m}\" w:header=\"0\" w:footer=\"0\" w:gutter=\"0\"/></w:sectPr>");
            sb.Append("</w:body></w:document>");
            return sb.ToString();
        }

        private static string Run(string text, int halfPoints, bool bold) =>
            $"<w:r><w:rPr><w:rFonts w:ascii=\"Arial\" w:hAnsi=\"Arial\" w:cs=\"Arial\"/>{(bold ? "<w:b/>" : "")}<w:sz w:val=\"{halfPoints}\"/></w:rPr>" +
            $"<w:t xml:space=\"preserve\">{X(text)}</w:t></w:r>";

        private static string Table(Drawing d, int widthTwips)
        {
            int cols = d.Table.Max(r => r.Length);
            int colWidth = widthTwips / cols;
            var sb = new StringBuilder();
            sb.Append($"<w:tbl><w:tblPr><w:tblW w:w=\"{widthTwips}\" w:type=\"dxa\"/><w:tblBorders>");
            foreach (var side in new[] { "top", "left", "bottom", "right", "insideH", "insideV" })
                sb.Append($"<w:{side} w:val=\"single\" w:sz=\"4\" w:space=\"0\" w:color=\"000000\"/>");
            sb.Append("</w:tblBorders><w:tblCellMar><w:left w:w=\"80\" w:type=\"dxa\"/><w:right w:w=\"80\" w:type=\"dxa\"/></w:tblCellMar></w:tblPr><w:tblGrid>");
            for (int c = 0; c < cols; c++) sb.Append($"<w:gridCol w:w=\"{colWidth}\"/>");
            sb.Append("</w:tblGrid>");
            for (int r = 0; r < d.Table.Count; r++)
            {
                sb.Append(r == 0 ? "<w:tr><w:trPr><w:tblHeader/></w:trPr>" : "<w:tr>");
                for (int c = 0; c < cols; c++)
                {
                    string text = c < d.Table[r].Length ? d.Table[r][c] : "";
                    string shade = r == 0 ? "<w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"E7E6E6\"/>" : "";
                    sb.Append($"<w:tc><w:tcPr><w:tcW w:w=\"{colWidth}\" w:type=\"dxa\"/>{shade}</w:tcPr>");
                    sb.Append($"<w:p><w:pPr><w:spacing w:before=\"40\" w:after=\"40\"/></w:pPr>{Run(text, 20, r == 0)}</w:p></w:tc>");
                }
                sb.Append("</w:tr>");
            }
            sb.Append("</w:tbl><w:p/>");
            return sb.ToString();
        }
    }
}
