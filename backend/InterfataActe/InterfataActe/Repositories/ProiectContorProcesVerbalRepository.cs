using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;

namespace InterfataActe.Repositories
{
    public class ProiectContorProcesVerbalRepository:IProiectContorProcesVerbalRepository
    {

        private readonly InterfataActeBrocker _context;

        public ProiectContorProcesVerbalRepository(InterfataActeBrocker context)
        {
            _context = context;
        }

        public ProiectContorProcesVerbal CreateProiectContorProcesVerbal(ProiectContorProcesVerbal proiectcontorprocesverbal)
        {
            _context.ProiectContorProcesVerbale.Add(proiectcontorprocesverbal);
            _context.SaveChanges();
            return proiectcontorprocesverbal;
        }


    }
}
