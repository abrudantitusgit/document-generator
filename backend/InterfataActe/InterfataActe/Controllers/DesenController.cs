using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InterfataActe.Drawings;
using InterfataActe.Drawings.Core;
using InterfataActe.Drawings.Model;
using InterfataActe.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InterfataActe.Controllers
{
    [ApiController]
    [Route("desene")]
    public class DesenController : ControllerBase
    {
        private const string GrupBransament = "Bransament";

        private readonly DrawingService _drawings;
        private readonly IProiectFotovoltaiceRepository _fotovoltaice;
        private readonly IProiectRepository _proiecte;

        public DesenController(DrawingService drawings, IProiectFotovoltaiceRepository fotovoltaice, IProiectRepository proiecte)
        {
            _drawings = drawings;
            _fotovoltaice = fotovoltaice;
            _proiecte = proiecte;
        }

        // GET: desene/usecases
        [HttpGet("usecases")]
        public IActionResult UseCases() => Ok(new
        {
            formats = _drawings.Formats,
            useCases = DrawingUseCaseResolver.Catalog.Select(c => new { code = c.Code, family = c.Family, description = c.Description }),
        });

        // GET: desene/fotovoltaice/{id}
        [HttpGet("fotovoltaice/{id:guid}")]
        public async Task<IActionResult> Fotovoltaice(Guid id, CancellationToken ct)
        {
            var proiect = await _fotovoltaice.GetByIdAsync(id, true, ct);
            if (proiect is null) return NotFound();
            return Ok(Describe(_drawings.UseCase(proiect), _drawings.Build(proiect)));
        }

        // POST: desene/fotovoltaice/{id}?overwrite=false
        [HttpPost("fotovoltaice/{id:guid}")]
        public async Task<IActionResult> GenerateFotovoltaice(Guid id, [FromQuery] bool overwrite = false, CancellationToken ct = default)
        {
            var proiect = await _fotovoltaice.GetByIdAsync(id, true, ct);
            if (proiect is null) return NotFound();
            var result = _drawings.GenerateForFotovoltaice(proiect, DrawingService.FotovoltaiceFolder(proiect), overwrite);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: desene/fotovoltaice/{id}/E1/pdf
        [HttpGet("fotovoltaice/{id:guid}/{code}/{format}")]
        public async Task<IActionResult> RenderFotovoltaice(Guid id, string code, string format, CancellationToken ct)
        {
            var proiect = await _fotovoltaice.GetByIdAsync(id, true, ct);
            if (proiect is null) return NotFound();
            return Render(_drawings.Build(proiect).FirstOrDefault(d => d.Code == code), format);
        }

        // GET: desene/bransament/{id}
        [HttpGet("bransament/{id:guid}")]
        public IActionResult Bransament(Guid id)
        {
            var proiect = _proiecte.GetProiectByIdInclude(id);
            if (proiect is null) return NotFound();
            return Ok(Describe(_drawings.UseCase(proiect), _drawings.Build(proiect)));
        }

        // POST: desene/bransament/{id}?overwrite=false
        [HttpPost("bransament/{id:guid}")]
        public IActionResult GenerateBransament(Guid id, [FromQuery] bool overwrite = false)
        {
            var proiect = _proiecte.GetProiectByIdInclude(id);
            if (proiect is null) return NotFound();
            var result = _drawings.GenerateForBransament(proiect, DrawingService.BransamentFolder(proiect, GrupBransament), overwrite);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // GET: desene/bransament/{id}/E1/pdf
        [HttpGet("bransament/{id:guid}/{code}/{format}")]
        public IActionResult RenderBransament(Guid id, string code, string format)
        {
            var proiect = _proiecte.GetProiectByIdInclude(id);
            if (proiect is null) return NotFound();
            return Render(_drawings.Build(proiect).FirstOrDefault(d => d.Code == code), format);
        }

        private object Describe(DrawingUseCase uc, System.Collections.Generic.List<Drawing> drawings) => new
        {
            useCase = uc.Code,
            description = uc.Description,
            formats = _drawings.Formats,
            drawings = drawings.Select(d => new { code = d.Code, title = d.Title, fileName = d.FileName }),
        };

        private IActionResult Render(Drawing drawing, string format)
        {
            if (drawing is null) return NotFound();
            format = format.ToLowerInvariant();
            if (!_drawings.Formats.Contains(format)) return BadRequest($"Format necunoscut: {format}");
            var content = _drawings.Render(drawing, format);
            // inline for browser-viewable formats, attachment for the rest
            bool inline = format is "pdf" or "svg" or "png";
            var fileName = $"{drawing.FileName}.{format}";
            return inline
                ? File(content, _drawings.ContentType(format))
                : File(content, _drawings.ContentType(format), fileName);
        }
    }
}
