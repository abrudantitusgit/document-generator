using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InterfataActe.Models;

namespace InterfataActe.Brockers
{
    public class InterfataActeBrocker : DbContext
    {
        public InterfataActeBrocker(DbContextOptions<InterfataActeBrocker> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("first");
            //Contoare
            modelBuilder.HasSequence<long>("ContorBuletinCablu").StartsAt(1168).IncrementsBy(1);
            modelBuilder.HasSequence<long>("ContorBuletinPriza").StartsAt(1794).IncrementsBy(1);
            modelBuilder.HasSequence<long>("ContorContract").StartsAt(180).IncrementsBy(1);
            modelBuilder.HasSequence<long>("ContorProcesVerbal").StartsAt(1055).IncrementsBy(1);
            modelBuilder.HasSequence<long>("NrProiect").StartsAt(1).IncrementsBy(1);

            //ATR
            modelBuilder.Entity<Atr>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Atr>().HasIndex(x => x.ProiectID).IsUnique();
            modelBuilder.Entity<Atr>().HasIndex(x => x.NrAtr).IsUnique();

            //Client
            modelBuilder.Entity<Client>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Client>().HasIndex(x => x.Nume).IsUnique();
            modelBuilder.Entity<Client>().HasIndex(x => x.CNP).IsUnique();

            //Email
            modelBuilder.Entity<Email>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
   
            //Firma
            modelBuilder.Entity<Firma>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Firma>().HasIndex(x => x.CUI).IsUnique();
            modelBuilder.Entity<Firma>().HasIndex(x => x.ORC).IsUnique();

            //Fisier
            modelBuilder.Entity<Fisier>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Fisier>().HasIndex(x => x.Nume).IsUnique();

            //Folder
            modelBuilder.Entity<Folder>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Folder>().HasIndex(x => x.Nume).IsUnique();

            //Grup
            modelBuilder.Entity<Grup>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Grup>().HasIndex(x => x.Nume).IsUnique();

            //Proiect
            modelBuilder.Entity<Proiect>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Proiect>().Property(x => x.NrProiect).HasDefaultValueSql("nextval('\"NrProiect\"')");

             modelBuilder.Entity<ProiectFotovoltaice>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
             modelBuilder.Entity<DataInvertor>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
             modelBuilder.Entity<DataPanou>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
             modelBuilder.Entity<DataBaterie>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

            //ProiectContorBuletinCablu
            modelBuilder.Entity<ProiectContorBuletinCablu>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<ProiectContorBuletinCablu>().Property(x => x.Contor).HasDefaultValueSql("nextval('\"ContorBuletinCablu\"')");

            //ProiectContorBuletinPriza
            modelBuilder.Entity<ProiectContorBuletinPriza>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<ProiectContorBuletinPriza>().Property(x => x.Contor).HasDefaultValueSql("nextval('\"ContorBuletinPriza\"')");

            //ProiectContorContract
            modelBuilder.Entity<ProiectContorContract>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<ProiectContorContract>().Property(x => x.Contor).HasDefaultValueSql("nextval('\"ContorContract\"')");

            //ProiectContorProcesVerbal
            modelBuilder.Entity<ProiectContorProcesVerbal>().Property(x => x.ID).HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<ProiectContorProcesVerbal>().Property(x => x.Contor).HasDefaultValueSql("nextval('\"ContorProcesVerbal\"')");
        }

        public DbSet<Atr> Atr { get; set; }
        public DbSet<Client> Clienti { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<Firma> Firme { get; set; }
        public DbSet<Fisier> Fisiere { get; set; }
        public DbSet<Folder> Foldere { get; set; }
        public DbSet<ProiectFotovoltaice> ProiecteFotovoltaice { get; set; }
        public DbSet<DataInvertor> DataInvertor { get; set; }
        public DbSet<DataPanou> DataPanou { get; set; }
        public DbSet<DataBaterie> DataBaterie { get; set; }
        public DbSet<Grup> Grupe { get; set; }
        public DbSet<Proiect> Proiecte { get; set; }
        public DbSet<ProiectContorBuletinCablu> ProiectContorBuletinCabluri { get; set; }
        public DbSet<ProiectContorBuletinPriza> ProiectContorBuletinPrize { get; set; }
        public DbSet<ProiectContorContract> ProiectContorContracte { get; set; }
        public DbSet<ProiectContorProcesVerbal> ProiectContorProcesVerbale { get; set; }

    } 
}
