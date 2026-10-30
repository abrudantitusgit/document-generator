using InterfataActe.Drawings.Model;

namespace InterfataActe.Drawings.Core
{
    // Sheet frame and the "cartuș" in the bottom-right corner, laid out like the company's CAD sheets.
    public static class TitleBlock
    {
        public const double Margin = 10;
        public const double BlockWidth = 190;
        public const double BlockHeight = 42;

        public static double Top(Drawing d) => d.Height - Margin - BlockHeight;
        public static double Left(Drawing d) => d.Width - Margin - BlockWidth;

        public static void Draw(Drawing d, DrawingContext ctx, DrawingOptions options, string sheetTitle)
        {
            d.Rect(Layers.Frame, Margin, Margin, d.Width - 2 * Margin, d.Height - 2 * Margin, 0.5);

            double x = Left(d), y = Top(d);
            d.Rect(Layers.Frame, x, y, BlockWidth, BlockHeight, 0.5, "#FFFFFF");

            // Columns: company + people | beneficiary + sheet title | project + sheet number
            double c1 = x + 68, c2 = x + 158, mid = y + 22;
            d.Line(Layers.Frame, c1, y, c1, y + BlockHeight, 0.35);
            d.Line(Layers.Frame, c2, y, c2, y + BlockHeight, 0.35);
            d.Line(Layers.Frame, x, mid, x + BlockWidth, mid, 0.35);

            var company = options.Company;
            d.Text(x + 2, y + 4, company.Name, 2.3, bold: true);
            d.TextLines(x + 2, y + 7.5, new[] { company.Address, company.Registration, $"{company.Phone}, {company.Email}", company.Web }, 1.6, spacing: 1.5);

            var people = new[] { ("Șef proiect", options.SefProiect), ("Proiectat", options.Proiectat), ("Desenat", options.Desenat), ("Verificat", options.Verificat) };
            for (int i = 0; i < people.Length; i++)
            {
                double ry = mid + i * 5;
                if (i > 0) d.Line(Layers.Frame, x, ry, c1, ry, 0.2);
                d.Text(x + 2, ry + 3.6, people[i].Item1 + ":", 1.9);
                d.Text(x + 20, ry + 3.6, people[i].Item2, 1.9);
            }
            d.Line(Layers.Frame, x + 18.5, mid, x + 18.5, y + BlockHeight, 0.2);

            double ty = y + 4;
            d.Text(c1 + 2, ty, "Beneficiar: " + ctx.Beneficiar, 2, bold: true);
            ty += 3.2;
            foreach (var line in Symbols.Wrap("Adresa: " + ctx.Adresa, 86, 1.7))
            {
                if (ty > mid - 6) break;
                d.Text(c1 + 2, ty, line, 1.7);
                ty += 2.5;
            }
            foreach (var line in Symbols.Wrap("Lucrare: " + ctx.Lucrare, 86, 1.7))
            {
                if (ty > mid - 3) break;
                d.Text(c1 + 2, ty, line, 1.7);
                ty += 2.5;
            }
            d.Text(c1 + 2, mid - 1.2, ctx.Putere, 1.8, bold: true);

            d.Line(Layers.Frame, c1 + 24, mid, c1 + 24, y + BlockHeight, 0.35);
            d.Text(c1 + 2, mid + 5, "Scara:", 1.9);
            d.Text(c1 + 12, mid + 13, "-", 2.4, TextAnchor.Middle);
            d.Text(c1 + 2, y + BlockHeight - 2, "Data: " + ctx.Data, 1.9);
            double titleX = c1 + 24 + (c2 - c1 - 24) / 2;
            var titleLines = Symbols.Wrap(sheetTitle, c2 - c1 - 28, 2.6);
            d.TextLines(titleX, mid + 11 - (titleLines.Count - 1) * 1.8, titleLines, 2.6, TextAnchor.Middle, bold: true, spacing: 1.4);

            d.Text(c2 + 2, y + 4, "Proiect nr.", 1.9);
            d.Text(c2 + 16, y + 11, ctx.NrProiect, 3.2, TextAnchor.Middle, bold: true);
            d.Text(c2 + 16, y + 18, company.City, 2, TextAnchor.Middle);
            d.Text(c2 + 2, mid + 4, "Planșa", 1.9);
            d.Text(c2 + 16, mid + 15, d.Code, 6, TextAnchor.Middle, bold: true);

            d.StampArea = new RectShape(c1 - 30, y - 26, 30, 24);
        }
    }
}
