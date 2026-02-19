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
    [Route(template: "atr")]
    [EnableCors]
    [ApiController]
    public class AtrController : ControllerBase
    {
        private readonly IAtrRepository _atr;
        private readonly IProiectContorBuletinCabluRepository _contorbuletincablu;
        private readonly IProiectContorBuletinPrizaRepository _contorbuletinpriza;
        private readonly IProiectContorContractRepository _contorcontract;
        private readonly IProiectContorProcesVerbalRepository _contorprocesverbal;
        private readonly IProiectRepository _proiect;

        public AtrController(IAtrRepository atr, IProiectContorBuletinCabluRepository contorbuletincablu, IProiectContorBuletinPrizaRepository contorbuletinpriza, IProiectContorContractRepository contorcontract, IProiectContorProcesVerbalRepository contorprocesverbal, IProiectRepository proiect)
        {
            _atr = atr;
            _contorbuletincablu = contorbuletincablu;
            _contorbuletinpriza = contorbuletinpriza;
            _contorcontract = contorcontract;
            _contorprocesverbal = contorprocesverbal;
            _proiect = proiect;
        }

        [HttpPost(template: "create")]
        public IActionResult CreateAtr(Atr atr)
        {

            if (ModelState.IsValid)
            {
                _atr.CreateAtr(atr);
                var proiect = _proiect.GetProiectById(atr.ProiectID);
                _contorcontract.CreateProiectContorContract(new ProiectContorContract { ProiectID = proiect.ID, Tag = "Contract" });
                _contorprocesverbal.CreateProiectContorProcesVerbal(new ProiectContorProcesVerbal { ProiectID = proiect.ID, Tag = "ProcesVerbal" });
                _contorbuletincablu.CreateProiectContorBuletinCablu(new ProiectContorBuletinCablu { ProiectID = proiect.ID, Tag = "BuletinCablu" });
                _contorbuletincablu.CreateProiectContorBuletinCablu(new ProiectContorBuletinCablu { ProiectID = proiect.ID, Tag = "BuletinCabluUtilizare" });
                _contorbuletinpriza.CreateProiectContorBuletinPriza(new ProiectContorBuletinPriza { ProiectID = proiect.ID, Tag = "BuletinPriza" });
                _contorbuletinpriza.CreateProiectContorBuletinPriza(new ProiectContorBuletinPriza { ProiectID = proiect.ID, Tag = "BuletinPrizaUtilizare" });
                return Ok();
            }
            else
                return BadRequest();
            
        }

        [HttpPost(template:"modify")]
        public IActionResult ModifyAtr(Atr atr)
        {
            return Ok(_atr.ModifyAtr(atr));
        }
        [HttpGet(template:"list")]
        public IActionResult ListAtr()
        {
            return Ok(_atr.GetAllAtr());
        }
        [HttpPost(template:"createmanual")]
        public IActionResult CreateAtrManual(Atr atr)
        {
            return Ok(_atr.CreateAtr(atr));
        }


    }
}
