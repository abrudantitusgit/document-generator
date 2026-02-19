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

namespace InterfataActe.Controllers
{

    [Route(template: "email")]
    [EnableCors]
    [ApiController]
    public class EmailController : ControllerBase
    {

        //   public readonly IEmailSender _email;
        private readonly IEmailRepository _email;

        public EmailController(IEmailRepository email)
        {
            _email = email;
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




    }

}