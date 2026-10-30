using System;
using System.Linq;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Model;
using InterfataActe.Models;
using static InterfataActe.Drawings.Model.DrawingContext;

namespace InterfataActe.Drawings.Generators
{
    // E2 for photovoltaic projects: every string with its modules, wired to its inverter.
    public static class PvStringsGenerator
    {
        public const string Code = "E2";
        public const string FileName = "E2_Schema_stringuri";
        public const string Title = "Schema de conectare a stringurilor";

        private const double XLabel = 18, XPanels = 30, XInv = 330, WInv = 34;

        public static Drawing Generate(ProiectFotovoltaice p, DrawingUseCase uc, DrawingOptions o)
        {
            var d = new Drawing(Code, FileName, Title) { UseCase = uc.Code };
            bool tri = uc.Trifazat;
            int max = tri ? o.MaxPanouriStringTrifazat : o.MaxPanouriStringMonofazat;
            var units = StringPlanner.Plan(p, max);

            d.Text(20, 22, $"SCHEMA DE CONECTARE A STRINGURILOR - GENERATOR FOTOVOLTAIC {Num(p.PutereTotalaPanouri / 1000)} kWp", 5, bold: true);
            d.Text(20, 28.5, $"{units.Sum(u => u.Strings.Count)} stringuri, {units.Sum(u => u.Panels)} panouri, {units.Count} {(units.Count == 1 ? "invertor" : "invertoare")} - max. {max} panouri/string", 2.4);

            const double areaTop = 38, areaBottom = 222, header = 9, gap = 3;
            int totalStrings = Math.Max(1, units.Sum(u => u.Strings.Count));
            double pitch = Math.Min(13, (areaBottom - areaTop - units.Count * (header + gap)) / totalStrings);
            bool aggregate = pitch < 8;
            if (aggregate) pitch = Math.Min(13, (areaBottom - areaTop) / units.Count - header - gap);

            double y = areaTop;
            foreach (var unit in units)
            {
                d.Text(XLabel, y + 5, $"Invertor {unit.Index}: {unit.Model} - {Num(unit.Kw)} kW ({(tri ? "trifazat" : "monofazat")}), {Num(unit.Kwp)} kWp", 2.3, bold: true);
                y += header;
                double blockTop = y;
                int rows = aggregate ? 1 : unit.Strings.Count;
                for (int j = 0; j < rows; j++)
                {
                    double rowY = y + pitch / 2;
                    double panelH = Math.Max(4, Math.Min(9, pitch - 4));
                    double panelW = panelH * 0.68;
                    int count = aggregate ? unit.Panels : unit.Strings[j].Panels;
                    int maxDraw = (int)((XInv - 12 - XPanels) / (panelW + 0.8));
                    double endX = count > 0 ? Symbols.PvString(d, XPanels, rowY - panelH / 2, count, panelW, panelH, maxDraw) : XPanels;

                    string label = aggregate
                        ? string.Join("; ", StringPlanner.Describe(unit))
                        : $"{count} x {unit.Strings[j].PanelModel} {Num(unit.Strings[j].PanelW)} W = {Num(count * unit.Strings[j].PanelW / 1000)} kWp";
                    d.Text(XLabel, rowY + 1, aggregate ? "S" : $"S{j + 1}", 2.2, bold: true);
                    d.Text(XPanels, rowY - panelH / 2 - 0.8, label, 1.7);
                    d.Line(Layers.DcPlus, endX, rowY - 1.2, XInv, rowY - 1.2, 0.35);
                    d.Line(Layers.DcMinus, endX, rowY + 1.2, XInv, rowY + 1.2, 0.35);
                    d.Text(XInv - 2, rowY - 2, "+", 1.8, TextAnchor.End, layer: Layers.DcPlus);
                    d.Text(XInv - 2, rowY + 3.6, "-", 1.8, TextAnchor.End);
                    y += pitch;
                }

                double invTop = blockTop - 1, invH = Math.Max(14, y - blockTop + 2);
                Symbols.Inverter(d, XInv, invTop, WInv, invH);
                d.TextLines(XInv + WInv + 3, invTop + 3, new[] { unit.Model, $"{Num(unit.Kw)} kW", $"{unit.Strings.Count} intrări DC" }, 1.8);
                y += gap;
            }

            d.Table.AddRange(EquipmentTables.Fotovoltaice(p));
            PvSingleLineGenerator.DrawFooter(d, p);
            d.Text(15, 226, $"Notă: configurația stringurilor este orientativă (max. {max} panouri/string) și se confirmă la execuție conform fișei tehnice a invertorului.", 1.8);
            TitleBlock.Draw(d, From(p), o, Title);
            return d;
        }
    }
}
