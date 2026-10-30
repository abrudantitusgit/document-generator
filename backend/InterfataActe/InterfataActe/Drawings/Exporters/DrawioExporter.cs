using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using InterfataActe.Drawings.Core;
using static InterfataActe.Drawings.Exporters.Fmt;

namespace InterfataActe.Drawings.Exporters
{
    // Uncompressed draw.io (diagrams.net) file. Every drawing layer becomes a draw.io layer,
    // lines become edges with waypoints, boxes/circles vertices, labels text cells.
    public class DrawioExporter : IDrawingExporter
    {
        private const double S = 4; // draw.io units per millimetre

        public string Extension => "drawio";
        public string ContentType => "application/vnd.jgraph.mxfile";

        public void Export(Drawing d, Stream output)
        {
            var sb = new StringBuilder();
            int id = 0;
            string Next() => "c" + (++id);

            sb.AppendLine($"<mxfile host=\"InterfataActe\" type=\"device\">");
            sb.AppendLine($"  <diagram id=\"{d.Code}\" name=\"{X(d.Code + " " + d.Title)}\">");
            sb.AppendLine($"    <mxGraphModel grid=\"0\" gridSize=\"10\" guides=\"1\" tooltips=\"1\" connect=\"0\" arrows=\"0\" fold=\"1\" page=\"1\" pageScale=\"1\" pageWidth=\"{F(d.Width * S)}\" pageHeight=\"{F(d.Height * S)}\" math=\"0\" shadow=\"0\">");
            sb.AppendLine("      <root>");
            sb.AppendLine("        <mxCell id=\"0\" />");

            foreach (var layer in Layers.All.Where(l => d.UsedLayers.Contains(l.Name)))
            {
                string layerId = "layer_" + layer.Name;
                sb.AppendLine($"        <mxCell id=\"{layerId}\" value=\"{X(layer.Name)}\" parent=\"0\" />");
                foreach (var shape in d.Shapes.Where(s => s.Layer == layer.Name))
                    sb.AppendLine("        " + Cell(shape, Next(), layerId, layer.Color));
            }

            sb.AppendLine("      </root>");
            sb.AppendLine("    </mxGraphModel>");
            sb.AppendLine("  </diagram>");
            sb.AppendLine("</mxfile>");

            var bytes = new UTF8Encoding(false).GetBytes(sb.ToString());
            output.Write(bytes, 0, bytes.Length);
        }

        private static string X(string value) => SecurityElement.Escape(value ?? "");

        private static string StrokeStyle(Shape s, string color) =>
            $"strokeColor={color};strokeWidth={F(System.Math.Max(0.5, s.Width * S))};" + (s.Dashed ? "dashed=1;dashPattern=8 5;" : "");

        private static string P(double x, double y, string extra = "") => $"<mxPoint x=\"{F(x * S)}\" y=\"{F(y * S)}\"{extra} />";

        private static string Cell(Shape shape, string id, string parent, string color)
        {
            switch (shape)
            {
                case LineShape l:
                    return Edge(id, parent, StrokeStyle(l, color), l.X1, l.Y1, l.X2, l.Y2, "");
                case PolylineShape p:
                    var pts = p.Closed ? p.Points.Append(p.Points[0]).ToList() : p.Points.ToList();
                    var inner = string.Concat(pts.Skip(1).Take(pts.Count - 2).Select(pt => P(pt.X, pt.Y)));
                    return Edge(id, parent, StrokeStyle(p, color), pts[0].X, pts[0].Y, pts[^1].X, pts[^1].Y,
                        inner.Length > 0 ? $"<Array as=\"points\">{inner}</Array>" : "");
                case RectShape r:
                    return Vertex(id, parent, "", $"rounded=0;whiteSpace=wrap;html=1;fillColor={r.Fill ?? "none"};" + StrokeStyle(r, color), r.X, r.Y, r.W, r.H);
                case CircleShape c:
                    return Vertex(id, parent, "", $"ellipse;html=1;fillColor={(c.Filled ? color : "none")};" + StrokeStyle(c, color), c.Cx - c.R, c.Cy - c.R, 2 * c.R, 2 * c.R);
                case TextShape t:
                    return TextCell(id, parent, t, color);
                default:
                    return "";
            }
        }

        private static string Edge(string id, string parent, string style, double x1, double y1, double x2, double y2, string points) =>
            $"<mxCell id=\"{id}\" style=\"endArrow=none;startArrow=none;html=1;rounded=0;{style}\" edge=\"1\" parent=\"{parent}\">" +
            $"<mxGeometry relative=\"1\" as=\"geometry\">{P(x1, y1, " as=\"sourcePoint\"")}{P(x2, y2, " as=\"targetPoint\"")}{points}</mxGeometry></mxCell>";

        private static string Vertex(string id, string parent, string value, string style, double x, double y, double w, double h) =>
            $"<mxCell id=\"{id}\" value=\"{X(value)}\" style=\"{style}\" vertex=\"1\" parent=\"{parent}\">" +
            $"<mxGeometry x=\"{F(x * S)}\" y=\"{F(y * S)}\" width=\"{F(w * S)}\" height=\"{F(h * S)}\" as=\"geometry\" /></mxCell>";

        // A text box around the baseline point; rotated boxes are moved so that the
        // rotation about the box centre keeps the text at its anchor.
        private static string TextCell(string id, string parent, TextShape t, string color)
        {
            double w = Symbols.TextWidth(t.Value, t.Size) * 1.15 + 1, h = t.Size * 1.4;
            double offset = t.Anchor switch { TextAnchor.Middle => w / 2, TextAnchor.End => w, _ => 0 };
            // centre of the unrotated box relative to the anchor
            double cx = w / 2 - offset, cy = -t.Size * 0.35;
            double a = -t.Rotation * System.Math.PI / 180;
            double rx = cx * System.Math.Cos(a) - cy * System.Math.Sin(a);
            double ry = cx * System.Math.Sin(a) + cy * System.Math.Cos(a);
            double x = t.X + rx - w / 2, y = t.Y + ry - h / 2;
            string align = t.Anchor switch { TextAnchor.Middle => "center", TextAnchor.End => "right", _ => "left" };
            string style = $"text;html=0;whiteSpace=nowrap;strokeColor=none;fillColor=none;align={align};verticalAlign=middle;spacing=0;" +
                $"fontFamily=DejaVu Sans;fontSize={F(t.Size * S)};fontColor={color};" +
                (t.Bold ? "fontStyle=1;" : "") + (t.Rotation != 0 ? $"rotation={F(-t.Rotation)};" : "");
            return Vertex(id, parent, t.Value, style, x, y, w, h);
        }
    }
}
