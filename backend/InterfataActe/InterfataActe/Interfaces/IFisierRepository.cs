using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IFisierRepository
    {
        Fisier CreateFisier(Fisier fisier);
        Fisier GetFisierByID(Guid id);
        List<Fisier> GetFisiereByFolderID(Guid id);
    }
}
