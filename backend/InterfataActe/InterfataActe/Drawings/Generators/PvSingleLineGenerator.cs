using System;
using System.Collections.Generic;
using System.Linq;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Model;
using InterfataActe.Models;
using static InterfataActe.Drawings.Model.DrawingContext;

namespace InterfataActe.Drawings.Generators
{
    // E1 for photovoltaic projects: strings -> T-DC -> inverter(s) -> TE CEF -> TG -> BMPT -> grid,
    // following the structure of exampleDraws/Schema monofilara Universitate-Model 2.pdf.
    public static class PvSingleLineGenerator
    {
        public const string Code = "E1";
        public const string FileName = "E1_Schema_electrica_monofilara";
        public const string Title = "Schema electrică monofilară";

        private const double XStrings = 18, XTdc = 128, WTdc = 24, XInv = 168, WInv = 30;
        private const double XTe = 214, XTeEnd = 300, XTg = 348, XBmpt = 376;

        private record Row(double Yc, double InvTop, double InvBottom);

        public static Drawing Generate(ProiectFotovoltaice p, DrawingUseCase uc, DrawingOptions o)
        {
            var d = new Drawing(Code, FileName, Title) { UseCase = uc.Code };
            bool tri = uc.Trifazat;
            int conductors = ElectricalSizing.Conductors(tri);
            var units = StringPlanner.Plan(p, tri ? o.MaxPanouriStringTrifazat : o.MaxPanouriStringMonofazat);
            bool multi = units.Count > 1;
            string backup = V(p.Backup), smart = V(p.SmartMeter);
            string poles = tri ? "4P" : "2P";

            d.Text(20, 22, $"SCHEMA ELECTRICĂ MONOFILARĂ - SISTEM FOTOVOLTAIC {Num(p.PutereTotalaPanouri / 1000)} kWp", 5, bold: true);
            d.Text(20, 28.5, $"{uc.Description} - racord {(tri ? "trifazat 400 V" : "monofazat 230 V")}", 2.4);

            double areaTop = 44;
            double areaBottom = multi ? (uc.Baterie || backup != null ? 175 : 222) : 160;
            double rowH = (areaBottom - areaTop) / units.Count;

            var rows = new List<Row>();
            for (int i = 0; i < units.Count; i++)
                rows.Add(DrawUnit(d, units[i], areaTop + rowH * (i + 0.5), rowH, tri, !multi, uc.Baterie && i == units.Count - 1));

            double yMain = (rows.First().Yc + rows.Last().Yc) / 2;
            double teTop = rows.Min(r => r.InvTop) - 10;
            double teBottom = Math.Max(rows.Max(r => r.InvBottom) + 4, yMain + 22);

            // AC side of every inverter up to the TE CEF
            for (int i = 0; i < rows.Count; i++)
            {
                double yc = rows[i].Yc;
                int breaker = ElectricalSizing.InverterBreaker((double)units[i].Kw, tri);
                d.Line(Layers.Ac, XInv + WInv, yc, XTe + (multi ? 4 : 6), yc, Symbols.Wire);
                Symbols.Ticks(d, Layers.Ac, 206, yc, conductors);
                d.Text(199, yc - 3.2, ElectricalSizing.AcCable(breaker, tri), 1.5);
                if (multi)
                {
                    Symbols.BreakerH(d, Layers.Ac, XTe + 4, yc, $"Q{i + 1} {poles} {breaker}A");
                    d.Line(Layers.Ac, XTe + 18, yc, 240, yc, Symbols.Wire);
                    d.Dot(Layers.Ac, 240, yc);
                }
            }

            Symbols.Board(d, XTe, teTop, XTeEnd - XTe, teBottom - teTop, "TE CEF", "Tablou electric centrală fotovoltaică");
            int mainBreaker = ElectricalSizing.InverterBreaker((double)p.PutereTotalaInvertor, tri);
            if (multi)
            {
                d.Line(Layers.Ac, 240, rows.First().Yc, 240, rows.Last().Yc, 1.0);
                d.Line(Layers.Ac, 240, yMain, 244, yMain, Symbols.Wire);
                Symbols.ContactorH(d, Layers.Ac, 244, yMain, "Contactor K\nprotecție interfață");
                d.Line(Layers.Ac, 258, yMain, 262, yMain, Symbols.Wire);
                Symbols.BreakerH(d, Layers.Ac, 262, yMain, $"Q0 {poles} {mainBreaker}A");
                d.Line(Layers.Ac, 276, yMain, XTeEnd, yMain, Symbols.Wire);
                Symbols.SpdDown(d, 288, yMain, "DPS AC\ntip 2");
            }
            else
            {
                Symbols.BreakerH(d, Layers.Ac, XTe + 6, yMain, $"Q1 {poles} {mainBreaker}A, C");
                d.Line(Layers.Ac, XTe + 20, yMain, 238, yMain, Symbols.Wire);
                Symbols.RcdH(d, Layers.Ac, 238, yMain, "Id = 30 mA, tip A");
                d.Line(Layers.Ac, 257, yMain, XTeEnd, yMain, Symbols.Wire);
                Symbols.SpdDown(d, 278, yMain, "DPS AC\ntip 2");
            }

            DrawGridSide(d, p, yMain, multi, smart, tri, mainBreaker, conductors);

            if (smart != null)
            {
                // RS485 link from the smart meter to the first inverter, routed above the TE CEF
                double yComm = teTop - 5;
                d.Path(Layers.Comm, 0.3, false, true,
                    new Pt(340, yMain - 25), new Pt(340, yComm), new Pt(202, yComm),
                    new Pt(202, rows[0].InvTop + 3), new Pt(XInv + WInv, rows[0].InvTop + 3));
                d.Text(270, yComm - 1.2, "RS485 / comunicație", 1.6, TextAnchor.Middle, layer: Layers.Comm);
            }

            var last = rows.Last();
            double extraTop = last.InvBottom + 14;
            if (uc.Baterie) DrawBattery(d, p, last, extraTop);
            if (backup != null) DrawBackup(d, backup, last, extraTop);

            d.Table.AddRange(EquipmentTables.Fotovoltaice(p));
            DrawFooter(d, p);
            TitleBlock.Draw(d, From(p), o, Title);
            return d;
        }

