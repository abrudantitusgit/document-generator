using System;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class Fisier
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid FolderID { get; set; }
        [Required]
        public string Nume { get; set; }
        [Required]
        public string DenumireContor { get; set; }
    }
}
