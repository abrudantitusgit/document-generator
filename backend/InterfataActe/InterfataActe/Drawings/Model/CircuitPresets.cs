using System.Collections.Generic;

namespace InterfataActe.Drawings.Model
{
    public record Circuit(string Destinatie, int Breaker, bool Trifazat, string Cablu, double PutereKw, bool Iluminat = false)
    {
        public string Protectie => $"{(Trifazat ? "3P+N" : "1P+N")} {Breaker}A, 30mA";
        public string Tensiune => Trifazat ? "400 V" : "230 V";
    }

    public record CircuitPreset(string Code, string Coloana, int ColoanaLungime, IReadOnlyList<Circuit> Circuits);

    // Circuits of the standard user installation behind each BMPT, taken from the
    // company's existing schemes (exampleDraws/Schema_monofilara_*.pdf).
    public static class CircuitPresets
    {
        private static Circuit Priza(string name = "Circuit priză") => new(name, 16, false, "MYYM 3x2.5 mmp", 2);
        private static Circuit Iluminat() => new("Circuit iluminat", 10, false, "MYYM 3x1.5 mmp", 1, true);

        public static readonly CircuitPreset Mono = new("BR-MONO", "CYABY-F 3x6 mmp", 15, new[]
        {
            Priza(), Priza(), Iluminat(), new Circuit("Siguranță generală", 25, false, "MYYM 3x2.5 mmp", 0),
        });

        // The scanned 16A template shows 32A breakers on 2.5 mmp sockets; normalised to 16A.
        public static readonly CircuitPreset Bmpt16 = new("BR-TRIF-16A", "CYABY-F 5x10 mmp", 25, new[]
        {
            Priza(), Priza(), Priza(), Iluminat(),
        });

        public static readonly CircuitPreset Bmpt20 = new("BR-TRIF-20A", "CYABY-F 5x10 mmp", 18, new[]
        {
            new Circuit("Circuit plită", 10, true, "MYYM 5x4 mmp", 6),
            new Circuit("Circuit AC", 16, false, "MYYM 3x2.5 mmp", 2.5),
            new Circuit("Circuit mașină spălat", 16, false, "MYYM 3x2.5 mmp", 2.2),
            Iluminat(),
        });

        private static readonly Circuit[] Bmpt25Circuits =
        {
            new("Circuit plită", 16, true, "MYYM 5x2.5 mmp", 7),
            new("Circuit priză trifazică", 16, true, "MYYM 5x2.5 mmp", 6),
            new("Circuit mașină spălat", 16, false, "MYYM 3x2.5 mmp", 2.2),
            Priza("Circuit priză uz general 1"),
            Priza("Circuit priză uz general 2"),
            new("Circuit AC", 16, false, "MYYM 3x2.5 mmp", 2.5),
            Iluminat(),
        };

        public static readonly CircuitPreset Bmpt25 = new("BR-TRIF-25A", "CYABY-F 5x10 mmp", 18, Bmpt25Circuits);
        public static readonly CircuitPreset Bmpt32 = new("BR-TRIF-32A", "CYABY-F 5x10 mmp", 18, Bmpt25Circuits);

        public static readonly CircuitPreset OrganizareSantier = new("BR-OS", "CYY-F 5x6 mmp", 18, new[]
        {
            new Circuit("Circuit priză trifazică", 16, true, "MYYM 5x2.5 mmp", 6),
            Priza("Circuit priză monofazată 1"),
            Priza("Circuit priză monofazată 2"),
        });

        public static CircuitPreset ForRating(int rating) => rating switch
        {
            <= 16 => Bmpt16,
            <= 20 => Bmpt20,
            <= 25 => Bmpt25,
            _ => Bmpt32,
        };
    }
}
