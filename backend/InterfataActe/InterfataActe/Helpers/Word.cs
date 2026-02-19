using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InterfataActe.Models;
using InterfataActe.DTO;
using InterfataActe.Interfaces;
using Aspose.Words;
using System.IO;
using Microsoft.AspNetCore.Cors;

namespace InterfataActe.Helpers
{

    public class Word
    {

        private readonly IFisierRepository _fisier;

        public Word(IFisierRepository fisier)
        {
            _fisier = fisier;
        }
        public String GenerateBransament(Imprima imprima)
        {
            
            Atr atr = imprima.Proiect.Atr; //
            Client client = imprima.Proiect.Client; //
            Firma firma = imprima.Proiect.Firma; //
            var clientfirma = new ClientFirma (client.Nume,client.CNP,client.Eliberat,client.DataBuletin,client.Seria,client.NrSeria,client.Judet,client.Localitatea,client.Comuna,client.Strada,client.Sector,client.NrStrada,client.Bloc,client.Scara,client.Etaj,client.Apartament,client.CodPostal,client.Telefon,DateTime.Now,"-","-","-");
            Grup grup = imprima.Grup; //
            Proiect proiect = imprima.Proiect; //
            if (firma != null) clientfirma = new ClientFirma (firma.Nume,firma.CUI,"-",DateTime.Now,"-","0",firma.Judet,firma.Localitatea,firma.Comuna,firma.Strada,firma.Sector,firma.NrStrada,firma.Bloc,firma.Scara,firma.Etaj,firma.Apartament,firma.CodPostal,firma.Telefon,firma.DataCUI,firma.ORC,firma.Functia,client.Nume);

            var contorContract = proiect.ContorContract == null ? 0 : proiect.ContorContract.Contor;
            var contorProcesVerbal = proiect.ContorProcesVerbal == null ? 0 : proiect.ContorProcesVerbal.Contor;

            var buletinCablu = proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCablu").FirstOrDefault() == null ? 0 : proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCablu").FirstOrDefault().Contor;
            var buletinPriza = proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPriza").FirstOrDefault() == null ? 0 : proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPriza").FirstOrDefault().Contor;
            var buletinCabluUtilizare = proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCabluUtilizare").FirstOrDefault() == null ? 0 : proiect.ContorBuletinCabluri.Where(x => x.Tag == "BuletinCabluUtilizare").FirstOrDefault().Contor;
            var buletinPrizaUtilizare = proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPrizaUtilizare").FirstOrDefault() == null ? 0 : proiect.ContorBuletinPrize.Where(x => x.Tag == "BuletinPrizaUtilizare").FirstOrDefault().Contor;
       
            foreach (var fold in grup.Foldere)

