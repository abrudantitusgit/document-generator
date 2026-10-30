using System;
using System.Linq;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Model;
using InterfataActe.Models;
using static InterfataActe.Drawings.Model.DrawingContext;

namespace InterfataActe.Drawings.Generators
{
    // E1 for branșament projects: BMPT -> coloană -> TG with DPST/descărcător -> user circuits.
    // Content follows exampleDraws/Schema_monofilara_BMPT_*.pdf, layout follows the CAD sheets
    // (Monofilara hala): vertical circuit drops above a circuit table.
    public static class BransamentSingleLineGenerator
    {
        public const string Code = "E1";
        public const string FileName = "E1_Schema_electrica_monofilara";
        public const string Title = "Schema electrică monofilară";

        private const double XFeed = 90, XBus = 74, YBusL = 124, YBusN = 130, YBusPe = 136, XCircuits = 112, YTable = 212;

        public static Drawing Generate(Proiect p, DrawingUseCase uc, DrawingOptions o)
        {
            var d = new Drawing(Code, FileName, Title) { UseCase = uc.Code };
            var preset = uc.Preset;
            var circuits = preset.Circuits;
            var atr = p.Atr;
            bool tri = uc.Trifazat;
            string poles = tri ? "3P+N" : "1P+N";
            string punct = Or(atr?.PunctBransament, $"{(tri ? "BMPT" : "BMPM")} {uc.BmptRating}A");
            int ohmi = atr == null || atr.Ohmi == 0 ? 4 : atr.Ohmi;

            d.Text(20, 22, tri ? "SCHEMA ELECTRICĂ MONOFILARĂ" : "SCHEMA MONOFILARĂ INSTALAȚIA DE UTILIZARE", 5, bold: true);
            d.Text(20, 28.5, $"Tablou electric general (TG) - {uc.Description}", 2.4);

            // Installed / absorbed power marker
            d.Circle(Layers.Mark, 382, 46, 14, 0.5);
            d.Text(382, 44, $"Pi = {Num(p.Pi)} kW", 2.6, TextAnchor.Middle);
            d.Text(382, 50, $"Pa = {Num(p.Pa)} kW", 2.6, TextAnchor.Middle);

            // Supply: BMPT and the column cable down to the main breaker
            d.Rect(Layers.Equip, XFeed - 30, 36, 60, 14, 0.45, "#FFFFFF");
            d.Text(XFeed, 42, punct, 2.6, TextAnchor.Middle, bold: true);
            d.Text(XFeed, 47, "Contor: " + Or(atr?.Contor), 1.8, TextAnchor.Middle);
            d.Line(Layers.L1, XFeed, 50, XFeed, 96, Symbols.Wire);
            Symbols.Ticks(d, Layers.L1, XFeed, 58, ElectricalSizing.Conductors(tri), vertical: true);
            d.Text(XFeed + 4, 93, $"{preset.Coloana}, L = {preset.ColoanaLungime} ML", 1.9, rotation: 90);

            d.Line(Layers.L1, XFeed, 70, 68, 70, 0.35);
            Symbols.SpdDown(d, 68, 70, "Descărcător JT", Layers.L1);

            Symbols.BreakerV(d, Layers.L1, XFeed, 96, false);
            d.Text(XFeed - 5, 102, $"Q0 - {poles} {uc.BmptRating}A", 1.9, TextAnchor.End);
            d.Text(XFeed - 5, 104.8, "cu declanșator ASA 230V", 1.6, TextAnchor.End);

            d.Line(Layers.L1, XFeed, 110, XFeed, YBusL, Symbols.Wire);
            d.Line(Layers.L1, XFeed, 116, 40, 116, 0.35);
            Symbols.SpdDown(d, 40, 116, "DPST", Layers.L1);
            d.Text(20, 113.5, "dispozitiv de protecție la supratensiune", 1.7);

            // Busbars
            int n = circuits.Count;
            double spacing = Math.Min(40, (398 - XCircuits) / n);
            double busEnd = XCircuits + n * spacing + 4;
            d.Line(Layers.L1, XBus, YBusL, busEnd, YBusL, 0.6);
            d.Line(Layers.N, XFeed, YBusN, busEnd, YBusN, 0.6);
            d.Line(Layers.Pe, XBus, YBusPe, busEnd, YBusPe, 0.6);
            d.Dot(Layers.L1, XFeed, YBusL);
            d.Text(XBus - 2, YBusL + 0.7, tri ? "R,S,T - 400/230 V" : "F - 230 V", 1.9, TextAnchor.End, layer: Layers.L1);
            d.Text(XFeed - 2, YBusN + 0.7, "N", 1.9, TextAnchor.End, layer: Layers.N);
            d.Text(XBus - 2, YBusPe + 0.7, "PE", 1.9, TextAnchor.End, layer: Layers.Pe);

            // Earthing of the PE bar
            d.Line(Layers.Pe, XBus + 2, YBusPe, XBus + 2, 160, 0.4);
            Symbols.Ground(d, XBus + 2, 160, $"Rp < {ohmi} Ω");

            for (int i = 0; i < n; i++)
            {
                var c = circuits[i];
                double x = XCircuits + i * spacing + spacing / 2;
                d.Dot(Layers.L1, x, YBusL);
                d.Line(Layers.L1, x, YBusL, x, 142, Symbols.Wire);
                d.Dot(Layers.N, x + 2.5, YBusN);
                d.Line(Layers.N, x + 2.5, YBusN, x + 2.5, 140, 0.2);
                d.Dot(Layers.Pe, x + 4.5, YBusPe);
                d.Line(Layers.Pe, x + 4.5, YBusPe, x + 4.5, 140, 0.2);

                Symbols.BreakerV(d, Layers.L1, x, 142, true);
                d.Text(x - 4.5, 148, $"Q{i + 1}", 2, TextAnchor.End, bold: true);
                d.Text(x + 5, 148, $"{(c.Trifazat ? "3P+N" : "1P+N")} {c.Breaker}A", 1.7);
                d.Text(x + 5, 150.6, "30 mA", 1.7);

                d.Line(Layers.L1, x, 156, x, 203, Symbols.Wire);
                Symbols.Ticks(d, Layers.L1, x, 166, ElectricalSizing.Conductors(c.Trifazat), vertical: true);
                d.Text(x - 1.5, 200, c.Cablu, 1.8, rotation: 90);
                if (c.Iluminat) Symbols.Lamp(d, x, 205);
                else Symbols.ArrowDown(d, Layers.L1, x, 205);
            }

            // Circuit table aligned with the drops
            var table = new[]
            {
                new[] { "Nr. crt" }.Concat(circuits.Select((_, i) => $"C{i + 1}")).ToArray(),
                new[] { "Destinație" }.Concat(circuits.Select(c => c.Destinatie.Replace("Circuit ", ""))).ToArray(),
                new[] { "Putere" }.Concat(circuits.Select(c => c.PutereKw > 0 ? $"{Num(c.PutereKw)} kW" : "-")).ToArray(),
                new[] { "Tensiune" }.Concat(circuits.Select(c => c.Tensiune)).ToArray(),
                new[] { "Protecție" }.Concat(circuits.Select(c => c.Protectie)).ToArray(),
            };
            var widths = new[] { 22.0 }.Concat(Enumerable.Repeat(spacing, n)).ToArray();
            Symbols.Table(d, XCircuits - 22, YTable, widths, 6, table, 1.7);

            d.TextLines(20, 252, new[]
            {
                "Notă:",
                "Tablourile electrice vor fi cu capac de vizitare și echipate cu siguranțe automate fază-nul.",
                $"Coloana {preset.Coloana}, L = {preset.ColoanaLungime} ML, din {punct}.",
                "Puterile pe circuite sunt estimate și se confirmă la execuție.",
            }, 1.9, spacing: 1.6);

            Symbols.Legend(d, 20, 176, new[] { (Layers.L1, tri ? "Faze R, S, T" : "Fază F"), (Layers.N, "Nul de lucru N"), (Layers.Pe, "Conductor de protecție PE") });

            d.Table.AddRange(EquipmentTables.Circuite(circuits));
            TitleBlock.Draw(d, From(p), o, Title);
            return d;
        }
    }
}
