using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterfataActe.Models
{
    public class ProiectContorProcesVerbal
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ProiectID { get; set; }
        [Required]
        public long Contor { get; set; }
        [NotMapped]
        public string Cablu { get { return Contor.ToString() + "A"; } }
        [Required]
        public string Tag { get; set; }
    }
}
