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
    public class GrupRepository:IGrupRepository
    {
        private readonly InterfataActeBrocker _context;

        public GrupRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public Grup CreateGrup(Grup grup)
        {
            _context.Grupe.Add(grup);
            _context.SaveChanges();
            return grup;
        }

        public List<Grup> GetAllGrupe()
        {
            return _context.Grupe.
                Include(x => x.Foldere).
                ThenInclude(x => x.Fisiere).ToList();
        }

        public Grup GetGrupByNume(string nume)
        {
            return _context.Grupe.Include(x=>x.Foldere).ThenInclude(x=>x.Fisiere.OrderBy(x=>x.Nume)).FirstOrDefault(x=>x.Nume == nume);
        }
        public Grup GetGrupById(Guid id)
        {
            return _context.Grupe.Include(x=>x.Foldere).ThenInclude(x=>x.Fisiere).FirstOrDefault(x=>x.ID == id);
        }
        public Grup UpdateGrup(Grup g)
        {
            var existing = _context.Grupe
                .Include(gr => gr.Foldere)
                    .ThenInclude(f => f.Fisiere)
                .FirstOrDefault(gr => gr.ID == g.ID);

            if (existing is null)
                return null;

            // 1️⃣ Update câmpuri simple Grup
            _context.Entry(existing).CurrentValues.SetValues(g);

            // 2️⃣ Update Foldere + Fisiere (simplu: clear & re-add)
            existing.Foldere.Clear();

            foreach (var f in g.Foldere ?? new List<Folder>())
            {
                var newFolder = new Folder
                {
                    ID = f.ID == Guid.Empty ? Guid.NewGuid() : f.ID,
                    GrupID = existing.ID,
                    Nume = f.Nume,
                    Fisiere = new List<Fisier>()
                };

                foreach (var fi in f.Fisiere ?? new List<Fisier>())
                {
                    newFolder.Fisiere.Add(new Fisier
                    {
                        ID = fi.ID == Guid.Empty ? Guid.NewGuid() : fi.ID,
                        FolderID = newFolder.ID,
                        Nume = fi.Nume,
                        DenumireContor = fi.DenumireContor
                    });
                }

                existing.Foldere.Add(newFolder);
            }

            _context.SaveChanges();

            return existing;
        }


    }
}
