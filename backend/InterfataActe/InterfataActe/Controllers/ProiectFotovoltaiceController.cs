using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InterfataActe.Models;
using InterfataActe.Repositories;
using Microsoft.AspNetCore.Mvc;
using InterfataActe.Interfaces;
using InterfataActe.Helpers;
using ClosedXML.Excel;
using System.IO;
namespace InterfataActe.Controllers
{
    [ApiController]
    [Route("proiectfotovoltaice")]
    public class ProiecteFotovoltaiceController : ControllerBase
    {
        private readonly IProiectFotovoltaiceRepository _repo;

        public ProiecteFotovoltaiceController(IProiectFotovoltaiceRepository repo)
        {
            _repo = repo;
        }


        [HttpGet("sugestii/invertoare")]
        public async Task<ActionResult<List<DataInvertor>>> GetSugestiiInvertoare()
        {
            var items = await _repo.GetSugestiiInvertoare();
            return Ok(items);
        }

        [HttpGet("sugestii/panouri")]
        public async Task<ActionResult<List<DataPanou>>> GetSugestiiPanouri()
        {
            var items = await _repo.GetSugestiiPanouri();
            return Ok(items);
        }

        [HttpGet("sugestii/baterii")]
        public async Task<ActionResult<List<DataBaterie>>> GetSugestiiBaterii()
        {
            var items = await _repo.GetSugestiiBaterii();
            return Ok(items);
        }

        [HttpGet("sugestii/smartmeter")]
        public async Task<ActionResult<List<DataBaterie>>> GetSugestiiSmartMeter()
        {
            var items = await _repo.GetSugestiiSmartMeter();
            return Ok(items);
        }
        [HttpGet("sugestii/backup")]
        public async Task<ActionResult<List<DataBaterie>>> GetSugestiiBackup()
        {
            var items = await _repo.GetSugestiiBackup();
            return Ok(items);
        }

        // GET: proiectfotovoltaice
        [HttpGet]
        public async Task<ActionResult<List<ProiectFotovoltaice>>> GetAll([FromQuery] bool includeChildren = true,CancellationToken ct=default)
        {
            var items = await _repo.GetAllAsync(includeChildren,ct);
            return Ok(items);
        }

        // GET: proiectfotovoltaice/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProiectFotovoltaice>> GetById(Guid id, [FromQuery] bool includeChildren = true, CancellationToken ct = default)
        {
            var item = await _repo.GetByIdAsync(id, includeChildren, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }

        // GET: proiectfotovoltaice/by-client/{clientId}
        [HttpGet("by-client/{clientId:guid}")]
        public async Task<ActionResult<List<ProiectFotovoltaice>>> GetByClient(Guid clientId, CancellationToken ct)
        {
            var items = await _repo.GetByClientIdAsync(clientId, ct);
            return Ok(items);
        }

        // POST: api/ProiecteFotovoltaice
        [HttpPost]
        public async Task<ActionResult<ProiectFotovoltaice>> Create([FromBody] ProiectFotovoltaice model, CancellationToken ct)
        {
            var created = await _repo.AddAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.ID, includeChildren = true }, created);
        }

        // PUT: api/ProiecteFotovoltaice/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProiectFotovoltaice model, CancellationToken ct)
        {
            if (id != model.ID)
                return BadRequest("ID mismatch");

            var updated = await _repo.UpdateAsync(model, ct);
            if (updated is null)
                return NotFound();

            return Ok(updated); // 🔥 frontend primește proiectul
        }

        // DELETE: api/ProiecteFotovoltaice/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var ok = await _repo.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("next-numbers")]
        public async Task<IActionResult> GetNextNumbers(CancellationToken ct)
        {
            var result = await _repo.GetNextNumbersAsync(ct);

            return Ok(new
            {
                nrProiect = result.nextNrProiect,
                nrPV = result.nextNrPV
            });
        }
    [HttpGet("export")]
    public async Task<IActionResult> ExportProjectsToExcel([FromQuery] bool includeChildren = true,CancellationToken ct = default)
    {
        var projects = await _repo.GetAllAsync(includeChildren,ct); // List<ProjectDto>

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Proiecte");
        // Header
        ws.Cell(1, 1).Value = "Nr Proiect";
        ws.Cell(1, 2).Value = "Nr PV";
        ws.Cell(1, 3).Value = "Data";
        ws.Cell(1, 4).Value = "Client";
        ws.Cell(1, 5).Value = "Firma";
        ws.Cell(1, 6).Value = "Tip sistem";
        ws.Cell(1, 7).Value = "POD";
        ws.Cell(1, 8).Value = "Localitate";
        ws.Cell(1, 9).Value = "Comuna";
        ws.Cell(1, 10).Value = "Strada";
        ws.Cell(1, 11).Value = "NrStrada";
        ws.Cell(1, 12).Value = "Apartament";
        ws.Cell(1, 13).Value = "Judet";
        ws.Cell(1, 14).Value = "Putere panouri";
        ws.Cell(1, 15).Value = "Putere invertor";
        ws.Cell(1, 16).Value = "Capacitate baterii";
        ws.Cell(1, 17).Value = "Invertor";
        ws.Cell(1, 18).Value = "Panouri";
        ws.Cell(1, 19).Value = "Baterii";
        ws.Cell(1, 20).Value = "PRM masurat";
        ws.Cell(1, 21).Value = "PRM calculat";

        // Stil header
        var header = ws.Range(1, 1, 1, 20);
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Rows
        int row = 2;
        foreach (var p in projects)
        {
            ws.Cell(row, 1).Value = p.NrProiect;
            ws.Cell(row, 2).Value = p.NrPV;
            ws.Cell(row, 3).Value = p.Data;
            ws.Cell(row, 3).Style.DateFormat.Format = "dd.MM.yyyy";
            ws.Cell(row, 4).Value = p.Client?.Nume ?? "";
            ws.Cell(row, 5).Value = p.Firma?.Nume ?? "";
            ws.Cell(row, 6).Value = p.TipSistem;
            ws.Cell(row, 7).Value = p.POD;
            ws.Cell(row, 8).Value = p.Localitatea;
            ws.Cell(row, 9).Value = p.Comuna;
            ws.Cell(row, 10).Value = p.Strada;
            ws.Cell(row, 11).Value = p.NrStrada;
            ws.Cell(row, 12).Value = p.Apartament;
            ws.Cell(row, 13).Value = p.Judet;
            ws.Cell(row, 14).Value = p.PutereTotalaPanouri/1000;
            ws.Cell(row, 15).Value = p.PutereTotalaInvertor;
            ws.Cell(row, 16).Value = p.CapacitateTotalaBaterii;
            ws.Cell(row, 17).Value = ExportHelpers.BuildInvertor(p.DateInvertoare);
            ws.Cell(row, 17).Style.Alignment.WrapText = true;
            ws.Cell(row, 18).Value = ExportHelpers.BuildPanouri(p.DatePanouri);
            ws.Cell(row, 18).Style.Alignment.WrapText = true;
            ws.Cell(row, 19).Value = ExportHelpers.BuildBaterii(p.DateBaterii);
            ws.Cell(row, 19).Style.Alignment.WrapText = true;
            ws.Cell(row, 20).Value = p.PRM_Masurat;
            ws.Cell(row, 21).Value = p.PRM_Calculat;
            row++;
        }

        ws.Columns().AdjustToContents();
        ws.RangeUsed().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        ws.RangeUsed().Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"proiecteFotovoltaice_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }


    }
}
