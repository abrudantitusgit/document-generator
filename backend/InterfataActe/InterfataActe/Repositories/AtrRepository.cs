using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InterfataActe.Repositories
{
    public class AtrRepository:IAtrRepository
    {
        private readonly InterfataActeBrocker _context;
        public AtrRepository(InterfataActeBrocker context)
        {
            _context = context;
        }

        public Atr CreateAtr(Atr atr)
        {
            _context.Atr.Add(atr);
            _context.SaveChanges();
            return atr;
        }

        public Atr ModifyAtr(Atr atr)
        {
            _context.Atr.Update(atr);
            _context.SaveChanges();
            return atr;
        }

        public List<Atr> GetAllAtr()
        {
            return _context.Atr.ToList();
        }

        public Atr GetAtrByNrAtr(long nrAtr)
        {
            var atr = _context.Atr.Where(x => x.NrAtr == nrAtr).FirstOrDefault();
            return atr;
        }

    }
}
