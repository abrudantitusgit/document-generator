using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InterfataActe.Models;
using InterfataActe.Repositories;
using Microsoft.AspNetCore.Mvc;
using InterfataActe.Interfaces;

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
    }
}
