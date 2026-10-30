using System;
using System.Collections.Generic;
using System.Linq;
using InterfataActe.Models;

namespace InterfataActe.Drawings.Model
{
    public record StringPlan(int Panels, string PanelModel, decimal PanelW);

    public class InverterPlan
    {
        public int Index { get; init; }
        public string Model { get; init; } = "-";
        public decimal Kw { get; init; }
        public string Serie { get; init; }
        public List<StringPlan> Strings { get; } = new();
        public int Panels => Strings.Sum(s => s.Panels);
        public decimal Kwp => Strings.Sum(s => s.Panels * s.PanelW) / 1000;
    }

    // The forms record panels and inverters but not the string wiring, so the drawing proposes
    // one: each panel group is split into balanced strings no longer than the configured maximum,
    // then whole strings go to the inverter unit with the lowest DC/AC loading.
    public static class StringPlanner
    {
        public static List<InverterPlan> Plan(ProiectFotovoltaice p, int maxPerString)
        {
            maxPerString = Math.Max(1, maxPerString);
            var units = new List<InverterPlan>();
            foreach (var inv in p.DateInvertoare ?? new())
                for (int i = 0; i < Math.Max(1, inv.NrInvertoare); i++)
                    units.Add(new InverterPlan { Index = units.Count + 1, Model = DrawingContext.Or(inv.Model), Kw = inv.PutereInstalata, Serie = inv.Serie });
            if (units.Count == 0)
                units.Add(new InverterPlan { Index = 1, Model = "-", Kw = 0 });

            var strings = (p.DatePanouri ?? new())
                .Where(g => g.NrPanouri > 0)
                .SelectMany(g => Split(g.NrPanouri, maxPerString, units.Count).Select(n => new StringPlan(n, DrawingContext.Or(g.Model), g.PutereInstalata)))
                .OrderByDescending(s => s.Panels * s.PanelW)
                .ToList();

            foreach (var s in strings)
            {
                var target = units
                    .OrderBy(u => (u.Kwp + s.Panels * s.PanelW / 1000) / (u.Kw > 0 ? u.Kw : 1))
                    .ThenBy(u => u.Index)
                    .First();
                target.Strings.Add(s);
            }
            foreach (var u in units)
            {
                // keep strings of the same module type together, longest first
                var ordered = u.Strings.OrderBy(s => s.PanelModel).ThenByDescending(s => s.Panels).ToList();
                u.Strings.Clear();
                u.Strings.AddRange(ordered);
            }
            return units;
        }

        // With several inverters the string count is rounded up to a multiple of the unit count
        // (strings stay at least half the maximum length) so that strings spread evenly.
        private static IEnumerable<int> Split(int panels, int max, int units)
        {
            int strings = (panels + max - 1) / max;
            if (units > 1 && strings % units != 0)
            {
                int rounded = (strings / units + 1) * units;
                if (panels / rounded >= Math.Max(1, max / 2)) strings = rounded;
            }
            int size = panels / strings, extra = panels % strings;
            for (int s = 0; s < strings; s++) yield return size + (s < extra ? 1 : 0);
        }

        // "3 stringuri a câte 15 panouri" style summary, grouping identical strings.
        public static IEnumerable<string> Describe(InverterPlan unit) =>
            unit.Strings
                .GroupBy(s => (s.Panels, s.PanelModel, s.PanelW))
                .Select(g => $"{g.Count()} {(g.Count() == 1 ? "string" : "stringuri")} x {g.Key.Panels} panouri {g.Key.PanelModel} {DrawingContext.Num(g.Key.PanelW)} W");
    }
}
