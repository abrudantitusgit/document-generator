using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;

namespace InterfataActe.Repositories
{
    public class FisierRepository:IFisierRepository
    {
        private readonly InterfataActeBrocker _context;
        public FisierRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public Fisier CreateFisier(Fisier fisier)
        {
            _context.Fisiere.Add(fisier);
            _context.SaveChanges();
            return fisier;
        }


        public Fisier GetFisierByID(Guid id)
        {
            return _context.Fisiere.Where(x => x.ID == id).FirstOrDefault();
        }

        public List<Fisier> GetFisiereByFolderID(Guid id)
        {
            return _context.Fisiere.Where(x => x.FolderID == id).ToList();
        }



    }
}
