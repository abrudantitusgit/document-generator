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
    [Route(template:"folder")]
    [EnableCors]
    [ApiController]
    public class FolderController:ControllerBase
    {


        private readonly IFolderRepository _folder;

        public FolderController(IFolderRepository folder)
        {
            _folder = folder;
        }


        [HttpPost(template:"create")]
        public IActionResult CreateFolder(Folder folder)
        {
            return Ok(_folder.CreateFolder(folder));
        }


        [HttpGet(template:"foldere")]
        public IActionResult GetAllFoldere()
        {
            return Ok(_folder.GetAllFoldere());
        }

    }
}
