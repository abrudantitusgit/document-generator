using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterfataActe.Models
{
    public class Proiect
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public Guid ClientID { get; set; }
        [Required]
        public long NrProiect { get; set; }
        public Guid? FirmaID { get; set; }
        public double PRM_Masurat { 
            get {
                Random r = new Random(); 
                double x = r.NextDouble() * (2.5 - 1.5) + 1.5; 
                return Math.Round(x, 2); 
            }  
        }
        public double PRM_Calculat { get { 
                return Math.Round(PRM_Masurat * 1.3, 2);
            }  
        }
        public string Denumire { get; set; }
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
        public double Pi { get; set; }
        public double Pa { get; set; }
        public string LocConsum { get; set; }
        public string TensiuneUtilizare { get; set; }
        public DateTime Data { get; set; }
        public Client Client { get; set; }
        public Firma Firma { get; set; } 
        public Atr Atr { get; set; }
        public ProiectContorContract ContorContract { get; set; }
        public ProiectContorProcesVerbal ContorProcesVerbal { get; set; }
        public ICollection<ProiectContorBuletinCablu> ContorBuletinCabluri { get; set; }
        public ICollection<ProiectContorBuletinPriza> ContorBuletinPrize { get; set; }


    }
}
