using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using InterfataActe.Drawings.Core;
using static InterfataActe.Drawings.Exporters.Fmt;

namespace InterfataActe.Drawings.Exporters
{
    public class SvgExporter : IDrawingExporter
    {
        public string Extension => "svg";
        public string ContentType => "image/svg+xml";

        public void Export(Drawing d, Stream output)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{F(d.Width)}mm\" height=\"{F(d.Height)}mm\" viewBox=\"0 0 {F(d.Width)} {F(d.Height)}\">");
            sb.AppendLine($"<title>{SecurityElement.Escape(d.Title)}</title>");
            sb.AppendLine($"<rect width=\"{F(d.Width)}\" height=\"{F(d.Height)}\" fill=\"#FFFFFF\"/>");
            sb.AppendLine("<g font-family=\"DejaVu Sans, Arial, sans-serif\" stroke-linecap=\"round\" stroke-linejoin=\"round\">");

            // One group per layer, in the same order and names as the DXF and draw.io layers.
            foreach (var layer in Layers.All.Where(l => d.UsedLayers.Contains(l.Name)))
            {
                sb.AppendLine($"<g id=\"{layer.Name}\">");
                foreach (var s in d.Shapes.Where(s => s.Layer == layer.Name))
                    sb.AppendLine(Shape(s, layer.Color));
                sb.AppendLine("</g>");
            }
            sb.AppendLine("</g></svg>");

            var bytes = new UTF8Encoding(false).GetBytes(sb.ToString());
            output.Write(bytes, 0, bytes.Length);
        }

        private static string Stroke(Shape s, string color) =>
            $"stroke=\"{color}\" stroke-width=\"{F(s.Width)}\"" + (s.Dashed ? " stroke-dasharray=\"2 1.2\"" : "");

        private static string Shape(Shape shape, string color) => shape switch
        {
            LineShape l => $"<line x1=\"{F(l.X1)}\" y1=\"{F(l.Y1)}\" x2=\"{F(l.X2)}\" y2=\"{F(l.Y2)}\" {Stroke(l, color)}/>",
            PolylineShape p => $"<{(p.Closed ? "polygon" : "polyline")} points=\"{string.Join(" ", p.Points.Select(pt => $"{F(pt.X)},{F(pt.Y)}"))}\" fill=\"none\" {Stroke(p, color)}/>",
            RectShape r => $"<rect x=\"{F(r.X)}\" y=\"{F(r.Y)}\" width=\"{F(r.W)}\" height=\"{F(r.H)}\" fill=\"{r.Fill ?? "none"}\" {Stroke(r, color)}/>",
            CircleShape c => $"<circle cx=\"{F(c.Cx)}\" cy=\"{F(c.Cy)}\" r=\"{F(c.R)}\" fill=\"{(c.Filled ? color : "none")}\" {Stroke(c, color)}/>",
            TextShape t => Text(t, color),
            _ => "",
        };

        private static string Text(TextShape t, string color)
        {
            string anchor = t.Anchor switch { TextAnchor.Middle => "middle", TextAnchor.End => "end", _ => "start" };
            string transform = t.Rotation != 0 ? $" transform=\"rotate({F(-t.Rotation)} {F(t.X)} {F(t.Y)})\"" : "";
            string weight = t.Bold ? " font-weight=\"bold\"" : "";
            return $"<text x=\"{F(t.X)}\" y=\"{F(t.Y)}\" font-size=\"{F(t.Size)}\" text-anchor=\"{anchor}\" fill=\"{color}\"{weight}{transform}>{SecurityElement.Escape(t.Value)}</text>";
        }
    }
}
