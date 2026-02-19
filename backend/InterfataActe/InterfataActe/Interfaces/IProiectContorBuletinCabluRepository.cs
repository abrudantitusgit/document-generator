using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IProiectContorBuletinCabluRepository
    {
        ProiectContorBuletinCablu CreateProiectContorBuletinCablu(ProiectContorBuletinCablu proiectcontorbuletincablu);
    }
}
