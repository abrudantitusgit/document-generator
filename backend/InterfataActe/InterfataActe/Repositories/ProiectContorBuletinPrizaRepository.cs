using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;

namespace InterfataActe.Repositories
{
    public class ProiectContorBuletinPrizaRepository:IProiectContorBuletinPrizaRepository
    {
        private readonly InterfataActeBrocker _context;

        public ProiectContorBuletinPrizaRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public ProiectContorBuletinPriza CreateProiectContorBuletinPriza(ProiectContorBuletinPriza proiectcontorbuletinpriza)
        {
            _context.ProiectContorBuletinPrize.Add(proiectcontorbuletinpriza);
            _context.SaveChanges();
            return proiectcontorbuletinpriza;
        }




    }
}
