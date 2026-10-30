using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InterfataActe.Brockers;
using InterfataActe.Drawings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InterfataActe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            if (args.Length > 0 && args[0] == "--generate-samples")
            {
                Environment.Exit(GenerateSamples(args.Length > 1 ? args[1] : "generatedDraws"));
            }
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<InterfataActeBrocker>();
                database.Database.OpenConnection();
                using (var command = database.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT to_regclass('first.\"Clienti\"') IS NOT NULL";
                    var hasClientTable = (bool)command.ExecuteScalar();
                    if (!hasClientTable)
                    {
                        database.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS first.\"__EFMigrationsHistory\"");
                    }
                }
                database.Database.CloseConnection();
                // The checked-in baseline assumes an already populated legacy
                // database, so it cannot be applied to a fresh local install.
                // EnsureCreated builds the current EF model for first-run Docker.
                database.Database.EnsureCreated();
            }
            host.Run();
        }

        // Writes the generatedDraws catalogue without starting the web host or touching the database.
        private static int GenerateSamples(string folder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            var options = configuration.GetSection("Drawings").Get<DrawingOptions>() ?? new DrawingOptions();
            return SampleCatalog.Generate(folder, new DrawingService(Options.Create(options)));
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
