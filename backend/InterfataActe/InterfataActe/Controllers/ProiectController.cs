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
    [Route(template: "proiect")]
    [EnableCors]
    [ApiController]
    public class ProiectController:ControllerBase
    {
        private readonly IProiectRepository _proiect;

        public ProiectController(IProiectRepository proiect)
        {
            _proiect = proiect;
        }


        [HttpPost(template:"create")]
        public IActionResult CreateProiect(Proiect proiect)
        {       
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



    }
}
