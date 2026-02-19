import React, {useState,useEffect } from "react";
import Form from "../components/Form";
import { baseUrl } from "../config/api";
import '../css/components/grup.css';

const Grup = () => {

    const[grup, setGrup] = useState({});
    const [editGrup,setEditGrup]=useState(null);
    const [grupuri,setGrupuri]=useState([]);


    const labelStyle = {
      fontSize: 12,
      fontWeight: 600,
      color: "#555",
      margin: 4
    };



      useEffect(() => {
    fetch(`${baseUrl}/grup/grupe`)
      .then(r => r.json())
      .then(setGrupuri);
  }, []);

    const save = () => {
      console.log(editGrup)
    fetch(`${baseUrl}/grup/edit`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(editGrup)
    }).then(r => {
      if (!r.ok) alert("Eroare la salvare");
      else alert("Salvat");
    });
  };

    const selectGrup = (id) => {
    fetch(`${baseUrl}/grup/id/${id}`)
      .then(r => r.json())
      .then(setEditGrup);
  };

    function Create(){
        fetch(`${baseUrl}/grup/create`, {
            method: 'POST',
            headers: {
              "Content-Type": "application/json",
              "Accept": "application/json"
            },
            body: JSON.stringify(grup)
          }).then(response => {
            if (!response.ok) {
             alert('Eroare');
            }
            else
            {
              setGrup({});
              alert('Salvat');
            }
          }).catch(error =>alert('Eroare'));
    }

    return (
      <div>
        <Form title="Adauga Grup">

            <input value={grup.nume || ''} placeholder="Introdu Nume (Ex: Bransament, Fotovoltaice):" onChange={(e) => setGrup({...grup,nume:e.target.value})}/>

            <button className="adauga" onClick={()=>{Create();}}>Adauga</button>
 
        </Form>

      <Form title="Editează Grup">
       
        <select onChange={e => selectGrup(e.target.value)}>
           <option value="">Selectează grup</option>
          {grupuri.map(g => (
            <option key={g.id} value={g.id}>{g.nume}</option>
          ))}
        </select>
      </Form>

      {editGrup &&    
      <div className="grup">
      <span style={labelStyle}>Grup</span>
      <input
        value={editGrup.nume}
        onChange={e => setEditGrup({ ...editGrup, nume: e.target.value })}
        placeholder="Nume grup"
      />

      {/* FOLDERE */}
      {editGrup.foldere.map((f, fi) => (
        <div key={f.id} >
        <div className="folder-box">
          <span style={labelStyle}>Folder</span>
          <input
            value={f.nume}
            onChange={e => {
              const foldere = [...editGrup.foldere];
              foldere[fi].nume = e.target.value;
              setEditGrup({ ...editGrup, foldere });
            }}
            placeholder="Nume folder"
          />

          <button
            className="btn-danger"
            onClick={() => {
              if (!window.confirm("Ștergi folderul?")) return;
              const foldere = editGrup.foldere.filter((_, idx) => idx !== fi);
              setEditGrup({ ...editGrup, foldere });
            }}
          >
            Șterge folder
          </button>
        </div>

          {/* FISIERE */}
          {f.fisiere.map((fs, fj) => (
            <div className="row file-row">
            <span style={labelStyle}>Fisier</span>
            <input
              value={fs.nume}
              placeholder="Nume fișier"
              onChange={e => {
                const foldere = [...editGrup.foldere];
                foldere[fi].fisiere[fj].nume = e.target.value;
                setEditGrup({ ...editGrup, foldere });
              }}
            />
            <span style={labelStyle}>Denumire contor</span>
            <input
              value={fs.denumireContor}
              placeholder="Denumire contor"
              onChange={e => {
                const foldere = [...editGrup.foldere];
                foldere[fi].fisiere[fj].denumireContor = e.target.value;
                setEditGrup({ ...editGrup, foldere });
              }}
            />

            <button
              className="btn-danger"
              onClick={() => {
                if (!window.confirm("Ștergi fișierul?")) return;
                const foldere = [...editGrup.foldere];
                foldere[fi].fisiere = foldere[fi].fisiere.filter((_, idx) => idx !== fj);
                setEditGrup({ ...editGrup, foldere });
              }}
            >
              Șterge fișier
            </button>
          </div>
          ))}


        </div>
      ))}

      <button className="btn-save" onClick={save}>
        Salvează
      </button>
    </div>
}
    </div>
    );
}

export default Grup;