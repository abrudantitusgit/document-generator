using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IGrupRepository
    {
        Grup CreateGrup(Grup grup);
        List<Grup> GetAllGrupe();
        Grup GetGrupByNume(string nume);
        Grup UpdateGrup(Grup grup);
        Grup GetGrupById(Guid id);
    }
}
