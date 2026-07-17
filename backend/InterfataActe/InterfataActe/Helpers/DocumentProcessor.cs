using System.Linq;
using Xceed.Words.NET;
using Xceed.Document.NET;
using InterfataActe.Models;
using System.Text;
using System;
namespace InterfataActe.Helpers
{
    

public static class DocumentProcessor
{
    public static void Process(DocX doc, ProiectFotovoltaice proiect, string templateName)
    {
        switch (templateName)
            {
            case var name when name.Contains("NOTIFICARE ANEXA 2"):
                ProcessNotificareAnexa2(doc,proiect);
                break;
            case var name when name.Contains("NOTIFICARE ANEXA 1"):
                ProcessNotificareAnexa1(doc,proiect);
                break;
            case var name when name.Contains("COMUNICARE"):
                ProcessComunicare(doc,proiect);
                break;
            case var name when name.Contains("DOSAR UTILIZARE"):
                ProcessDosarUtilizare(doc,proiect);
                break;
            case var name when name.Contains("PV RECEPTIE"):
                ProcessPvReceptie(doc,proiect);
                break;
            case var name when name.Contains("PV PROBE"):
                ProcessPvProbe(doc,proiect);
                break;
            case var name when name.Contains("PIF CEF"):
                ProcessPIF(doc,proiect);
                break;

            }

    }

private static void ProcessNotificareAnexa2(DocX doc, ProiectFotovoltaice proiect)
{
    try
    {
        var tabel_panouri = doc.Tables[2];
        var tabel_invertor = doc.Tables[3];

        int insertIndexPanouri = tabel_panouri.Rows.Count - 1;
        int insertIndexInvertor = tabel_invertor.Rows.Count - 1;

        var modelRowPanouri = tabel_panouri.Rows[insertIndexPanouri - 1];
        var modelRowInvertor = tabel_invertor.Rows[insertIndexInvertor - 1];

        // header values
        tabel_panouri.Rows[3].Cells[2].Paragraphs[0].Append((proiect.PutereTotalaPanouri / 1000).ToString());
        tabel_panouri.Rows[3].Cells[3].Paragraphs[0].Append((proiect.PutereTotalaPanouri / 1000).ToString());
        tabel_panouri.Rows[3].Cells[4].Paragraphs[0].Append("{{Proiect.PutereMax}}");

        foreach (var panou in proiect.DatePanouri)
        {
            var row = tabel_panouri.InsertRow(modelRowPanouri, insertIndexPanouri, true);

            row.Cells[1].Paragraphs[0].Append(panou.NrPanouri.ToString());
            row.Cells[2].Paragraphs[0].Append(panou.Model);
            row.Cells[3].Paragraphs[0].Append(panou.PutereInstalata.ToString());
            row.Cells[4].Paragraphs[0].Append((panou.NrPanouri * panou.PutereInstalata / 1000).ToString());
            row.Cells[5].Paragraphs[0].Append((panou.NrPanouri * panou.PutereInstalata / 1000).ToString());
            row.Cells[6].Paragraphs[0].Append(
                Math.Min((panou.NrPanouri * panou.PutereInstalata / 1000), proiect.PutereTotalaInvertor).ToString()
            );

            insertIndexPanouri++;
        }

        // remove model row (as in your code)
        int removeIndexPanouri = insertIndexPanouri - proiect.DatePanouri.Count - 1;
        Console.WriteLine($"[Anexa2] removeIndexPanouri={removeIndexPanouri}, rows={tabel_panouri.Rows.Count}");

        if (removeIndexPanouri >= 0 && removeIndexPanouri < tabel_panouri.Rows.Count)
            tabel_panouri.RemoveRow(removeIndexPanouri);
        else
            Console.WriteLine($"[Anexa2] ATENTIE: removeIndexPanouri invalid: {removeIndexPanouri}");

        // header values invertor
        tabel_invertor.Rows[3].Cells[1].Paragraphs[0].Append(proiect.PutereTotalaInvertor.ToString());
        tabel_invertor.Rows[3].Cells[2].Paragraphs[0].Append(proiect.PutereTotalaInvertor.ToString());
        tabel_invertor.Rows[3].Cells[3].Paragraphs[0].Append("{{Proiect.PutereMax}}");

        foreach (var inv in proiect.DateInvertoare)
        {
            var row = tabel_invertor.InsertRow(modelRowInvertor, insertIndexInvertor, true);

            row.Cells[1].Paragraphs[0].Append(inv.NrInvertoare.ToString());
            row.Cells[2].Paragraphs[0].Append(
                proiect.TipSistem == "Monofazat" ? "Monofazat,\n" + inv.Model : "Trifazat,\n" + inv.Model
            );
            row.Cells[3].Paragraphs[0].Append(proiect.TipSistem == "Monofazat" ? "230" : "400");
            row.Cells[4].Paragraphs[0].Append(inv.PutereInstalata.ToString());
            row.Cells[5].Paragraphs[0].Append(inv.PutereInstalata.ToString());
            row.Cells[6].Paragraphs[0].Append("{{Proiect.PutereMax}}");

            insertIndexInvertor++;
        }

        int removeIndexInvertor = insertIndexInvertor - proiect.DateInvertoare.Count - 1;
        Console.WriteLine($"[Anexa2] removeIndexInvertor={removeIndexInvertor}, rows={tabel_invertor.Rows.Count}");

        if (removeIndexInvertor >= 0 && removeIndexInvertor < tabel_invertor.Rows.Count)
            tabel_invertor.RemoveRow(removeIndexInvertor);
        else
            Console.WriteLine($"[Anexa2] ATENTIE: removeIndexInvertor invalid: {removeIndexInvertor}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("EROARE ProcessNotificareAnexa2:");
        Console.WriteLine(ex.ToString());
        throw;
    }
}


  private static void ProcessNotificareAnexa1(DocX doc, ProiectFotovoltaice proiect)
{
    try
    {
        var tabel_panouri = doc.Tables[5];
        var tabel_invertor = doc.Tables[6];

        int insertIndexPanouri = tabel_panouri.Rows.Count;
        int insertIndexInvertor = tabel_invertor.Rows.Count - 1;

        var modelRowPanouri = tabel_panouri.Rows[insertIndexPanouri - 1];
        var modelRowInvertor = tabel_invertor.Rows[insertIndexInvertor - 1];

        foreach (var panou in proiect.DatePanouri)
        {
            var row = tabel_panouri.InsertRow(modelRowPanouri, insertIndexPanouri, true);

            row.Cells[1].Paragraphs[0].Append(panou.NrPanouri.ToString());
            row.Cells[2].Paragraphs[0].Append(panou.Model);
            row.Cells[3].Paragraphs[0].Append(panou.PutereInstalata.ToString());
            row.Cells[4].Paragraphs[0].Append((panou.NrPanouri * panou.PutereInstalata / 1000).ToString());
            row.Cells[5].Paragraphs[0].Append((panou.NrPanouri * panou.PutereInstalata / 1000).ToString());
            row.Cells[6].Paragraphs[0].Append(
                Math.Min((panou.NrPanouri * panou.PutereInstalata / 1000), proiect.PutereTotalaInvertor).ToString()
            );

            insertIndexPanouri++;
        }

        int removeIndexPanouri = insertIndexPanouri - proiect.DatePanouri.Count - 1;
        Console.WriteLine($"[Anexa1] removeIndexPanouri={removeIndexPanouri}, rows={tabel_panouri.Rows.Count}");

        if (removeIndexPanouri >= 0 && removeIndexPanouri < tabel_panouri.Rows.Count)
            tabel_panouri.RemoveRow(removeIndexPanouri);
        else
            Console.WriteLine($"[Anexa1] ATENTIE: removeIndexPanouri invalid: {removeIndexPanouri}");

        foreach (var inv in proiect.DateInvertoare)
        {
            var row = tabel_invertor.InsertRow(modelRowInvertor, insertIndexInvertor, true);

            row.Cells[1].Paragraphs[0].Append(inv.NrInvertoare.ToString());
            row.Cells[2].Paragraphs[0].Append(
                proiect.TipSistem == "Monofazat" ? "Monofazat,\n" + inv.Model : "Trifazat,\n" + inv.Model
            );
            row.Cells[3].Paragraphs[0].Append(proiect.TipSistem == "Monofazat" ? "230" : "400");
            row.Cells[4].Paragraphs[0].Append(inv.PutereInstalata.ToString());
            row.Cells[5].Paragraphs[0].Append(inv.PutereInstalata.ToString());
            row.Cells[6].Paragraphs[0].Append("{{Proiect.PutereMax}}");

            insertIndexInvertor++;
        }

        int removeIndexInvertor = insertIndexInvertor - proiect.DateInvertoare.Count - 1;
        Console.WriteLine($"[Anexa1] removeIndexInvertor={removeIndexInvertor}, rows={tabel_invertor.Rows.Count}");

        if (removeIndexInvertor >= 0 && removeIndexInvertor < tabel_invertor.Rows.Count)
            tabel_invertor.RemoveRow(removeIndexInvertor);
        else
            Console.WriteLine($"[Anexa1] ATENTIE: removeIndexInvertor invalid: {removeIndexInvertor}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("EROARE ProcessNotificareAnexa1:");
        Console.WriteLine(ex.ToString());
        throw;
    }
}


        private static void ProcessComunicare(DocX doc, ProiectFotovoltaice proiect)
        {
            var descriereSistem   = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_SISTEM}}"));
            var descrierePanouri  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_PANOURI}}"));
            var descriereInvertor = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_INVERTOR}}"));
            var descriereBaterie  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_BATERIE}}"));

            if (descriereSistem == null || descrierePanouri == null || descriereInvertor == null || descriereBaterie == null)
                return;

            // Creează stringuri cu descrieri
            var sbSistem = new StringBuilder();
            var sbPanouri = new StringBuilder();
            var sbInvertor = new StringBuilder();
            var sbBaterie = new StringBuilder();

            foreach (var panou in proiect.DatePanouri)
            {
                sbSistem.Append($"- {panou.NrPanouri} panouri fotovoltaice a cate {panou.PutereInstalata}W\n");
                sbPanouri.Append($"- {panou.NrPanouri} Panouri Fotovoltaice {panou.Model}\n");
            }

            foreach (var inv in proiect.DateInvertoare)
                sbInvertor.Append($"- Invertor {proiect.TipSistem} marca {inv.Model}\n");

            if (proiect.CapacitateTotalaBaterii != 0)
            {
                sbBaterie.Append("Denumire baterie de stocare:\n");
                foreach (var bat in proiect.DateBaterii)
                    sbBaterie.Append($"-{bat.NrBaterii} buc. Baterie {bat.Model}, {bat.PutereInstalata} kW, {bat.Capacitate} Ah\n");
            }

            // Înlocuiește placeholder-ul cu conținutul generat, păstrând paragraf existent
            descriereSistem.ReplaceText("{{DESCRIERE_SISTEM}}", sbSistem.ToString());
            descrierePanouri.ReplaceText("{{DESCRIERE_PANOURI}}", sbPanouri.ToString());
            descriereInvertor.ReplaceText("{{DESCRIERE_INVERTOR}}", sbInvertor.ToString());
            descriereBaterie.ReplaceText("{{DESCRIERE_BATERIE}}", sbBaterie.ToString());
        }

        private static void ProcessDosarUtilizare(DocX doc, ProiectFotovoltaice proiect)
        {
            var descrierePanouri  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_PANOURI}}"));
            var descriereInvertor = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_INVERTOR}}"));

            if (descrierePanouri == null || descriereInvertor == null )
                return;
            var sbPanouri = new StringBuilder();
            var sbInvertor = new StringBuilder();
            
            foreach (var panou in proiect.DatePanouri)
                sbPanouri.Append($"{panou.NrPanouri} panouri fotovoltaice de {panou.PutereInstalata}W, ");


            foreach (var inv in proiect.DateInvertoare)
                sbInvertor.Append($"Invertor {proiect.TipSistem} {inv.PutereInstalata} kW, ");

            descrierePanouri.ReplaceText("{{DESCRIERE_PANOURI}}", sbPanouri.ToString());
            descriereInvertor.ReplaceText("{{DESCRIERE_INVERTOR}}", sbInvertor.ToString());
        }

        private static void ProcessPvReceptie(DocX doc, ProiectFotovoltaice proiect)
        {
            var descrierePanouri  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_PANOURI}}"));
            var descrierePanouri2  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_PANOURI_2}}"));
            var descriereInvertor = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_INVERTOR}}"));
            var descriereInvertor2 = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_INVERTOR_2}}"));
            var descriereBaterie = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_BATERIE}}"));

            if (descrierePanouri == null || descriereInvertor == null || descriereBaterie == null )
                return;
            var sbPanouri = new StringBuilder();
            var sbPanouri2 = new StringBuilder();
            var sbInvertor = new StringBuilder();
            var sbInvertor2 = new StringBuilder();
            var sbBaterie = new StringBuilder();
            
            foreach (var panou in proiect.DatePanouri)
            {
                 sbPanouri.Append($"\n- {panou.NrPanouri} panouri fotovoltaice {panou.Model} {panou.PutereInstalata}W");
                 sbPanouri2.Append($"{panou.Model} {panou.PutereInstalata}W\n");
                 sbPanouri2.Append($"Seriile: {panou.Serie}\n");
            }
               
            foreach (var inv in proiect.DateInvertoare)
            {
                sbInvertor.Append($"\n- {inv.NrInvertoare} buc. Invertor {proiect.TipSistem} marca {inv.Model} {inv.PutereInstalata} kW");
                sbInvertor2.Append($"{inv.Model} (serial number {inv.Serie}), ");
            }
                
            if (proiect.CapacitateTotalaBaterii != 0)
            {
            foreach (var bat in proiect.DateBaterii)
                sbBaterie.Append($"\n- {bat.NrBaterii} buc. Baterie {bat.Model} {bat.PutereInstalata} kW");
            }

            descrierePanouri.ReplaceText("{{DESCRIERE_PANOURI}}", sbPanouri.ToString());
            descriereInvertor.ReplaceText("{{DESCRIERE_INVERTOR}}", sbInvertor.ToString());
            descrierePanouri2.ReplaceText("{{DESCRIERE_PANOURI_2}}", sbPanouri2.ToString());
            descriereInvertor2.ReplaceText("{{DESCRIERE_INVERTOR_2}}", sbInvertor2.ToString());
            descriereBaterie.ReplaceText("{{DESCRIERE_BATERIE}}", sbBaterie.ToString());
        }

        private static void ProcessPvProbe(DocX doc, ProiectFotovoltaice proiect)
        {
            var serii  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{SERII_INV}}"));
            var modele  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{MODELE_INV}}"));
            
            if (serii==null || modele ==null) return;

            var sbSerii=new StringBuilder();
            var sbModele=new StringBuilder();
            foreach (var inv in proiect.DateInvertoare)
            {
                sbSerii.Append(inv.Serie+",");
                sbModele.Append(inv.Model+",");
            }

            serii.ReplaceText("{{SERII_INV}}",sbSerii.ToString());
            modele.ReplaceText("{{MODELE_INV}}",sbModele.ToString());
        }
        private static void ProcessPIF(DocX doc, ProiectFotovoltaice proiect)
        {
             var descrierePanouri  = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_PANOURI}}"));
             var descriereInvertor = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_INVERTOR}}"));
             var descriereBaterie = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_BATERIE}}"));
             var descriereBackup = doc.Paragraphs.FirstOrDefault(x => x.Text.Contains("{{DESCRIERE_BACKUP}}"));

              if (descrierePanouri == null || descriereInvertor == null || descriereBaterie == null )
                return;

             var sbPanouri = new StringBuilder();
             var sbInvertor = new StringBuilder();
             var sbBaterie = new StringBuilder();
             var sbBackup = new StringBuilder();

             foreach (var panou in proiect.DatePanouri)
            {
                 sbPanouri.Append($"- {panou.NrPanouri} Bucati panouri fotovoltaice, puterea {panou.PutereInstalata}W, Marca {panou.Model}, serii: {panou.Serie}\n");

            }
               
            foreach (var inv in proiect.DateInvertoare)
            {
                sbInvertor.Append($"- {inv.NrInvertoare} buc. Invertor {proiect.TipSistem} Marca {inv.Model} {inv.PutereInstalata} kW, serie: {inv.Serie}\n");

            }
                
            if (proiect.CapacitateTotalaBaterii != 0)
            {
            foreach (var bat in proiect.DateBaterii)
                {     
                    sbBaterie.Append($"- {bat.NrBaterii} buc. Baterie de Stocare, Marca {bat.Model} {bat.PutereInstalata} kW, serie: {bat.Serie}\n");
                    sbBaterie.Append($"- 1 buc. Modul de incarcare al Bateriei de Stocare, Marca {bat.Model}\n");   
                }
               descriereBaterie.ReplaceText("{{DESCRIERE_BATERIE}}", sbBaterie.ToString()); 
            }
            else descriereBaterie.Remove(false);
           if (!string.IsNullOrWhiteSpace(proiect.Backup) && proiect.Backup != "-")
            {
                sbBackup.Append($"1 buc. Modul de Backup, Marca {proiect.Backup}");
                descriereBackup.ReplaceText("{{DESCRIERE_BACKUP}}", sbBackup.ToString());
            }
            else descriereBackup.Remove(false);

            descrierePanouri.ReplaceText("{{DESCRIERE_PANOURI}}", sbPanouri.ToString());
            descriereInvertor.ReplaceText("{{DESCRIERE_INVERTOR}}", sbInvertor.ToString());
              
        }




    
}
}