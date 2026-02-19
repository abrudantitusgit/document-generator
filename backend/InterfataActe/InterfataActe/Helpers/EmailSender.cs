using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace InterfataActe.Helpers
{
    public class EmailSender : IEmailSender
    {
        public EmailSender()
        {
 
        }
 
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string fromMail = "no.reply.titus@gmail.com";
            string fromPassword = "cbenwzyttyimqqss";
 
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(email));
            message.Body ="<html><body> " + htmlMessage + " </body></html>";
            message.IsBodyHtml = true;
 
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };
            smtpClient.Send(message);
        }
 
    }
}
 