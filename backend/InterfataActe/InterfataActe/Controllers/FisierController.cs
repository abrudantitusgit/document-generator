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
    [Route(template: "fisier")]
    [EnableCors]
    [ApiController]
    public class FisierController : ControllerBase
    {

        private readonly IFisierRepository _fisier;
        private readonly InterfataActeBrocker _database;

        public FisierController(IFisierRepository fisier, InterfataActeBrocker database)
        {
            _fisier = fisier;
            _database = database;
        }



        [HttpPost(template:"create")]
        public IActionResult CreateFisier(Fisier fisier)
        {
            return Ok(_fisier.CreateFisier(fisier));
        }

        [HttpGet("list")]
        public IActionResult ListFisiere()
        {
            return Ok(_database.Fisiere.OrderBy(file => file.Nume).ToList());
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateFisier(Guid id, Fisier input)
        {
            var fisier = _database.Fisiere.Find(id);
            if (fisier == null) return NotFound(new { message = "Fișierul nu a fost găsit." });
            fisier.Nume = input.Nume;
            fisier.FolderID = input.FolderID;
            fisier.DenumireContor = input.DenumireContor;
            _database.SaveChanges();
            return Ok(fisier);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteFisier(Guid id)
        {
            var fisier = _database.Fisiere.Find(id);
            if (fisier == null) return NotFound(new { message = "Fișierul nu a fost găsit." });
            _database.Fisiere.Remove(fisier);
            _database.SaveChanges();
            return NoContent();
        }

    }


}
