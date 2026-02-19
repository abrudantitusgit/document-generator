using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace InterfataActe.Models
{
    public class Atr
    {

        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ProiectID { get; set; }
        [Required]
        public long NrAtr { get; set; }
        [Required]
        public DateTime DataAtr { get; set; }
    //    [Required]
    //    public long Contract { get; set; }
    //    [Required]
     //   public long ProcesVerbal { get; set; }
     //   [Required]
     //   public long BuletinCablu { get; set; }
    //    [Required]
   //     public long BuletinPriza { get; set; }
     ///   [Required]
    //    public long BuletinCabluUtilizare { get; set; }
     //   [Required]
     //   public long BuletinPrizaUtilizare { get; set; }
        //Date
        public string NrCadastral { get; set; }
        public string CodPostal { get; set; }
        public string PunctRacordare { get; set; }
        public string PunctAlimentare { get; set; }
        public string PunctBransament { get; set; }
        public string Cablu { get; set; }
        public long LungimeCablu { get; set; }
        public int Ohmi { get; set; }
        public string Contor { get; set; }
        public double Temperatura { get; set; }
        public string Descriere { get; set; }
     //   public DateTime Data { get; set; }
        public double Pret { get; set; }
        public double Tva { get; set; }

    }
}
