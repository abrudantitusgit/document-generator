using System;
using System.Collections.Generic;
using System.Globalization;
using InterfataActe.Models;

namespace InterfataActe.Drawings.Model
{
    // Data shared by every drawing of a project (title block, headings).
    public class DrawingContext
    {
        public string Beneficiar { get; set; } = "-";
        public string Adresa { get; set; } = "-";
        public string NrProiect { get; set; } = "-";
        public string Data { get; set; } = "-";
        public string Lucrare { get; set; } = "-";
        public string Putere { get; set; } = "";

        public static string V(string value) =>
            string.IsNullOrWhiteSpace(value) || value.Trim() == "-" ? null : value.Trim();

        public static string Or(string value, string fallback = "-") => V(value) ?? fallback;

        public static string Num(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        public static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        public static string Address(string judet, string localitatea, string comuna, string strada, string nrStrada,
            string bloc, string scara, string etaj, string apartament, string sector)
        {
            var parts = new List<string>();
            void Add(string prefix, string value)
            {
                if (V(value) != null) parts.Add(prefix + V(value));
            }
            Add("Com. ", comuna);
            Add("Loc. ", localitatea);
            Add("Sector ", sector);
            Add("Str. ", strada);
            Add("Nr. ", nrStrada);
            Add("Bl. ", bloc);
            Add("Sc. ", scara);
            Add("Et. ", etaj);
            Add("Ap. ", apartament);
            Add("Jud. ", judet);
            return parts.Count == 0 ? "-" : string.Join(", ", parts);
        }

        private static string Date(DateTime data) => data == default ? DateTime.Now.ToString("MM.yyyy") : data.ToString("MM.yyyy");

        public static DrawingContext From(ProiectFotovoltaice p)
        {
            var kwp = p.PutereTotalaPanouri / 1000;
            return new DrawingContext
            {
                // Same PF/PJ rule as the documents: the company is the beneficiary when present.
                Beneficiar = Or(p.Firma?.Nume ?? p.Client?.Nume),
                Adresa = Address(p.Judet, p.Localitatea, p.Comuna, p.Strada, p.NrStrada, p.Bloc, p.Scara, p.Etaj, p.Apartament, p.Sector),
                NrProiect = p.NrProiect.ToString(CultureInfo.InvariantCulture),
                Data = Date(p.Data),
                Lucrare = $"Instalație de utilizare prosumator - sistem fotovoltaic {Num(kwp)} kWp",
                Putere = $"P = {Num(kwp)} kWp / {Num(p.PutereTotalaInvertor)} kW",
            };
        }

        public static DrawingContext From(Proiect p)
        {
            return new DrawingContext
            {
                Beneficiar = Or(p.Firma?.Nume ?? p.Client?.Nume),
                Adresa = Address(p.Judet, p.Localitatea, p.Comuna, p.Strada, p.NrStrada, p.Bloc, p.Scara, p.Etaj, p.Apartament, p.Sector),
                NrProiect = p.NrProiect.ToString(CultureInfo.InvariantCulture),
                Data = Date(p.Data),
                Lucrare = Or(p.Denumire, "Branșament electric"),
                Putere = $"Pi = {Num(p.Pi)} kW, Pa = {Num(p.Pa)} kW",
            };
        }
    }
}
