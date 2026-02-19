import React, { useEffect, useMemo, useState } from "react";
import { baseUrl } from "../config/api";
import ProiectFotovoltaice from "./ProiectFotovoltaice.js";
import Modal from "../components/Modal.js";
import GenerateFotovoltaice from "./GenerateFotovoltaice.js"
import { Link } from "react-router-dom";

export default function ProiecteFotovoltaiceList() {

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");
  const [q, setQ] = useState("");
  const [expanded, setExpanded] = useState(() => new Set());
  const [editing, setEditing] = useState(null);
  const [isEditOpen, setIsEditOpen]=useState(false);
  const [generating, setGenerating]=useState(null);
  const [isGenerateOpen, setIsGenerateOpen]=useState(false);
  const [sortBy, setSortBy] = useState("data"); // data, nrProiect, client
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
  }, [items, q,sortBy, sortDir]);

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


  return (
    <div style={{ maxWidth: 1200, margin: "0 auto", padding: 16 }}>
      <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 12, flexWrap: "wrap" }}>
        <h2 style={{ margin: 0 }}>Proiecte Fotovoltaice</h2>
        <Link to="/proiectfotovoltaice">
        <button style={styles.btnAdd}>Adaugă Proiect</button>
      </Link>
        <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
          <input
            value={q}
            onChange={(e) => setQ(e.target.value)}
            placeholder="Caută după nrProiect, județ, localitate, POD, tip..."
            style={styles.input}
          />
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

      <div style={{ marginTop: 12 }}>
        {filtered.map((p) => {
          const id = p.id || p.ID; // în funcție de serializer (camelCase / PascalCase)
          const isOpen = expanded.has(id);


          return (
            <div key={id} style={styles.card}>
              <div style={{ display: "flex", gap: 12, alignItems: "flex-start", justifyContent: "space-between" }}>
                <div style={{ flex: 1, minWidth: 260 }}>
                  <div style={{ display: "flex", gap: 10, alignItems: "baseline", flexWrap: "wrap" }}>
                    <div style={{ fontSize: 18, fontWeight: 800 }}>
                      Nr Proiect: {p.nrProiect }
                    </div>
                    <div style={{ fontSize: 18, fontWeight: 800 }}>
                      | Nr PV: {p.nrPV } 
                    </div>
                    <span style={styles.badge}>{p.tipSistem ?? "-"}</span>
                    {p.pod  ? <span style={styles.badgeMuted}>POD: {p.pod }</span> : null}
                  </div>

                  <div style={{ marginTop: 6, color: "#444" }}>
                    <b>Data:</b> {formatDate(p.data )}{" "}
                    <span style={{ color: "#888" }}>•</span>{" "}
                    <b>Client:</b> {p.client.nume ??"-"}
                    {p.firmaID  ? (
                      <>
                        {" "}
                        <span style={{ color: "#888" }}>•</span> <b>Firma:</b> {p.firma.nume}
                      </>
                    ) : null}
                  </div>

                  <div style={{ marginTop: 6, color: "#555" }}>
                    <b>Adresă:</b>{" "}
                    {[p.judet ?? p.Judet, p.localitatea , p.comuna , p.strada , p.nrStrada ]
                      .filter(Boolean)
                      .join(", ") || "-"}
                  </div>

                  <div style={{ marginTop: 10, display: "flex", gap: 10, flexWrap: "wrap" }}>
                    <MiniStat label="Putere panouri" value={`${p.putereTotalaPanouri.toFixed(3)/1000} kW`} />
                    <MiniStat label="Putere invertor" value={`${p.putereTotalaInvertor.toFixed(2)} kW`} />
                    <MiniStat label="Capacitate baterii" value={`${p.capacitateTotalaBaterii.toFixed(2)} kWh`} />
                    <MiniStat label="PRM măsurat" value={(p.prM_Masurat  ?? "-").toString()} />
                    <MiniStat label="PRM calculat" value={(p.prM_Calculat  ?? "-").toString()} />
                  </div>
                </div>

                <div style={{ display: "flex", gap: 8 }}>
                  <button onClick={() => toggle(id)} style={styles.btnPrimary}>
                    {isOpen ? "Ascunde" : "Detalii"}
                  </button>

                <button onClick={() => onDelete(id)} style={styles.btnDanger}>
                  Șterge
                </button>

                <button onClick={() => startEdit(p)} style={styles.btnEdit}>
                  Edit
                </button>

                <button onClick={() => startGenerate(p)} style={styles.btnGenerate}>
                  Genereaza
                </button>

                </div>
              </div>

              {isOpen && (
                <div style={{ marginTop: 12 }}>
                  <DetailsBlock title="Invertoare">
                    <SimpleTable
                      headers={["Nume", "Nr", "Putere instalată (kW)","Serie"]}
                      rows={(p.dateInvertoare ?? []).map((x) => [
                        x.model ?? "-",
                        x.nrInvertoare ?? "-",
                        x.putereInstalata  ?? "-",
                        x.serie ?? "-"
                      ])}
                      emptyText="Nu există invertoare."
                    />
                  </DetailsBlock>

                  <DetailsBlock title="Panouri">
                    <SimpleTable
                      headers={["Nume","Nr panouri", "Putere panou (W)", "Serii", "Total (kW)"]}
                      rows={(p.datePanouri ?? []).map((x) => {
                        const nr = Number(x.nrPanouri  ?? 0);
                        const pw = Number(x.putereInstalata ?? 0);
                        return [x.model ?? "-",nr, pw,x.serie ?? "-", ((nr * pw)/1000).toFixed(3)];
                      })}
                      emptyText="Nu există panouri."
                    />
                  </DetailsBlock>

                  <DetailsBlock title="Baterii">
                    <SimpleTable
                      headers={["Nume","Nr baterii", "Putere baterie (kW)", "Capacitate (Ah)","Serie"]}
                      rows={(p.dateBaterii ?? []).map((x) => {
                        const nr = Number(x.nrBaterii ?? x.NrBaterii ?? 0);
                        const pw = Number(x.putereInstalata ?? x.PutereInstalata ?? 0);
                        const cap = Number(x.capacitate ?? x.Capacitate ?? 0);
                        return [x.model ?? "-",nr, pw, cap,x.serie ?? "-"];
                      })}
                      emptyText="Nu există baterii"
                    />
                  </DetailsBlock>
                  <DetailsBlock title="Accesorii">
                    <SimpleTable
                      headers={["Sistem Backup","Smart Meter"]}
                      rows={p.backup || p.smartMeter ? [[p.backup || "-", p.smartMeter || "-"]]: []}
                      emptyText="Nu există"
                    />
                  </DetailsBlock>
                </div>
              )}
            </div>
          );
        })}
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

    <Modal open={isGenerateOpen} onClose={() => setIsGenerateOpen(false)} width={1300}>
        <GenerateFotovoltaice proiect={generating}/>
    </Modal>

    </div>
  );
}

function DetailsBlock({ title, children }) {
  return (
    <div style={{ borderTop: "1px solid #eee", paddingTop: 10, marginTop: 10 }}>
      <div style={{ fontSize: 14, fontWeight: 800, marginBottom: 6 }}>{title}</div>
      {children}
    </div>
  );
}

function SimpleTable({ headers, rows, emptyText }) {
  if (!rows || rows.length === 0) return <div style={{ color: "#666" }}>{emptyText}</div>;

  return (
    <div style={{ overflowX: "auto" }}>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            {headers.map((h) => (
              <th key={h} style={styles.th}>
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

function MiniStat({ label, value }) {
  return (
    <div style={styles.miniStat}>
      <div style={{ fontSize: 11, color: "#666" }}>{label}</div>
      <div style={{ fontSize: 14, fontWeight: 800 }}>{value}</div>
    </div>
  );
}

const styles = {
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
  },
  th: { textAlign: "left", padding: 10, fontSize: 12, color: "#666", borderBottom: "1px solid #eee" },
  td: { padding: 10, fontSize: 13 },
};
