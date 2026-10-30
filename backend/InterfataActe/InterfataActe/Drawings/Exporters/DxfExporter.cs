using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InterfataActe.Drawings.Core;
using static InterfataActe.Drawings.Exporters.Fmt;

namespace InterfataActe.Drawings.Exporters
{
    // ASCII DXF R12 (AC1009): opens in every AutoCAD version, BricsCAD, LibreCAD.
    // Units are millimetres, Y axis flipped to the CAD convention, one DXF layer per drawing layer.
    public class DxfExporter : IDrawingExporter
    {
        public string Extension => "dxf";
        public string ContentType => "application/dxf";

        public void Export(Drawing d, Stream output)
        {
            var w = new DxfWriter();
            w.Section("HEADER");
            w.Pair(9, "$ACADVER"); w.Pair(1, "AC1009");
            w.Pair(9, "$DWGCODEPAGE"); w.Pair(3, "ANSI_1252");
            w.Pair(9, "$INSBASE"); w.Point(10, 0, 0);
            w.Pair(9, "$EXTMIN"); w.Point(10, 0, 0);
            w.Pair(9, "$EXTMAX"); w.Point(10, d.Width, d.Height);
            w.Pair(9, "$LIMMIN"); w.Pair(10, "0.0"); w.Pair(20, "0.0");
            w.Pair(9, "$LIMMAX"); w.Pair(10, F(d.Width)); w.Pair(20, F(d.Height));
            w.Pair(9, "$LTSCALE"); w.Pair(40, "1.0");
            w.EndSection();

            w.Section("TABLES");
            w.Pair(0, "TABLE"); w.Pair(2, "LTYPE"); w.Pair(70, "2");
            w.Pair(0, "LTYPE"); w.Pair(2, "CONTINUOUS"); w.Pair(70, "0"); w.Pair(3, "Solid line"); w.Pair(72, "65"); w.Pair(73, "0"); w.Pair(40, "0.0");
            w.Pair(0, "LTYPE"); w.Pair(2, "DASHED"); w.Pair(70, "0"); w.Pair(3, "__ __ __"); w.Pair(72, "65"); w.Pair(73, "2"); w.Pair(40, "3.2");
            w.Pair(49, "2.0"); w.Pair(49, "-1.2");
            w.Pair(0, "ENDTAB");

            var layers = Layers.All.Where(l => d.UsedLayers.Contains(l.Name)).ToList();
            w.Pair(0, "TABLE"); w.Pair(2, "LAYER"); w.Pair(70, layers.Count.ToString());
            foreach (var l in layers)
            {
                w.Pair(0, "LAYER"); w.Pair(2, l.Name); w.Pair(70, "0"); w.Pair(62, l.Aci.ToString()); w.Pair(6, "CONTINUOUS");
            }
            w.Pair(0, "ENDTAB");

            w.Pair(0, "TABLE"); w.Pair(2, "STYLE"); w.Pair(70, "1");
            w.Pair(0, "STYLE"); w.Pair(2, "STANDARD"); w.Pair(70, "0"); w.Pair(40, "0.0"); w.Pair(41, "1.0"); w.Pair(50, "0.0");
            w.Pair(71, "0"); w.Pair(42, "2.5"); w.Pair(3, "txt"); w.Pair(4, "");
            w.Pair(0, "ENDTAB");
            w.EndSection();

            w.Section("ENTITIES");
            double H = d.Height;
            foreach (var shape in d.Ordered)
            {
                switch (shape)
                {
                    case LineShape l:
                        w.Entity("LINE", shape);
                        w.Point(10, l.X1, H - l.Y1);
                        w.Point(11, l.X2, H - l.Y2);
                        break;
                    case PolylineShape p:
                        Polyline(w, shape, p.Points.Select(pt => (pt.X, H - pt.Y)), p.Closed);
                        break;
                    case RectShape r:
                        Polyline(w, shape, new[] { (r.X, H - r.Y), (r.X + r.W, H - r.Y), (r.X + r.W, H - r.Y - r.H), (r.X, H - r.Y - r.H) }, true);
                        break;
                    case CircleShape c:
                        w.Entity("CIRCLE", shape);
                        w.Point(10, c.Cx, H - c.Cy);
                        w.Pair(40, F(c.R));
                        break;
                    case TextShape t:
                        w.Entity("TEXT", shape);
                        w.Point(10, t.X, H - t.Y);
                        w.Pair(40, F(t.Size * 0.72));
                        w.Pair(1, Encode(t.Value));
                        if (t.Rotation != 0) w.Pair(50, F(t.Rotation));
                        if (t.Anchor != TextAnchor.Start)
                        {
                            w.Pair(72, t.Anchor == TextAnchor.Middle ? "1" : "2");
                            w.Point(11, t.X, H - t.Y);
                        }
                        break;
                }
            }
            w.EndSection();
            w.Pair(0, "EOF");

            var bytes = Encoding.ASCII.GetBytes(w.ToString());
            output.Write(bytes, 0, bytes.Length);
        }

        private static void Polyline(DxfWriter w, Shape shape, IEnumerable<(double X, double Y)> points, bool closed)
        {
            w.Entity("POLYLINE", shape);
            w.Pair(66, "1");
            w.Point(10, 0, 0);
            w.Pair(70, closed ? "1" : "0");
            foreach (var (x, y) in points)
            {
                w.Pair(0, "VERTEX"); w.Pair(8, shape.Layer);
                w.Point(10, x, y);
            }
            w.Pair(0, "SEQEND"); w.Pair(8, shape.Layer);
        }

        // The file stays pure ASCII: non-ASCII characters (ă, ș, ț, Ω...) use the \U+XXXX escape
        // that AutoCAD, BricsCAD, LibreCAD and ezdxf decode independently of $DWGCODEPAGE.
        private static string Encode(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text.Replace("±", "%%p"))
                sb.Append(c < 128 ? c.ToString() : $"\\U+{(int)c:X4}");
            return sb.ToString();
        }

        private class DxfWriter
        {
            private readonly StringBuilder _sb = new();

            public void Pair(int code, string value) => _sb.Append(code).Append("\r\n").Append(value).Append("\r\n");
            public void Point(int code, double x, double y)
            {
                Pair(code, F(x));
                Pair(code + 10, F(y));
                Pair(code + 20, "0.0");
            }
            public void Section(string name) { Pair(0, "SECTION"); Pair(2, name); }
            public void EndSection() => Pair(0, "ENDSEC");
            public void Entity(string type, Shape shape)
            {
                Pair(0, type);
                Pair(8, shape.Layer);
                if (shape.Dashed) Pair(6, "DASHED");
            }
            public override string ToString() => _sb.ToString();
        }
    }
}
