import React, { useState, useEffect } from "react";
import { baseUrl } from "../config/api";

// Drawings generated together with the documents: toggle, detected use case and
// preview / download links for every drawing and format.
const Desene = ({ endpoint, checked, onChange }) => {
  const [info, setInfo] = useState(null);

  useEffect(() => {
    setInfo(null);
    fetch(`${baseUrl}${endpoint}`)
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => setInfo(data))
      .catch((error) => console.error("Eroare la încărcarea desenelor", error));
  }, [endpoint]);

  return (
    <div className="checkzone">
      <h3>Desene (generatedDraws)</h3>
      <div className="check">
        <input id="genereazaDesene" type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)} />
        <label htmlFor="genereazaDesene">Generează desenele împreună cu documentele</label>
      </div>
      {info && (
        <>
          <p>
            Caz de utilizare: <strong>{info.useCase}</strong> — {info.description}
          </p>
          {info.drawings.map((drawing) => (
            <div className="check" key={drawing.code}>
              <label>
                {drawing.code} {drawing.title}:{" "}
                {info.formats.map((format) => (
                  <a key={format} href={`${baseUrl}${endpoint}/${drawing.code}/${format}`} target="_blank" rel="noreferrer" style={{ marginRight: 10 }}>
                    {format}
                  </a>
                ))}
              </label>
            </div>
          ))}
        </>
      )}
    </div>
  );
};

export default Desene;
