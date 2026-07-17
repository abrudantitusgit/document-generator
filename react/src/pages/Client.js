import React, {useState, useEffect } from "react";
import Combobox from "react-widgets/Combobox";
import Form from "../components/Form";
import moment from "moment";
import { baseUrl } from "../config/api";

const Client = () => {

    const [client, setClient] = useState({});
    const [modificaclient,setModificaClient] = useState({});
    const [clienti,setClienti] = useState([]);

    useEffect(()=>{
      fetch(`${baseUrl}/client/clienti`)
      .then(response => response.json())
      .then(data => setClienti(data)).catch(error=>console.log(error));
  },[]);

  

    function Create() {
      fetch(`${baseUrl}/client/create`, {
        method: 'POST',
        headers: {
          "Content-Type": "application/json",
          "Accept": "application/json"
        },
        body: JSON.stringify(client)
      })
      .then(async response => {
        const data = await response.json().catch(() => null);

        if (!response.ok) {

        let mesaj = "Eroare necunoscută";

        if (data?.message) {
          mesaj = data.message;
        }
        else if (data?.errors) {
          mesaj = Object.values(data.errors)
            .flat()
            .join("\n");
        }

        alert(mesaj);
        return;
      }

        setClient({});
        alert(data?.message || "Client salvat cu succes!");
      })
      .catch(() => alert('Eroare la server'));
    }

  function Modify() {
    fetch(`${baseUrl}/client/modify`, { // <-- corectat
      method: 'POST',
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(modificaclient)
    })
    .then(async response => {
      const data = await response.json().catch(() => null);

      if (!response.ok) {

        let mesaj = "Eroare necunoscută";

        if (data?.message) {
          mesaj = data.message;
        }
        else if (data?.errors) {
          mesaj = Object.values(data.errors)
            .flat()
            .join("\n");
        }

        alert(mesaj);
        return;
      }

      setModificaClient({});
      alert(data?.message || "Client modificat cu succes!");
    })
    .catch(() => alert('Eroare la server'));
  }

    return (
      <>
    <Form title = "Adauga Client">

    <input value={client.nume || ''} placeholder="Introdu Nume (Ex: Ioan Viorel):" onChange={(e) => setClient({...client,nume:e.target.value})}/>
    <input type="number" value={client.cnp || ''} placeholder="Introdu CNP (Ex: 1234567890123):" onChange={(e) => setClient({...client,cnp:Number(e.target.value)})}/>
    <label>Introdu data eliberare buletin (Ex: 12/01/2021):</label>
    <input type="date" value={client.dataBuletin || ''} onChange={(e) => setClient({...client,dataBuletin:e.target.value})}/>
    <input value={client.seria || ''} placeholder="Introdu Seria (Ex: ZH):" onChange={(e) => setClient({...client,seria:e.target.value})}/>
    <input  value={client.nrSeria || ''} placeholder="Introdu Nr. Seria (Ex: 123456):" onChange={(e) => setClient({...client,nrSeria:e.target.value})}/>
    <input value={client.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setClient({...client,judet:e.target.value})}/>
    <input value={client.localitatea || ''} placeholder="Introdu Localitatea (Ex: Oradea):" onChange={(e) => setClient({...client,localitatea:e.target.value})}/>
    <input value={client.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setClient({...client,comuna:e.target.value})}/>
    <input value={client.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setClient({...client,strada:e.target.value})}/>
    <input value={client.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 12 A/2):" onChange={(e) => setClient({...client,nrStrada:e.target.value})}/>
    <input value={client.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setClient({...client,bloc:e.target.value})}/>
    <input value={client.scara || ''} placeholder="Introdu Scara (Ex: 2A):" onChange={(e) => setClient({...client,scara:e.target.value})}/>
    <input value={client.etaj || ''} placeholder="Introdu Etaj (Ex: 10L):" onChange={(e) => setClient({...client,etaj:e.target.value})}/>
    <input value={client.apartament || ''} placeholder="Introdu Apartament (Ex: 9L):" onChange={(e) => setClient({...client,apartament:e.target.value})}/>
    <input value={client.sector || ''} placeholder="Introdu Sector (Ex: A32B):" onChange={(e)=>setClient({...client,sector:e.target.value})}/>
    <input value={client.eliberat || ''} placeholder="Introdu buletin eliberat (Ex: Oradea):" onChange={(e) => setClient({...client,eliberat:e.target.value})}/>
    <input value={client.codPostal || ''} placeholder="Introdu Cod Postal (Ex: T3924):" onChange={(e) => setClient({...client,codPostal:e.target.value})}/>
    <input value={client.telefon || ''} placeholder="Introdu Telefon (Ex: 075124...):" onChange={(e) => setClient({...client,telefon:e.target.value})}/>

    <button className="adauga" onClick={()=>{Create();}}>Adauga</button>
    
    </Form>

<br/>
<Form title = "Modifica Client">
<Combobox className="combobox" placeholder="Selecteaza Client:" data={clienti} dataKey='id' textField='nume' onChange={value=>{setModificaClient(value);}} />
<input value={modificaclient.nume || ''} placeholder="Introdu Nume (Ex: Ioan Viorel):" onChange={(e) => setModificaClient({...modificaclient,nume:e.target.value})}/>
<input type="number" value={modificaclient.cnp || ''} placeholder="Introdu CNP (Ex: 1234567890123):" onChange={(e) => setModificaClient({...modificaclient,cnp:Number(e.target.value)})}/>
<label>Introdu data eliberare buletin (Ex: 12/01/2021):</label>
<input type="date" value={moment(modificaclient.dataBuletin).format('YYYY-MM-DD') || ''} onChange={(e) => setModificaClient({...modificaclient,dataBuletin:e.target.value})}/>
<input value={modificaclient.seria || ''} placeholder="Introdu Seria (Ex: ZH):" onChange={(e) => setModificaClient({...modificaclient,seria:e.target.value})}/>
<input  value={modificaclient.nrSeria || ''} placeholder="Introdu Nr. Seria (Ex: 123456):" onChange={(e) => setModificaClient({...modificaclient,nrSeria:e.target.value})}/>
<input value={modificaclient.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setModificaClient({...modificaclient,judet:e.target.value})}/>
<input value={modificaclient.localitatea || ''} placeholder="Introdu Localitatea (Ex: Oradea):" onChange={(e) => setModificaClient({...modificaclient,localitatea:e.target.value})}/>
<input value={modificaclient.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setModificaClient({...modificaclient,comuna:e.target.value})}/>
<input value={modificaclient.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setModificaClient({...modificaclient,strada:e.target.value})}/>
<input value={modificaclient.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 12 A/2):" onChange={(e) => setModificaClient({...modificaclient,nrStrada:e.target.value})}/>
<input value={modificaclient.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setModificaClient({...modificaclient,bloc:e.target.value})}/>
<input value={modificaclient.scara || ''} placeholder="Introdu Scara (Ex: 2A):" onChange={(e) => setModificaClient({...modificaclient,scara:e.target.value})}/>
<input value={modificaclient.etaj || ''} placeholder="Introdu Etaj (Ex: 10L):" onChange={(e) => setModificaClient({...modificaclient,etaj:e.target.value})}/>
<input value={modificaclient.apartament || ''} placeholder="Introdu Apartament (Ex: 9L):" onChange={(e) => setModificaClient({...modificaclient,apartament:e.target.value})}/>
<input value={modificaclient.sector || ''} placeholder="Introdu Sector (Ex: A32B):" onChange={(e)=>setModificaClient({...modificaclient,sector:e.target.value})}/>
<input value={modificaclient.eliberat || ''} placeholder="Introdu buletin eliberat (Ex: Oradea):" onChange={(e) => setModificaClient({...modificaclient,eliberat:e.target.value})}/>
<input value={modificaclient.codPostal || ''} placeholder="Introdu Cod Postal (Ex: T3924):" onChange={(e) => setModificaClient({...modificaclient,codPostal:e.target.value})}/>
<input value={modificaclient.telefon || ''} placeholder="Introdu Telefon (Ex: 075124...):" onChange={(e) =>setModificaClient({...modificaclient,telefon:e.target.value})}/>

<button className="modifica" onClick={()=>{Modify();}}>Modifica</button>

</Form>

      </>

);

}

export default Client;