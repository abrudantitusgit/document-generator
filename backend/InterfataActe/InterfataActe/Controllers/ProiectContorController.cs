using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.Interfaces;
using InterfataActe.Models;
using InterfataActe.Brockers;
using InterfataActe.DTO;
using Microsoft.EntityFrameworkCore;

namespace InterfataActe.Controllers
{
    [Route(template: "proiectcontor")]
    [EnableCors]
    [ApiController]
    public class ProiectContorController:ControllerBase
    {
        private readonly IProiectRepository _proiect;
        private readonly IProiectContorBuletinCabluRepository _proiectcontorbuletincablu;
        private readonly IProiectContorBuletinPrizaRepository _proiectcontorbuletinpriza;
        private readonly IProiectContorContractRepository _proiectcontorcontract;
        private readonly IProiectContorProcesVerbalRepository _proiectcontorprocesverbal;
        private readonly InterfataActeBrocker _database;

        public ProiectContorController(IProiectRepository proiect, IProiectContorBuletinCabluRepository proiectcontorbuletincablu, IProiectContorBuletinPrizaRepository proiectcontorbuletinpriza, IProiectContorContractRepository proiectcontorcontract, IProiectContorProcesVerbalRepository proiectcontorprocesverbal, InterfataActeBrocker database)
        {
            _proiect = proiect;
            _proiectcontorbuletincablu = proiectcontorbuletincablu;
            _proiectcontorbuletinpriza = proiectcontorbuletinpriza;
            _proiectcontorcontract = proiectcontorcontract;
            _proiectcontorprocesverbal = proiectcontorprocesverbal;
            _database = database;
        }

        [HttpPost(template: "createproiectcontorbuletincablu")]
        public IActionResult CreateContorBuletinCablu(ProiectContorBuletinCablu proiectcontorbuletincablu)
        {
            proiectcontorbuletincablu.Tag = "BuletinCabluManual";
            return Ok(_proiectcontorbuletincablu.CreateProiectContorBuletinCablu(proiectcontorbuletincablu));
        }

        [HttpPost(template: "createproiectcontorbuletinpriza")]
        public IActionResult CreateContorBuletinPriza(ProiectContorBuletinPriza proiectcontorbuletinpriza)
        {
            proiectcontorbuletinpriza.Tag = "BuletinPrizaManual";
            return Ok(_proiectcontorbuletinpriza.CreateProiectContorBuletinPriza(proiectcontorbuletinpriza));
        }

        [HttpPost(template: "createproiectcontorcontract")]
        public IActionResult CreateContorContract(ProiectContorContract proiectcontorcontract)
        {
            if (_proiect.GetProiectById(proiectcontorcontract.ProiectID).ContorContract == null)
            {
                proiectcontorcontract.Tag = "ContractManual";
                return Ok(_proiectcontorcontract.CreateProiectContorContract(proiectcontorcontract));
            }
            else
                return BadRequest();
        }

        [HttpPost(template: "createproiectcontorprocesverbal")]
        public IActionResult CreateContorProcesVerbal(ProiectContorProcesVerbal proiectcontorprocesverbal)
        {
            if (_proiect.GetProiectById(proiectcontorprocesverbal.ProiectID).ContorProcesVerbal == null)
            {
                proiectcontorprocesverbal.Tag = "ProcesVerbalManual";
                return Ok(_proiectcontorprocesverbal.CreateProiectContorProcesVerbal(proiectcontorprocesverbal));
            }
            else
                return BadRequest();
        }

