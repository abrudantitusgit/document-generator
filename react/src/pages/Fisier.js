import React, {useEffect, useState } from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const Fisier = () => {

    const[fisier, setFisier] = useState({});
    const[foldere,setFoldere] = useState([]);
    const contor = ["Nu Are","Contract","Proces Verbal","Proces Verbal Cablu", "Buletin cablu","Buletin priza", "Buletin cablu utilizare", "Buletin priza utilizare"];

    useEffect(()=>{
        fetch(`${baseUrl}/folder/foldere`)
        .then(response => response.json())
        .then(data => setFoldere(data)).catch(error=>console.log(error));
    },[]);

    function Create(){
        fetch(`${baseUrl}/fisier/create`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(fisier)
          }).then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            {

              alert('Salvat');
            }
          }).catch(error =>alert('Eroare'));
    }

console.log(fisier);

    return (
        <Form title="Adauga Fisier">

            <Combobox className="combobox" placeholder="Selecteaza Folder:" data={foldere} dataKey='id' textField='nume' onChange={value=>{setFisier({...fisier,folderID:value.id})}} />
            <Combobox className="combobox" placeholder="Selecteaza Denumire Contor:" data={contor}  onChange={value=>{setFisier({...fisier,denumireContor:value})}} />
            <input value={fisier.nume || ''} placeholder="Introdu Nume (Ex: Proces Verbal):" onChange={(e)=> setFisier({...fisier,nume:e.target.value})}/>

            <button className="adauga" onClick={()=>{Create();}}>Adauga</button>
 
        </Form>
    );
}

export default Fisier;