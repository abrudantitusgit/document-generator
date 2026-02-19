using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.Interfaces;
using InterfataActe.Models;

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

        public ProiectContorController(IProiectRepository proiect, IProiectContorBuletinCabluRepository proiectcontorbuletincablu, IProiectContorBuletinPrizaRepository proiectcontorbuletinpriza, IProiectContorContractRepository proiectcontorcontract, IProiectContorProcesVerbalRepository proiectcontorprocesverbal)
        {
            _proiect = proiect;
            _proiectcontorbuletincablu = proiectcontorbuletincablu;
            _proiectcontorbuletinpriza = proiectcontorbuletinpriza;
            _proiectcontorcontract = proiectcontorcontract;
            _proiectcontorprocesverbal = proiectcontorprocesverbal;
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

    }
}
