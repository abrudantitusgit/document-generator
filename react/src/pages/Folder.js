import React, { useState, useEffect } from "react";
import Form from "../components/Form";
import Combobox from "react-widgets/Combobox";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const Folder = () => {

  const [folder, setFolder] = useState({});
  const [grup, setGrup] = useState([]);

  useEffect(()=>{
    fetch(`${baseUrl}/grup/grupe`)
    .then(response => response.json())
    .then(data => setGrup(data)).catch(error=>console.log(error));
  },[]);

  function Create() {
    fetch(`${baseUrl}/folder/create`, {
      method: 'POST',
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(folder)
    }).then(response => {
      if (!response.ok) {
        alert('Eroare');
      }
      else {
        alert('Salvat');
      }
    }).catch(error => alert('Eroare'));
  }

  return (
    <Form title="Adauga Folder">

      <Combobox className="combobox" placeholder="Selecteaza Grup:" data={grup} dataKey='id' textField='nume' onChange={value=>{setFolder({...folder,grupID:value.id})}} />
      <input value={folder.nume || ''} placeholder="Introdu Nume (Ex: Cartea tehnica):" onChange={(e) => setFolder({ ...folder, nume: e.target.value })} />

      <button className="adauga" onClick={() => { Create(); }}>Adauga</button>

    </Form>
  );
}

export default Folder;