using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;

namespace InterfataActe.Repositories
{
    public class ProiectContorContractRepository:IProiectContorContractRepository
    {

        private readonly InterfataActeBrocker _context;

        public ProiectContorContractRepository(InterfataActeBrocker context)
        {
            _context = context;
        }

        public ProiectContorContract CreateProiectContorContract(ProiectContorContract proiectcontorcontract)
        {
            _context.ProiectContorContracte.Add(proiectcontorcontract);
            _context.SaveChanges();
            return proiectcontorcontract;
        }


    }
}
