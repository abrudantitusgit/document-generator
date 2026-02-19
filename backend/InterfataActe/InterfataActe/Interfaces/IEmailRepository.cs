using InterfataActe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterfataActe.Interfaces
{
    public interface IEmailRepository
    {

        Email CreateEmail(Email email);

        Email DezactivateEmail(Guid Id);

        Email DeleteById(Guid Id);

        List<Email> ListToSend();

        List<Email> List();

    }
}
