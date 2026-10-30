using System;
using System.Collections.Generic;
using System.Linq;

namespace InterfataActe.Drawings.Core
{
    // Electrical symbols built from primitives. Horizontal symbols sit on a wire at height y
    // starting at x and return the x where the wire continues; vertical ones run top to bottom.
    public static class Symbols
    {
        public const double Wire = 0.5;

        public static double TextWidth(string text, double size) => (text ?? "").Length * size * 0.56;

        public static List<string> Wrap(string text, double maxWidth, double size)
        {
            var lines = new List<string>();
            var current = "";
            foreach (var word in (text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = current.Length == 0 ? word : current + " " + word;
                if (TextWidth(candidate, size) > maxWidth && current.Length > 0)
                {
                    lines.Add(current);
                    current = word;
                }
                else current = candidate;
            }
            if (current.Length > 0) lines.Add(current);
            return lines;
        }

        // Label lines stacked upwards, the last one ending at baseline y.
        private static void LabelAbove(Drawing d, double x, double y, string label, double size = 2)
        {
            if (string.IsNullOrEmpty(label)) return;
            var lines = label.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                d.Text(x, y - (lines.Length - 1 - i) * size * 1.35, lines[i], size, TextAnchor.Middle);
        }

        public static double BreakerH(Drawing d, string layer, double x, double y, string label = null)
        {
            d.Line(layer, x, y, x + 4, y, Wire);
            d.Line(layer, x + 4, y, x + 10, y - 3.5, Wire);
            d.Line(layer, x + 10, y, x + 14, y, Wire);
            Cross(d, layer, x + 10, y);
            LabelAbove(d, x + 7, y - 5.5, label);
            return x + 14;
        }

        public static double RcdH(Drawing d, string layer, double x, double y, string label = null)
        {
            d.Line(layer, x, y, x + 5, y, Wire);
            d.Circle(Layers.Equip, x + 2.5, y, 1.8, 0.25);
            d.Line(Layers.Equip, x + 2.5, y - 1.8, x + 12, y - 1.8, 0.2, dashed: true);
            BreakerH(d, layer, x + 5, y);
            LabelAbove(d, x + 9.5, y - 5.5, label);
            return x + 19;
        }

        public static double ContactorH(Drawing d, string layer, double x, double y, string label = null)
        {
            d.Line(layer, x, y, x + 4, y, Wire);
            d.Line(layer, x + 4, y, x + 10, y - 3.5, Wire);
            d.Line(layer, x + 10, y, x + 14, y, Wire);
            d.Rect(Layers.Equip, x + 4.5, y + 3.5, 5, 3, 0.25);
            d.Line(Layers.Equip, x + 7, y - 1.75, x + 7, y + 3.5, 0.2, dashed: true);
            d.Text(x + 11, y + 6.3, "K", 2, bold: true);
            LabelAbove(d, x + 7, y - 5.5, label);
            return x + 14;
        }

        public static double FuseH(Drawing d, string layer, double x, double y)
        {
            d.Line(layer, x, y, x + 10, y, Wire);
            d.Rect(Layers.Equip, x + 2, y - 1.2, 6, 2.4, 0.25);
            return x + 10;
        }

        // Vertical breaker / RCBO from (x, y) down to y + 14.
        public static double BreakerV(Drawing d, string layer, double x, double y, bool residual)
        {
            d.Line(layer, x, y, x, y + 4, Wire);
            d.Line(layer, x - 3.5, y + 4.5, x, y + 10, Wire);
            d.Line(layer, x, y + 10, x, y + 14, Wire);
            Cross(d, layer, x, y + 4);
            if (residual)
            {
                d.Circle(Layers.Equip, x, y + 12, 1.5, 0.25);
                d.Line(Layers.Equip, x + 1.5, y + 12, x + 3, y + 12, 0.2);
                d.Line(Layers.Equip, x + 3, y + 12, x + 3, y + 6, 0.2, dashed: true);
                d.Line(Layers.Equip, x + 3, y + 6, x - 1.6, y + 6, 0.2, dashed: true);
            }
            return y + 14;
        }

        private static void Cross(Drawing d, string layer, double x, double y)
        {
            d.Line(layer, x - 0.8, y - 0.8, x + 0.8, y + 0.8, 0.25);
            d.Line(layer, x - 0.8, y + 0.8, x + 0.8, y - 0.8, 0.25);
        }

        // Surge protective device / arrester hanging from (x, y) down to earth.
        public static void SpdDown(Drawing d, double x, double y, string label, string wireLayer = Layers.Ac)
        {
            d.Dot(wireLayer, x, y);
            d.Line(wireLayer, x, y, x, y + 4, 0.35);
            d.Rect(Layers.Equip, x - 2, y + 4, 4, 7, 0.3);
            d.Path(Layers.Equip, 0.25, new Pt(x, y + 5), new Pt(x, y + 8.5), new Pt(x - 1, y + 7.5));
            d.Line(Layers.Pe, x, y + 11, x, y + 15, 0.35);
            Ground(d, x, y + 15);
            if (!string.IsNullOrEmpty(label))
            {
                var lines = label.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                    d.Text(x + 3, y + 7 + i * 2.6, lines[i], 1.9);
            }
        }

        public static void Ground(Drawing d, double x, double y, string label = null)
        {
            d.Line(Layers.Pe, x - 3, y, x + 3, y, 0.35);
            d.Line(Layers.Pe, x - 2, y + 1.2, x + 2, y + 1.2, 0.35);
            d.Line(Layers.Pe, x - 1, y + 2.4, x + 1, y + 2.4, 0.35);
            if (!string.IsNullOrEmpty(label)) d.Text(x + 4, y + 2, label, 2);
        }

        public static void Inverter(Drawing d, double x, double y, double w, double h)
        {
            d.Rect(Layers.Equip, x, y, w, h, 0.45, "#FFFFFF");
            d.Line(Layers.Equip, x, y + h, x + w, y, 0.3);
            d.Line(Layers.Equip, x + 3, y + 4, x + 10, y + 4, 0.35);
            d.Line(Layers.Equip, x + 3, y + 5.6, x + 10, y + 5.6, 0.35, dashed: true);
            d.Text(x + w - 7, y + h - 2.5, "~", 6);
        }

        public static void Battery(Drawing d, double x, double y, double w, double h)
        {
            d.Rect(Layers.Equip, x, y, w, h, 0.45, "#FFFFFF");
            double cx = x + w / 2, cy = y + h / 2;
            d.Line(Layers.Equip, cx - 1.2, cy - 4, cx - 1.2, cy + 4, 0.5);
            d.Line(Layers.Equip, cx + 1.2, cy - 2, cx + 1.2, cy + 2, 0.9);
            d.Text(cx - 4.5, cy - 2, "+", 2.4);
            d.Text(cx + 3, cy - 2, "-", 2.4);
        }

        public static void Meter(Drawing d, double x, double y, double w, double h, bool bidirectional)
        {
            d.Rect(Layers.Equip, x, y, w, h, 0.45, "#FFFFFF");
            d.Text(x + w / 2, y + h / 2 + 0.2, "kWh", 2.4, TextAnchor.Middle, bold: true);
            if (!bidirectional) return;
            double ay = y + h - 2.2, ax = x + w / 2;
            d.Line(Layers.Equip, ax - 4, ay, ax + 4, ay, 0.25);
            d.Path(Layers.Equip, 0.25, new Pt(ax + 2.8, ay - 0.9), new Pt(ax + 4, ay), new Pt(ax + 2.8, ay + 0.9));
            d.Path(Layers.Equip, 0.25, new Pt(ax - 2.8, ay - 0.9), new Pt(ax - 4, ay), new Pt(ax - 2.8, ay + 0.9));
        }

        // Row of PV panels; draws at most maxDraw modules and abbreviates the middle.
        public static double PvString(Drawing d, double x, double y, int count, double panelW, double panelH, int maxDraw)
        {
            int drawn = Math.Min(count, maxDraw);
            bool abbreviated = count > maxDraw;
            double cx = x;
            for (int i = 0; i < drawn; i++)
            {
                if (abbreviated && i == drawn - 1)
                {
                    d.Text(cx + 1.8, y + panelH / 2 + 1, "...", 2.5, TextAnchor.Middle);
                    cx += 3.6;
                }
                Panel(d, cx, y, panelW, panelH);
                cx += panelW + 0.8;
            }
            return cx - 0.8;
        }

        public static void Panel(Drawing d, double x, double y, double w, double h)
        {
            d.Rect(Layers.Panel, x, y, w, h, 0.25, "#FDE3E3");
            d.Line(Layers.Panel, x, y + h / 3, x + w, y + h / 3, 0.15);
            d.Line(Layers.Panel, x, y + 2 * h / 3, x + w, y + 2 * h / 3, 0.15);
            d.Line(Layers.Panel, x + w / 2, y, x + w / 2, y + h, 0.15);
        }

        // Conductor-count ticks on a wire.
        public static void Ticks(Drawing d, string layer, double x, double y, int n, bool vertical = false)
        {
            for (int i = 0; i < n; i++)
            {
                double o = (i - (n - 1) / 2.0) * 1.6;
                if (vertical) d.Line(layer, x - 2, y + o + 1.2, x + 2, y + o - 1.2, 0.3);
                else d.Line(layer, x + o - 1.2, y + 2, x + o + 1.2, y - 2, 0.3);
            }
        }

        public static void ArrowRight(Drawing d, string layer, double x, double y) =>
            d.Path(layer, 0.35, new Pt(x - 3, y - 1.5), new Pt(x, y), new Pt(x - 3, y + 1.5));

        public static void ArrowDown(Drawing d, string layer, double x, double y) =>
            d.Path(layer, 0.35, new Pt(x - 1.5, y - 3), new Pt(x, y), new Pt(x + 1.5, y - 3));

        public static void Lamp(Drawing d, double x, double y)
        {
            d.Circle(Layers.Equip, x, y, 2, 0.3);
            d.Line(Layers.Equip, x - 1.4, y - 1.4, x + 1.4, y + 1.4, 0.3);
            d.Line(Layers.Equip, x - 1.4, y + 1.4, x + 1.4, y - 1.4, 0.3);
        }

        // Dashed enclosure for an electrical board, title written inside top-left.
        public static void Board(Drawing d, double x, double y, double w, double h, string title, string subtitle = null)
        {
            d.Rect(Layers.Equip, x, y, w, h, 0.35, dashed: true);
            d.Text(x + 2, y + 4.5, title, 2.8, bold: true);
            if (!string.IsNullOrEmpty(subtitle)) d.Text(x + 2, y + 8, subtitle, 1.9);
        }

        public static void Pole(Drawing d, double x, double y, string label)
        {
            d.Rect(Layers.Equip, x - 1.5, y, 3, 34, 0.4, "#E0E0E0");
            d.Line(Layers.Equip, x - 9, y + 4, x + 9, y + 4, 0.5);
            foreach (var ix in new[] { -7.5, -2.5, 2.5, 7.5 }) d.Circle(Layers.Equip, x + ix, y + 2.8, 0.7, 0.25, true);
            Ground(d, x, y + 34);
            d.TextLines(x, y - 6, Wrap(label, 40, 2.2), 2.2, TextAnchor.Middle);
        }

        // Current transformer / clamp on a horizontal wire.
        public static void Clamp(Drawing d, double x, double y)
        {
            d.Circle(Layers.Comm, x, y, 1.8, 0.3);
        }

        public static void Legend(Drawing d, double x, double y, IEnumerable<(string Layer, string Text)> items)
        {
            d.Text(x, y, "LEGENDĂ", 2.4, bold: true);
            y += 4.5;
            foreach (var (layer, text) in items)
            {
                d.Line(layer, x, y - 0.8, x + 10, y - 0.8, 0.6, layer == Layers.Comm);
                d.Text(x + 13, y, text, 2);
                y += 3.6;
            }
        }

        public static void Table(Drawing d, double x, double y, double[] colWidths, double rowHeight, IList<string[]> rows,
            double size = 1.9, bool headerBold = true)
        {
            double total = colWidths.Sum();
            for (int r = 0; r < rows.Count; r++)
            {
                double cx = x, ry = y + r * rowHeight;
                d.Line(Layers.Frame, x, ry, x + total, ry, 0.25);
                for (int c = 0; c < colWidths.Length && c < rows[r].Length; c++)
                {
                    var lines = Wrap(rows[r][c], colWidths[c] - 1.5, size);
                    double ty = ry + rowHeight / 2 + size * 0.35 - (lines.Count - 1) * size * 0.65;
                    foreach (var line in lines)
                    {
                        d.Text(cx + colWidths[c] / 2, ty, line, size, TextAnchor.Middle, bold: headerBold && r == 0);
                        ty += size * 1.3;
                    }
                    cx += colWidths[c];
                }
            }
            double bottom = y + rows.Count * rowHeight;
            d.Line(Layers.Frame, x, bottom, x + total, bottom, 0.25);
            double vx = x;
            d.Line(Layers.Frame, vx, y, vx, bottom, 0.25);
            foreach (var w in colWidths)
            {
                vx += w;
                d.Line(Layers.Frame, vx, y, vx, bottom, 0.25);
            }
        }
    }
}
