using System;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class ProiectContorBuletinPriza
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ProiectID { get; set; }
        [Required]
        public long Contor { get; set; }
        [Required]
        public string Tag { get; set; }
    }
}
