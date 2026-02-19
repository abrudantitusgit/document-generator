using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.DTO;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IProiectRepository
    {
        Proiect CreateProiect(Proiect proiect);
        Proiect ModifyProiect(Proiect proiect);
        List<Proiect> GetAllProiecte();
        Proiect GetProiectById(Guid Id);
        List<Proiect> GetAllProiecteIncludeNull();
        List<Proiect> GetAllProiecteInclude();
        List<DTORegistru> GetRegistru();
    }
}
