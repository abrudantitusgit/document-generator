import React, {useEffect,useState } from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import moment from "moment";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const Proiect = () => {

    const [proiect, setProiect] = useState({});
    const [modificaproiect, setModificaProiect] = useState({});
    const [clienti, setClienti] = useState([]);
    const [proiecte, setProiecte] = useState([]);
    const[firme, setFirme] = useState([]);

    useEffect(() => {
        fetch(`${baseUrl}/client/clientifirme`)
        .then(response => response.json())
        .then(data => setClienti(data)).catch(error=>console.log(error));
      },[]);
      
      useEffect(() => {
        fetch(`${baseUrl}/proiect/proiecteinclude`)
        .then(response => response.json())
        .then(data => setProiecte(data)).catch(error=>console.log(error));
      },[]);

      function Create(){
          fetch(`${baseUrl}/proiect/create`, {
              method: 'POST',
              headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
              },
              body: JSON.stringify(proiect)
            }).then(response => {
                if (!response.ok) {
                 alert('Eroare');
                }
                else
                {
                alert('Salvat');
                setProiect({});
                }
              }).catch(error =>alert('Eroare'));
      }
      
      function Modify(){
        fetch(`${baseUrl}/proiect/modify`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(modificaproiect)
          }).then(response => {
              if (!response.ok) {
               alert('Eroare');
              }
              else
              {
              alert('Salvat');
              setModificaProiect({});
              }
            }).catch(error =>alert('Eroare'));
    }

      console.log(proiect);

    return (
      <>
    <Form title = "Adauga Proiect"> 
        <Combobox className="combobox" placeholder="Selecteaza Client:" data={clienti} dataKey='id' textField='nume' onChange={value=>{setProiect({...proiect,clientID:value.id});setFirme(value.firme)}} />
        <Combobox className="combobox" placeholder="Selecteaza Firma:" data={firme} dataKey='id' textField='nume' onChange={value=>{setProiect({...proiect,firmaID:value.id})}} />


        <input value={proiect.denumire || ''} placeholder="Introdu Denumire Proiect (Ex: Bransament...):" onChange={(e) => setProiect({...proiect,denumire:e.target.value})}/>
        <input value={proiect.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setProiect({...proiect,judet:e.target.value})}/>
        <input value={proiect.localitatea || ''} placeholder="Introdu Localitatea (Ex: Localitatea):" onChange={(e) => setProiect({...proiect,localitatea:e.target.value})}/>
        <input value={proiect.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setProiect({...proiect,comuna:e.target.value})}/>
        <input value={proiect.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setProiect({...proiect,strada:e.target.value})}/>
        <input value={proiect.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 9M):" onChange={(e) =>setProiect({...proiect,nrStrada:e.target.value})}/>
        <input value={proiect.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setProiect({...proiect,bloc:e.target.value})}/>
        <input value={proiect.scara || ''} placeholder="Introdu Scara (Ex: C1):" onChange={(e) => setProiect({...proiect,scara:e.target.value})}/>
        <input value={proiect.etaj || ''} placeholder="Introdu Etaj (Ex: 2A):" onChange={(e) => setProiect({...proiect,etaj:e.target.value})}/>
        <input value={proiect.apartament || ''} placeholder="Introdu Apartament (Ex: 3L):" onChange={(e) => setProiect({...proiect,apartament:e.target.value})}/>
        <input value={proiect.sector || ''} placeholder="Introdu Sector (Ex: 2B):" onChange={(e) => setProiect({...proiect,sector:e.target.value})}/>
        <label>Introdu Data emiteri actelor:</label>
        <input type="date" value={proiect.data || ''} onChange={(e) => setProiect({ ...proiect, data: e.target.value })} />
        <input type="number" value={proiect.pi || ''} placeholder="Introdu Putere Instalata (Ex: 17.78):" onChange={(e) => setProiect({ ...proiect, pi: Number(e.target.value) })} />
        <input type="number" value={proiect.pa || ''} placeholder="Introdu Putere Absorbita (Ex: 16.00):" onChange={(e) => setProiect({ ...proiect, pa: Number(e.target.value) })} />
        <input value={proiect.locConsum || ''} placeholder="Introdu Loc Consum (Ex: Locuinta/Casa):" onChange={(e) => setProiect({ ...proiect, locConsum: e.target.value })} />
        <input value={proiect.tensiuneUtilizare || ''} placeholder="Introdu Tensiunea De Utilizare (Ex: 240-400):" onChange={(e) => setProiect({ ...proiect, tensiuneUtilizare:e.target.value})} />
       
     
        <button className="adauga" onClick={()=>{Create();}}>Adauga</button>
    
    </Form>
    <br/>

    <Form title = "Modifica Proiect"> 
    <Combobox className="combobox" placeholder="Selecteaza Proiect:" data={proiecte} dataKey='nrProiect' textField={value=>value.nrProiect} onChange={value=>{setModificaProiect(value);}} renderListItem={({item})=>(<span><strong>{item.nrProiect}</strong>{" | " + item.denumire + " | " + item.strada + " | nr. " + item.nrStrada + " | ap. " + item.apartament + " | " + (item.firma ? item.firma.nume : item.client.nume) + " | " + (item.atr ? item.atr.nrAtr : '')}</span>)}/>
<Combobox disabled className="combobox" placeholder="Selecteaza Client:" value={modificaproiect.client} data={clienti} dataKey='id' textField='nume' onChange={value=>{setModificaProiect({...modificaproiect,clientID:value.id});}} />
<Combobox disabled className="combobox" placeholder="Selecteaza Firma:"  value={modificaproiect.firma} data={firme} dataKey='id' textField='nume' onChange={value=>{setModificaProiect({...modificaproiect,firmaID:value.id})}} />


<input value={modificaproiect.denumire || ''} placeholder="Introdu Denumire Proiect (Ex: Bransament...):" onChange={(e) => setModificaProiect({...modificaproiect,denumire:e.target.value})}/>
<input value={modificaproiect.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setModificaProiect({...modificaproiect,judet:e.target.value})}/>
<input value={modificaproiect.localitatea || ''} placeholder="Introdu Localitatea (Ex: Localitatea):" onChange={(e) => setModificaProiect({...modificaproiect,localitatea:e.target.value})}/>
<input value={modificaproiect.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setModificaProiect({...modificaproiect,comuna:e.target.value})}/>
<input value={modificaproiect.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setModificaProiect({...modificaproiect,strada:e.target.value})}/>
<input value={modificaproiect.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 9M):" onChange={(e) =>setModificaProiect({...modificaproiect,nrStrada:e.target.value})}/>
<input value={modificaproiect.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setModificaProiect({...modificaproiect,bloc:e.target.value})}/>
<input value={modificaproiect.scara || ''} placeholder="Introdu Scara (Ex: C1):" onChange={(e) => setModificaProiect({...modificaproiect,scara:e.target.value})}/>
<input value={modificaproiect.etaj || ''} placeholder="Introdu Etaj (Ex: 2A):" onChange={(e) => setModificaProiect({...modificaproiect,etaj:e.target.value})}/>
<input value={modificaproiect.apartament || ''} placeholder="Introdu Apartament (Ex: 3L):" onChange={(e) => setModificaProiect({...modificaproiect,apartament:e.target.value})}/>
<input value={modificaproiect.sector || ''} placeholder="Introdu Sector (Ex: 2B):" onChange={(e) => setModificaProiect({...modificaproiect,sector:e.target.value})}/>
<label>Introdu Data emiteri actelor:</label>
<input type="date" value={moment(modificaproiect.data).format('YYYY-MM-DD') || ''} onChange={(e) => setModificaProiect({ ...modificaproiect, data: e.target.value })} />
<input type="number" value={modificaproiect.pi || ''} placeholder="Introdu Putere Instalata (Ex: 17.78):" onChange={(e) => setModificaProiect({ ...modificaproiect, pi: Number(e.target.value) })} />
<input type="number" value={modificaproiect.pa || ''} placeholder="Introdu Putere Absorbita (Ex: 16.00):" onChange={(e) => setModificaProiect({ ...modificaproiect, pa: Number(e.target.value) })} />
<input value={modificaproiect.locConsum || ''} placeholder="Introdu Loc Consum (Ex: Locuinta/Casa):" onChange={(e) => setModificaProiect({ ...modificaproiect, locConsum: e.target.value })} />
<input value={modificaproiect.tensiuneUtilizare || ''} placeholder="Introdu Tensiunea De Utilizare (Ex: 240-400):" onChange={(e) => setModificaProiect({ ...modificaproiect, tensiuneUtilizare:e.target.value})} />


<button className="modifica" onClick={()=>{Modify();}}>Modifica</button>

</Form>
    </>
);

}

export default Proiect;