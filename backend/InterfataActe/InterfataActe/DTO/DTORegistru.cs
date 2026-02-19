using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.DTO
{
    public class DTORegistru
    {
        
        public string NrProiect { get; set; }
        
        public string NumeClient { get; set; }
        
        public string NumeFirma { get; set; }
        
        public string NrAtr { get; set; }
        
        public string Denumire { get; set; }
        
        public string Pret { get; set; }
        
        public string Localitatea { get; set; }
        
        public string Strada { get; set; }
        
        public string NrStrada { get; set; }
        
        public string Apartament { get; set; }
        
        public string Data { get; set; }
        
        public string Contract { get; set; }
        
        public string ProcesVerbal { get; set; }
        
        public string ProcesVerbalCablu { get; set; }
        
        public string BuletinCablu { get; set; }
        
        public string BuletinPriza { get; set; }
    }
}
