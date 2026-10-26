using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using Microsoft.EntityFrameworkCore;
using InterfataActe.Repositories;
using InterfataActe.Interfaces;
using InterfataActe.Helpers;
using Microsoft.AspNetCore.Identity.UI.Services;
using InterfataActe.Services;
using System.Text.Json;

namespace InterfataActe
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            services.AddEntityFrameworkNpgsql().AddDbContext<InterfataActeBrocker>(opt => opt.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), o => o.MigrationsHistoryTable("__EFMigrationsHistory", "first")));

            services.AddScoped<IAtrRepository, AtrRepository>();

            services.AddScoped<IProiectContorBuletinCabluRepository, ProiectContorBuletinCabluRepository>();

            services.AddScoped<IProiectContorBuletinPrizaRepository, ProiectContorBuletinPrizaRepository>();

            services.AddScoped<IProiectContorContractRepository, ProiectContorContractRepository>();

            services.AddScoped<IProiectContorProcesVerbalRepository, ProiectContorProcesVerbalRepository>();

            services.AddScoped<IClientRepository, ClientRepository>();

            services.AddScoped<IEmailRepository, EmailRepository>();

            services.AddScoped<IFirmaRepository, FirmaRepository>();

            services.AddScoped<IFisierRepository, FisierRepository>();

            services.AddScoped<IFolderRepository, FolderRepository>();

            services.AddScoped<IGrupRepository, GrupRepository>();

            services.AddScoped<IProiectRepository, ProiectRepository>();
            services.AddScoped<IProiectFotovoltaiceRepository, ProiectFotovoltaiceRepository>();


            services.AddScoped<Word2>();

            services.AddTransient<IEmailSender, EmailSender>();

            services.AddHostedService<AutoSendEmailService>();

            services.AddCors();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Disable HTTPS redirection to allow HTTP connections from any IP
            // app.UseHttpsRedirection();

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
