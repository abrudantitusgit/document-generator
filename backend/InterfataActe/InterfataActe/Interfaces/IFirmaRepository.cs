using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IFirmaRepository
    {
        Firma CreateFirma(Firma firma);
        Firma ModifyFirma(Firma firma);
        List<Firma> GetAllFirme();
    }
}
