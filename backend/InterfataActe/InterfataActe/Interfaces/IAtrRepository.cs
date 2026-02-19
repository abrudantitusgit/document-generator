using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IAtrRepository
    {
        Atr CreateAtr(Atr atr);
        Atr ModifyAtr(Atr atr);
        List<Atr> GetAllAtr();
        Atr GetAtrByNrAtr(long nrAtr);
    }
}