        private static Row DrawUnit(Drawing d, InverterPlan unit, double yc, double rowH, bool tri, bool single, bool hybrid)
        {
            const double pitch = 12;
            int nS = unit.Strings.Count;
            bool aggregate = nS == 0 || nS * pitch > rowH - 8;
            var ys = aggregate
                ? new List<double> { yc }
                : Enumerable.Range(0, nS).Select(j => yc + (j - (nS - 1) / 2.0) * pitch).ToList();

            for (int j = 0; j < ys.Count; j++)
            {
                double y = ys[j];
                int count = aggregate ? unit.Panels : unit.Strings[j].Panels;
                double endX = XStrings + 10;
                if (count > 0) endX = Symbols.PvString(d, XStrings, y - 3.5, count, 4.2, 7, 12);

                var labels = aggregate
                    ? StringPlanner.Describe(unit).ToList()
                    : new List<string> { $"S{j + 1}: {count} x {unit.Strings[j].PanelModel} {Num(unit.Strings[j].PanelW)} W" };
                for (int l = 0; l < labels.Count; l++)
                    d.Text(XStrings, y - 4.8 - (labels.Count - 1 - l) * 2.4, labels[l], 1.7);

                d.Line(Layers.DcPlus, endX, y - 1.5, XTdc + 3, y - 1.5, 0.35);
                d.Line(Layers.DcMinus, endX, y + 1.5, XInv, y + 1.5, 0.35);
                Symbols.FuseH(d, Layers.DcPlus, XTdc + 3, y - 1.5);
                d.Line(Layers.DcPlus, XTdc + 13, y - 1.5, XInv, y - 1.5, 0.35);
            }

            double tdcTop = ys.First() - 7, tdcBottom = ys.Last() + 7;
            d.Rect(Layers.Equip, XTdc, tdcTop, WTdc, tdcBottom - tdcTop, 0.4);
            d.Text(XTdc + WTdc / 2, tdcTop - 1.5, "T-DC", 2.2, TextAnchor.Middle, bold: true);
            if (single) Symbols.SpdDown(d, XTdc + WTdc - 4, tdcBottom, "DPS DC\ntip 2", Layers.Equip);
            else d.Text(XTdc + WTdc / 2, tdcBottom + 2.5, "siguranțe gPV + DPS DC", 1.4, TextAnchor.Middle);
            d.Text(XTdc + WTdc + 0.5, ys.First() - 3, "cablu solar 1x6 mmp", 1.4);

            double span = ys.Last() - ys.First();
            double invH = Math.Max(aggregate ? 16 : 22, span + 10);
            double invTop = yc - invH / 2;
            Symbols.Inverter(d, XInv, invTop, WInv, invH);
            var invLabels = new[] { $"Invertor {(hybrid ? "hibrid " : "")}{(tri ? "trifazat" : "monofazat")} {Num(unit.Kw)} kW", unit.Model };
            d.Text(XInv + WInv / 2, invTop - 4, invLabels[0], 1.8, TextAnchor.Middle, bold: true);
            d.Text(XInv + WInv / 2, invTop - 1.3, invLabels[1], 1.7, TextAnchor.Middle);
            return new Row(yc, invTop, invTop + invH);
        }

