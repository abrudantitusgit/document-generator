using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IProiectFotovoltaiceRepository
    {
         Task<List<InterfataActe.Models.DataInvertor>> GetSugestiiInvertoare();
         Task<List<InterfataActe.Models.DataPanou>> GetSugestiiPanouri();
         Task<List<InterfataActe.Models.DataBaterie>> GetSugestiiBaterii();
        Task<List<InterfataActe.Models.ProiectFotovoltaice>> GetAllAsync(bool includeChildren = true,CancellationToken ct = default);
        Task<InterfataActe.Models.ProiectFotovoltaice> GetByIdAsync(Guid id, bool includeChildren = true, CancellationToken ct = default);

        Task<List<InterfataActe.Models.ProiectFotovoltaice>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);

        Task<InterfataActe.Models.ProiectFotovoltaice> AddAsync(InterfataActe.Models.ProiectFotovoltaice entity, CancellationToken ct = default);
        Task<ProiectFotovoltaice> UpdateAsync(InterfataActe.Models.ProiectFotovoltaice entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    }
}
