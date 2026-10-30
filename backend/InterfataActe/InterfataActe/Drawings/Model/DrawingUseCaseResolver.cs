using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using InterfataActe.Models;

namespace InterfataActe.Drawings.Model
{
    public record DrawingUseCase(string Code, string Family, string Description, bool Trifazat)
    {
        public bool Baterie { get; init; }
        public int InverterUnits { get; init; } = 1;
        public int BmptRating { get; init; }
        public CircuitPreset Preset { get; init; }
    }

    // Picks the drawing set for a project, in the same spirit as Helpers/TemplateResolver.
    public static class DrawingUseCaseResolver
    {
        public const string Fotovoltaice = "fotovoltaice";
        public const string Bransamente = "bransamente";

        public static readonly IReadOnlyList<(string Code, string Family, string Description)> Catalog = new[]
        {
            ("PV-MONO", Fotovoltaice, "Sistem fotovoltaic monofazat, fără baterie, un invertor"),
            ("PV-MONO-BAT", Fotovoltaice, "Sistem fotovoltaic monofazat cu baterie de stocare"),
            ("PV-TRIF", Fotovoltaice, "Sistem fotovoltaic trifazat, fără baterie, un invertor"),
            ("PV-TRIF-BAT", Fotovoltaice, "Sistem fotovoltaic trifazat cu baterie de stocare"),
            ("PV-MULTI", Fotovoltaice, "Sistem fotovoltaic cu mai multe invertoare (TE CEF comun, contactor K)"),
            ("BR-MONO", Bransamente, "Branșament monofazat - instalație de utilizare monofazată"),
            ("BR-TRIF-16A", Bransamente, "Branșament trifazat BMPT 16A"),
            ("BR-TRIF-20A", Bransamente, "Branșament trifazat BMPT 20A"),
            ("BR-TRIF-25A", Bransamente, "Branșament trifazat BMPT 25A"),
            ("BR-TRIF-32A", Bransamente, "Branșament trifazat BMPT 32A (și peste)"),
            ("BR-OS", Bransamente, "Branșament pentru organizare de șantier"),
        };

        public static DrawingUseCase Resolve(ProiectFotovoltaice p)
        {
            bool trifazat = p.TipSistem == "Trifazat";
            bool baterie = p.CapacitateTotalaBaterii != 0;
            int units = (p.DateInvertoare ?? new()).Sum(i => System.Math.Max(1, i.NrInvertoare));

            string code = units >= 2 ? "PV-MULTI" : (trifazat ? "PV-TRIF" : "PV-MONO") + (baterie ? "-BAT" : "");
            return new DrawingUseCase(code, Fotovoltaice, Describe(code), trifazat)
            {
                Baterie = baterie,
                InverterUnits = System.Math.Max(1, units),
            };
        }

        public static DrawingUseCase Resolve(Proiect p)
        {
            var denumire = Plain(p.Denumire);
            var punct = Plain(p.Atr?.PunctBransament);
            var tensiune = Plain(p.TensiuneUtilizare);

            if (denumire.Contains("organizare"))
            {
                int osRating = Rating(punct) ?? ElectricalSizing.BransamentRating(p.Pi, true);
                return Build("BR-OS", true, osRating, CircuitPresets.OrganizareSantier);
            }

            bool trifazat;
            if (punct.Contains("bmpm") || punct.Contains("mono")) trifazat = false;
            else if (punct.Contains("bmpt") || punct.Contains("trif")) trifazat = true;
            else if (denumire.Contains("monofazat")) trifazat = false;
            else if (denumire.Contains("trifazat")) trifazat = true;
            else if (tensiune.Length > 0) trifazat = tensiune.Contains("400") || tensiune.Contains("380");
            else trifazat = true;

            int rating = Rating(punct) ?? ElectricalSizing.BransamentRating(p.Pi, trifazat);
            if (!trifazat) return Build("BR-MONO", false, rating, CircuitPresets.Mono);

            var preset = CircuitPresets.ForRating(rating);
            return Build(preset.Code, true, rating, preset);
        }

        private static DrawingUseCase Build(string code, bool trifazat, int rating, CircuitPreset preset) =>
            new(code, Bransamente, Describe(code), trifazat) { BmptRating = rating, Preset = preset };

        private static string Describe(string code) => Catalog.First(c => c.Code == code).Description;

        private static int? Rating(string punctBransament)
        {
            var m = Regex.Match(punctBransament, @"(\d{2,3})\s*a\b");
            return m.Success ? int.Parse(m.Groups[1].Value) : null;
        }

        // Lower-case without Romanian diacritics (no ICU dependency, the API may run in invariant mode).
        public static string Plain(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            var map = new Dictionary<char, char> { ['ă'] = 'a', ['â'] = 'a', ['î'] = 'i', ['ș'] = 's', ['ş'] = 's', ['ț'] = 't', ['ţ'] = 't' };
            return new string(value.ToLowerInvariant().Select(c => map.TryGetValue(c, out var r) ? r : c).ToArray());
        }
    }
}
