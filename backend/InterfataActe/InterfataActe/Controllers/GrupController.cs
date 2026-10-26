using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.Interfaces;
using InterfataActe.Models;
using InterfataActe.Brockers;

namespace InterfataActe.Controllers
{
    [Route(template: "grup")]
    [EnableCors]
    [ApiController]
    public class GrupController : Controller
    {
        public readonly IGrupRepository _grup;
        private readonly InterfataActeBrocker _database;

        public GrupController(IGrupRepository grup, InterfataActeBrocker database)
        {
            _grup = grup;
            _database = database;
        }

        [HttpPost(template: "create")]
        public IActionResult CreateGrup(Grup grup)
        {
            return Ok(_grup.CreateGrup(grup));
        }

        [HttpGet(template: "grupe")]
        public IActionResult GetAllGrupe()
        {
            return Ok(_grup.GetAllGrupe());
        }

        [HttpGet("{nume}")]
        public IActionResult GetGrupByNume(string nume)
        {
            return Ok(_grup.GetGrupByNume(nume));
        }
        [HttpGet("id/{id}")]
        public IActionResult GetGrupById(Guid id)
        {
            return Ok(_grup.GetGrupById(id));
        }

        [HttpPut(template: "edit")]
        public IActionResult UpdateGrup(Grup grup)
        {
            return Ok(_grup.UpdateGrup(grup));
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteGrup(Guid id)
        {
            var grup = _database.Grupe.Find(id);
            if (grup == null) return NotFound(new { message = "Grupul nu a fost găsit." });
            _database.Grupe.Remove(grup);
            _database.SaveChanges();
            return NoContent();
        }

    }
}
