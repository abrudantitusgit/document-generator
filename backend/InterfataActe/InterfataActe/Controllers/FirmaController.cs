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
    [Route(template: "firma")]
    [EnableCors]
    [ApiController]
    public class FirmaController:ControllerBase
    {
        public readonly IFirmaRepository _firma;

        public FirmaController(IFirmaRepository firma)
        {
            _firma = firma;
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


    }
}
