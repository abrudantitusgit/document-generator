using System;
using System.IO;
using InterfataActe.Drawings.Core;
using SkiaSharp;

namespace InterfataActe.Drawings.Exporters
{
    public class PngExporter : IDrawingExporter
    {
        private readonly SkiaRenderer _renderer;
        private readonly int _dpi;

        public PngExporter(SkiaRenderer renderer, int dpi)
        {
            _renderer = renderer;
            _dpi = dpi;
        }

        public string Extension => "png";
        public string ContentType => "image/png";

        public void Export(Drawing d, Stream output) => output.Write(Render(d, _dpi));

        public byte[] Render(Drawing d, int dpi)
        {
            float scale = dpi / 25.4f;
            using var bitmap = new SKBitmap((int)Math.Ceiling(d.Width * scale), (int)Math.Ceiling(d.Height * scale));
            using (var canvas = new SKCanvas(bitmap))
                _renderer.Render(d, canvas, scale);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
