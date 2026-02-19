using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class Grup
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string Nume { get; set; }
        public ICollection<Folder> Foldere { get; set; }
    }
}
