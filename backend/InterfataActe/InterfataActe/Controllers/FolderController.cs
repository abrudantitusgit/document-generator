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
    [Route(template:"folder")]
    [EnableCors]
    [ApiController]
    public class FolderController:ControllerBase
    {


        private readonly IFolderRepository _folder;
        private readonly InterfataActeBrocker _database;

        public FolderController(IFolderRepository folder, InterfataActeBrocker database)
        {
            _folder = folder;
            _database = database;
        }


        [HttpPost(template:"create")]
        public IActionResult CreateFolder(Folder folder)
        {
            return Ok(_folder.CreateFolder(folder));
        }


        [HttpGet(template:"foldere")]
        public IActionResult GetAllFoldere()
        {
            return Ok(_folder.GetAllFoldere());
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateFolder(Guid id, Folder input)
        {
            var folder = _database.Foldere.Find(id);
            if (folder == null) return NotFound(new { message = "Folderul nu a fost găsit." });
            folder.Nume = input.Nume;
            folder.GrupID = input.GrupID;
            _database.SaveChanges();
            return Ok(folder);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteFolder(Guid id)
        {
            var folder = _database.Foldere.Find(id);
            if (folder == null) return NotFound(new { message = "Folderul nu a fost găsit." });
            try
            {
                _database.Foldere.Remove(folder);
                _database.SaveChanges();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Folderul conține fișiere și nu poate fi șters." });
            }
        }

    }
}
