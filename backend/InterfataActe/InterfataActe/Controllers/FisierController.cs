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
    [Route(template: "fisier")]
    [EnableCors]
    [ApiController]
    public class FisierController : ControllerBase
    {

        private readonly IFisierRepository _fisier;

        public FisierController(IFisierRepository fisier)
        {
            _fisier = fisier;
        }



        [HttpPost(template:"create")]
        public IActionResult CreateFisier(Fisier fisier)
        {
            return Ok(_fisier.CreateFisier(fisier));
        }

    }


}
