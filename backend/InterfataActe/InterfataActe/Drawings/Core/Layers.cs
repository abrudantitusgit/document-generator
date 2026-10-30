using System.Collections.Generic;
using System.Linq;

namespace InterfataActe.Drawings.Core
{
    // Aci is the AutoCAD Color Index used in the DXF layer table.
    public sealed record LayerDef(string Name, string Color, int Aci, string Description);

    // All is ordered bottom to top: every exporter stacks layers in this order.
    public static class Layers
    {
        public const string Frame = "CHENAR";
        public const string Text = "TEXT";
        public const string Equip = "ECHIPAMENTE";
        public const string Ac = "AC";
        public const string L1 = "L1";
        public const string L2 = "L2";
        public const string L3 = "L3";
        public const string N = "N";
        public const string Pe = "PE";
        public const string DcPlus = "DC_PLUS";
        public const string DcMinus = "DC_MINUS";
        public const string Comm = "COMUNICATIE";
        public const string Panel = "PANOURI";
        public const string Mark = "MARCAJ";

        public static readonly IReadOnlyList<LayerDef> All = new[]
        {
            new LayerDef(Frame, "#000000", 7, "Chenar si cartus"),
            new LayerDef(Equip, "#000000", 7, "Aparataj si tablouri"),
            new LayerDef(Ac, "#000000", 7, "Circuite curent alternativ (monofilar)"),
            new LayerDef(L1, "#8B4513", 34, "Faza L1 / R"),
            new LayerDef(L2, "#000000", 7, "Faza L2 / S"),
            new LayerDef(L3, "#808080", 8, "Faza L3 / T"),
            new LayerDef(N, "#1565C0", 5, "Nul de lucru"),
            new LayerDef(Pe, "#2E9E3E", 3, "Conductor de protectie"),
            new LayerDef(DcPlus, "#D32F2F", 1, "Curent continuu +"),
            new LayerDef(DcMinus, "#212121", 250, "Curent continuu -"),
            new LayerDef(Comm, "#8E24AA", 6, "Comunicatie / masura"),
            new LayerDef(Panel, "#D32F2F", 1, "Panouri fotovoltaice"),
            new LayerDef(Mark, "#D81B60", 6, "Marcaje"),
            new LayerDef(Text, "#000000", 7, "Texte si cote"),
        };

        private static readonly Dictionary<string, LayerDef> ByName = All.ToDictionary(l => l.Name);

        public static LayerDef Get(string name) => name != null && ByName.TryGetValue(name, out var l) ? l : ByName[Equip];
    }
}
