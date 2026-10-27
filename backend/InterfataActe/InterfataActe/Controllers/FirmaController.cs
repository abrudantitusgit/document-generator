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
    [Route(template: "firma")]
    [EnableCors]
    [ApiController]
    public class FirmaController:ControllerBase
    {
        public readonly IFirmaRepository _firma;
        private readonly InterfataActeBrocker _database;

        public FirmaController(IFirmaRepository firma, InterfataActeBrocker database)
        {
            _firma = firma;
            _database = database;
        }


        [HttpPost(template: "create")]
        public IActionResult CreateFirma(Firma firma)
        {
            return Ok(_firma.CreateFirma(firma));
        }

        [HttpPost(template:"modify")]
        public IActionResult ModifyFirma(Firma firma)
        {
            return Ok(_firma.ModifyFirma(firma));
        }

        [HttpGet(template:"list")]
        public IActionResult ListFirma()
        {
            return Ok(_firma.GetAllFirme());
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteFirma(Guid id)
        {
            var firma = _database.Firme.Find(id);
            if (firma == null) return NotFound(new { message = "Compania nu a fost găsită." });
            try
            {
                _database.Firme.Remove(firma);
                _database.SaveChanges();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Compania este folosită de proiecte și nu poate fi ștearsă." });
            }
        }


    }
}
