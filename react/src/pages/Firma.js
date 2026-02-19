import React, {useState, useEffect} from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import moment from "moment";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const Firma = () => {

    const [firma, setFirma] = useState({});
    const [modificafirma,setModificaFirma] = useState({});
    const [firme,setFirme] = useState([]);
    const [clienti, setClienti] = useState([]);
    console.log(modificafirma)
    useEffect(() => {
      fetch(`${baseUrl}/client/clientifirme`)
      .then(response => response.json())
      .then(data => setClienti(data)).catch(error=>console.log(error));
    },[]);

      useEffect(()=>{
        fetch(`${baseUrl}/firma/list`)
        .then(response => response.json())
        .then(data => setFirme(data)).catch(error=>console.log(error));
    },[]);

    function Create(){
        fetch(`${baseUrl}/firma/create`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(firma)
          }).then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            {
              setFirma({});
              alert('Salvat');
            }
          }).catch(error =>alert('Eroare'));
    }

    function Modify(){
      console.log(modificafirma)
      fetch(`${baseUrl}/firma/modify`, {
          method: 'POST',
          headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
          },
          body: JSON.stringify(modificafirma)
        }).then(response => {
          if (!response.ok) {
           alert('Eroare');
          }
          else
          {
            setModificaFirma({});
            alert('Salvat');
          }
        }).catch(error =>alert('Eroare'));
  }
