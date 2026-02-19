using InterfataActe.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InterfataActe.Services
{
    public class AutoSendEmailService:IHostedService
    {
        private Timer _timer;

        public readonly IEmailSender _email;
        public readonly IEmailRepository _emailRepository;

        public AutoSendEmailService(IServiceScopeFactory factory)
        {
            _email = factory.CreateScope().ServiceProvider.GetRequiredService<IEmailSender>();
            _emailRepository = factory.CreateScope().ServiceProvider.GetRequiredService<IEmailRepository>();

        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(HelloWorld, null, 0, 1000 * 60 * 30);
            return Task.CompletedTask;
        }
        void HelloWorld(object state)
        {
            var listToSend = _emailRepository.ListToSend();
            listToSend.ForEach(x =>
            {
                _email.SendEmailAsync(x.Adresa,"OVINSTAL ELECTRIC", x.Text);
                _emailRepository.DezactivateEmail(x.Id);
            });


        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            //New Timer does not have a stop. 
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
