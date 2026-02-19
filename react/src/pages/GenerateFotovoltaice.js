import React, { useState, useEffect } from "react";
import Form from "../components/Form";
import "../css/components/form.css";
import Combobox from "react-widgets/Combobox";
import "react-widgets/styles.css";
import { baseUrl } from "../config/api";

const Folder = (props) => {
  const [grup, setGrup] = useState({ foldere: [] });
  const [imprima, setImprima] = useState({ proiect: props.proiect });
  const [foldere, setFoldere] = useState([]);
  const [loading, setLoading] = useState(false); // <-- loading state

  useEffect(() => {
    fetch(`${baseUrl}/grup/Fotovoltaice`)
      .then((response) => response.json())
      .then((data) => {
        setGrup(data);
        setFoldere(data.foldere);
      });
  }, []);

  useEffect(() => {
    setImprima({ ...imprima, grup });
  }, [grup]);

  function AddRemove(fisier, check) {
    if (check === false)
      return grup.foldere.map((folder) => {
        if (folder.id === fisier.folderID) {
          return { ...folder, fisiere: folder.fisiere.filter((f) => f.id !== fisier.id) };
        }
        return folder;
      });
    else
      return grup.foldere.map((folder) => {
        if (folder.id === fisier.folderID) {
          return { ...folder, fisiere: [...folder.fisiere, fisier] };
        }
        return folder;
      });
  }

  const HandleAddRemoveAll = (folderIndex, checkState) => {
    var x = JSON.parse(JSON.stringify(grup.foldere));

    document
      .querySelectorAll(`input[type='checkbox'][id='foldercheck${folderIndex}'`)
      .forEach((element) => {
        element.checked = checkState;
      });

    if (checkState) {
      x[folderIndex] = foldere[folderIndex];
    } else {
      x[folderIndex].fisiere = [];
    }

    setGrup({ ...grup, foldere: x });
  };

  function Create() {
    console.log("apasat")
    setLoading(true); // <-- start loading
    fetch(`${baseUrl}/word/createfotovoltaice`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(imprima),
    })
      .then((response) => {
        setLoading(false); // <-- stop loading
        if (!response.ok) {
          alert("Eroare");
        } else alert("Salvat");
      })
      .catch((error) => {
        setLoading(false); // <-- stop loading on error
        alert("Eroare");
      });
  }

  return (
    <Form title="Genereaza Fotovoltaice">
      <h1>
         {imprima.proiect.firma?.nume || imprima.proiect.client.nume} | Proiect nr. {imprima.proiect.nrProiect}
      </h1>

      <div className="checkzone">
        {foldere.map((folder, findex) => (
          <div key={findex}>
            <h3>{folder.nume}</h3>
            <input
              type="checkbox"
              defaultChecked={true}
              onClick={(e) => HandleAddRemoveAll(findex, e.target.checked)}
            />
            {folder.fisiere.map((item, index) => (
              <div className="check" key={index}>
                <input
                  id={`foldercheck${findex}`}
                  type="checkbox"
                  value={JSON.stringify(item)}
                  defaultChecked={true}
                  onChange={(e) => {
                    setGrup({
                      ...grup,
                      foldere: AddRemove(JSON.parse(e.target.value), e.target.checked),
                    });
                  }}
                />
                <label>{item.nume}</label>
              </div>
            ))}
          </div>
        ))}
      </div>

      <button className={`generate ${loading ? "loading" : ""}`} onClick={Create} disabled={loading}>
        {loading ? "Se generează..." : "Generează"}
      </button>
    </Form>
  );
};

export default Folder;