        [HttpGet("records")]
        public IActionResult ListRecords()
        {
            var projectNumbers = _database.Proiecte.AsNoTracking().ToDictionary(project => project.ID, project => project.NrProiect);
            var records = new List<ContorRecord>();
            records.AddRange(_database.ProiectContorBuletinCabluri.AsNoTracking().Select(item => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = "cablu", TypeLabel = "Buletin cablu" }).ToList());
            records.AddRange(_database.ProiectContorBuletinPrize.AsNoTracking().Select(item => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = "priza", TypeLabel = "Buletin priză" }).ToList());
            records.AddRange(_database.ProiectContorContracte.AsNoTracking().Select(item => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = "contract", TypeLabel = "Contract" }).ToList());
            records.AddRange(_database.ProiectContorProcesVerbale.AsNoTracking().Select(item => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = "procesVerbal", TypeLabel = "Proces verbal" }).ToList());
            records.ForEach(record => record.NrProiect = projectNumbers.TryGetValue(record.ProiectID, out var number) ? number : 0);
            return Ok(records.OrderByDescending(record => record.Contor));
        }

        [HttpPost("records")]
        public IActionResult CreateRecord(ContorRecord input)
        {
            if (!_database.Proiecte.Any(project => project.ID == input.ProiectID))
                return BadRequest(new { message = "Selectează un proiect valid." });

            var tag = string.IsNullOrWhiteSpace(input.Tag) ? "Manual" : input.Tag.Trim();
            switch (input.Type)
            {
                case "cablu":
                    return Ok(ToRecord(_proiectcontorbuletincablu.CreateProiectContorBuletinCablu(new ProiectContorBuletinCablu { ProiectID = input.ProiectID, Tag = tag }), "cablu", "Buletin cablu"));
                case "priza":
                    return Ok(ToRecord(_proiectcontorbuletinpriza.CreateProiectContorBuletinPriza(new ProiectContorBuletinPriza { ProiectID = input.ProiectID, Tag = tag }), "priza", "Buletin priză"));
                case "contract":
                    if (_database.ProiectContorContracte.Any(item => item.ProiectID == input.ProiectID)) return Conflict(new { message = "Proiectul are deja un număr de contract." });
                    return Ok(ToRecord(_proiectcontorcontract.CreateProiectContorContract(new ProiectContorContract { ProiectID = input.ProiectID, Tag = tag }), "contract", "Contract"));
                case "procesVerbal":
                    if (_database.ProiectContorProcesVerbale.Any(item => item.ProiectID == input.ProiectID)) return Conflict(new { message = "Proiectul are deja un număr de proces verbal." });
                    return Ok(ToRecord(_proiectcontorprocesverbal.CreateProiectContorProcesVerbal(new ProiectContorProcesVerbal { ProiectID = input.ProiectID, Tag = tag }), "procesVerbal", "Proces verbal"));
                default:
                    return BadRequest(new { message = "Tipul documentului nu este valid." });
            }
        }

        [HttpPut("records/{type}/{id:guid}")]
        public IActionResult UpdateRecord(string type, Guid id, ContorRecord input)
        {
            switch (type)
            {
                case "cablu":
                    var cable = _database.ProiectContorBuletinCabluri.Find(id);
                    if (cable == null) return NotFound();
                    cable.ProiectID = input.ProiectID; cable.Contor = input.Contor; cable.Tag = input.Tag;
                    break;
                case "priza":
                    var outlet = _database.ProiectContorBuletinPrize.Find(id);
                    if (outlet == null) return NotFound();
                    outlet.ProiectID = input.ProiectID; outlet.Contor = input.Contor; outlet.Tag = input.Tag;
                    break;
                case "contract":
                    var contract = _database.ProiectContorContracte.Find(id);
                    if (contract == null) return NotFound();
                    contract.ProiectID = input.ProiectID; contract.Contor = input.Contor; contract.Tag = input.Tag;
                    break;
                case "procesVerbal":
                    var minutes = _database.ProiectContorProcesVerbale.Find(id);
                    if (minutes == null) return NotFound();
                    minutes.ProiectID = input.ProiectID; minutes.Contor = input.Contor; minutes.Tag = input.Tag;
                    break;
                default: return BadRequest(new { message = "Tipul documentului nu este valid." });
            }
            if (!_database.Proiecte.Any(project => project.ID == input.ProiectID)) return BadRequest(new { message = "Selectează un proiect valid." });
            _database.SaveChanges();
            return NoContent();
        }

        [HttpDelete("records/{type}/{id:guid}")]
        public IActionResult DeleteRecord(string type, Guid id)
        {
            object record = type switch
            {
                "cablu" => _database.ProiectContorBuletinCabluri.Find(id),
                "priza" => _database.ProiectContorBuletinPrize.Find(id),
                "contract" => _database.ProiectContorContracte.Find(id),
                "procesVerbal" => _database.ProiectContorProcesVerbale.Find(id),
                _ => null
            };
            if (record == null) return NotFound(new { message = "Numărul de registru nu a fost găsit." });
            switch (record)
            {
                case ProiectContorBuletinCablu item: _database.ProiectContorBuletinCabluri.Remove(item); break;
                case ProiectContorBuletinPriza item: _database.ProiectContorBuletinPrize.Remove(item); break;
                case ProiectContorContract item: _database.ProiectContorContracte.Remove(item); break;
                case ProiectContorProcesVerbal item: _database.ProiectContorProcesVerbale.Remove(item); break;
            }
            _database.SaveChanges();
            return NoContent();
        }

        private static ContorRecord ToRecord(ProiectContorBuletinCablu item, string type, string label) => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = type, TypeLabel = label };
        private static ContorRecord ToRecord(ProiectContorBuletinPriza item, string type, string label) => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = type, TypeLabel = label };
        private static ContorRecord ToRecord(ProiectContorContract item, string type, string label) => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = type, TypeLabel = label };
        private static ContorRecord ToRecord(ProiectContorProcesVerbal item, string type, string label) => new ContorRecord { Id = item.ID, ProiectID = item.ProiectID, Contor = item.Contor, Tag = item.Tag, Type = type, TypeLabel = label };

    }
}
