using System;
using System.IO;
using InterfataActe.Drawings.Core;
using SkiaSharp;

namespace InterfataActe.Drawings.Exporters
{
    public class PdfExporter : IDrawingExporter
    {
        private const float PointsPerMm = 72f / 25.4f;
        private readonly SkiaRenderer _renderer;

        public PdfExporter(SkiaRenderer renderer) => _renderer = renderer;

        public string Extension => "pdf";
        public string ContentType => "application/pdf";

        public void Export(Drawing d, Stream output)
        {
            var metadata = new SKDocumentPdfMetadata
            {
                Title = d.Title,
                Subject = d.UseCase,
                Author = d.Author,
                Creator = "InterfataActe - generatedDraws",
                Creation = DateTime.Now,
                RasterDpi = 300,
            };
            using var managed = new SKManagedWStream(output);
            using (var doc = SKDocument.CreatePdf(managed, metadata))
            {
                var canvas = doc.BeginPage((float)d.Width * PointsPerMm, (float)d.Height * PointsPerMm);
                _renderer.Render(d, canvas, PointsPerMm);
                doc.EndPage();
                doc.Close();
            }
            managed.Flush();
        }
    }
}
