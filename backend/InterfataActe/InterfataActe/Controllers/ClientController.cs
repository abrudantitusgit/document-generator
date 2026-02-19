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
    [Route(template: "client")]
    [EnableCors]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _client;

        public ClientController(IClientRepository client)
        {
            _client = client;
        }

        [HttpPost(template:"create")]
        public IActionResult CreateClient(Client client)
        {
            return Ok(_client.CreateClient(client));
        }

        [HttpPost(template:"modifiy")]
        public IActionResult ModifyClient(Client client)
        {
            return Ok(_client.ModifyClient(client));
        }

        [HttpGet(template:"clienti")]
        public IActionResult GetClienti()
        {
            return Ok(_client.GetAllClienti());
        }

        [HttpGet(template:"clientifirme")]
        public IActionResult GetClientiFirme()
        {
            return Ok(_client.GetAllClientiFirme());
        }

        [HttpGet(template:"caietclient")]
        public IActionResult CaietClient()
        {
            return Ok(_client.CaietClient());
        }

    /*    [HttpGet(template:"caietproiect")]
        public IActionResult CaietProiect()
        {
            return Ok(_client.CaietProiect());
        }
    */

        


    }
}
