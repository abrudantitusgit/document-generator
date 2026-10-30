using System.Text.RegularExpressions;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Model;
using InterfataActe.Models;
using static InterfataActe.Drawings.Model.DrawingContext;

namespace InterfataActe.Drawings.Generators
{
    // E2 for branșament projects: stâlp -> cablu subteran -> BMPT at the property limit -> coloană -> TG,
    // the drawing counterpart of the ATR "descriere" sentence.
    public static class BransamentRacordareGenerator
    {
        public const string Code = "E2";
        public const string FileName = "E2_Schema_racordare";
        public const string Title = "Schema de racordare - branșament";

        private const double YCable = 112, YGround = 104, XPole = 52, XBmpt = 172, WBmpt = 56, XTg = 330;

        public static Drawing Generate(Proiect p, DrawingUseCase uc, DrawingOptions o)
        {
            var d = new Drawing(Code, FileName, Title) { UseCase = uc.Code };
            var atr = p.Atr;
            bool tri = uc.Trifazat;
            string punct = Or(atr?.PunctBransament, $"{(tri ? "BMPT" : "BMPM")} {uc.BmptRating}A");
            string cablu = Or(atr?.Cablu);
            string lungime = atr == null ? "-" : atr.LungimeCablu.ToString();
            int ohmi = atr == null || atr.Ohmi == 0 ? 4 : atr.Ohmi;

            d.Text(20, 22, "SCHEMA DE RACORDARE - BRANȘAMENT " + (tri ? "TRIFAZAT" : "MONOFAZAT"), 5, bold: true);
            d.Text(20, 28.5, $"{uc.Description} - punct de racordare: {Or(atr?.PunctRacordare)}", 2.4);

            // Ownership zones and property limit
            double xLimit = XBmpt + WBmpt / 2;
            d.Text(XPole, 44, "Instalație operator de distribuție", 2.4, bold: true);
            d.Text(xLimit + 40, 44, "Instalație de utilizare", 2.4, bold: true);
            d.Line(Layers.Mark, xLimit, 48, xLimit, 172, 0.35, dashed: true);
            d.Text(xLimit + 1.5, 172, "Limită de proprietate", 1.8, rotation: 90, layer: Layers.Mark);

            // Ground level, interrupted by the BMPT and the building (TG)
            foreach (var (from, to) in new[] { (20.0, XBmpt), (XBmpt + WBmpt, XTg - 6) })
            {
                d.Line(Layers.Equip, from, YGround, to, YGround, 0.3);
                for (double hx = from + 3; hx < to; hx += 6) d.Line(Layers.Equip, hx, YGround, hx - 2.5, YGround + 2.5, 0.15);
            }
            d.Text(XTg - 7, YGround - 1.2, "CTN ±0.00", 1.7, TextAnchor.End);

            Symbols.Pole(d, XPole, 70, "Stâlp " + Or(atr?.PunctAlimentare));

            // Underground cable pole -> BMPT
            int conductors = Conductors(cablu, tri ? 4 : 2);
            d.Path(Layers.L1, Symbols.Wire, new Pt(XPole + 1.5, 80), new Pt(XPole + 6, 80), new Pt(XPole + 6, YCable), new Pt(XBmpt, YCable));
            Symbols.Ticks(d, Layers.L1, 110, YCable, conductors);
            d.Text(70, YCable + 6, $"Cablu {cablu}, L = {lungime} m, pozat subteran", 2.1);

            // BMPT at the property limit
            Symbols.Board(d, XBmpt, 84, WBmpt, 46, punct, "postament poliester armat");
            d.Line(Layers.L1, XBmpt, YCable, XBmpt + 4, YCable, Symbols.Wire);
            Symbols.Meter(d, XBmpt + 4, YCable - 5, 14, 10, false);
            d.Line(Layers.L1, XBmpt + 18, YCable, XBmpt + 22, YCable, Symbols.Wire);
            d.Text(XBmpt + 11, YCable + 9, "Contor " + Or(atr?.Contor), 1.6, TextAnchor.Middle);
            Symbols.BreakerH(d, Layers.L1, XBmpt + 22, YCable, $"PD {uc.BmptRating}A");
            d.Line(Layers.L1, XBmpt + 36, YCable, XBmpt + WBmpt, YCable, Symbols.Wire);
            Symbols.SpdDown(d, XBmpt + 44, YCable, "DPS", Layers.L1);
            d.Text(XBmpt + 2, 128, $"DPS + PD + MN, Rp < {ohmi} Ω", 1.7);

            // Column BMPT -> TG
            d.Line(Layers.L1, XBmpt + WBmpt, YCable, XTg, YCable, Symbols.Wire);
            Symbols.Ticks(d, Layers.L1, 268, YCable, ElectricalSizing.Conductors(tri));
            d.Text(240, YCable + 6, $"Coloană {uc.Preset.Coloana}, L = {uc.Preset.ColoanaLungime} m", 2.1);

            Symbols.Board(d, XTg, 92, 36, 40, "TG", "tablou general");
            Symbols.BreakerH(d, Layers.L1, XTg + 4, YCable, $"Q0 {uc.BmptRating}A");
            d.Line(Layers.L1, XTg + 18, YCable, 390, YCable, Symbols.Wire);
            Symbols.ArrowRight(d, Layers.L1, 390, YCable);
            d.Text(392, YCable - 3, "Circuite", 1.8, TextAnchor.End);
            d.Text(XTg + 18, 138, $"Pi = {Num(p.Pi)} kW, Pa = {Num(p.Pa)} kW", 1.9, TextAnchor.Middle);

            // Description (ATR text) and data table
            string descriere = V(atr?.Descriere) ??
                $"De la stâlpul {Or(atr?.PunctAlimentare)} se va realiza un {Or(p.Denumire, "branșament")} subteran cu cablu {cablu} de lungime {lungime} ml până la un {punct}, echipat cu DPS+PD+MN+Rp < {ohmi} Ohmi, montat pe postament din poliester armat cu fibră de sticlă în fundație turnată de beton, amplasat la limita de proprietate.";
            d.Text(20, 164, "Descriere:", 2.2, bold: true);
            d.TextLines(20, 169, Symbols.Wrap(descriere, 190, 2), 2, spacing: 1.5);

            d.Table.AddRange(EquipmentTables.Racordare(p, uc));
            Symbols.Table(d, 20, 196, new double[] { 45, 150 }, 5, d.Table, 1.8);
            TitleBlock.Draw(d, From(p), o, Title);
            return d;
        }

        // Number of conductors read from the cable name ("ACYABY 4x25mmp" -> 4).
        private static int Conductors(string cable, int fallback)
        {
            var m = Regex.Match(cable ?? "", @"(\d)\s*[xX]");
            return m.Success ? int.Parse(m.Groups[1].Value) : fallback;
        }
    }
}
