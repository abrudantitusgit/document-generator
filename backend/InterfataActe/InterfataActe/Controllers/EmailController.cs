using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using InterfataActe.DTO;
using InterfataActe.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using InterfataActe.Interfaces;
using InterfataActe.Models;
using InterfataActe.Brockers;

namespace InterfataActe.Controllers
{

    [Route(template: "email")]
    [EnableCors]
    [ApiController]
    public class EmailController : ControllerBase
    {

        //   public readonly IEmailSender _email;
        private readonly IEmailRepository _email;
        private readonly InterfataActeBrocker _database;

        public EmailController(IEmailRepository email, InterfataActeBrocker database)
        {
            _email = email;
            _database = database;
        }
        /*
                public EmailController(IEmailSender email)
                {
                    _email = email;
                }


                [HttpGet(template: "send")]
                public IActionResult SendEmail()
                {
                    return Ok( _email.SendEmailAsync("petrana@ovinstalelectric.ro","test","some message"));

                }
        */

        [HttpPost(template: "create")]
        public IActionResult CreateEmail(Email email)
        {
            return Ok(_email.CreateEmail(email));
        }

        [HttpGet(template: "delete/{id}")]
        public IActionResult DeleteById(Guid id)
        {
            return Ok(_email.DeleteById(id));
        }

        [HttpGet(template: "list")]
        public IActionResult List()
        {
            return Ok(_email.List());
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateEmail(Guid id, Email input)
        {
            var email = _database.Emails.Find(id);
            if (email == null) return NotFound(new { message = "E-mailul nu a fost găsit." });
            email.Adresa = input.Adresa;
            email.Text = input.Text;
            email.Data = input.Data;
            email.Activ = input.Activ;
            _database.SaveChanges();
            return Ok(email);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult RemoveEmail(Guid id)
        {
            var email = _database.Emails.Find(id);
            if (email == null) return NotFound(new { message = "E-mailul nu a fost găsit." });
            _database.Emails.Remove(email);
            _database.SaveChanges();
            return NoContent();
        }




    }

}
