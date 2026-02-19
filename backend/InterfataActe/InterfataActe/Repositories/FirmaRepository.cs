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
    public class FirmaRepository:IFirmaRepository
    {
        private readonly InterfataActeBrocker _context;

        public FirmaRepository(InterfataActeBrocker context)
        {
            _context = context;

        }

        public Firma CreateFirma(Firma firma)
        {
            _context.Firme.Add(firma);
            _context.SaveChanges();
            return firma;
        }

        public Firma ModifyFirma(Firma firma)
        {
            _context.Firme.Update(firma);
            _context.SaveChanges();
            return firma;
        }

        public List<Firma> GetAllFirme()
        {
            return _context.Firme.ToList();
        }



    }
}
