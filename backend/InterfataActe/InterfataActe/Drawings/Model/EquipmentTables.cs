using System.Collections.Generic;
using System.Linq;
using InterfataActe.Models;
using static InterfataActe.Drawings.Model.DrawingContext;

namespace InterfataActe.Drawings.Model
{
    // Tables printed on the sheets and repeated as native tables in the DOCX export.
    public static class EquipmentTables
    {
        public static List<string[]> Fotovoltaice(ProiectFotovoltaice p)
        {
            bool tri = p.TipSistem == "Trifazat";
            var rows = new List<string[]> { new[] { "Echipament", "Model", "Buc.", "Putere unitară", "Total" } };
            foreach (var g in p.DatePanouri ?? new())
                rows.Add(new[] { "Panouri fotovoltaice", Or(g.Model), g.NrPanouri.ToString(), $"{Num(g.PutereInstalata)} W", $"{Num(g.NrPanouri * g.PutereInstalata / 1000)} kWp" });
            foreach (var i in p.DateInvertoare ?? new())
                rows.Add(new[] { $"Invertor {(tri ? "trifazat" : "monofazat")}", Or(i.Model), i.NrInvertoare.ToString(), $"{Num(i.PutereInstalata)} kW", $"{Num(i.NrInvertoare * i.PutereInstalata)} kW" });
            if (p.CapacitateTotalaBaterii != 0)
                foreach (var b in p.DateBaterii ?? new())
                    rows.Add(new[] { "Baterie de stocare", Or(b.Model), b.NrBaterii.ToString(), $"{Num(b.PutereInstalata)} kW, {Num(b.Capacitate)} Ah", $"{Num(b.NrBaterii * b.Capacitate)} Ah" });
            if (V(p.SmartMeter) != null) rows.Add(new[] { "Smart meter", V(p.SmartMeter), "1", "-", "-" });
            if (V(p.Backup) != null) rows.Add(new[] { "Modul backup", V(p.Backup), "1", "-", "-" });
            return rows;
        }

        public static string[] FotovoltaiceSummary(ProiectFotovoltaice p)
        {
            bool tri = p.TipSistem == "Trifazat";
            var kwp = p.PutereTotalaPanouri / 1000;
            var max = System.Math.Min(p.PutereTotalaInvertor, kwp);
            return new[]
            {
                $"Putere instalată panouri: {Num(kwp)} kWp   Putere invertoare: {Num(p.PutereTotalaInvertor)} kW   Putere maximă evacuată: {Num(max)} kW",
                $"Racord: {(tri ? "trifazat 400 V" : "monofazat 230 V")}   POD: {Or(p.POD)}   Amplasare: {Or(p.Amplasare)}",
            };
        }

        public static List<string[]> Circuite(IReadOnlyList<Circuit> circuits)
        {
            var rows = new List<string[]> { new[] { "Circuit", "Destinație", "Protecție", "Cablu", "Putere estimată", "Tensiune" } };
            for (int i = 0; i < circuits.Count; i++)
            {
                var c = circuits[i];
                rows.Add(new[] { $"C{i + 1}", c.Destinatie, c.Protectie, c.Cablu, c.PutereKw > 0 ? $"{Num(c.PutereKw)} kW" : "-", c.Tensiune });
            }
            return rows;
        }

        public static List<string[]> Racordare(Proiect p, DrawingUseCase uc)
        {
            var a = p.Atr;
            return new List<string[]>
            {
                new[] { "Element", "Valoare" },
                new[] { "ATR", a == null ? "-" : $"nr. {a.NrAtr} / {a.DataAtr:dd.MM.yyyy}" },
                new[] { "Punct de racordare", Or(a?.PunctRacordare) },
                new[] { "Punct de alimentare", Or(a?.PunctAlimentare) },
                new[] { "Punct de branșament", Or(a?.PunctBransament, $"BMPT {uc.BmptRating}A") },
                new[] { "Cablu branșament", $"{Or(a?.Cablu)}, L = {(a == null ? "-" : a.LungimeCablu.ToString())} m" },
                new[] { "Coloană", $"{uc.Preset.Coloana}, L = {uc.Preset.ColoanaLungime} m" },
                new[] { "Contor", Or(a?.Contor) },
                new[] { "Priză de pământ", $"Rp < {(a == null || a.Ohmi == 0 ? 4 : a.Ohmi)} Ω" },
                new[] { "Puteri", $"Pi = {Num(p.Pi)} kW, Pa = {Num(p.Pa)} kW" + (V(p.TensiuneUtilizare) != null ? $", {V(p.TensiuneUtilizare)} V" : "") },
            };
        }
    }
}
