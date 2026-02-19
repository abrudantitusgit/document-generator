using System;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class Firma
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ClientID { get; set; }
        public string Nume { get; set; }
        [Required]
        public long CUI {get;set;}
        [Required]
        public DateTime DataCUI { get; set; }
        [Required]
        public string ORC { get; set; }
        public string Functia { get; set; }
        public string Judet { get; set; }
        public string Localitatea { get; set; }
        public string Comuna { get; set; }
        public string Strada { get; set; }
        public string Sector { get; set; }
        public string NrStrada { get; set; }
        public string Bloc { get; set; }
        public string Scara { get; set; }
        public string Etaj { get; set; }
        public string Apartament { get; set; }
        public string CodPostal { get; set; }
        public string Telefon { get; set; }
    }
}
