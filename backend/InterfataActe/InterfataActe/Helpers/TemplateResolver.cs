
using System;
using System.Linq;
using System.Reflection.Metadata;
using InterfataActe.Models;
using Microsoft.AspNetCore.Identity;
namespace InterfataActe.Helpers
{

public class TemplateResolver
{
    
    public string Resolve(ProiectFotovoltaice proiect,string documentCode)
    {

        // 1. document simplu
        if (HasSingleTemplate(documentCode))
            return documentCode;

        // 2. document cu variante
        var baseCode = documentCode;

        if (baseCode is "01.NOTIFICARE ANEXA 2" or "01.NOTIFICARE ANEXA 1" or "04.COMUNICARE FINALIZARE LUCRARI" or 
            "05.DOSAR UTILIZARE" or "06.BULETIN PRM" or 
            "08.PV RECEPTIE" or "09.PV PROBE" or "10.PV PROIECTARE" or "11.DECLARATIE DE BUNA CALITATE SI GARANTIE" or "12.PIF CEF" or "ANTET DOSAR PROSUMATOR")
        {
            // PJ / PF
            documentCode += proiect.Firma != null ? "_PJ" : "_PF";

            // MONO / TRIF
            if (baseCode is "05.DOSAR UTILIZARE")
            {
                documentCode += proiect.TipSistem == "Monofazat" ? "_MONO" : "_TRIF";
            }

            // BATERIE
            if ((baseCode == "01.NOTIFICARE ANEXA 2" || baseCode=="01.NOTIFICARE ANEXA 1") && proiect.CapacitateTotalaBaterii != 0)
            {
                documentCode += "_BATERIE";
            }
            if (baseCode=="06.BULETIN PRM")
                {
                    documentCode+="_"+proiect.PRM_Coeficient.ToString();
                }
        }

        
        return documentCode;
    }

    private bool HasSingleTemplate(string documentCode)
    {
        return documentCode is
            "DECLARATIE_AUTENTICITATE_DATE" or
            "PROTECTIA_DATELOR";
    }

}
}