using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cors;

using InterfataActe.DTO;
using InterfataActe.Interfaces;
using InterfataActe.Models;

using Xceed.Words.NET;
using System.Reflection.Metadata;
using InterfataActe.Drawings;

namespace InterfataActe.Helpers
{

    public class Word2
    {

        private readonly IFisierRepository _fisier;
        private readonly DrawingService _drawings;

        public Word2(IFisierRepository fisier, DrawingService drawings)
        {
            _fisier = fisier;
            _drawings = drawings;
        }
        public Boolean GenerateBransament(Imprima imprima)

        {

            Atr atr = imprima.Proiect.Atr; //
            Client client = imprima.Proiect.Client; //
            Firma firma = imprima.Proiect.Firma; //
            var clientfirma = new ClientFirma(client.Nume, client.CNP, client.Eliberat, client.DataBuletin, client.Seria, client.NrSeria, client.Judet, client.Localitatea, client.Comuna, client.Strada, client.Sector, client.NrStrada, client.Bloc, client.Scara, client.Etaj, client.Apartament, client.CodPostal, client.Telefon, DateTime.Now, "-", "-", "-");
            Grup grup = imprima.Grup; //
            Proiect proiect = imprima.Proiect; //
            if (firma != null) clientfirma = new ClientFirma(firma.Nume, firma.CUI, "-", DateTime.Now, "-", "0", firma.Judet, firma.Localitatea, firma.Comuna, firma.Strada, firma.Sector, firma.NrStrada, firma.Bloc, firma.Scara, firma.Etaj, firma.Apartament, firma.CodPostal, firma.Telefon, firma.DataCUI, firma.ORC, firma.Functia, client.Nume);

            var contorContract = proiect.ContorContract == null ? 0 : proiect.ContorContract.Contor;
            var contorProcesVerbal = proiect.ContorProcesVerbal == null ? 0 : proiect.ContorProcesVerbal.Contor;

            var buletinCablu = proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCablu" || x.Tag=="BuletinCabluManual").FirstOrDefault() == null ? 0 : proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCablu" || x.Tag == "BuletinCabluManual").FirstOrDefault().Contor;
            var buletinPriza = proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPriza" || x.Tag == "BuletinPrizaManual").FirstOrDefault() == null ? 0 : proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPriza" || x.Tag == "BuletinPrizaManual").FirstOrDefault().Contor;
            var buletinCabluUtilizare = proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCabluUtilizare" || x.Tag=="BuletinCabluManual").FirstOrDefault() == null ? 0 : proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCabluUtilizare" || x.Tag=="BuletinCabluManual").FirstOrDefault().Contor;
            var buletinPrizaUtilizare = proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPrizaUtilizare" || x.Tag == "BuletinPrizaManual").FirstOrDefault() == null ? 0 : proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPrizaUtilizare"|| x.Tag == "BuletinPrizaManual").FirstOrDefault().Contor;
            Console.WriteLine(buletinPriza);
            foreach (var fold in grup.Foldere)

                foreach (var file in fold.Fisiere)
                {

                    //MODEL FILE EXIST ?

                    //D:\Data\Public\Documente\TempWord\
                    var pathload = $"/mnt/share/temp/{grup.Nume}/{fold.Nume}/{file.Nume}.docx";
                   
                    
                    if (File.Exists(pathload))
                    {
                         Console.WriteLine(pathload);      
                        var folderPath = $"/mnt/share/clientiBransamente/{client.Nume}/{grup.Nume}/{proiect.NrProiect}.Proiect/{fold.Nume}";
                        var path = $"{folderPath}/{file.Nume}";
                        
                       
                        if (atr != null)
                        {
                            if (file.DenumireContor == "Contract") path += " - Nr." + contorContract.ToString();

                            if (file.DenumireContor == "Proces Verbal") path += " - Nr." + contorProcesVerbal.ToString();

                            if (file.DenumireContor == "Proces Verbal Cablu") path += " - Nr." + contorProcesVerbal.ToString() + "A";

                            if (file.DenumireContor == "Buletin cablu") path += " - Nr." + buletinCablu.ToString();

                            if (file.DenumireContor == "Buletin priza") path += " - Nr." + buletinPriza.ToString();

                            if (file.DenumireContor == "Buletin cablu utilizare") path += " - Nr." + buletinCabluUtilizare.ToString();

                            if (file.DenumireContor == "Buletin priza utilizare") path += " - Nr." + buletinPrizaUtilizare.ToString();
                        }
                        path += ".docx";
                        
                        if (!File.Exists(path))
                        {
                            
                            if (!Directory.Exists(folderPath))                      
                                Directory.CreateDirectory(folderPath);
                            
                            using (var document = DocX.Load(pathload))
                            {
                                
                   
                        foreach (var paragraph in document.Paragraphs)
{
    if (atr!=null){
    // ATR
    paragraph.ReplaceText("<Atr.NrAtr>", atr.NrAtr.ToString());
    paragraph.ReplaceText("<Atr.DataAtr>", atr.DataAtr.ToString("dd.MM.yyyy"));
    paragraph.ReplaceText("<Atr.NrCadastral>", atr.NrCadastral.ToString());
    paragraph.ReplaceText("<Atr.CodPostal>", atr.CodPostal.ToString());
    paragraph.ReplaceText("<Atr.PunctRacordare>", atr.PunctRacordare.ToString());
    paragraph.ReplaceText("<Atr.PunctAlimentare>", atr.PunctAlimentare.ToString());
    paragraph.ReplaceText("<Atr.PunctBransament>", atr.PunctBransament.ToString());
    paragraph.ReplaceText("<Atr.Cablu>", atr.Cablu.ToString());
    paragraph.ReplaceText("<Atr.LungimeCablu>", atr.LungimeCablu.ToString());
    paragraph.ReplaceText("<Atr.LungimeCabluStalp>", (atr.LungimeCablu - 10).ToString());
    paragraph.ReplaceText("<Atr.Ohmi>", atr.Ohmi.ToString());
    paragraph.ReplaceText("<Atr.Contor>", atr.Contor.ToString());
    paragraph.ReplaceText("<Atr.Temperatura>", atr.Temperatura.ToString());
    paragraph.ReplaceText("<Atr.Descriere>", atr.Descriere.ToString());
    paragraph.ReplaceText("<Atr.Pret>", atr.Pret.ToString());
    paragraph.ReplaceText("<Atr.Tva>", atr.Tva.ToString());
    }
    // Client
    paragraph.ReplaceText("<Client.Nume>", client.Nume.ToString());
    paragraph.ReplaceText("<Client.CNP>", client.CNP.ToString());
    paragraph.ReplaceText("<Client.Eliberat>", client.Eliberat.ToString());
    paragraph.ReplaceText("<Client.DataBuletin>", client.DataBuletin.ToString("dd.MM.yyyy"));
    paragraph.ReplaceText("<Client.Seria>", client.Seria.ToString());
    paragraph.ReplaceText("<Client.NrSeria>", client.NrSeria.ToString());
    paragraph.ReplaceText("<Client.Judet>", client.Judet.ToString());
    paragraph.ReplaceText("<Client.Localitatea>", client.Localitatea.ToString());
    paragraph.ReplaceText("<Client.Comuna>", client.Comuna.ToString());
    paragraph.ReplaceText("<Client.Strada>", client.Strada.ToString());
    paragraph.ReplaceText("<Client.Sector>", client.Sector.ToString());
    paragraph.ReplaceText("<Client.NrStrada>", client.NrStrada.ToString());
    paragraph.ReplaceText("<Client.Bloc>", client.Bloc.ToString());
    paragraph.ReplaceText("<Client.Scara>", client.Scara.ToString());
    paragraph.ReplaceText("<Client.Etaj>", client.Etaj.ToString());
    paragraph.ReplaceText("<Client.Apartament>", client.Apartament.ToString());
    paragraph.ReplaceText("<Client.CodPostal>", client.CodPostal.ToString());
    paragraph.ReplaceText("<Client.Telefon>", client.Telefon.ToString());

    // Firma
    if (firma != null)
    {
        paragraph.ReplaceText("<Firma.Nume>", firma.Nume.ToString());
        paragraph.ReplaceText("<Firma.CUI>", firma.CUI.ToString());
        paragraph.ReplaceText("<Firma.DataCUI>", firma.DataCUI.ToString("dd.MM.yyyy"));
        paragraph.ReplaceText("<Firma.ORC>", firma.ORC.ToString());
        paragraph.ReplaceText("<Firma.Functia>", firma.Functia.ToString());
        paragraph.ReplaceText("<Firma.Judet>", firma.Judet.ToString());
        paragraph.ReplaceText("<Firma.Localitatea>", firma.Localitatea.ToString());
        paragraph.ReplaceText("<Firma.Comuna>", firma.Comuna.ToString());
        paragraph.ReplaceText("<Firma.Strada>", firma.Strada.ToString());
        paragraph.ReplaceText("<Firma.Sector>", firma.Sector.ToString());
        paragraph.ReplaceText("<Firma.NrStrada>", firma.NrStrada.ToString());
        paragraph.ReplaceText("<Firma.Bloc>", firma.Bloc.ToString());
        paragraph.ReplaceText("<Firma.Scara>", firma.Scara.ToString());
        paragraph.ReplaceText("<Firma.Etaj>", firma.Etaj.ToString());
        paragraph.ReplaceText("<Firma.Apartament>", firma.Apartament.ToString());
        paragraph.ReplaceText("<Firma.CodPostal>", firma.CodPostal.ToString());
        paragraph.ReplaceText("<Firma.Telefon>", firma.Telefon.ToString());
    }

    // ClientFirma
    paragraph.ReplaceText("<ClientFirma.Nume>", clientfirma.Nume.ToString());
    paragraph.ReplaceText("<ClientFirma.CNPCUI>", clientfirma.CnpCui.ToString());
    paragraph.ReplaceText("<ClientFirma.Eliberat>", clientfirma.Eliberat.ToString());
    paragraph.ReplaceText("<ClientFirma.DataBuletin", clientfirma.DataBuletin.ToString("dd.MM.yyyy"));
    paragraph.ReplaceText("<ClientFirma.Seria>", clientfirma.Seria.ToString());
    paragraph.ReplaceText("<ClientFirma.NrSeria>", clientfirma.NrSeria.ToString());
    paragraph.ReplaceText("<ClientFirma.Judet>", clientfirma.Judet.ToString());
    paragraph.ReplaceText("<ClientFirma.Localitatea>", clientfirma.Localitatea.ToString());
    paragraph.ReplaceText("<ClientFirma.Comuna>", clientfirma.Comuna.ToString());
    paragraph.ReplaceText("<ClientFirma.Strada>", clientfirma.Strada.ToString());
    paragraph.ReplaceText("<ClientFirma.Sector>", clientfirma.Sector.ToString());
    paragraph.ReplaceText("<ClientFirma.NrStrada>", clientfirma.NrStrada.ToString());
    paragraph.ReplaceText("<ClientFirma.Bloc>", clientfirma.Bloc.ToString());
    paragraph.ReplaceText("<ClientFirma.Scara>", clientfirma.Scara.ToString());
    paragraph.ReplaceText("<ClientFirma.Etaj>", clientfirma.Etaj.ToString());
    paragraph.ReplaceText("<ClientFirma.Apartament>", clientfirma.Apartament.ToString());
    paragraph.ReplaceText("<ClientFirma.CodPostal>", clientfirma.CodPostal.ToString());
    paragraph.ReplaceText("<ClientFirma.Telefon>", clientfirma.Telefon.ToString());
    paragraph.ReplaceText("<ClientFirma.DataCUI>", clientfirma.DataCUI.ToString("dd.MM.yyyy"));
    paragraph.ReplaceText("<ClientFirma.ORC>", clientfirma.ORC.ToString());
    paragraph.ReplaceText("<ClientFirma.Functia>", clientfirma.Functia.ToString());
    paragraph.ReplaceText("<ClientFirma.Reprezentant>", clientfirma.Reprezentant.ToString());

    // Proiect
paragraph.ReplaceText("<Proiect.NrProiect>", proiect.NrProiect.ToString());
paragraph.ReplaceText("<Proiect.Denumire>", proiect.Denumire.ToString());
paragraph.ReplaceText("<Proiect.Judet>", proiect.Judet.ToString());
paragraph.ReplaceText("<Proiect.Sector>", proiect.Sector.ToString());
paragraph.ReplaceText("<Proiect.Localitatea>", proiect.Localitatea.ToString());
paragraph.ReplaceText("<Proiect.Comuna>", proiect.Comuna.ToString());
paragraph.ReplaceText("<Proiect.Strada>", proiect.Strada.ToString());
paragraph.ReplaceText("<Proiect.NrStrada>", proiect.NrStrada.ToString());
paragraph.ReplaceText("<Proiect.Bloc>", proiect.Bloc.ToString());
paragraph.ReplaceText("<Proiect.Scara>", proiect.Scara.ToString());
paragraph.ReplaceText("<Proiect.Etaj>", proiect.Etaj.ToString());
paragraph.ReplaceText("<Proiect.Apartament>", proiect.Apartament.ToString());
paragraph.ReplaceText("<Proiect.Pi>", proiect.Pi.ToString());
paragraph.ReplaceText("<Proiect.Pa>", proiect.Pa.ToString());
paragraph.ReplaceText("<Proiect.LocConsum>", proiect.LocConsum.ToString());
paragraph.ReplaceText("<Proiect.TensiuneUtilizare>", proiect.TensiuneUtilizare.ToString());
paragraph.ReplaceText("<Proiect.Contract>", contorContract.ToString());
paragraph.ReplaceText("<Proiect.ProcesVerbal>", contorProcesVerbal.ToString());
paragraph.ReplaceText("<Proiect.ProcesVerbalCablu>", contorProcesVerbal.ToString() + "A");
paragraph.ReplaceText("<Proiect.BuletinCablu>", buletinCablu.ToString());
paragraph.ReplaceText("<Proiect.BuletinPriza>", buletinPriza.ToString());
paragraph.ReplaceText("<Proiect.BuletinCabluUtilizare>", buletinCabluUtilizare.ToString());
paragraph.ReplaceText("<Proiect.BuletinPrizaUtilizare>", buletinPrizaUtilizare.ToString());
paragraph.ReplaceText("<Proiect.Data>", proiect.Data.ToString("dd.MM.yyyy"));
paragraph.ReplaceText("<Proiect.DataScurta>", proiect.Data.ToString("dd.MM.yy"));
paragraph.ReplaceText("<PRM_Masurat>", proiect.PRM_Masurat.ToString());
paragraph.ReplaceText("<PRM_Calculat>", proiect.PRM_Calculat.ToString());
paragraph.ReplaceText("<Proiect.DataPlus>", proiect.Data.AddMonths(1).ToString("dd.MM.yyyy"));

}
                               
                                document.SaveAs(path);
                                Console.WriteLine("PATH FOLDER: "+path);
                            }

                        }
                    }
                    else{
                         Console.WriteLine("File not exist " + pathload); 
                    }
                   
                }

            if (imprima.GenereazaDesene)
            {
                try
                {
                    var desene = _drawings.GenerateForBransament(proiect, DrawingService.BransamentFolder(proiect, grup.Nume), false);
                    desene.Messages.ForEach(Console.WriteLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Eroare la generarea desenelor: " + ex.Message);
                }
            }
            return true;
        }


        private void ReplacePlaceholders(DocX document,Dictionary<string, string> placeholders)
        {
            foreach (var kvp in placeholders)
            {
                document.ReplaceText(kvp.Key, kvp.Value ?? string.Empty);
            }
        }



        public GenerateResult GenerateFotovoltaice(ImprimaFotovoltaice imprima)
        {
             var result = new GenerateResult();
            try
            {
                
            
            Grup grup =imprima.Grup;
            ProiectFotovoltaice proiect = imprima.Proiect;
            decimal putereMax=Math.Min(proiect.PutereTotalaInvertor,proiect.PutereTotalaPanouri/1000);
            var resolver= new TemplateResolver();
            string S(object? v) => v?.ToString() ?? "";

            var placeholders = new Dictionary<string, string>
            {
                ["{{Client.Nume}}"] = proiect.Client?.Nume ?? "-",
                ["{{Client.CNP}}"] = S(proiect.Client?.CNP) ?? "-",
                ["{{Client.Seria}}"] = S(proiect.Client?.Seria) ?? "-",
                ["{{Client.NrSeria}}"] = S(proiect.Client?.NrSeria) ?? "-",
                ["{{Client.Judet}}"] = proiect.Client?.Judet ?? "-",
                ["{{Client.Comuna}}"] = proiect.Client?.Comuna ?? "-",
                ["{{Client.Localitate}}"] = proiect.Client?.Localitatea ?? "-",
                ["{{Client.CodPostal}}"] = proiect.Client?.CodPostal ?? "-",
                ["{{Client.Strada}}"] = proiect.Client?.Strada ?? "-",
                ["{{Client.NrStrada}}"] = proiect.Client?.NrStrada ?? "-",
                ["{{Client.Bloc}}"] = proiect.Client?.Bloc ?? "-",
                ["{{Client.Scara}}"] = proiect.Client?.Scara ?? "-",
                ["{{Client.Etaj}}"] = proiect.Client?.Etaj ?? "-",
                ["{{Client.Apartament}}"] = proiect.Client?.Apartament ?? "-",
                ["{{Client.Telefon}}"] = proiect.Client?.Telefon ?? "-",

                ["{{Firma.Nume}}"] = proiect.Firma?.Nume ?? "-",
                ["{{Firma.CUI}}"] = S(proiect.Firma?.CUI) ?? "-",
                ["{{Firma.ORC}}"] = proiect.Firma?.ORC ?? "-",
                ["{{Firma.DataCUI}}"] = S(proiect.Firma?.DataCUI) ?? "-",
                ["{{Firma.Functia}}"] = proiect.Firma?.Functia ?? "-",
                ["{{Firma.Judet}}"] = proiect.Firma?.Localitatea ?? "-",
                ["{{Firma.Localitate}}"] = proiect.Firma?.Localitatea ?? "-",
                ["{{Firma.Comuna}}"] = proiect.Firma?.Comuna ?? "-",
                ["{{Firma.Strada}}"] = proiect.Firma?.Strada ?? "-",
                ["{{Firma.NrStrada}}"] = proiect.Firma?.NrStrada ?? "-",
                ["{{Firma.Bloc}}"] = proiect.Firma?.Bloc ?? "-",
                ["{{Firma.Scara}}"] = proiect.Firma?.Scara ?? "-",
                ["{{Firma.Etaj}}"] = proiect.Firma?.Etaj ?? "-",
                ["{{Firma.Apartament}}"] = proiect.Firma?.Apartament ?? "-",
                ["{{Firma.CodPostal}}"] = proiect.Firma?.CodPostal ?? "-",
                ["{{Firma.Telefon}}"] = proiect.Firma?.Telefon ?? "-",

                ["{{NrProiect}}"] = S(proiect.NrProiect),
                ["{{Proiect.Judet}}"] = proiect.Judet ?? "-",
                ["{{Proiect.Localitate}}"] = proiect.Localitatea ?? "-",
                ["{{Proiect.Comuna}}"] = proiect.Comuna ?? "-",
                 ["{{Proiect.Strada}}"] = proiect.Strada ?? "-",
                ["{{Proiect.NrStrada}}"] = proiect.NrStrada ?? "-",
                ["{{Proiect.Bloc}}"] = proiect.Bloc ?? "-",
                ["{{Proiect.Scara}}"] = proiect.Scara ?? "-",
                ["{{Proiect.Etaj}}"] = proiect.Etaj ?? "-",
                ["{{Proiect.Apartament}}"] = proiect.Apartament ?? "-",
                ["{{Proiect.NrCF}}"] = proiect.NrCF ?? "-",
                ["{{Proiect.NrCadastral}}"] = proiect.NrCadastral ?? "-",
                ["{{Proiect.Amplasare}}"] = proiect.Amplasare ?? "-",
                ["{{Proiect.POD}}"] = proiect.POD ?? "-",
                ["{{Proiect.PutereMax}}"] = S(putereMax),

                ["{{Proiect.NrPanouri1}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(0)?.NrPanouri),
                ["{{Proiect.NrPanouri2}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(1)?.NrPanouri),
                ["{{Proiect.NrPanouri3}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(2)?.NrPanouri),
                ["{{Proiect.NrPanouri4}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(3)?.NrPanouri),
                ["{{Proiect.NrPanouri5}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(4)?.NrPanouri),

                ["{{Proiect.ModelPanou1}}"] = proiect.DatePanouri?.ElementAtOrDefault(0)?.Model ?? "-",
                ["{{Proiect.ModelPanou2}}"] = proiect.DatePanouri?.ElementAtOrDefault(1)?.Model ?? "-",
                ["{{Proiect.ModelPanou3}"] = proiect.DatePanouri?.ElementAtOrDefault(2)?.Model ?? "-",
                ["{{Proiect.ModelPanou4}}"] = proiect.DatePanouri?.ElementAtOrDefault(3)?.Model ?? "-",
                ["{{Proiect.ModelPanou5}}"] = proiect.DatePanouri?.ElementAtOrDefault(4)?.Model ?? "-",

                ["{{Proiect.PuterePanou1}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(0)?.PutereInstalata),
                ["{{Proiect.PuterePanou2}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(1)?.PutereInstalata),
                ["{{Proiect.PuterePanou3}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(2)?.PutereInstalata),
                ["{{Proiect.PuterePanou4}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(3)?.PutereInstalata),
                ["{{Proiect.PuterePanou5}}"] = S(proiect.DatePanouri?.ElementAtOrDefault(4)?.PutereInstalata),

                ["{{Proiect.PutereTotalaPanou1}}"] =
                    S((proiect.DatePanouri?.ElementAtOrDefault(0)?.PutereInstalata ?? 0) *
                    (proiect.DatePanouri?.ElementAtOrDefault(0)?.NrPanouri ?? 0)),
                ["{{Proiect.PutereTotalaPanou2}}"] =
                    S((proiect.DatePanouri?.ElementAtOrDefault(1)?.PutereInstalata ?? 0) *
                    (proiect.DatePanouri?.ElementAtOrDefault(1)?.NrPanouri ?? 0)),
                ["{{Proiect.PutereTotalaPanou3}}"] =
                    S((proiect.DatePanouri?.ElementAtOrDefault(2)?.PutereInstalata ?? 0) *
                    (proiect.DatePanouri?.ElementAtOrDefault(2)?.NrPanouri ?? 0)),
                ["{{Proiect.PutereTotalaPanou4}}"] =
                    S((proiect.DatePanouri?.ElementAtOrDefault(3)?.PutereInstalata ?? 0) *
                    (proiect.DatePanouri?.ElementAtOrDefault(3)?.NrPanouri ?? 0)),
                ["{{Proiect.PutereTotalaPanou5}}"] =
                    S((proiect.DatePanouri?.ElementAtOrDefault(4)?.PutereInstalata ?? 0) *
                    (proiect.DatePanouri?.ElementAtOrDefault(4)?.NrPanouri ?? 0)),

                ["{{Proiect.SeriiPanouri1}}"] = proiect.DatePanouri?.ElementAtOrDefault(0)?.Serie ?? "-",
                ["{{Proiect.SeriiPanouri2}}"] = proiect.DatePanouri?.ElementAtOrDefault(1)?.Serie ?? "-",
                ["{{Proiect.SeriiPanouri3}}"] = proiect.DatePanouri?.ElementAtOrDefault(2)?.Serie ?? "-",
                ["{{Proiect.SeriiPanouri4}}"] = proiect.DatePanouri?.ElementAtOrDefault(3)?.Serie ?? "-",
                ["{{Proiect.SeriiPanouri5}}"] = proiect.DatePanouri?.ElementAtOrDefault(4)?.Serie ?? "-",

                ["{{Proiect.PutereTotalaPanouri}}"] = (proiect.PutereTotalaPanouri/1000).ToString("F3"),

                ["{{Proiect.PutereMax1}}"] = S(Math.Min(
                (proiect.DatePanouri?.ElementAtOrDefault(0)?.PutereInstalata ?? 0) *
                (proiect.DatePanouri?.ElementAtOrDefault(0)?.NrPanouri ?? 0),
                (proiect.DateInvertoare?.ElementAtOrDefault(0)?.PutereInstalata ?? 0) *
                (proiect.DateInvertoare?.ElementAtOrDefault(0)?.NrInvertoare ?? 0)
            )),

            ["{{Proiect.PutereMax2}}"] = S(Math.Min(
                (proiect.DatePanouri?.ElementAtOrDefault(1)?.PutereInstalata ?? 0) *
                (proiect.DatePanouri?.ElementAtOrDefault(1)?.NrPanouri ?? 0),
                (proiect.DateInvertoare?.ElementAtOrDefault(1)?.PutereInstalata ?? 0) *
                (proiect.DateInvertoare?.ElementAtOrDefault(1)?.NrInvertoare ?? 0)
            )),

            ["{{Proiect.PutereMax3}}"] = S(Math.Min(
                (proiect.DatePanouri?.ElementAtOrDefault(2)?.PutereInstalata ?? 0) *
                (proiect.DatePanouri?.ElementAtOrDefault(2)?.NrPanouri ?? 0),
                (proiect.DateInvertoare?.ElementAtOrDefault(2)?.PutereInstalata ?? 0) *
                (proiect.DateInvertoare?.ElementAtOrDefault(2)?.NrInvertoare ?? 0)
            )),

            ["{{Proiect.PutereMax4}}"] = S(Math.Min(
                (proiect.DatePanouri?.ElementAtOrDefault(3)?.PutereInstalata ?? 0) *
                (proiect.DatePanouri?.ElementAtOrDefault(3)?.NrPanouri ?? 0),
                (proiect.DateInvertoare?.ElementAtOrDefault(3)?.PutereInstalata ?? 0) *
                (proiect.DateInvertoare?.ElementAtOrDefault(3)?.NrInvertoare ?? 0)
            )),

            ["{{Proiect.PutereMax5}}"] = S(Math.Min(
                (proiect.DatePanouri?.ElementAtOrDefault(4)?.PutereInstalata ?? 0) *
                (proiect.DatePanouri?.ElementAtOrDefault(4)?.NrPanouri ?? 0),
                (proiect.DateInvertoare?.ElementAtOrDefault(4)?.PutereInstalata ?? 0) *
                (proiect.DateInvertoare?.ElementAtOrDefault(4)?.NrInvertoare ?? 0)
            )),

                ["{{Proiect.NrInvertor1}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(0)?.NrInvertoare),
                ["{{Proiect.NrInvertor2}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(1)?.NrInvertoare),
                ["{{Proiect.NrInvertor3}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(2)?.NrInvertoare),
                ["{{Proiect.NrInvertor4}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(3)?.NrInvertoare),
                ["{{Proiect.NrInvertor5}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(4)?.NrInvertoare),

                ["{{Proiect.ModelInvertor1}}"] = proiect.DateInvertoare?.ElementAtOrDefault(0)?.Model ?? "-",
                ["{{Proiect.ModelInvertor2}}"] = proiect.DateInvertoare?.ElementAtOrDefault(1)?.Model ?? "-",
                ["{{Proiect.ModelInvertor3}}"] = proiect.DateInvertoare?.ElementAtOrDefault(2)?.Model ?? "-",
                ["{{Proiect.ModelInvertor4}}"] = proiect.DateInvertoare?.ElementAtOrDefault(3)?.Model ?? "-",
                ["{{Proiect.ModelInvertor5}}"] = proiect.DateInvertoare?.ElementAtOrDefault(4)?.Model ?? "-",

                ["{{Proiect.PutereInvertor1}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(0)?.PutereInstalata),
                ["{{Proiect.PutereInvertor2}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(1)?.PutereInstalata),
                ["{{Proiect.PutereInvertor3}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(2)?.PutereInstalata),
                ["{{Proiect.PutereInvertor4}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(3)?.PutereInstalata),
                ["{{Proiect.PutereInvertor5}}"] = S(proiect.DateInvertoare?.ElementAtOrDefault(4)?.PutereInstalata),
                   
                ["{{Proiect.PutereTotalaInvertor1}}"] =
                    S((proiect.DateInvertoare?.ElementAtOrDefault(0)?.PutereInstalata ?? 0) *
                    (proiect.DateInvertoare?.ElementAtOrDefault(0)?.NrInvertoare ?? 0)),
                ["{{Proiect.PutereTotalaInvertor2}}"] =
                    S((proiect.DateInvertoare?.ElementAtOrDefault(1)?.PutereInstalata ?? 0) *
                    (proiect.DateInvertoare?.ElementAtOrDefault(1)?.NrInvertoare ?? 0)),
                ["{{Proiect.PutereTotalaInvertor3}}"] =
                    S((proiect.DateInvertoare?.ElementAtOrDefault(2)?.PutereInstalata ?? 0) *
                    (proiect.DateInvertoare?.ElementAtOrDefault(2)?.NrInvertoare ?? 0)),
                ["{{Proiect.PutereTotalaInvertor4}}"] =
                    S((proiect.DateInvertoare?.ElementAtOrDefault(3)?.PutereInstalata ?? 0) *
                    (proiect.DateInvertoare?.ElementAtOrDefault(3)?.NrInvertoare ?? 0)),
                ["{{Proiect.PutereTotalaInvertor5}}"] =
                    S((proiect.DateInvertoare?.ElementAtOrDefault(4)?.PutereInstalata ?? 0) *
                    (proiect.DateInvertoare?.ElementAtOrDefault(4)?.NrInvertoare ?? 0)),

                ["{{Proiect.SerieInvertor1}}"] = proiect.DateInvertoare?.ElementAtOrDefault(0)?.Serie ?? "-",
                ["{{Proiect.SerieInvertor2}}"] = proiect.DateInvertoare?.ElementAtOrDefault(1)?.Serie ?? "-",
                ["{{Proiect.SerieInvertor3}}"] = proiect.DateInvertoare?.ElementAtOrDefault(2)?.Serie ?? "-",
                ["{{Proiect.SerieInvertor4}}"] = proiect.DateInvertoare?.ElementAtOrDefault(3)?.Serie ?? "-",
                ["{{Proiect.SerieInvertor5}}"] = proiect.DateInvertoare?.ElementAtOrDefault(4)?.Serie ?? "-",

                ["{{Proiect.PutereTotalaInvertori}}"] = S(proiect.PutereTotalaInvertor),

                ["{{Proiect.NrBaterie1}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(0)?.NrBaterii),
                ["{{Proiect.NrBaterie2}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(1)?.NrBaterii),
                ["{{Proiect.NrBaterie3}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(2)?.NrBaterii),
                ["{{Proiect.NrBaterie4}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(3)?.NrBaterii),
                ["{{Proiect.NrBaterie5}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(4)?.NrBaterii),

                ["{{Proiect.ModelBaterie1}}"] = proiect.DateBaterii?.ElementAtOrDefault(0)?.Model ?? "-",
                ["{{Proiect.ModelBaterie2}}"] = proiect.DateBaterii?.ElementAtOrDefault(1)?.Model ?? "-",
                ["{{Proiect.ModelBaterie3}}"] = proiect.DateBaterii?.ElementAtOrDefault(2)?.Model ?? "-",
                ["{{Proiect.ModelBaterie4}}"] = proiect.DateBaterii?.ElementAtOrDefault(3)?.Model ?? "-",
                ["{{Proiect.ModelBaterie5}}"] = proiect.DateBaterii?.ElementAtOrDefault(4)?.Model ?? "-",

                ["{{Proiect.PutereBaterie1}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(0)?.PutereInstalata),
                ["{{Proiect.PutereBaterie2}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(1)?.PutereInstalata),
                ["{{Proiect.PutereBaterie3}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(2)?.PutereInstalata),
                ["{{Proiect.PutereBaterie4}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(3)?.PutereInstalata),
                ["{{Proiect.PutereBaterie5}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(4)?.PutereInstalata),

                ["{{Proiect.CapacitateBaterie1}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(0)?.Capacitate),
                ["{{Proiect.CapacitateBaterie2}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(1)?.Capacitate),
                ["{{Proiect.CapacitateBaterie3}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(2)?.Capacitate),
                ["{{Proiect.CapacitateBaterie4}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(3)?.Capacitate),
                ["{{Proiect.CapacitateBaterie5}}"] = S(proiect.DateBaterii?.ElementAtOrDefault(4)?.Capacitate),

                ["{{Proiect.SerieBaterie1}}"] = proiect.DateBaterii?.ElementAtOrDefault(0)?.Serie ?? "-",
                ["{{Proiect.SerieBaterie2}}"] = proiect.DateBaterii?.ElementAtOrDefault(1)?.Serie ?? "-",
                ["{{Proiect.SerieBaterie3}}"] = proiect.DateBaterii?.ElementAtOrDefault(2)?.Serie ?? "-",
                ["{{Proiect.SerieBaterie4}}"] = proiect.DateBaterii?.ElementAtOrDefault(3)?.Serie ?? "-",
                ["{{Proiect.SerieBaterie5}}"] = proiect.DateBaterii?.ElementAtOrDefault(4)?.Serie ?? "-",
                ["{{Proiect.SmartMeter}}"] = proiect.SmartMeter ?? "-",
                ["{{Proiect.Backup}}"]=proiect.Backup ?? "-",
                ["{{Proiect.CapacitateTotalaBaterii}}"] = S(proiect.CapacitateTotalaBaterii),
                ["{{Proiect.PutereTotalaBaterii}}"] = S(proiect.DateBaterii.Sum(x=>x.PutereInstalata)),
                ["{{Data}}"] = S(proiect.Data.ToString("dd.MM.yyyy")),
                ["{{Proiect.PRM_Masurat}}"] = S(proiect.PRM_Masurat),
                ["{{Proiect.PRM_Calculat}}"] = S(proiect.PRM_Calculat),
                ["{{Proiect.NrPV}}"] = S(proiect.NrPV)
            };

             foreach (var fold in grup.Foldere)

                foreach (var file in fold.Fisiere)
                {

                    //MODEL FILE EXIST ?

                    //D:\Data\Public\Documente\TempWord\
                    var templateName = resolver.Resolve(proiect, file.Nume);
                    var pathload = $"/mnt/share/temp/{grup.Nume}/{fold.Nume}/{templateName}.docx";

                        try
                        {
                                                
                          if (File.Exists(pathload))
                            {
                                Console.WriteLine(pathload);      
                                var folderPath = $"/mnt/share/clientiFotovoltaice/{proiect.Client.Nume}/{proiect.NrProiect}.Proiect/{fold.Nume}";
                                
                                var path = $"{folderPath}/{templateName}";
                                
                                path += ".docx";
                                
                                if (!File.Exists(path))
                                {
                                    
                                    if (!Directory.Exists(folderPath))                      
                                        Directory.CreateDirectory(folderPath);

                                    Console.WriteLine(path);
                                    
                                    using (var document = DocX.Load(pathload))
                                    {   
                                        DocumentProcessor.Process(document,proiect,templateName);  
                                        ReplacePlaceholders(document, placeholders);
                                        document.SaveAs(path);
                                        Console.WriteLine("PATH FOLDER: "+path);
                                    }

                                }
                            }
                        }

                        catch (Exception exFile)
                        {
                            result.Messages.Add($"Eroare la procesarea fișierului {pathload}: {exFile.Message}");
                        }
                   
                }
                 result.Success = true;

                if (imprima.GenereazaDesene)
                {
                    try
                    {
                        var desene = _drawings.GenerateForFotovoltaice(proiect, DrawingService.FotovoltaiceFolder(proiect), false);
                        result.Messages.AddRange(desene.Messages);
                        result.Fisiere.AddRange(desene.Fisiere);
                    }
                    catch (Exception exDesene)
                    {
                        result.Messages.Add($"Eroare la generarea desenelor: {exDesene.Message}");
                    }
                }
            }

            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add($"Eroare generală: {ex.Message}");
            }
            return result;
        }
    }

}