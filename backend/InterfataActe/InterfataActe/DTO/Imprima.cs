using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.DTO
{
    public class Imprima
    {
        [Required]
        public Proiect Proiect { get; set; }
        [Required]
        public Grup Grup { get; set; }
    }
}