console.log(firma)
    return (
      <>
    <Form title = "Adauga Firma">

    <Combobox className="combobox" placeholder="Select Client:" data={clienti} dataKey='id' textField='nume' onChange={value=>setFirma({...firma,clientID:value.id})} />
    <input value={firma.nume || ''} placeholder="Introdu Nume (Ex: S.C. AVA IMOB S.R.L):" onChange={(e) => setFirma({...firma,nume:e.target.value})}/>
    <input type="number" value={firma.cui || ''} placeholder="Introdu CUI (Ex: 123456):" onChange={(e) => setFirma({...firma,cui:Number(e.target.value)})}/>
    <label>Introdu data CUI (Ex: 12/01/2021):</label>
    <input type="date" value={firma.dataCui || ''} onChange={(e) => setFirma({...firma,dataCui:e.target.value})}/>
    <input value={firma.functia || ''} placeholder="Introdu Functia (Ex: Administrator):" onChange={(e) => setFirma({...firma,functia:e.target.value})}/>
    <input value={firma.orc || ''} placeholder="Introdu ORC (Ex: J05/89/2011):" onChange={(e) => setFirma({...firma,orc:e.target.value})}/>
    <input value={firma.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setFirma({...firma,judet:e.target.value})}/>
    <input value={firma.localitatea || ''} placeholder="Introdu Localitatea (Ex: Oradea):" onChange={(e) => setFirma({...firma,localitatea:e.target.value})}/>
    <input value={firma.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setFirma({...firma,comuna:e.target.value})}/>
    <input value={firma.sector || ''} placeholder="Introdu Sector (Ex: A32B):" onChange={(e)=>setFirma({...firma,sector:e.target.value})}/>
    <input value={firma.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setFirma({...firma,strada:e.target.value})}/>
    <input value={firma.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 12 A/2):" onChange={(e) => setFirma({...firma,nrStrada:e.target.value})}/>
    <input value={firma.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setFirma({...firma,bloc:e.target.value})}/>
    <input value={firma.scara || ''} placeholder="Introdu Scara (Ex: 2A):" onChange={(e) => setFirma({...firma,scara:e.target.value})}/>
    <input value={firma.etaj || ''} placeholder="Introdu Etaj (Ex: 10L):" onChange={(e) => setFirma({...firma,etaj:e.target.value})}/>
    <input value={firma.apartament || ''} placeholder="Introdu Apartament (Ex: 9L):" onChange={(e) => setFirma({...firma,apartament:e.target.value})}/>
    <input value={firma.codPostal || ''} placeholder="Introdu Cod Postal (Ex: T3924):" onChange={(e) => setFirma({...firma,codPostal:e.target.value})}/>
    <input value={firma.telefon || ''} placeholder="Introdu Telefon (Ex: 075124...):" onChange={(e) => setFirma({...firma,telefon:e.target.value})}/>

    <button className="adauga" onClick={()=>{Create();}}>Adauga</button>
    
    </Form>

    <br/>

    <Form title = "Modifica Firma">

    <Combobox className="combobox" placeholder="Selecteaza Client:" data={clienti} dataKey='id' textField='nume' onChange={value=>setModificaFirma({...modificafirma,clientID:value.id})} />
    <Combobox className="combobox" placeholder="Selecteaza Firma:" data={firme} dataKey='id' textField='nume' onChange={value=>{setModificaFirma(value)}} />
<input value={modificafirma.nume || ''} placeholder="Introdu Nume (Ex: S.C. AVA IMOB S.R.L):" onChange={(e) => setModificaFirma({...modificafirma,nume:e.target.value})}/>
<input type="number" value={modificafirma.cui || ''} placeholder="Introdu CUI (Ex: 123456):" onChange={(e) => setModificaFirma({...modificafirma,cui:Number(e.target.value)})}/>
<label>Introdu data CUI (Ex: 12/01/2021):</label>
<input type="date" value={moment(modificafirma.dataCui).format('YYYY-MM-DD') || ''} onChange={(e) => setModificaFirma({...modificafirma,dataCui:e.target.value})}/>
<input value={modificafirma.functia || ''} placeholder="Introdu Functia (Ex: Administrator):" onChange={(e) => setModificaFirma({...modificafirma,functia:e.target.value})}/>
<input value={modificafirma.orc || ''} placeholder="Introdu ORC (Ex: J05/89/2011):" onChange={(e) => setModificaFirma({...modificafirma,orc:e.target.value})}/>
<input value={modificafirma.judet || ''} placeholder="Introdu Judet (Ex: Bihor):" onChange={(e) => setModificaFirma({...modificafirma,judet:e.target.value})}/>
<input value={modificafirma.localitatea || ''} placeholder="Introdu Localitatea (Ex: Oradea):" onChange={(e) => setModificaFirma({...modificafirma,localitatea:e.target.value})}/>
<input value={modificafirma.comuna || ''} placeholder="Introdu Comuna (Ex: Osorhei):" onChange={(e) => setModificaFirma({...modificafirma,comuna:e.target.value})}/>
<input value={modificafirma.sector || ''} placeholder="Introdu Sector (Ex: A32B):" onChange={(e)=>setModificaFirma({...modificafirma,sector:e.target.value})}/>
<input value={modificafirma.strada || ''} placeholder="Introdu Strada (Ex: Principala):" onChange={(e) => setModificaFirma({...modificafirma,strada:e.target.value})}/>
<input value={modificafirma.nrStrada || ''} placeholder="Introdu Nr. Strada (Ex: 12 A/2):" onChange={(e) => setModificaFirma({...modificafirma,nrStrada:e.target.value})}/>
<input value={modificafirma.bloc || ''} placeholder="Introdu Bloc (Ex: PB32):" onChange={(e) => setModificaFirma({...modificafirma,bloc:e.target.value})}/>
<input value={modificafirma.scara || ''} placeholder="Introdu Scara (Ex: 2A):" onChange={(e) => setModificaFirma({...modificafirma,scara:e.target.value})}/>
<input value={modificafirma.etaj || ''} placeholder="Introdu Etaj (Ex: 10L):" onChange={(e) => setModificaFirma({...modificafirma,etaj:e.target.value})}/>
<input value={modificafirma.apartament || ''} placeholder="Introdu Apartament (Ex: 9L):" onChange={(e) => setModificaFirma({...modificafirma,apartament:e.target.value})}/>
<input value={modificafirma.codPostal || ''} placeholder="Introdu Cod Postal (Ex: T3924):" onChange={(e) => setModificaFirma({...modificafirma,codPostal:e.target.value})}/>
<input value={modificafirma.telefon || ''} placeholder="Introdu Telefon (Ex: 075124...):" onChange={(e) => setModificaFirma({...modificafirma,telefon:e.target.value})}/>

<button className="modifica" onClick={()=>{Modify();}}>Modifica</button>

</Form>

    </>
);

}

export default Firma;