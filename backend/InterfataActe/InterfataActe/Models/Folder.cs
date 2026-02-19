using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class Folder
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid GrupID { get; set; }
        [Required]
        public string Nume { get; set; }
        public ICollection<Fisier> Fisiere { get; set; }
    }
}
