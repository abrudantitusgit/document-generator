using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.Interfaces;
using InterfataActe.Models;
using InterfataActe.Brockers;
using Microsoft.EntityFrameworkCore;

namespace InterfataActe.Controllers
{
    [Route(template: "proiect")]
    [EnableCors]
    [ApiController]
    public class ProiectController:ControllerBase
    {
        private readonly IProiectRepository _proiect;
        private readonly InterfataActeBrocker _database;

        public ProiectController(IProiectRepository proiect, InterfataActeBrocker database)
        {
            _proiect = proiect;
            _database = database;
        }

        [HttpPost("create")]
        public IActionResult CreateProiect([FromBody] Proiect proiect)
        {
            if (proiect == null) return BadRequest("Body invalid");

            if (proiect.ClientID == Guid.Empty)
                return BadRequest("Lipsa client");

            return Ok(_proiect.CreateProiect(proiect));
        }

        [HttpPost(template:"modify")]
        public IActionResult ModifyProiect(Proiect proiect)
        {
            return Ok(_proiect.ModifyProiect(proiect));
        }

        [HttpGet(template:"list")]
        public IActionResult ListProiecte()
        {
            return Ok(_proiect.GetAllProiecte());
        }

        [HttpGet(template:"proiecteincludenull")]
        public IActionResult GetAllProiecte()
        {
            return Ok(_proiect.GetAllProiecteIncludeNull());
        }

        [HttpGet(template: "proiecteinclude")]
        public IActionResult GetAllProiecteInclude()
        {
            return Ok(_proiect.GetAllProiecteInclude());
        }

        [HttpGet(template:"registru")]
        public IActionResult Registru()
        {
            return Ok(_proiect.GetRegistru());
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteProiect(Guid id)
        {
            var proiect = _database.Proiecte.Find(id);
            if (proiect == null) return NotFound(new { message = "Proiectul nu a fost găsit." });
            try
            {
                _database.Proiecte.Remove(proiect);
                _database.SaveChanges();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Proiectul are documente asociate și nu poate fi șters." });
            }
        }



    }
}
