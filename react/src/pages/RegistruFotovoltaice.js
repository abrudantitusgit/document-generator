import React, { useEffect, useMemo, useState } from "react";
import { baseUrl } from "../config/api";
import ProiectFotovoltaice from "./ProiectFotovoltaice.js";
import Modal from "../components/Modal.js";
import GenerateFotovoltaice from "./GenerateFotovoltaice.js"

export default function ProiecteFotovoltaiceList() {

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");
  const [q, setQ] = useState("");
  const [filterCounty, setFilterCounty] = useState("");
  const [filterSystem, setFilterSystem] = useState("");
  const [expanded, setExpanded] = useState(() => new Set());
  const [editing, setEditing] = useState(null);
  const [isEditOpen, setIsEditOpen]=useState(false);
  const [isAddOpen, setIsAddOpen]=useState(false);
  const [generating, setGenerating]=useState(null);
  const [isGenerateOpen, setIsGenerateOpen]=useState(false);
  const [sortBy, setSortBy] = useState("nrProiect"); // data, nrProiect, client
  const [sortDir, setSortDir] = useState("desc"); // asc, desc
  console.log("In Registru")
  
  const load = async () => {
    setLoading(true);
    setErr("");
    try {
      const res = await fetch(`${baseUrl}/proiectfotovoltaice`);
      if (!res.ok) {
        const t = await res.text();
        throw new Error(`Eroare ${res.status}: ${t}`);
      }
      const data = await res.json();
      setItems(Array.isArray(data) ? data : []);
    } catch (e) {
      setErr(e?.message || "Eroare la încărcare proiecte.");
      setItems([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  
  
  const toggle = (id) => {
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };


  const startEdit = (p) => {
    setIsEditOpen(true);
    setEditing(p.id ?? p.ID);
};
  const startGenerate = (p) => {
    setIsGenerateOpen(true);
    setGenerating(p);
};


  const filtered = useMemo(() => {
    const s = q.trim().toLowerCase();
    let list = items;

    if (s) {
      list = list.filter((p) => {
        const hay = [
          p.nrProiect,
          p.judet,
          p.localitatea,
          p.comuna,
          p.strada,
          p.pod,
          p.tipSistem,
          p.client?.nume,
          p.dateInvertoare?.map(i => i.model).join(" "),
          p.datePanouri?.map(i => i.model).join(" "),
          p.dateBaterii?.map(i => i.model).join(" "),
        ]
          .filter(Boolean)
          .join(" ")
          .toLowerCase();

        return hay.includes(s);
      });
    }

    if (filterCounty) list = list.filter((project) => project.judet === filterCounty);
    if (filterSystem) list = list.filter((project) => project.tipSistem === filterSystem);

    // sort desc by data (dacă lipsește, lasă jos)
    list = [...list].sort((a, b) => {
      let va, vb;

      switch (sortBy) {
        case "nrProiect":
          va = a.nrProiect ?? "";
          vb = b.nrProiect ?? "";
          break;

        case "client":
          va = a.client?.nume ?? "";
          vb = b.client?.nume ?? "";
          break;

        case "data":
        default:
          va = a.data ? new Date(a.data).getTime() : 0;
          vb = b.data ? new Date(b.data).getTime() : 0;
          break;
      }

      if (va < vb) return sortDir === "asc" ? -1 : 1;
      if (va > vb) return sortDir === "asc" ? 1 : -1;
      return 0;
    });

    return list;
  }, [items, q, filterCounty, filterSystem, sortBy, sortDir]);

  const counties = [...new Set(items.map((item) => item.judet).filter(Boolean))].sort();
  const systemTypes = [...new Set(items.map((item) => item.tipSistem).filter(Boolean))].sort();

  const formatDate = (iso) => {
    if (!iso) return "-";

    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) return iso;

    return d.toLocaleDateString("ro-RO", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  };

const startAutocomplete = async (p) => {
  try {
    const res = await fetch("http://localhost:5001/autocomplete", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(p)
    });

    const data = await res.json();

    console.log(data);
    return data;

  } catch (err) {
    console.error("Eroare:", err);
  }
};

const onDelete = async (id) => {
  if (!window.confirm("Sigur ștergi proiectul?")) return;

  try {
    const res = await fetch(`${baseUrl}/proiectfotovoltaice/${id}`, {
      method: "DELETE",
    });

    if (!res.ok) {
      throw new Error(await res.text());
    }

    // scoate din listă fără reload
    setItems((prev) =>
      prev.filter((x) => (x.id ?? x.ID) !== id)
    );
  } catch (e) {
    alert(e?.message || "Eroare la ștergere");
  }
};

  const handleExport =  () => {
     const apiUrl =`${baseUrl}/proiectfotovoltaice/export`;
  // navigare directă către endpoint => download normal
    window.location.assign(apiUrl);
  };


  return (
    <div style={{ margin: "0 auto", padding: 16 }}>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
        <h2 style={{ margin: 0 }}>Proiecte Fotovoltaice</h2>

        <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
         <button style={styles.btn} onClick={handleExport}>Export Excel</button>
         
          <button style={styles.btnAdd} onClick={() => setIsAddOpen(true)}>Adaugă Proiect</button>
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Caută după nrProiect, județ, localitate, POD, tip..."
            style={styles.input}
          />
          <select className="pv-toolbar-filter" aria-label="Filtrează după județ" value={filterCounty} onChange={(event) => setFilterCounty(event.target.value)}>
            <option value="">Toate județele</option>{counties.map((county) => <option key={county} value={county}>{county}</option>)}
          </select>
          <select className="pv-toolbar-filter" aria-label="Filtrează după tipul sistemului" value={filterSystem} onChange={(event) => setFilterSystem(event.target.value)}>
            <option value="">Toate sistemele</option>{systemTypes.map((system) => <option key={system} value={system}>{system}</option>)}
          </select>
          <div style={{ display: "flex", gap: 6 }}>

          <button
            style={styles.btn}
            onClick={() => {
              setSortBy("data");
              setSortDir(sortDir === "asc" ? "desc" : "asc");
            }}
          >
            Sortează după dată {sortBy === "data" ? (sortDir === "asc" ? "↑" : "↓") : ""}
          </button>

          <button
            style={styles.btn}
            onClick={() => {
              setSortBy("nrProiect");
              setSortDir(sortDir === "asc" ? "desc" : "asc");
            }}
          >
            Sortează nr proiect {sortBy === "nrProiect" ? (sortDir === "asc" ? "↑" : "↓") : ""}
          </button>

          <button
            style={styles.btn}
            onClick={() => {
              setSortBy("client");
              setSortDir(sortDir === "asc" ? "desc" : "asc");
            }}
          >
            Sortează client {sortBy === "client" ? (sortDir === "asc" ? "↑" : "↓") : ""}
          </button>

        </div>
          <button onClick={load} style={styles.btn} disabled={loading}>
            {loading ? "Refresh..." : "Refresh"}
          </button>
        </div>
      </div>

      {err && (
        <div style={{ ...styles.alert, background: "#fdecea", borderColor: "#e74c3c" }}>
          {err}
        </div>
      )}

      <div style={{ marginTop: 12, color: "#666" }}>
        {loading ? "Se încarcă..." : `Rezultate: ${filtered.length} / ${items.length}`}
      </div>

    

      <div style={{ marginTop: 12, overflowX: "auto" }}>
        <table style={{ width: "100%", borderCollapse: "collapse", background: "#fff" }}>
          <thead>
            <tr>
              <th style={styles.th}>Nr Proiect</th>
              <th style={styles.th}>Nr PV</th>
              <th style={styles.th}>Data</th>
              <th style={styles.th}>Client</th>
              <th style={styles.th}>CNP</th>
              <th style={styles.th}>Firmă</th>
              <th style={styles.th}>CUI</th>
              <th style={styles.th}>Tip Sistem</th>
              <th style={styles.th}>POD</th>
              <th style={styles.th}>Adresă</th>
              <th style={styles.th}>Telefon</th>
              <th style={styles.th}>Panouri (kW)</th>
              <th style={styles.th}>Invertor (kW)</th>
              <th style={styles.th}>Baterii (Ah)</th>
              <th style={styles.th}>PRM Masurat</th>
              <th style={styles.th}>PRM Calculat</th>
              <th style={styles.th}>Acțiuni</th>
            </tr>
          </thead>

          <tbody>
            {filtered.map((p) => {
              const id = p.id || p.ID;
              const isOpen = expanded.has(id);

              const adresa =
                [
                  p.localitatea && `loc. ${p.localitatea}`,
                  p.comuna && `com. ${p.comuna}`,
                  p.strada && `str. ${p.strada}`,
                  p.nrStrada && `nr. ${p.nrStrada}`,
                  p.apartament && `ap. ${p.apartament}`,
                  p.judet && `jud. ${p.judet}`,
                ]
                  .filter(Boolean)
                  .join(", ") || "-";

              const panouriKw =
                p.putereTotalaPanouri != null
                  ? (Number(p.putereTotalaPanouri) / 1000).toFixed(3)
                  : "-";

              const invertorKw =
                p.putereTotalaInvertor != null
                  ? Number(p.putereTotalaInvertor).toFixed(2)
                  : "-";

              const bateriiAh =
                p.capacitateTotalaBaterii != null
                  ? Number(p.capacitateTotalaBaterii).toFixed(2)
                  : "-";

              return (
                <React.Fragment key={id}>
                  <tr style={{ borderTop: "1px solid #f1f1f1" }}>
                    <td style={styles.td}>{p.nrProiect ?? "-"}</td>
                    <td style={styles.td}>{p.nrPV ?? "-"}</td>
                    <td style={styles.td}>{formatDate(p.data)}</td>
                    <td style={styles.td}>{p.client?.nume ?? "-"}</td>
                    <td style={styles.td}>{p.client?.cnp ?? "-"}</td>
                    <td style={styles.td}>{p.firmaID ? (p.firma?.nume ?? "-") : "-"}</td>
                    <td style={styles.td}>{p.firmaID ? (p.firma?.cui ?? "-") : "-"}</td>
                    <td style={styles.td}>{p.tipSistem ?? "-"}</td>
                    <td style={styles.td}>{p.pod ? p.pod : "-"}</td>
                    <td style={styles.td}>{adresa}</td>
                    <td style={styles.td}>{p.client?.telefon ?? "-"}</td>
                    <td style={styles.td}>{panouriKw}</td>
                    <td style={styles.td}>{invertorKw}</td>
                    <td style={styles.td}>{bateriiAh}</td>
                    <td style={styles.td}>{(p.prM_Masurat ?? "-").toString()}</td>
                    <td style={styles.td}>{(p.prM_Calculat ?? "-").toString()}</td>

                    <td style={{ ...styles.td, whiteSpace: "nowrap" }}>
                      <button onClick={() => toggle(id)} style={styles.btnPrimary}>
                        {isOpen ? "Ascunde" : "Detalii"}
                      </button>{" "}
                      <button onClick={() => onDelete(id)} style={styles.btnDanger}>
                        Șterge
                      </button>{" "}
                      <button onClick={() => startEdit(p)} style={styles.btnEdit}>
                        Edit
                      </button>{" "}
                      <button onClick={() => startGenerate(p)} style={styles.btnGenerate}>
                        Generează Word
                      </button>
                    
                     { /*<button onClick={() => startAutocomplete(p)} style={styles.btn}>
                        Autocomplete Electrica
                      </button>*/ }
                    
                    </td>
                  </tr>

                  {isOpen && (
                   <tr>
                  <td style={{padding: 12, background: "#ffffff" }} colSpan={17}>
                      <div style={{display:"grid",width:"100%",gridTemplateColumns: "1fr 1fr",gap: 20,alignItems: "start"}}> 
                          <DetailsBlock title="Invertoare">
                            <SimpleTable
                          
                              headers={["Nume", "Nr", "Putere instalată (kW)", "Serie"]}
                              rows={(p.dateInvertoare ?? []).map((x) => [
                                x.model ?? "-",
                                x.nrInvertoare ?? "-",
                                x.putereInstalata ?? "-",
                                x.serie ?? "-",
                              ])}
                              emptyText="Nu există invertoare."
                            />
                          </DetailsBlock>

                          <DetailsBlock title="Panouri">
                            <SimpleTable
                              headers={["Nume", "Nr panouri", "Putere panou (W)", "Serii", "Total (kW)"]}
                              rows={(p.datePanouri ?? []).map((x) => {
                                const nr = Number(x.nrPanouri ?? 0);
                                const pw = Number(x.putereInstalata ?? 0);
                                return [x.model ?? "-", nr, pw, x.serie ?? "-", ((nr * pw) / 1000).toFixed(3)];
                              })}
                              emptyText="Nu există panouri."
                            />
                          </DetailsBlock>
             
                          <DetailsBlock title="Baterii">
                            <SimpleTable
                              headers={["Nume", "Nr baterii", "Putere baterie (kW)", "Capacitate (Ah)", "Serie"]}
                              rows={(p.dateBaterii ?? []).map((x) => {
                                const nr = Number(x.nrBaterii ?? 0);
                                const pw = Number(x.putereInstalata ?? 0);
                                const cap = Number(x.capacitate ?? 0);
                                return [x.model ?? "-", nr, pw, cap, x.serie ?? "-"];
                              })}
                              emptyText="Nu există baterii"
                            />
                          </DetailsBlock>
                
                          <DetailsBlock title="Accesorii">
                            <SimpleTable
                              headers={["Sistem Backup", "Smart Meter"]}
                              rows={p.backup || p.smartMeter ? [[p.backup || "-", p.smartMeter || "-"]] : []}
                              emptyText="Nu există"
                            />
                          </DetailsBlock>
                 
           </div>  
                  
                    </td>
                  </tr>
                  )}
                </React.Fragment>
              );
            })}
          </tbody>
        </table>
      </div>

      <Modal
        open={isEditOpen}
        onClose={() => setIsEditOpen(false)}
        width={1300}
      >
        <ProiectFotovoltaice id={editing}
        onSuccess={(updated) => {
            const updatedId = updated.id;

            setItems((prev) =>
              prev.map((p) => {
                const pid = p.id ;
                return pid === updatedId ? updated : p;
              })
            );

            setIsEditOpen(false);
            setEditing(null);
        }}
         />
      </Modal>

      <Modal open={isAddOpen} onClose={() => setIsAddOpen(false)} width={1300}>
        <ProiectFotovoltaice onSuccess={(created) => {
          setItems((previous) => [created, ...previous]);
          setIsAddOpen(false);
        }} />
      </Modal>

    <Modal open={isGenerateOpen} onClose={() => setIsGenerateOpen(false)} width={1300}>
        <GenerateFotovoltaice proiect={generating}/>
    </Modal>

    </div>
  );
}

function DetailsBlock({ title, children }) {
  return (
    <div style={{ width:"100%",minWidth: 0, paddingTop: 10, paddingLeft:10, marginTop: 10,borderLeft: "1px solid #ddd", borderBottom: "1px solid #ddd"  }}>
      <div style={{ fontSize: 14, fontWeight: 800, marginBottom: 6 }}>{title}</div>
      {children}
    </div>
  );
}

function SimpleTable({ headers, rows, emptyText }) {
  if (!rows || rows.length === 0) return <div style={{ color: "#666" }}>{emptyText}</div>;

  return (
    <div style={{overflowX: "auto" }}>
      <table style={{width:"100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            {headers.map((h) => (
                <th
              key={h}
              style={{
                ...styles.th,
                width: h === "Serii" ? 400 : "auto",   // aici o faci mai îngustă
              }}
            >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((r, idx) => (
            <tr key={idx} style={{ borderTop: "1px solid #f1f1f1" }}>
              {r.map((cell, j) => (
                <td key={j} style={styles.td}>
                  {cell}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}



const styles = {

th: {
  textAlign: "left",
  padding: 12,
  fontSize: 13,
  fontWeight: 600,
  color: "#0d0e0e",
  borderBottom: "1px solid #e5e7eb",
  borderRight: "1px solid #f3f4f6",
  whiteSpace: "nowrap",
},

td: {
  padding: 12,
  fontSize: 14,
  fontWeight: 400,
  color: "#111827",
  borderRight: "1px solid #f3f4f6",
  verticalAlign: "top",
},
  modalBackdrop: {
  position: "fixed",
  inset: 0,
  background: "rgba(0,0,0,0.4)",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  zIndex: 1000,
},
modal: {
  background: "#fff",
  padding: 20,
  borderRadius: 14,
  width: 420,
  display: "flex",
  flexDirection: "column",
  gap: 10,
},
  input: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #ccc",
    minWidth: 420,
    outline: "none",
  },
  card: {
    border: "1px solid #ddd",
    borderRadius: 14,
    padding: 14,
    marginBottom: 12,
    background: "#fff",
  },
  badge: {
    padding: "4px 8px",
    borderRadius: 999,
    border: "1px solid #111",
    fontSize: 12,
    fontWeight: 700,
  },
  badgeMuted: {
    padding: "4px 8px",
    borderRadius: 999,
    border: "1px solid #ccc",
    fontSize: 12,
    fontWeight: 600,
    color: "#444",
  },
  miniStat: {
    border: "1px solid #eee",
    borderRadius: 12,
    padding: "8px 10px",
    minWidth: 160,
    background: "#fafafa",
  },
  alert: {
    marginTop: 12,
    padding: 12,
    borderRadius: 10,
    border: "1px solid",
    whiteSpace: "pre-wrap",
  },
    btnGenerate: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #3498db",
    background: "#3498db", // albastru deschis
    color: "#fff",
    cursor: "pointer",
    transition: "background 0.2s ease",
  },
  btn: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #bbb",
    background: "#fff",
    cursor: "pointer",
  },
  btnAdd: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #111",
    background: "#04AA6D",
    color: "#fff",
    cursor: "pointer",
  },
    btnEdit: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #4d3a06",
    background: "#f59e0b",
    color: "#fff",
    cursor: "pointer",
  },
  btnPrimary: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #111",
    background: "#4e4e4e",
    color: "#fff",
    cursor: "pointer",
  },
  btnDanger: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #e74c3c",
    background: "#fdecea",
    cursor: "pointer",
  }
};