                foreach (var file in fold.Fisiere)
                {

                    //MODEL FILE EXIST ?

                    //D:\Data\Public\Documente\TempWord\
                    var pathload = @"D:\Data\0. DATA\Public\Documente\TempWord\" + grup.Nume + @"\" + fold.Nume + @"\" + file.Nume + ".docx";

                    if (File.Exists(pathload))
                    {

                       
                        var path = @"D:\Data\0. DATA\001. BRANSAMENTE\000. Clienti\" + client.Nume + @"\" + grup.Nume + @"\" + proiect.NrProiect + @".Proiect\" + fold.Nume + @"\" + file.Nume;

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
                            //D:\Data\Public\Documente\Clienti\
                            if (!Directory.Exists(@"D:\Data\0. DATA\001. BRANSAMENTE\000. Clienti\" + client.Nume + @"\" + grup.Nume + @"\" + proiect.NrProiect + @".Proiect\" + fold.Nume)) Directory.CreateDirectory(@"D:\Data\0. DATA\2. DOCUMENTE\Documente\Clienti\1. BRANSAMENTE\" + client.Nume + @"\" + grup.Nume + @"\" + proiect.NrProiect + @".Proiect\" + fold.Nume);

                            Document doc = new Document(pathload);

                           //ATR
                            if (atr != null)
                                {
                                    doc.Range.Replace("<Atr.NrAtr>", atr.NrAtr.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.DataAtr>", atr.DataAtr.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.NrCadastral>", atr.NrCadastral.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.CodPostal>", atr.CodPostal.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.PunctRacordare>", atr.PunctRacordare.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.PunctAlimentare>", atr.PunctAlimentare.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.PunctBransament>", atr.PunctBransament.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Cablu>", atr.Cablu.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.LungimeCablu>", atr.LungimeCablu.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.LungimeCabluStalp>", (atr.LungimeCablu - 10).ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Ohmi>", atr.Ohmi.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Contor>", atr.Contor.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Temperatura>", atr.Temperatura.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Descriere>", atr.Descriere.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Pret>", atr.Pret.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Atr.Tva>", atr.Tva.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                }
                      
                                //Client
                                doc.Range.Replace("<Client.Nume>", client.Nume.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.CNP>", client.CNP.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Eliberat>", client.Eliberat.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.DataBuletin>", client.DataBuletin.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Seria>", client.Seria.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.NrSeria>", client.NrSeria.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Judet>", client.Judet.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Localitatea>", client.Localitatea.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Comuna>", client.Comuna.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Strada>", client.Strada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Sector>", client.Sector.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.NrStrada>", client.NrStrada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Bloc>", client.Bloc.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Scara>", client.Scara.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Etaj>", client.Etaj.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Apartament>", client.Apartament.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.CodPostal>", client.CodPostal.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Client.Telefon>", client.Telefon.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                           
                                //Firma
                                if (firma != null)
                                {
                                    doc.Range.Replace("<Firma.Nume>", firma.Nume.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.CUI>", firma.CUI.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.DataCUI>", firma.DataCUI.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.ORC>", firma.ORC.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Functia>", firma.Functia.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Judet>", firma.Judet.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Localitatea>", firma.Localitatea.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Comuna>", firma.Comuna.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Strada>", firma.Strada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Sector>", firma.Sector.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.NrStrada>", firma.NrStrada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Bloc>", firma.Bloc.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Scara>", firma.Scara.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Etaj>", firma.Etaj.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Apartament>", firma.Apartament.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.CodPostal>", firma.CodPostal.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                    doc.Range.Replace("<Firma.Telefon>", firma.Telefon.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                }
                       
                                //ClientFirma
                                doc.Range.Replace("<ClientFirma.Nume>", clientfirma.Nume.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.CNPCUI>", clientfirma.CnpCui.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Eliberat>", clientfirma.Eliberat.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.DataBuletin", clientfirma.DataBuletin.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Seria>", clientfirma.Seria.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.NrSeria>", clientfirma.NrSeria.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Judet>", clientfirma.Judet.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Localitatea>", clientfirma.Localitatea.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Comuna>", clientfirma.Comuna.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Strada>", clientfirma.Strada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Sector>", clientfirma.Sector.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.NrStrada>", clientfirma.NrStrada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Bloc>", clientfirma.Bloc.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Scara>", clientfirma.Scara.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Etaj>", clientfirma.Etaj.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Apartament>", clientfirma.Apartament.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.CodPostal>", clientfirma.CodPostal.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Telefon>", clientfirma.Telefon.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.DataCUI>", clientfirma.DataCUI.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.ORC>", clientfirma.ORC.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Functia>", clientfirma.Functia.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<ClientFirma.Reprezentant>", clientfirma.Reprezentant.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                          
                                //Proiect
                                doc.Range.Replace("<Proiect.NrProiect>", proiect.NrProiect.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Denumire>", proiect.Denumire.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Judet>", proiect.Judet.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Sector>", proiect.Sector.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Localitatea>", proiect.Localitatea.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Comuna>", proiect.Comuna.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Strada>", proiect.Strada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.NrStrada>", proiect.NrStrada.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Bloc>", proiect.Bloc.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Scara>", proiect.Scara.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Etaj>", proiect.Etaj.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Apartament>", proiect.Apartament.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Pi>", proiect.Pi.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Pa>", proiect.Pa.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.LocConsum>", proiect.LocConsum.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.TensiuneUtilizare>", proiect.TensiuneUtilizare.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Contract>", contorContract.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.ProcesVerbal>", contorProcesVerbal.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.ProcesVerbalCablu>", contorProcesVerbal.ToString() + "A", new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.BuletinCablu>", buletinCablu.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.BuletinPriza>", buletinPriza.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.BuletinCabluUtilizare>", buletinCabluUtilizare.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.BuletinPrizaUtilizare>", buletinPrizaUtilizare.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.Data>", proiect.Data.ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.DataScurta>", proiect.Data.ToString("dd.MM.yy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<PRM_Masurat>", proiect.PRM_Masurat.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<PRM_Calculat>", proiect.PRM_Calculat.ToString(), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));
                                doc.Range.Replace("<Proiect.DataPlus>", proiect.Data.AddMonths(1).ToString("dd.MM.yyyy"), new Aspose.Words.Replacing.FindReplaceOptions(Aspose.Words.Replacing.FindReplaceDirection.Forward));


                            doc.Save(path);


                        }

                    }


                }
            return "da";

        }




    }
}