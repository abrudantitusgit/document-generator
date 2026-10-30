using System.Globalization;
using System.IO;
using InterfataActe.Drawings.Core;

namespace InterfataActe.Drawings.Exporters
{
    public interface IDrawingExporter
    {
        string Extension { get; }
        string ContentType { get; }
        void Export(Drawing drawing, Stream output);
    }

    internal static class Fmt
    {
        public static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
