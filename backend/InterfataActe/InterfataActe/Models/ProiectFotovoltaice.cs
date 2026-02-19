using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace InterfataActe.Models
{
    public class ProiectFotovoltaice
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ClientID { get; set; }
        [Required]
        public long NrProiect { get; set; }
        public long NrPV { get; set; }
        public DateTime Data { get; set; }
        public Guid? FirmaID { get; set; }
        public string Judet { get; set; }
        public string Sector { get; set; }
        public string Localitatea { get; set; }
        public string Comuna { get; set; }
        public string Strada { get; set; }
        public string NrStrada { get; set; }
        public string Bloc { get; set; }
        public string Scara { get; set; }
        public string Etaj { get; set; }
        public string Apartament { get; set; }
        public string NrCadastral{get;set;}
        public string NrCF{get;set;}
        public string Amplasare{get;set;}
        public double PRM_Masurat { get; set; }
        public double PRM_Calculat { get; set; }
        public double PRM_Coeficient {get;set;}

        public string TipSistem{get;set;}
        public List<DataInvertor> DateInvertoare { get; set; } = new();
        public List<DataPanou> DatePanouri { get; set; } = new();
        public List<DataBaterie> DateBaterii { get; set; } = new();

        [NotMapped]
        public decimal PutereTotalaPanouri => (DatePanouri ?? new()).Sum(p => p.NrPanouri * p.PutereInstalata);

        [NotMapped]
        public decimal CapacitateTotalaBaterii => (DateBaterii ?? new()).Sum(b => b.NrBaterii * b.Capacitate);
        [NotMapped]
        public decimal PutereTotalaInvertor => (DateInvertoare ?? new()).Sum(i => i.NrInvertoare * i.PutereInstalata);
        public string Backup{get;set;}
        public string SmartMeter{get;set;}
        public string POD{get;set;}
        public Client Client { get; set; }
        public Firma Firma { get; set; } 



    }
}
