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
    public class ClientRepository : IClientRepository
    {
        private readonly InterfataActeBrocker _context;
        public ClientRepository(InterfataActeBrocker context)
        {
            _context = context;
        }


        public Client CreateClient(Client client)
        {
            _context.Clienti.Add(client);
            _context.SaveChanges();
            return client;
        }

        public Client ModifyClient(Client client)
        {
            _context.Update(client);
            _context.SaveChanges();
            return client;
        }

        public List<Client> GetAllClienti()
        {
            return _context.Clienti.ToList();
        }

        public List<Client> GetAllClientiFirme()
        {
            return _context.Clienti.Include(x => x.Firme).ToList();
        }
        public List<Client> CaietClient()
        {
              return _context.Clienti.ToList();
            
        }




    }
}
