import React, {useEffect, useState } from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const ClientContor = () => {

    const[proiectcontor,setProiectContor] = useState({tag:'test'});
    const[proiecte,setProiecte] = useState([]);
    const[numere,setNumere] = useState([]);


    const[atr,setAtr] = useState({});
    const[proiect,setProiect] = useState({});

    useEffect(()=>
    {
        fetch(`${baseUrl}/proiect/proiecteinclude`)
        .then(response => response.json())
        .then(data => setProiecte(data)).catch(error=>console.log(error));
    },[]);

    useEffect(() => {
      setAtr({ ...atr, descriere: "De la stalpul " + atr.punctAlimentare + " se va realiza un " + proiect.denumire + " subteran cu cablu " + atr.cablu + " de lungime " + atr.lungimeCablu + " ml  pana la un " + atr.punctBransament + ", echipat cu DPS+PD+MN+Rp < " + atr.ohmi + " Ohmi, montat pe postament din poliester armat cu fibra de sticla in fundatie turnata de beton  amplasat la limită de proprietate." });
    }, [atr.punctAlimentare, proiect.denumire, atr.cablu, atr.lungimeCablu, atr.punctBransament, atr.ohmi])  
    
  useEffect(()=>{
    setAtr({...atr,tva:Number((atr.pret - (atr.pret / (1+(19/100)))).toFixed(2))})
  },[atr.pret])

    function Create(link,nume){
      if(window.confirm("Esti sigur ca doresti sa adaugi numar ?"))
      {
        fetch(`${baseUrl}/proiectcontor/${link}`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(proiectcontor)
          }).then(response => response.json())
            .then((data)=>{
              console.log(data);
            setNumere(some=>[...some,{nume,contor:data.contor}])
          }).catch(error =>alert('Eroare'));
        }
    }

    function CreateAtr(){
      fetch(`${baseUrl}/atr/createmanual`, {
          method: 'POST',
          headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
          },
          body: JSON.stringify(atr)
        }).then(response => {
          if (!response.ok) {
            alert('Eroare');
          }
          else
            {
              alert('Salvat');
              setAtr({});
        }
        }).catch(error => alert('Eroare'));
  }


    return (
      <>
        <Form title="Adauga Numar Registru">

        <Combobox className="combobox" placeholder="Selecteaza Proiect:" data={proiecte} dataKey='id' textField='nume' onChange={value=>{setProiectContor({...proiectcontor,proiectID:value.id});}} textField={value => value.nrProiect} renderListItem={({ item }) => (<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | " + item.nrStrada + " | " + (item.firma ? item.firma.nume : item.client.nume)+ " | " + (item.atr ? item.atr.nrAtr : '')}</span>)} required/>

        <button className="adauga" onClick={()=>{Create('createproiectcontorbuletincablu','Buletin Cablu');}}>Adauga Buletin Cablu</button>
        <button className="adauga" onClick={()=>{Create('createproiectcontorbuletinpriza','Buletin Priza');}}>Adauga Buletin Priza</button>
        <button className="adauga" onClick={()=>{Create('createproiectcontorcontract','Contract');}}>Adauga Contract</button>
        <button className="adauga" onClick={()=>{Create('createproiectcontorprocesverbal','Proces Verbal');}}>Adauga Proces Verbal</button>
           
           <ul>
             {
           numere.map((item,index)=>
              <li key={index}>{item.nume + " " + item.contor}</li>
            )
             }
            </ul> 
            
        </Form>
             <br/>
        <Form title="Adauga Atr">

        <Combobox className="combobox" placeholder="Selecteaza Proiect:" data={proiecte} dataKey='nrProiect' textField={value => value.nrProiect} onChange={value => { setAtr({ ...atr, proiectID: value.id }); setProiect(value); }} renderListItem={({ item }) => (<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | " + item.nrStrada + " | " + (item.firma ? item.firma.nume : item.client.nume)+ " | " + (item.atr ? item.atr.nrAtr : '')}</span>)} />  <input type="number" value={atr.nrATR || ''} placeholder="Introdu Numar ATR (Ex: 6020...):" onChange={(e) => setAtr({ ...atr, nrATR: Number(e.target.value) })} required />
          <label>Introdu data ATR:</label>
          <input type="date" value={atr.dataATR || ''} onChange={(e) => setAtr({ ...atr, dataATR: e.target.value })} required />
          <input value={atr.nrCadastral || ''} placeholder="Introdu Nr. Cadastral (Ex: 342-C1):" onChange={(e) => setAtr({ ...atr, nrCadastral: e.target.value })} />
          <input value={atr.codPostal || ''} placeholder="Introdu Cod Postal (Ex: 5433):" onChange={(e) => setAtr({ ...atr, codPostal: e.target.value })} />
          <input value={atr.cablu || ''} placeholder="Introdu Cablu (Ex: ACYAbY 4x6mmp):" onChange={(e) => setAtr({ ...atr, cablu: e.target.value })} />
          <input type="number" value={atr.lungimeCablu || ''} placeholder="Introdu Lungime Cablu (Ex: 19):" onChange={(e) => setAtr({ ...atr, lungimeCablu: Number(e.target.value) })} />
          <input type="number" value={atr.ohmi || ''} placeholder="Introdu Ohmi (Ex: 4, 10):" onChange={(e) => setAtr({ ...atr, ohmi: Number(e.target.value) })} />
          <input value={atr.punctRacordare || ''} placeholder="Introdu Punct Racordare (Ex: PTAB...):" onChange={(e) => setAtr({ ...atr, punctRacordare: e.target.value })} />
          <input value={atr.punctAlimentare || ''} placeholder="Introdu Punct Alimentare (Ex: Stalp/Firida nr.2):" onChange={(e) => setAtr({ ...atr, punctAlimentare: e.target.value })} />
          <input value={atr.punctBransament || ''} placeholder="Introdu Punct Bransament (Ex: BMPT IR-20A):" onChange={(e) => setAtr({ ...atr, punctBransament: e.target.value })} />
          <input type="number" value={atr.pret || ''} placeholder="Introdu Pret (Ex: 1200):" onChange={(e) => setAtr({ ...atr, pret: Number(e.target.value) })} />
          <input type="number" value={atr.tva || ''} placeholder="Introdu TVA (Ex: 12.23):" onChange={(e) => setAtr({ ...atr, tva: Number(e.target.value) })} />
          <input value={atr.contor || ''} placeholder="Introdu Contor (Ex: CONTOR CE 230/400V):" onChange={(e) => setAtr({ ...atr, contor: e.target.value })} />
          <input type="number" value={atr.temperatura || ''} placeholder="Introdu Temperatura (Ex: 26.6):" onChange={(e) => setAtr({ ...atr, temperatura: Number(e.target.value) })} />
          <textarea value={atr.descriere} placeholder="Introdu Descriere Instalatie (Ex: De la Stalpul...):" onChange={(e) => setAtr({ ...atr, descriere: e.target.value })} />

          <button className="adauga" onClick={()=>{CreateAtr();}}>Adauga Atr</button>
        
         </Form>
        </>
    );
}

export default ClientContor;