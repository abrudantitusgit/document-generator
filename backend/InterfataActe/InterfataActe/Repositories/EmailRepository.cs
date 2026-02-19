using InterfataActe.Brockers;
using InterfataActe.Interfaces;
using InterfataActe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterfataActe.Repositories
{
    public class EmailRepository:IEmailRepository
    {
        private readonly InterfataActeBrocker _context;
        public EmailRepository(InterfataActeBrocker context)
        {
            _context = context;
        }

        public Email CreateEmail(Email email)
        {
            _context.Emails.Add(email);
            _context.SaveChanges();
            return email;
        }

        public Email DeleteById(Guid Id)
        {
            var email = _context.Emails.FirstOrDefault(x => x.Id == Id);
            _context.Emails.Remove(email);
            _context.SaveChanges();
            return email;
        }

        public Email DezactivateEmail(Guid Id)
        {
            var email = _context.Emails.FirstOrDefault(x => x.Id == Id);
            email.Activ = false;
            _context.Emails.Update(email);
            _context.SaveChanges();
            return email;
        }

        public List<Email> ListToSend()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            return _context.Emails.Where(x => x.Activ == true && x.Data >= today && x.Data < tomorrow).ToList();
        }

        public List<Email> List()
        {
            return _context.Emails.ToList();
        }


    }
}
