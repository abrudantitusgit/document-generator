using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.DTO;
using InterfataActe.Helpers;


namespace InterfataActe.Controllers
{

    [Route(template: "word")]
    [EnableCors]
    [ApiController]
    public class WordController : ControllerBase
    {

        private readonly Word2 _word;

        public WordController(Word2 word)
        {
            _word = word;
        }


        [HttpPost(template: "create")]
        public IActionResult CreateWord(Imprima imprima)
        {
            return Ok(_word.GenerateBransament(imprima));

        }

        [HttpPost(template: "createfotovoltaice")]
        public IActionResult CreateWord(ImprimaFotovoltaice imprima)
        {
            var result = _word.GenerateFotovoltaice(imprima);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);

        }



    }

}