using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.Models
{
    public class Client
    {
        [Key]
        public Guid ID { get; set; }
        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Nume { get; set; }
        [Required(ErrorMessage = "CNP-ul este obligatoriu.")]
        public long CNP { get; set; }
        public string Eliberat { get; set; }
        public DateTime DataBuletin { get; set; }
        public string Seria { get; set; }
    //   public int NrSeria { get; set; }
        public string NrSeria { get; set; }
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
        public ICollection<Firma> Firme { get; set; }
    }
}
