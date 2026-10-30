using System;
using System.Linq;

namespace InterfataActe.Drawings.Model
{
    // Simplified sizing tables used to label protection and cables on the drawings.
    public static class ElectricalSizing
    {
        public static readonly int[] Ratings = { 10, 13, 16, 20, 25, 32, 40, 50, 63, 80, 100, 125, 160, 200, 250 };
        public static readonly int[] BransamentRatings = { 16, 20, 25, 32, 40, 50, 63 };

        public static double Current(double kw, bool trifazat, double cosPhi = 1.0) =>
            kw * 1000 / (trifazat ? Math.Sqrt(3) * 400 * cosPhi : 230 * cosPhi);

        public static int Breaker(double current) => Ratings.FirstOrDefault(r => r >= current * 1.25, Ratings[^1]);

        public static int InverterBreaker(double kw, bool trifazat) => Breaker(Current(kw, trifazat));

        // Copper section (mm²) for a given protection rating.
        public static double CuSection(int breaker) => breaker switch
        {
            <= 16 => 2.5,
            <= 25 => 4,
            <= 32 => 6,
            <= 50 => 10,
            <= 63 => 16,
            <= 80 => 25,
            <= 100 => 35,
            <= 125 => 50,
            <= 160 => 70,
            _ => 95,
        };

        public static string AcCable(int breaker, bool trifazat) =>
            $"CYY-F {(trifazat ? 5 : 3)}x{DrawingContext.Num(CuSection(breaker))} mmp";

        public static int Conductors(bool trifazat) => trifazat ? 5 : 3;

        public static int BransamentRating(double piKw, bool trifazat)
        {
            var current = Current(piKw, trifazat, 0.92);
            return BransamentRatings.FirstOrDefault(r => r >= current, BransamentRatings[^1]);
        }
    }
}
