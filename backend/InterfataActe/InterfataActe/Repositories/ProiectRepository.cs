using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Brockers;
using InterfataActe.Models;
using InterfataActe.Interfaces;
using Microsoft.EntityFrameworkCore;
using InterfataActe.DTO;

namespace InterfataActe.Repositories
{
    public class ProiectRepository:IProiectRepository
    {
        private readonly InterfataActeBrocker _context;


        public ProiectRepository(InterfataActeBrocker context)
        {
            _context = context;
        }

        public Proiect CreateProiect(Proiect proiect)
        {
            _context.Proiecte.Add(proiect);
            _context.SaveChanges();
            return proiect;
        }

        public Proiect GetProiectByIdInclude(Guid Id)
        {
            return _context.Proiecte.Where(x => x.ID == Id).
                Include(x => x.Client).
                Include(x => x.Firma).
                Include(x => x.Atr).FirstOrDefault();
        }

        public Proiect ModifyProiect(Proiect proiect)
        {
            _context.Update(proiect);
            _context.SaveChanges();
            return proiect;
        }

        public List<Proiect> GetAllProiecte()
        {
            return _context.Proiecte.ToList();
        }

        public List<Proiect> GetAllProiecteIncludeNull()
        {
            return _context.Proiecte.
                Include(x => x.Client).
                Include(x => x.Firma).
                Include(x => x.Atr).OrderByDescending(x => x.NrProiect).
                Include(x => x.ContorContract).
                Include(x => x.ContorProcesVerbal).
                Include(x => x.ContorBuletinCabluri).
                Include(x => x.ContorBuletinPrize).Where(x=>x.Atr == null && x.ContorContract == null && x.ContorProcesVerbal == null && x.ContorBuletinCabluri.Count <= 0 && x.ContorBuletinPrize.Count <= 0).ToList();
        }

        public List<Proiect> GetAllProiecteInclude()
        {
            return _context.Proiecte.
                Include(x => x.Client).
                Include(x => x.Firma).
                Include(x => x.Atr).OrderByDescending(x=>x.NrProiect).
                Include(x => x.ContorContract).
                Include(x => x.ContorProcesVerbal).
                Include(x => x.ContorBuletinCabluri).
                Include(x => x.ContorBuletinPrize).ToList();
        }

        public List<DTORegistru> GetRegistru()
        {
            List<DTORegistru> registru = new List<DTORegistru>();

            var proiecte =  _context.Proiecte.
                Include(x => x.Client).
                Include(x => x.Firma).
                Include(x => x.Atr).OrderByDescending(x => x.NrProiect).
                Include(x => x.ContorContract).
                Include(x => x.ContorProcesVerbal).
                Include(x => x.ContorBuletinCabluri).
                Include(x => x.ContorBuletinPrize).ToList();

            foreach (var proiect in proiecte)
            {
                if (proiect == null) continue;
                
                try
                {
                    var dto = new DTORegistru
                    {
                        NrProiect = proiect.NrProiect.ToString() ?? "",
                        NumeClient = proiect.Client?.Nume ?? "",
                        NumeFirma  = proiect.Firma?.Nume ?? "",
                        NrAtr =(proiect.Atr?.NrAtr.ToString() ?? "") + (proiect.Atr?.DataAtr.ToString("dd.MM.yyyy") ?? ""),
                        Denumire = proiect.Denumire ?? "",
                        Pret = proiect.Atr?.Pret.ToString() ?? "",
                        Localitatea = proiect.Localitatea ?? "",
                        Strada = proiect.Strada ?? "",
                        NrStrada = proiect.NrStrada?.ToString() ?? "",
                        Apartament = proiect.Apartament ?? "",
                        Data = proiect.Data.ToString("dd.MM.yyyy") ?? "",
                        Contract = proiect.ContorContract?.Contor.ToString() ?? "",
                        ProcesVerbal = proiect.ContorProcesVerbal?.Contor.ToString() ?? "",
                        ProcesVerbalCablu =(proiect.ContorProcesVerbal?.Contor.ToString() ?? "") + "A",
                        BuletinCablu = GetContorString(proiect.ContorBuletinCabluri),
                        BuletinPriza = GetContorString(proiect.ContorBuletinPrize)
                    };

                    registru.Add(dto);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing project {proiect.NrProiect}: {ex.Message}");
                    continue;
                }
            }            

            return registru;
        }

        private string GetContorString(IEnumerable<dynamic> contori)
        {
            try
            {
                if (contori == null) return "-";
                
                var contorStrings = contori.Select(x => x.Contor.ToString()).ToList();
                return contorStrings.Any() ? string.Join(",", contorStrings) : "-";
            }
            catch
            {
                return "-";
            }
        }

        public Proiect GetProiectById(Guid Id)
        {
            var proiect = _context.Proiecte.Where(x => x.ID == Id).
                Include(x=>x.ContorContract).
                Include(x=>x.ContorProcesVerbal).FirstOrDefault();
            return proiect;
        }


    }
}