        private static void DrawGridSide(Drawing d, ProiectFotovoltaice p, double yMain, bool multi, string smart, bool tri, int breaker, int conductors)
        {
            Symbols.Ticks(d, Layers.Ac, 306, yMain, conductors);
            d.Text(302, yMain - 3.2, ElectricalSizing.AcCable(breaker, tri), 1.5);

            if (!multi) d.Line(Layers.Ac, XTeEnd, yMain, XBmpt + 4, yMain, Symbols.Wire);
            else
            {
                d.Line(Layers.Ac, XTeEnd, yMain, 318, yMain, Symbols.Wire);
                d.Line(Layers.Ac, 332, yMain, XBmpt + 4, yMain, Symbols.Wire);
                Symbols.Meter(d, 318, yMain - 5, 14, 10, false);
                d.Text(325, yMain - 7, "Contor producție", 1.6, TextAnchor.Middle);
            }

            if (smart != null)
            {
                Symbols.Clamp(d, 340, yMain);
                d.Line(Layers.Comm, 340, yMain - 1.8, 340, yMain - 16, 0.3, dashed: true);
                d.Rect(Layers.Comm, 333, yMain - 25, 14, 9, 0.4, "#FFFFFF");
                d.Text(340, yMain - 19.4, "SM", 2.4, TextAnchor.Middle, bold: true);
                d.Text(340, yMain + 5, "Smart meter", 1.6, TextAnchor.Middle);
                d.Text(340, yMain + 7.5, smart, 1.3, TextAnchor.Middle);
            }

            Symbols.Board(d, XTg, yMain - 14, 22, 28, "TG", "existent");
            d.Line(Layers.Ac, XTg + 11, yMain + 14, XTg + 11, yMain + 24, 0.35);
            Symbols.ArrowDown(d, Layers.Ac, XTg + 11, yMain + 24);
            d.Text(XTg + 11, yMain + 28, "Consumatori", 1.7, TextAnchor.Middle);

            Symbols.Board(d, XBmpt, yMain - 18, 30, 36, "BMPT");
            Symbols.Meter(d, XBmpt + 4, yMain - 5, 16, 10, true);
            d.Path(Layers.Ac, Symbols.Wire, new Pt(XBmpt + 20, yMain), new Pt(XBmpt + 25, yMain), new Pt(XBmpt + 25, yMain - 30));
            d.Path(Layers.Ac, 0.35, new Pt(XBmpt + 23.5, yMain - 27), new Pt(XBmpt + 25, yMain - 30), new Pt(XBmpt + 26.5, yMain - 27));
            d.Text(XBmpt + 20, yMain - 32, "Rețea OD 0,4 kV", 1.9, TextAnchor.Middle, bold: true);
            d.Text(XBmpt + 15, yMain + 22, "Contor bidirecțional", 1.6, TextAnchor.Middle);
            d.Text(XBmpt + 15, yMain + 24.7, "POD: " + Or(p.POD), 1.5, TextAnchor.Middle);
        }

        private static void DrawBattery(Drawing d, ProiectFotovoltaice p, Row row, double y)
        {
            double cx = XInv + WInv / 2;
            d.Line(Layers.DcPlus, cx - 4, row.InvBottom, cx - 4, y, 0.35);
            d.Line(Layers.DcMinus, cx + 4, row.InvBottom, cx + 4, y, 0.35);
            Symbols.Battery(d, XInv + 3, y, WInv - 6, 16);
            var lines = new List<string> { "Baterie de stocare" };
            foreach (var b in p.DateBaterii ?? new())
            {
                lines.Add($"{b.NrBaterii} x {Or(b.Model)}");
                lines.Add($"{Num(b.PutereInstalata)} kW, {Num(b.Capacitate)} Ah");
            }
            for (int i = 0; i < lines.Count; i++)
                d.Text(cx, y + 20 + i * 2.5, lines[i], 1.7, TextAnchor.Middle, bold: i == 0);
        }

        private static void DrawBackup(Drawing d, string backup, Row row, double y)
        {
            d.Path(Layers.Ac, Symbols.Wire, new Pt(XInv + WInv, row.InvBottom - 3), new Pt(206, row.InvBottom - 3), new Pt(206, y + 7), new Pt(XTe, y + 7));
            Symbols.Board(d, XTe, y, 34, 14, "Modul backup");
            d.Text(XTe + 2, y + 10.5, backup, 1.6);
            d.Line(Layers.Ac, XTe + 34, y + 7, XTe + 44, y + 7, Symbols.Wire);
            Symbols.ArrowRight(d, Layers.Ac, XTe + 44, y + 7);
            Symbols.Board(d, XTe + 44, y, 42, 14, "Consumatori critici", "alimentați în regim backup");
        }

        internal static void DrawFooter(Drawing d, ProiectFotovoltaice p)
        {
            double y = 230;
            Symbols.Table(d, 15, y, new double[] { 32, 62, 12, 26, 22 }, 5, d.Table, 1.7);
            double ty = y + d.Table.Count * 5 + 4.5;
            d.TextLines(15, ty, EquipmentTables.FotovoltaiceSummary(p), 1.9, spacing: 1.5);
            Symbols.Legend(d, 176, y + 3, new[]
            {
                (Layers.DcPlus, "Curent continuu +"), (Layers.DcMinus, "Curent continuu -"),
                (Layers.Ac, "Curent alternativ"), (Layers.Pe, "Împământare / PE"), (Layers.Comm, "Comunicație"),
            });
        }
    }
}
