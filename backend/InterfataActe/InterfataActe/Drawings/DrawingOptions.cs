using System;
using System.Collections.Generic;
using System.IO;

namespace InterfataActe.Drawings
{
    // Bound from the "Drawings" section of appsettings.json.
    public class DrawingOptions
    {
        public CompanyInfo Company { get; set; } = new();
        public string SefProiect { get; set; } = "ing. Ovidiu Lupuț";
        public string Proiectat { get; set; } = "ing. Ovidiu Lupuț";
        public string Desenat { get; set; } = "ing. Ovidiu Lupuț";
        public string Verificat { get; set; } = "";
        // Empty means every supported format (pdf, docx, drawio, dxf, svg, png).
        public List<string> Formats { get; set; } = new();
        public int MaxPanouriStringMonofazat { get; set; } = 14;
        public int MaxPanouriStringTrifazat { get; set; } = 22;
        public int PngDpi { get; set; } = 200;
        public string StampImagePath { get; set; } = "";
        public string FontsPath { get; set; } = "";

        public string ResolveFontsPath() =>
            string.IsNullOrWhiteSpace(FontsPath) ? Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts") : FontsPath;
    }

    public class CompanyInfo
    {
        public string Name { get; set; } = "SC Ovinstal Electric SRL";
        public string Address { get; set; } = "Str. Ceyrat Nr. 35A, bloc WR6, ap. SC3, Loc. Oradea, Jud. Bihor";
        public string Registration { get; set; } = "RO 28433799, J05/889/2011";
        public string Phone { get; set; } = "Tel. +40 0743954660";
        public string Email { get; set; } = "office@ovinstalelectric.ro";
        public string Web { get; set; } = "www.ovinstalelectric.ro";
        public string City { get; set; } = "ORADEA";
    }
}
