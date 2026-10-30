using System.Collections.Generic;
using System.Linq;

namespace InterfataActe.Drawings.Core
{
    // Coordinates are millimetres on the sheet, origin top-left, Y pointing down
    // (same as SVG). Exporters convert to their own unit/axis conventions.
    public enum TextAnchor { Start, Middle, End }

    public readonly record struct Pt(double X, double Y);

    public abstract record Shape
    {
        public string Layer { get; init; } = Layers.Equip;
        public bool Dashed { get; init; }
        public double Width { get; init; } = 0.25;
    }

    public sealed record LineShape(double X1, double Y1, double X2, double Y2) : Shape;

    public sealed record PolylineShape(IReadOnlyList<Pt> Points, bool Closed) : Shape;

    public sealed record RectShape(double X, double Y, double W, double H) : Shape
    {
        public string Fill { get; init; }
    }

    public sealed record CircleShape(double Cx, double Cy, double R) : Shape
    {
        public bool Filled { get; init; }
    }

    // (X, Y) is the baseline point; Rotation is in degrees, counter-clockwise as seen on paper.
    public sealed record TextShape(double X, double Y, string Value, double Size) : Shape
    {
        public TextAnchor Anchor { get; init; }
        public double Rotation { get; init; }
        public bool Bold { get; init; }
    }

    public class Drawing
    {
        public Drawing(string code, string fileName, string title, double width = 420, double height = 297)
        {
            Code = code;
            FileName = fileName;
            Title = title;
            Width = width;
            Height = height;
        }

        public string Code { get; }
        public string FileName { get; }
        public string Title { get; }
        public double Width { get; }
        public double Height { get; }
        public string UseCase { get; set; }
        public string Author { get; set; }
        public List<Shape> Shapes { get; } = new();

        // Equipment / circuits list, rendered as a native table in the DOCX export.
        public List<string[]> Table { get; } = new();

        // Free area of the title block where a company stamp image may be placed.
        public RectShape StampArea { get; set; }

        public IEnumerable<string> UsedLayers => Shapes.Select(s => s.Layer).Distinct();

        // Shapes stacked by layer (Layers.All order), the paint order shared by every exporter.
        public IEnumerable<Shape> Ordered => Layers.All.SelectMany(l => Shapes.Where(s => s.Layer == l.Name));

        public Drawing Line(string layer, double x1, double y1, double x2, double y2, double width = 0.35, bool dashed = false)
        {
            Shapes.Add(new LineShape(x1, y1, x2, y2) { Layer = layer, Width = width, Dashed = dashed });
            return this;
        }

        public Drawing Path(string layer, double width, params Pt[] points) => Path(layer, width, false, false, points);

        public Drawing Path(string layer, double width, bool closed, bool dashed, params Pt[] points)
        {
            Shapes.Add(new PolylineShape(points, closed) { Layer = layer, Width = width, Dashed = dashed });
            return this;
        }

        public Drawing Rect(string layer, double x, double y, double w, double h, double width = 0.35, string fill = null, bool dashed = false)
        {
            Shapes.Add(new RectShape(x, y, w, h) { Layer = layer, Width = width, Fill = fill, Dashed = dashed });
            return this;
        }

        public Drawing Circle(string layer, double cx, double cy, double r, double width = 0.35, bool filled = false)
        {
            Shapes.Add(new CircleShape(cx, cy, r) { Layer = layer, Width = width, Filled = filled });
            return this;
        }

        public Drawing Dot(string layer, double cx, double cy) => Circle(layer, cx, cy, 0.7, 0.2, true);

        public Drawing Text(double x, double y, string value, double size = 2.5, TextAnchor anchor = TextAnchor.Start,
            double rotation = 0, bool bold = false, string layer = Layers.Text)
        {
            if (string.IsNullOrEmpty(value)) return this;
            Shapes.Add(new TextShape(x, y, value, size) { Layer = layer, Anchor = anchor, Rotation = rotation, Bold = bold, Width = 0 });
            return this;
        }

        // Several lines of text, first baseline at y.
        public double TextLines(double x, double y, IEnumerable<string> lines, double size = 2.5, TextAnchor anchor = TextAnchor.Start,
            bool bold = false, double spacing = 1.45)
        {
            foreach (var line in lines)
            {
                Text(x, y, line, size, anchor, 0, bold);
                y += size * spacing;
            }
            return y;
        }
    }
}
