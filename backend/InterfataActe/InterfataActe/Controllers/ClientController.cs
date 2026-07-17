using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.Interfaces;
using InterfataActe.Models;

namespace InterfataActe.Controllers
{
    [Route("client")]
    [EnableCors]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientRepository _client;

        public ClientController(IClientRepository client)
        {
            _client = client;
        }

        [HttpPost("create")]
        public IActionResult CreateClient([FromBody] Client client)
        {
            try
            {
                if (client == null)
                    return BadRequest(new { message = "Datele clientului sunt obligatorii." });

                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Date invalide.", errors = ModelState });

                if (client.CNP.ToString().Length != 13)
                    return BadRequest(new { message = "CNP-ul trebuie să conțină exact 13 cifre." });

                var existingClient = _client.GetClientByCnp(client.CNP);

                if (existingClient != null)
                    return Conflict(new { message = "Există deja un client cu acest CNP." });

                var result = _client.CreateClient(client);

                if (result == null)
                    return StatusCode(500, new { message = "A apărut o eroare la crearea clientului." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Eroare internă: {ex.Message}" });
            }
        }

        [HttpPost("modify")]
        public IActionResult ModifyClient([FromBody] Client client)
        {
            try
            {
                if (client == null)
                    return BadRequest(new { message = "Datele clientului sunt obligatorii." });

                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Date invalide.", errors = ModelState });

                if (client.CNP <= 0)
                    return BadRequest(new { message = "CNP-ul este obligatoriu." });

                if (client.CNP.ToString().Length != 13)
                    return BadRequest(new { message = "CNP-ul trebuie să conțină exact 13 cifre." });

                var result = _client.ModifyClient(client);

                if (result == null)
                    return StatusCode(500, new { message = "A apărut o eroare la modificarea clientului." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Eroare internă: {ex.Message}" });
            }
        }

        [HttpGet("clienti")]
        public IActionResult GetClienti()
        {
            try
            {
                var result = _client.GetAllClienti();

                if (result == null)
                    return StatusCode(500, new { message = "A apărut o eroare la preluarea clienților." });

                if (!result.Any())
                    return NotFound(new { message = "Nu există clienți în baza de date." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Eroare internă: {ex.Message}" });
            }
        }

        [HttpGet("clientifirme")]
        public IActionResult GetClientiFirme()
        {
            try
            {
                var result = _client.GetAllClientiFirme();

                if (result == null)
                    return StatusCode(500, new { message = "A apărut o eroare la preluarea clienților firmă." });

                if (!result.Any())
                    return NotFound(new { message = "Nu există clienți firmă în baza de date." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Eroare internă: {ex.Message}" });
            }
        }

        [HttpGet("caietclient")]
        public IActionResult CaietClient()
        {
            try
            {
                var result = _client.CaietClient();

                if (result == null)
                    return NotFound(new { message = "Nu există date pentru caietul de clienți." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Eroare internă: {ex.Message}" });
            }
        }
    }
}