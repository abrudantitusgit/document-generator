using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;

namespace InterfataActe.Interfaces
{
    public interface IClientRepository
    {
        Client CreateClient(Client client);
        Client ModifyClient(Client client);
        List<Client> GetAllClienti();
        List<Client> GetAllClientiFirme();
        List<Client> CaietClient();


    }
}
