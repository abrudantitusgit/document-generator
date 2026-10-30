using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InterfataActe.DTO;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Exporters;
using InterfataActe.Drawings.Generators;
using InterfataActe.Drawings.Model;
using InterfataActe.Models;
using Microsoft.Extensions.Options;

namespace InterfataActe.Drawings
{
    // Builds the drawings a project needs and writes each one in every configured format.
    public class DrawingService
    {
        public const string FolderName = "generatedDraws";

        private readonly DrawingOptions _options;
        private readonly Dictionary<string, IDrawingExporter> _exporters;

        public DrawingService(IOptions<DrawingOptions> options)
        {
            _options = options.Value ?? new DrawingOptions();
            var renderer = new SkiaRenderer(_options);
            var png = new PngExporter(renderer, _options.PngDpi);
            _exporters = new IDrawingExporter[]
            {
                new PdfExporter(renderer), new DocxExporter(png, _options.PngDpi), new DrawioExporter(),
                new DxfExporter(), new SvgExporter(), png,
            }.ToDictionary(e => e.Extension);
        }

        public IReadOnlyList<string> Formats =>
            _options.Formats.Count == 0
                ? _exporters.Keys.ToList()
                : _options.Formats.Select(f => f.Trim().ToLowerInvariant()).Where(_exporters.ContainsKey).Distinct().ToList();

        public static string FotovoltaiceFolder(ProiectFotovoltaice p) =>
            $"/mnt/share/clientiFotovoltaice/{p.Client?.Nume}/{p.NrProiect}.Proiect/{FolderName}";

        public static string BransamentFolder(Proiect p, string grup) =>
            $"/mnt/share/clientiBransamente/{p.Client?.Nume}/{grup}/{p.NrProiect}.Proiect/{FolderName}";

        public DrawingUseCase UseCase(ProiectFotovoltaice p) => DrawingUseCaseResolver.Resolve(p);

        public DrawingUseCase UseCase(Proiect p) => DrawingUseCaseResolver.Resolve(p);

        public List<Drawing> Build(ProiectFotovoltaice p)
        {
            var uc = UseCase(p);
            return Sign(new List<Drawing>
            {
                PvSingleLineGenerator.Generate(p, uc, _options),
                PvStringsGenerator.Generate(p, uc, _options),
            });
        }

        public List<Drawing> Build(Proiect p)
        {
            var uc = UseCase(p);
            return Sign(new List<Drawing>
            {
                BransamentSingleLineGenerator.Generate(p, uc, _options),
                BransamentRacordareGenerator.Generate(p, uc, _options),
            });
        }

        private List<Drawing> Sign(List<Drawing> drawings)
        {
            foreach (var d in drawings) d.Author = _options.Company.Name;
            return drawings;
        }

        public GenerateResult GenerateForFotovoltaice(ProiectFotovoltaice p, string folder, bool overwrite) =>
            Write(Build(p), folder, overwrite);

        public GenerateResult GenerateForBransament(Proiect p, string folder, bool overwrite) =>
            Write(Build(p), folder, overwrite);

        // Same rule as the documents: an existing file is kept unless overwrite is requested,
        // so drawings edited by hand (e.g. the DXF in AutoCAD) are not lost on regeneration.
        public GenerateResult Write(IEnumerable<Drawing> drawings, string folder, bool overwrite)
        {
            var result = new GenerateResult { Success = true };
            Directory.CreateDirectory(folder);
            foreach (var d in drawings)
                foreach (var format in Formats)
                {
                    var path = Path.Combine(folder, $"{d.FileName}.{format}");
                    if (File.Exists(path) && !overwrite) continue;
                    try
                    {
                        var content = Render(d, format);
                        File.WriteAllBytes(path, content);
                        result.Fisiere.Add(path);
                        Console.WriteLine("DESEN: " + path);
                    }
                    catch (Exception ex)
                    {
                        result.Success = false;
                        result.Messages.Add($"Eroare la generarea desenului {path}: {ex.Message}");
                    }
                }
            return result;
        }

        public byte[] Render(Drawing d, string format)
        {
            if (!_exporters.TryGetValue(format.ToLowerInvariant(), out var exporter))
                throw new ArgumentException($"Format necunoscut: {format}");
            using var ms = new MemoryStream();
            exporter.Export(d, ms);
            return ms.ToArray();
        }

        public string ContentType(string format) =>
            _exporters.TryGetValue(format.ToLowerInvariant(), out var e) ? e.ContentType : "application/octet-stream";
    }
}
