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
    public class FolderRepository:IFolderRepository
    {

        private readonly InterfataActeBrocker _context;

        public FolderRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public Folder GetFolderByID(Guid id)
        {
            return _context.Foldere.Where(x => x.ID == id).FirstOrDefault();
        }

        public Folder CreateFolder(Folder folder)
        {
            _context.Foldere.Add(folder);
            _context.SaveChanges();
            return folder;
        }


        public List<Folder> GetAllFoldere()
        {
            return _context.Foldere.Include(x=>x.Fisiere).ToList();
        }


    }
}
