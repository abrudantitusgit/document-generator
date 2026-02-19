using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InterfataActe.DTO
{
    public class ClientFirma
    {
        public ClientFirma(string nume, long cnpcui, string eliberat, DateTime dataBuletin, string seria, string nrSeria, string judet, string localitatea, string comuna, string strada, string sector, string nrStrada, string bloc, string scara, string etaj, string apartament, string codPostal, string telefon, DateTime dataCUI, string oRC, string functia, string reprezentant)
        {
            this.nume = nume;
            this.cnpcui = cnpcui;
            this.eliberat = eliberat;
            this.dataBuletin = dataBuletin;
            this.seria = seria;
            this.nrSeria = nrSeria;
            this.judet = judet;
            this.localitatea = localitatea;
            this.comuna = comuna;
            this.strada = strada;
            this.sector = sector;
            this.nrStrada = nrStrada;
            this.bloc = bloc;
            this.scara = scara;
            this.etaj = etaj;
            this.apartament = apartament;
            this.codPostal = codPostal;
            this.telefon = telefon;
            this.dataCUI = dataCUI;
            this.oRC = oRC;
            this.functia = functia;
            this.reprezentant = reprezentant;
        }

        private string nume;
        private long cnpcui;
        private string eliberat;
        private DateTime dataBuletin;
        private string seria;
        private string nrSeria;
        private string judet;
        private string localitatea;
        private string comuna;
        private string strada;
        private string sector;
        private string nrStrada;
        private string bloc;
        private string scara;
        private string etaj;
        private string apartament;
        private string codPostal;
        private string telefon;
        //firma
        private DateTime dataCUI;
        private string oRC;
        private string functia;
        //
        private string reprezentant;

        public string Nume { get => nume; set => nume = value; }
        public long CnpCui { get => cnpcui; set => cnpcui = value; }
        public string Eliberat { get => eliberat; set => eliberat = value; }
        public DateTime DataBuletin { get => dataBuletin; set => dataBuletin = value; }
        public string Seria { get => seria; set => seria = value; }
        public string NrSeria { get => nrSeria; set => nrSeria = value; }
        public string Judet { get => judet; set => judet = value; }
        public string Localitatea { get => localitatea; set => localitatea = value; }
        public string Comuna { get => comuna; set => comuna = value; }
        public string Strada { get => strada; set => strada = value; }
        public string Sector { get => sector; set => sector = value; }
        public string NrStrada { get => nrStrada; set => nrStrada = value; }
        public string Bloc { get => bloc; set => bloc = value; }
        public string Scara { get => scara; set => scara = value; }
        public string Etaj { get => etaj; set => etaj = value; }
        public string Apartament { get => apartament; set => apartament = value; }
        public string CodPostal { get => codPostal; set => codPostal = value; }
        public string Telefon { get => telefon; set => telefon = value; }
        public DateTime DataCUI { get => dataCUI; set => dataCUI = value; }
        public string ORC { get => oRC; set => oRC = value; }
        public string Functia { get => functia; set => functia = value; }
        public string Reprezentant { get => reprezentant; set => reprezentant = value; }
    }
}
