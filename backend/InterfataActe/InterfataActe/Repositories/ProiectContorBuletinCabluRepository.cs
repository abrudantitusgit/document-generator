using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;

namespace InterfataActe.Repositories
{
    public class ProiectContorBuletinCabluRepository:IProiectContorBuletinCabluRepository
    {
        private readonly InterfataActeBrocker _context;

        public ProiectContorBuletinCabluRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public ProiectContorBuletinCablu CreateProiectContorBuletinCablu(ProiectContorBuletinCablu proiectcontorbuletincablu)
        {
            _context.ProiectContorBuletinCabluri.Add(proiectcontorbuletincablu);
            _context.SaveChanges();
            return proiectcontorbuletincablu;
        }


    }
}
