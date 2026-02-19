import React, { useState, useEffect } from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import moment from "moment";
import "react-widgets/styles.css";
import { isCompositeComponent } from "react-dom/cjs/react-dom-test-utils.production.min";
import { baseUrl } from "../config/api";


const Atr = () => {

  const [atr, setAtr] = useState({});
  const [modificaatr, setModificaAtr] = useState({});
  const [proiecte, setProiecte] = useState([]);
  const [mproiecte,setMProiecte] = useState([]);
  const [proiect, setProiect] = useState({});
  const [modificaproiect,setModificaProiect] = useState({});

  const contor = ["mono CMA 230V/10-40A","trif CE 3x230/400V 5/100A"];

  useEffect(() => {
    fetch(`${baseUrl}/proiect/proiecteincludenull`)
      .then(response => response.json())
      .then(data => setProiecte(data)).catch(error => console.log(error));
  }, []);

  useEffect(() => {
    fetch(`${baseUrl}/proiect/proiecteinclude`)
    .then(response => response.json())
    .then(data => setMProiecte(data)).catch(error=>console.log(error));
  },[]);

  useEffect(() => {
    setAtr({ ...atr, descriere: "De la stalpul " + atr.punctAlimentare + " se va realiza un " + proiect.denumire + " subteran cu cablu " + atr.cablu + " de lungime " + atr.lungimeCablu + " ml  pana la un " + atr.punctBransament + ", echipat cu DPS+PD+MN+Rp < " + atr.ohmi + " Ohmi, montat pe postament din poliester armat cu fibra de sticla in fundatie turnata de beton  amplasat la limită de proprietate." });
  }, [atr.punctAlimentare, proiect.denumire, atr.cablu, atr.lungimeCablu, atr.punctBransament, atr.ohmi])

  useEffect(() => {
    setModificaAtr({ ...modificaatr, descriere: "De la stalpul " + modificaatr.punctAlimentare + " se va realiza un " + modificaproiect.denumire + " subteran cu cablu " + modificaatr.cablu + " de lungime " + modificaatr.lungimeCablu + " ml  pana la un " + modificaatr.punctBransament + ", echipat cu DPS+PD+MN+Rp < " + modificaatr.ohmi + " Ohmi, montat pe postament din poliester armat cu fibra de sticla in fundatie turnata de beton  amplasat la limită de proprietate." });
  }, [modificaatr.punctAlimentare, modificaproiect.denumire, modificaatr.cablu, modificaatr.lungimeCablu, modificaatr.punctBransament, modificaatr.ohmi])

  useEffect(()=>{
    setAtr({...atr,tva:Number((atr.pret - (atr.pret / (1+(21/100)))).toFixed(2))})
  },[atr.pret])

  useEffect(()=>{
    setModificaAtr({...modificaatr,tva:Number((modificaatr.pret - (modificaatr.pret / (1+(21/100)))).toFixed(2))})
  },[modificaatr.pret])

  function Create() {
    fetch(`${baseUrl}/atr/create`, {
      method: 'POST',
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(atr)
    }).then(response => {
      if (!response.ok) {
        alert('Eroare');
        console.log(response.statusText)
      }
      else
        {
          alert('Salvat');
          setAtr({});
    }
    }).catch(error => alert('Eroare'));
  }

  function Modify() {
    fetch(`${baseUrl}/atr/modify`, {
      method: 'POST',
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(modificaatr)
    }).then(response => {
      if (!response.ok) {
        alert('Eroare');
      }
      else
        {
          alert('Salvat');
          setModificaAtr({});
    }
    }).catch(error => alert('Eroare'));
  }
  console.log(atr);

  return (
    <>
    <Form title="Adauga Atr">

      <Combobox className="combobox" placeholder="Selecteaza Proiect:" data={proiecte} dataKey='nrProiect' textField={value => value.nrProiect} onChange={value => { setAtr({ ...atr, proiectID: value.id }); setProiect(value); }} renderListItem={({ item }) => (<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | " + item.nrStrada + " | " + (item.firma ? item.firma.nume : item.client.nume)+ " | " + (item.atr ? item.atr.nrAtr : '')}</span>)} />
      <input type="number" value={atr.nrATR || ''} placeholder="Introdu Numar ATR (Ex: 6020...):" onChange={(e) => setAtr({ ...atr, nrATR: Number(e.target.value) })} required />
      <label>Introdu data ATR:</label>
      <input type="date" value={atr.dataATR || ''} onChange={(e) => setAtr({ ...atr, dataATR: e.target.value })} required />
      <input value={atr.nrCadastral || ''} placeholder="Introdu Nr. Cadastral (Ex: 342-C1):" onChange={(e) => setAtr({ ...atr, nrCadastral: e.target.value })} />
      <input value={atr.codPostal || ''} placeholder="Introdu Cod Postal (Ex: 5433):" onChange={(e) => setAtr({ ...atr, codPostal: e.target.value })} />
      <input value={atr.cablu || ''} placeholder="Introdu Cablu (Ex: ACYAbY 4x6mmp):" onChange={(e) => setAtr({ ...atr, cablu: e.target.value })} />
      <input type="number" value={atr.lungimeCablu || ''} placeholder="Introdu Lungime Cablu (Ex: 18):" onChange={(e) => setAtr({ ...atr, lungimeCablu: Number(e.target.value) })} />
      <input type="number" value={atr.ohmi || ''} placeholder="Introdu Ohmi (Ex: 4, 10):" onChange={(e) => setAtr({ ...atr, ohmi: Number(e.target.value) })} />
      <input value={atr.punctRacordare || ''} placeholder="Introdu Punct Racordare (Ex: PTAB...):" onChange={(e) => setAtr({ ...atr, punctRacordare: e.target.value })} />
      <input value={atr.punctAlimentare || ''} placeholder="Introdu Punct Alimentare (Ex: Stalp/Firida nr.2):" onChange={(e) => setAtr({ ...atr, punctAlimentare: e.target.value })} />
      <input value={atr.punctBransament || ''} placeholder="Introdu Punct Bransament (Ex: BMPT IR-20A):" onChange={(e) => setAtr({ ...atr, punctBransament: e.target.value })} />
      <input type="number" value={atr.pret || ''} placeholder="Introdu Pret (Ex: 1200):" onChange={(e) => setAtr({ ...atr, pret: Number(e.target.value) })} />
      <input type="number" value={atr.tva || ''} placeholder="Introdu TVA (Ex: 12.23):" onChange={(e) => setAtr({ ...atr, tva: Number(e.target.value) })} />
      <Combobox className="combobox" placeholder="Selecteaza Contor:" data={contor}  onChange={value => { setAtr({ ...atr, contor: value }); }} />
      <input type="number" value={atr.temperatura || ''} placeholder="Introdu Temperatura (Ex: 26.6):" onChange={(e) => setAtr({ ...atr, temperatura: Number(e.target.value) })} />
      <textarea value={atr.descriere} placeholder="Introdu Descriere Instalatie (Ex: De la Stalpul...):" onChange={(e) => setAtr({ ...atr, descriere: e.target.value })} />

      <button className="adauga" onClick={() => { Create(); }}>Adauga</button>

    </Form>

    <br/>

    <Form title="Modifica Atr">

<Combobox className="combobox" placeholder="Selecteaza Proiect:" data={mproiecte} dataKey='nrProiect' textField={value => value.nrProiect} onChange={value => { setModificaAtr(value.atr); setModificaProiect(value); }} renderListItem={({ item }) => (<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | " + item.nrStrada + " | " + (item.firma ? item.firma.nume : item.client.nume)+ " | " + (item.atr ? item.atr.nrAtr : '')}</span>)} />
<input type="number" value={modificaatr.nrAtr || ''} placeholder="Introdu Numar ATR (Ex: 6020...):" onChange={(e) => setModificaAtr({ ...modificaatr, nrAtr: Number(e.target.value) })} required />
<label>Introdu data ATR:</label>
<input type="date" value={modificaatr.dataAtr ? moment(modificaatr.dataAtr).format('YYYY-MM-DD') : ''} onChange={(e) => setModificaAtr({ ...modificaatr, dataAtr: e.target.value })} required />
<input value={modificaatr.nrCadastral || ''} placeholder="Introdu Nr. Cadastral (Ex: 342-C1):" onChange={(e) => setModificaAtr({ ...modificaatr, nrCadastral: e.target.value })} />
<input value={modificaatr.codPostal || ''} placeholder="Introdu Cod Postal (Ex: 5433):" onChange={(e) => setModificaAtr({ ...modificaatr, codPostal: e.target.value })} />
<input value={modificaatr.cablu || ''} placeholder="Introdu Cablu (Ex: ACYAbY 4x6mmp):" onChange={(e) => setModificaAtr({ ...modificaatr, cablu: e.target.value })} />
<input type="number" value={modificaatr.lungimeCablu || ''} placeholder="Introdu Lungime Cablu (Ex: 18):" onChange={(e) =>setModificaAtr({ ...modificaatr, lungimeCablu: Number(e.target.value) })} />
<input type="number" value={modificaatr.ohmi || ''} placeholder="Introdu Ohmi (Ex: 4, 10):" onChange={(e) => setModificaAtr({ ...modificaatr, ohmi: Number(e.target.value) })} />
<input value={modificaatr.punctRacordare || ''} placeholder="Introdu Punct Racordare (Ex: PTAB...):" onChange={(e) => setModificaAtr({ ...modificaatr, punctRacordare: e.target.value })} />
<input value={modificaatr.punctAlimentare || ''} placeholder="Introdu Punct Alimentare (Ex: Stalp/Firida nr.2):" onChange={(e) => setModificaAtr({ ...modificaatr, punctAlimentare: e.target.value })} />
<input value={modificaatr.punctBransament || ''} placeholder="Introdu Punct Bransament (Ex: BMPT IR-20A):" onChange={(e) => setModificaAtr({ ...modificaatr, punctBransament: e.target.value })} />
<input type="number" value={modificaatr.pret || ''} placeholder="Introdu Pret (Ex: 1200):" onChange={(e) => setModificaAtr({ ...modificaatr, pret: Number(e.target.value) })} />
<input type="number" value={modificaatr.tva || ''} placeholder="Introdu TVA (Ex: 12.23):" onChange={(e) => setModificaAtr({ ...modificaatr, tva: Number(e.target.value) })} />
<Combobox className="combobox" placeholder="Selecteaza Contor:" data={contor}  onChange={value => { setModificaAtr({ ...atr, contor: value }); }} />
<input type="number" value={modificaatr.temperatura || ''} placeholder="Introdu Temperatura (Ex: 26.6):" onChange={(e) => setModificaAtr({ ...modificaatr, temperatura: Number(e.target.value) })} />
<textarea value={modificaatr.descriere} placeholder="Introdu Descriere Instalatie (Ex: De la Stalpul...):" onChange={(e) => setModificaAtr({ ...modificaatr, descriere: e.target.value })} />

<button className="modifica" onClick={() => {Modify(); }}>Modifica</button>

</Form>

    </>
  );
}

export default Atr;