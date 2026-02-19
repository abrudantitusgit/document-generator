using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace InterfataActe.Models
{
    public class Email
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Adresa { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public DateTime Data { get; set; }
        [Required]
        public bool Activ { get; set; }
    }
}
