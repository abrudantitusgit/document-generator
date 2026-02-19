using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InterfataActe.Models;
using Microsoft.EntityFrameworkCore;
using InterfataActe.Interfaces;
using InterfataActe.Brockers;

namespace InterfataActe.Repositories
{
    public class ProiectFotovoltaiceRepository : IProiectFotovoltaiceRepository
    {
        private readonly InterfataActeBrocker _db;

        public ProiectFotovoltaiceRepository(InterfataActeBrocker db)
        {
            _db = db;
        }
         public async Task<List<DataInvertor>> GetSugestiiInvertoare()
         {
                return await _db.DataInvertor.GroupBy(u=>u.Model).Select(g=>g.First()).ToListAsync();
         }

        public async Task<List<DataPanou>> GetSugestiiPanouri()
         {
                return await _db.DataPanou.GroupBy(u=>u.Model).Select(g=>g.First()).ToListAsync();
         }

        public async Task<List<DataBaterie>> GetSugestiiBaterii()
         {
                return await _db.DataBaterie.GroupBy(u=>u.Model).Select(g=>g.First()).ToListAsync();
         }
        public async Task<List<ProiectFotovoltaice>> GetAllAsync(bool includeChildren = true,CancellationToken ct = default)
        {
            IQueryable<ProiectFotovoltaice> q = _db.ProiecteFotovoltaice
                .AsNoTracking();

            if (includeChildren)
            {
                q = q
                    .Include(p => p.DateInvertoare)
                    .Include(p => p.DatePanouri)
                    .Include(p => p.DateBaterii)
                    .Include(p => p.Client)
                    .Include(p => p.Firma);
            }

            return await q
                .OrderByDescending(p => p.Data)
                .ToListAsync(ct);
        }

        public async Task<ProiectFotovoltaice> GetByIdAsync(Guid id, bool includeChildren = true, CancellationToken ct = default)
        {
            IQueryable<ProiectFotovoltaice> q = _db.ProiecteFotovoltaice.AsNoTracking();

            if (includeChildren)
            {
                q = q
                    .Include(p => p.DateInvertoare)
                    .Include(p => p.DatePanouri)
                    .Include(p => p.DateBaterii)
                    .Include(p => p.Client)
                    .Include(p => p.Firma);
            }

            return await q.FirstOrDefaultAsync(p => p.ID == id, ct);
        }

        public async Task<List<ProiectFotovoltaice>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
        {
            return await _db.ProiecteFotovoltaice
                .AsNoTracking()
                .Where(p => p.ClientID == clientId)
                .OrderByDescending(p => p.Data)
                .ToListAsync(ct);
        }

        public async Task<ProiectFotovoltaice> AddAsync(ProiectFotovoltaice entity, CancellationToken ct = default)
        {
            if (entity.ID == Guid.Empty)
                entity.ID = Guid.NewGuid();

            // dacă nu vine Data din client, seteaz-o aici
            if (entity.Data == default)
                entity.Data = DateTime.UtcNow;

            // protecție null-lists (în caz că vin null din request)
            entity.DateInvertoare ??= new List<DataInvertor>();
            entity.DatePanouri ??= new List<DataPanou>();
            entity.DateBaterii ??= new List<DataBaterie>();

            _db.ProiecteFotovoltaice.Add(entity);
            await _db.SaveChangesAsync(ct);

            return entity;
        }

        public async Task<ProiectFotovoltaice> UpdateAsync(ProiectFotovoltaice entity, CancellationToken ct = default)
        {
            var existing = await _db.ProiecteFotovoltaice
                .Include(p => p.DateInvertoare)
                .Include(p => p.DatePanouri)
                .Include(p => p.DateBaterii)
                .FirstOrDefaultAsync(p => p.ID == entity.ID, ct);

            if (existing is null) return null;

            // Update simple fields (poți ajusta lista)
            _db.Entry(existing).CurrentValues.SetValues(entity);

            // Update children: abordare simplă = șterge și re-adaugă (OK la început)
            // Dacă vrei update “fin” (diff), îți fac și varianta aceea.
            existing.DateInvertoare.Clear();
            foreach (var inv in (entity.DateInvertoare ?? new()))
                existing.DateInvertoare.Add(inv);

            existing.DatePanouri.Clear();
            foreach (var pan in (entity.DatePanouri ?? new()))
                existing.DatePanouri.Add(pan);

            existing.DateBaterii.Clear();
            foreach (var bat in (entity.DateBaterii ?? new()))
                existing.DateBaterii.Add(bat);

            await _db.SaveChangesAsync(ct);
             return await _db.ProiecteFotovoltaice
                .AsNoTracking()
                .Include(p => p.DateInvertoare)
                .Include(p => p.DatePanouri)
                .Include(p => p.DateBaterii)
                .Include(p => p.Client)
                .Include(p => p.Firma)
                .FirstOrDefaultAsync(p => p.ID == entity.ID, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _db.ProiecteFotovoltaice.FirstOrDefaultAsync(p => p.ID == id, ct);
            if (existing is null) return false;

            _db.ProiecteFotovoltaice.Remove(existing);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        {
            return _db.ProiecteFotovoltaice.AnyAsync(p => p.ID == id, ct);
        }
    }
}
