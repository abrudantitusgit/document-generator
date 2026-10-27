import React,{ useMemo, useState, useEffect } from "react";
import { createPortal } from "react-dom";
import { baseUrl } from "../config/api";


/**
 * Pagina: Adaugare Proiect Fotovoltaice
 * - Trimite JSON compatibil cu modelul ProiectFotovoltaice (camelCase)
 * - Endpoint default: /api/ProiecteFotovoltaice
 */
export default function ProiectFotovoltaice(props) {
  const [clienti, setClienti] = useState([]);
  const [loadingClienti, setLoadingClienti] = useState(false);
  const id = props.id;
  const isEdit = Boolean(id);
  const [sugestiiPanouri, setSugestiiPanouri] = useState([]);
  const [sugestiiInvertoare, setSugestiiInvertoare] = useState([]);
  const [sugestiiBaterii, setSugestiiBaterii] = useState([]);
  const [sugestiiSmartMeter, setSugestiiSmartMeter] = useState([]);
  const [sugestiiBackup, setSugestiiBackup] = useState([]);

  useEffect(() => {
    fetch(`${baseUrl}/proiectfotovoltaice/sugestii/panouri`)
      .then(r => r.json())
      .then(setSugestiiPanouri);

    fetch(`${baseUrl}/proiectfotovoltaice/sugestii/invertoare`)
      .then(r => r.json())
      .then(setSugestiiInvertoare);

    fetch(`${baseUrl}/proiectfotovoltaice/sugestii/baterii`)
      .then(r => r.json())
      .then(setSugestiiBaterii);
    fetch(`${baseUrl}/proiectfotovoltaice/sugestii/smartmeter`)
      .then(r => r.json())
      .then(setSugestiiSmartMeter);
    fetch(`${baseUrl}/proiectfotovoltaice/sugestii/backup`)
      .then(r => r.json())
      .then(setSugestiiBackup);
  }, []);

  useEffect(() => {
    if (isEdit) return;

    const loadNext = async () => {
      const res = await fetch(`${baseUrl}/proiectfotovoltaice/next-numbers`);
      const data = await res.json();

      setField("nrProiect", String(data.nrProiect));
      setField("nrPV", String(data.nrPV));
    };

    loadNext();
  }, [isEdit]);

  const [form, setForm] = useState({
    clientID: "",
    nrProiect: "",
    nrPV:"",
    data: new Date().toISOString().slice(0, 16),
    firmaID: "",
    judet: "",
    sector: "",
    localitatea: "",
    comuna: "",
    strada: "",
    nrStrada: "",
    bloc: "",
    scara: "",
    etaj: "",
    apartament: "",
    nrCadastral: "",
    nrCF: "",
    amplasare: "",

    // PRM
    coeficientPRM: "1.1", // ✅ select (1.1, 1.3, 1.5)
    prm_Masurat: "",
    prm_Calculat: "", // ✅ auto = prm_Masurat * coeficient

    tipSistem: "Monofazat",
    pod: "",
    backup:"",
    smartMeter:"",
    dateInvertoare: [{ nume: "", nrInvertoare: 0, putereInstalata: 0, serie: "" }],
    datePanouri: [{ nume: "", nrPanouri: 0, putereInstalata: 0, serie: "" }],
    dateBaterii: [],
  });

  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState({ ok: false, message: "" });

  const setField = (name, value) => setForm((p) => ({ ...p, [name]: value }));

  useEffect(() => {
  if (!isEdit) return;

  const loadProiect = async () => {
    try {
      const res = await fetch(`${baseUrl}/proiectfotovoltaice/${id}`);
      if (!res.ok) throw new Error(await res.text());
      const p = await res.json();
      console.log(p);
      // 🔁 map API → form
      setForm({
        id: p.id,
        clientID: p.clientID ?? "",
        nrProiect: p.nrProiect ?? "",
        nrPV: p.nrPV ?? "",
        data: p.data ? p.data.slice(0, 16) : "",
        firmaID: p.firmaID ?? "",
        judet: p.judet ?? "",
        sector: p.sector ?? "",
        localitatea: p.localitatea ?? "",
        comuna: p.comuna ?? "",
        strada: p.strada ?? "",
        nrStrada: p.nrStrada ?? "",
        bloc: p.bloc ?? "",
        scara: p.scara ?? "",
        etaj: p.etaj ?? "",
        apartament: p.apartament ?? "",
        nrCadastral: p.nrCadastral ?? "",
        nrCF: p.nrCF ?? "",
        amplasare: p.amplasare ?? "",
        backup: p.backup ?? "",
        smartMeter: p.smartMeter ?? "",
        coeficientPRM: String(p.prm_Coeficient ?? "1.1"),
        prm_Masurat: p.prM_Masurat?.toString() ?? "",
        prm_Calculat: p.prM_Calculat?.toString() ?? "",

        tipSistem: p.tipSistem ?? "Monofazat",
        pod: p.pod ?? "",

        dateInvertoare: p.dateInvertoare?.map(i => ({
          nume: i.model,
          nrInvertoare: i.nrInvertoare,
          putereInstalata: i.putereInstalata,
          serie: i.serie ?? ""
        })) ?? [],

        datePanouri: p.datePanouri?.map(x => ({
          nume: x.model,
          nrPanouri: x.nrPanouri,
          putereInstalata: x.putereInstalata,
          serie: x.serie ?? ""
        })) ?? [],

        dateBaterii: p.dateBaterii?.map(b => ({
          nume: b.model,
          nrBaterii: b.nrBaterii,
          putereInstalata: b.putereInstalata,
          capacitate: b.capacitate,
          serie: b.serie ?? ""
        })) ?? [],
      });
    } catch (e) {
      alert("Eroare la încărcare proiect");
    }
  };

  loadProiect();
}, [id, isEdit]);


  useEffect(() => {
    const loadClienti = async () => {
      setLoadingClienti(true);
      try {
        const res = await fetch(`${baseUrl}/client/clientifirme`);
        const data = await res.json().catch(() => []);
        setClienti(res.ok && Array.isArray(data) ? data : []);
      } catch (e) {
        console.error("Eroare la încărcare clienți", e);
      } finally {
        setLoadingClienti(false);
      }
    };

    loadClienti();
  }, []);

  const clientSelectat = useMemo(() => {
    return clienti.find((c) => c.id === form.clientID) || null;
  }, [clienti, form.clientID]);

  const firmeDisponibile = useMemo(() => {
    return clientSelectat?.firme ?? [];
  }, [clientSelectat]);

  // când schimbi clientul, resetezi firma
  useEffect(() => {
    if (!isEdit) {
      setField("firmaID", "");
    }
  }, [form.clientID, isEdit]);

  // ✅ PRM_Calculat = PRM_Masurat * coeficient
  useEffect(() => {
    const masurat = Number(form.prm_Masurat);
    const coef = Number(form.coeficientPRM);

    if (form.prm_Masurat === "" || !Number.isFinite(masurat)) {
      setField("prm_Calculat", "");
      return;
    }

    const calculat = masurat * coef;
    const rounded = Math.round(calculat * 100) / 100; // 2 zecimale
    setField("prm_Calculat", String(rounded));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.prm_Masurat, form.coeficientPRM]);

  const updateListItem = (listName, index, key, value) => {
    setForm((p) => {
      const next = structuredClone(p);
      next[listName][index][key] = value;
      return next;
    });
  };

  const addListItem = (listName, item) => {
    setForm((p) => ({ ...p, [listName]: [...p[listName], item] }));
  };

  const removeListItem = (listName, index) => {
    
    setForm((p) => {
      const next = structuredClone(p);
      next[listName].splice(index, 1);

      if (next[listName].length === 0) {
        if (listName === "dateInvertoare") next[listName].push({ nume: "", nrInvertoare: 0, putereInstalata: 0, serie: "" });
        if (listName === "datePanouri") next[listName].push({ nume: "", nrPanouri: 0, putereInstalata: 0, serie: "" });
      
      }
      return next;
    });
  };

  const validate = () => {
    const errors = [];
    if (!form.clientID) errors.push("Nu ai selectat client");
    if (!form.nrProiect) errors.push("Numar proiect este obligatoriu.");
    if (!form.nrPV) errors.push("Numar PV este obligatoriu.");
    const guidRe = /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/;
    if (form.clientID && !guidRe.test(form.clientID)) errors.push("clientID nu pare un GUID valid.");
    if (form.firmaID && !guidRe.test(form.firmaID)) errors.push("firmaID nu pare un GUID valid.");

    return errors;
  };

  const buildPayload = () => {
    const payload = {
      id: form.id,
      clientID: form.clientID.trim(),
      nrProiect: Number(form.nrProiect),
      nrPV: Number(form.nrPV),
      data: form.data ? new Date(form.data).toISOString() : null,
      firmaID: form.firmaID ? form.firmaID.trim() : null,

      judet: form.judet,
      sector: form.sector,
      localitatea: form.localitatea,
      comuna: form.comuna,
      strada: form.strada,
      nrStrada: form.nrStrada,
      bloc: form.bloc,
      scara: form.scara,
      etaj: form.etaj,
      apartament: form.apartament,
      nrCadastral: form.nrCadastral,
      nrCF: form.nrCF,
      amplasare: form.amplasare,

      prm_Masurat: form.prm_Masurat === "" ? 0 : Number(form.prm_Masurat),
      prm_Calculat: form.prm_Calculat === "" ? 0 : Number(form.prm_Calculat),
      prm_Coeficient: Number(form.coeficientPRM), // ✅ dacă vrei să-l trimiți și pe acesta
      tipSistem: form.tipSistem,
      pod: form.pod,
      backup: form.backup,
      smartMeter: form.smartMeter,
      dateInvertoare: (form.dateInvertoare || []).map((i) => ({
        model: i.nume,
        nrInvertoare: Number(i.nrInvertoare || 0),
        putereInstalata: Number(i.putereInstalata || 0),
        serie: i.serie,
      })),

      datePanouri: (form.datePanouri || []).map((p) => ({
        model: p.nume,
        nrPanouri: Number(p.nrPanouri || 0),
        putereInstalata: Number(p.putereInstalata || 0),
        serie: p.serie,
      })),

      dateBaterii: (form.dateBaterii || []).map((b) => ({
        model: b.nume,
        nrBaterii: Number(b.nrBaterii || 0),
        putereInstalata: Number(b.putereInstalata || 0),
        capacitate: Number(b.capacitate || 0), // ✅ FIX (era greșit)
        serie: b.serie,
      })),
    };
console.log(payload)
    if (!payload.data) delete payload.data;
    return payload;
  };

    

  const onSubmit = async (e) => {
    
  e.preventDefault();
  setResult({ ok: false, message: "" });

  const errors = validate();
  if (errors.length) {
    setResult({ ok: false, message: errors.join(" ") });
    return;
  }

  const payload = buildPayload();
  setSubmitting(true);

  try {
    const resp = await fetch(
      isEdit
        ? `${baseUrl}/proiectfotovoltaice/${id}`
        : `${baseUrl}/proiectfotovoltaice`,
      {
        method: isEdit ? "PUT" : "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      }
    );
    
    if (!resp.ok) throw new Error(await resp.text());

    setResult({
      ok: true,
      message: isEdit
        ? "Proiect actualizat cu succes!"
        : "Proiect creat cu succes!",
    });
    const updated = await resp.json();
    if (props.onSuccess) {
      props.onSuccess(updated);
    }
 
  } catch (err) {
    setResult({ ok: false, message: err.message });
  } finally {
    setSubmitting(false);
  }
};

  return (
    <div style={{ maxWidth: 1100, margin: "0 auto", padding: 16 }}>
      <h2>
        {isEdit ? "Editare Proiect Fotovoltaic" : "Adăugare Proiect Fotovoltaic"}
      </h2>

      {result.message && (
        <div
          style={{
            position: "sticky",
            top: 0,
            zIndex: 1000,
            padding: 12,
            borderRadius: 8,
            marginBottom: 12,
            background: result.ok ? "#e9f7ef" : "#fdecea",
            border: `1px solid ${result.ok ? "#2ecc71" : "#e74c3c"}`,
            color: "#111",
            whiteSpace: "pre-wrap",
          }}
        >
          {result.message}
        </div>
      )}

      <form onSubmit={onSubmit}>
        <Section title="Date generale">
          <Grid>
            {/* CLIENT */}
            <label style={styles.label}>
              <span style={styles.labelText}>Client *</span>
              <select
                value={form.clientID}
                onChange={(e) => setField("clientID", e.target.value)}
                style={styles.input}
                disabled={loadingClienti}
              >
                <option value="">{loadingClienti ? "Se încarcă..." : "-- selectează client --"}</option>

                {clienti.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nume} {c.cnp ? `(${c.cnp})` : ""}
                  </option>
                ))}
              </select>
            </label>

            <Input label="Nr Proiect*" value={form.nrProiect} onChange={(v) => setField("nrProiect", v)} type="number" />
            <Input label="Nr PV*" value={form.nrPV} onChange={(v) => setField("nrPV", v)} type="number" />
            <Input label="Data" value={form.data} onChange={(v) => setField("data", v)} type="datetime-local" />

            {/* FIRMA */}
            <label style={styles.label}>
              <span style={styles.labelText}>Firmă</span>
              <select
                value={form.firmaID}
                onChange={(e) => setField("firmaID", e.target.value)}
                style={styles.input}
                disabled={!form.clientID || firmeDisponibile.length === 0}
              >
                <option value="">
                  {!form.clientID
                    ? "-- alege întâi clientul --"
                    : firmeDisponibile.length === 0
                    ? "-- clientul nu are firme --"
                    : "-- selectează firmă --"}
                </option>

                {firmeDisponibile.map((f) => (
                  <option key={f.id} value={f.id}>
                    {f.denumire ?? f.nume ?? f.id}
                  </option>
                ))}
              </select>
            </label>

            <Select
              label="Tip sistem"
              value={form.tipSistem}
              onChange={(v) => setField("tipSistem", v)}
              options={["Monofazat", "Trifazat"]}
            />
            <Input label="POD" value={form.pod} onChange={(v) => setField("pod", v)} placeholder="ex: 59404020000..." />
            <Input label="Amplasare" value={form.amplasare} onChange={(v) => setField("amplasare", v)} placeholder="ex: sol, casa" />
          </Grid>
        </Section>

        <Section title="Adresă">
          <Grid>
            <Input label="Județ" value={form.judet} onChange={(v) => setField("judet", v)} />
            <Input label="Sector" value={form.sector} onChange={(v) => setField("sector", v)} />
            <Input label="Localitatea" value={form.localitatea} onChange={(v) => setField("localitatea", v)} />
            <Input label="Comuna" value={form.comuna} onChange={(v) => setField("comuna", v)} />
            <Input label="Strada" value={form.strada} onChange={(v) => setField("strada", v)} />
            <Input label="Nr Stradă" value={form.nrStrada} onChange={(v) => setField("nrStrada", v)} />
            <Input label="Bloc" value={form.bloc} onChange={(v) => setField("bloc", v)} />
            <Input label="Scara" value={form.scara} onChange={(v) => setField("scara", v)} />
            <Input label="Etaj" value={form.etaj} onChange={(v) => setField("etaj", v)} />
            <Input label="Apartament" value={form.apartament} onChange={(v) => setField("apartament", v)} />
            <Input label="Nr Cadastral" value={form.nrCadastral} onChange={(v) => setField("nrCadastral", v)} />
            <Input label="Nr CF" value={form.nrCF} onChange={(v) => setField("nrCF", v)} />
          </Grid>
        </Section>

        {/* ✅ PRM: masurat input, coeficient select, calculat auto */}
        <Section title="PRM">
          <Grid>
            <Input
              label="PRM Măsurat"
              value={form.prm_Masurat}
              onChange={(v) => setField("prm_Masurat", v)}
              type="number"
              step="0.01"
            />

          <Select
            label="Coeficient PRM"
            value={form.coeficientPRM}
            onChange={(v) => setField("coeficientPRM", v)}
            options={[
              { value: "1.1", label: "1.1 — uscat" },
              { value: "1.3", label: "1.3 — umed" },
              { value: "1.5", label: "1.5 — foarte umed" },
            ]}
          />

            <label style={styles.label}>
              <span style={styles.labelText}>PRM Calculat (auto)</span>
              <input value={form.prm_Calculat} readOnly style={{ ...styles.input, background: "#f5f5f5" }} />
            </label>
          </Grid>
        </Section>

        <Section title="Invertoare">
          <ListTable
            headers={["Nume", "Nr invertoare", "Putere invertor", "Serie"]}
            rows={form.dateInvertoare}
            renderRow={(row, idx) => (
              <>
                <td>
                  <AutoCompleteInput
                    value={row.nume}
                    suggestions={sugestiiInvertoare}
                    placeholder="ex: Huawei SUN2000-10KTL-M1"
                    onChange={(v) =>
                      updateListItem("dateInvertoare", idx, "nume", v)
                    }
                    onSelectSuggestion={(s)=>{
                      updateListItem("dateInvertoare",idx,"putereInstalata",String(s.putereInstalata))
                    }}
                  />
                </td>
                <td>
                  <input
                    value={row.nrInvertoare}
                    onChange={(e) => updateListItem("dateInvertoare", idx, "nrInvertoare", e.target.value)}
                    type="number"
                    min="0"
                    style={styles.input}
                  />
                </td>
                <td>
                  <input
                    value={row.putereInstalata}
                    onChange={(e) => updateListItem("dateInvertoare", idx, "putereInstalata", e.target.value)}
                    type="number"
                    step="0.01"
                    min="0"
                    style={styles.input}
                  />
                </td>
                <td>
                  <input value={row.serie} onChange={(e) => updateListItem("dateInvertoare", idx, "serie", e.target.value)} style={styles.input} />
                </td>
                <td style={{ textAlign: "right" }}>
                  <button type="button" onClick={() => removeListItem("dateInvertoare", idx)} style={styles.btnDanger}>
                    Șterge
                  </button>
                </td>
              </>
            )}
          />
          <div style={{ marginTop: 8 }}>
            <button
              type="button"
              onClick={() => addListItem("dateInvertoare", { nume: "", nrInvertoare: 0, putereInstalata: 0, serie: "" })}
              style={styles.btn}
            >
              + Adaugă model invertor
            </button>
          </div>
        </Section>

        <Section title="Panouri">
          <ListTable
            headers={["Nume", "Numar panouri", "Putere panou (W)", "Serii"]}
            rows={form.datePanouri}
            renderRow={(row, idx) => (
              <>
                <td>
                     <AutoCompleteInput
                    value={row.nume}
                    suggestions={sugestiiPanouri}
                    placeholder="ex: LR5-54HTH-455M LONGI"
                    onChange={(v) =>
                      updateListItem("datePanouri", idx, "nume", v)
                    }
                    onSelectSuggestion={(s)=>{
                      updateListItem("datePanouri",idx,"putereInstalata",String(s.putereInstalata))
                    }}
                  />

                </td>
                <td>
                  <input
                    value={row.nrPanouri}
                    onChange={(e) => updateListItem("datePanouri", idx, "nrPanouri", e.target.value)}
                    type="number"
                    min="0"
                    style={styles.input}
                  />
                </td>
                <td>
                  <input
                    value={row.putereInstalata}
                    onChange={(e) => updateListItem("datePanouri", idx, "putereInstalata", e.target.value)}
                    type="number"
                    step="0.001"
                    min="0"
                    style={styles.input}
                    placeholder="ex: 0.55"
                  />
                </td>
                <td style={{ minWidth: 400 }}>
                  <input value={row.serie} onChange={(e) => updateListItem("datePanouri", idx, "serie", e.target.value)} style={styles.input} />
                </td>
                <td style={{ textAlign: "right" }}>
                  <button type="button" onClick={() => removeListItem("datePanouri", idx)} style={styles.btnDanger}>
                    Șterge
                  </button>
                </td>
              </>
            )}
          />
          <div style={{ marginTop: 8 }}>
            <button
              type="button"
              onClick={() => addListItem("datePanouri", { nume: "", nrPanouri: 0, putereInstalata: 0, serie: "" })}
              style={styles.btn}
            >
              + Adaugă model panou
            </button>
          </div>
        </Section>

        <Section title="Baterii">
          <ListTable
            headers={["Nume", "Nr baterii", "Putere baterie (kW)", "Capacitate baterie (Ah)", "Serie"]}
            rows={form.dateBaterii}
            renderRow={(row, idx) => (
              <>
                <td>
                    <AutoCompleteInput
                    value={row.nume}
                    suggestions={sugestiiBaterii}
                    placeholder="ex: Huawei LUNA2000-5-S0"
                    onChange={(v) =>
                      updateListItem("dateBaterii", idx, "nume", v)
                    }
                    onSelectSuggestion={(s)=>{
                      updateListItem("dateBaterii",idx,"putereInstalata",String(s.putereInstalata))
                      updateListItem("dateBaterii",idx,"capacitate",String(s.capacitate))
                    }}
                  />
                </td>
                <td>
                  <input
                    value={row.nrBaterii}
                    onChange={(e) => updateListItem("dateBaterii", idx, "nrBaterii", e.target.value)}
                    type="number"
                    min="0"
                    style={styles.input}
                  />
                </td>
                <td>
                  <input
                    value={row.putereInstalata}
                    onChange={(e) => updateListItem("dateBaterii", idx, "putereInstalata", e.target.value)}
                    type="number"
                    step="0.01"
                    min="0"
                    style={styles.input}
                    placeholder="ex: 5.12"
                  />
                </td>
                <td>
                  <input
                    value={row.capacitate}
                    onChange={(e) => updateListItem("dateBaterii", idx, "capacitate", e.target.value)}
                    type="number"
                    step="0.01"
                    min="0"
                    style={styles.input}
                    placeholder="ex: 100"
                  />
                </td>
                <td>
                  <input value={row.serie} onChange={(e) => updateListItem("dateBaterii", idx, "serie", e.target.value)} style={styles.input} />
                </td>
                <td style={{ textAlign: "right" }}>
                  <button type="button" onClick={() => removeListItem("dateBaterii", idx)} style={styles.btnDanger}>
                    Șterge
                  </button>
                </td>
              </>
            )}
          />
          <div style={{ marginTop: 8 }}>
            <button
              type="button"
              onClick={() => addListItem("dateBaterii", { nume: "", nrBaterii: 0, putereInstalata: 0, capacitate: 0, serie: "" })}
              style={styles.btn}
            >
              + Adaugă model baterie
            </button>
          </div>
        </Section>

        <Section title="Accesorii">
          <Grid>
               <div>
              <label style={styles.label}>Sistem Backup</label>
              <AutoCompleteInput
                value={form.backup}
                suggestions={sugestiiBackup}
                placeholder="ex: Huawei SmartGuard-63A-T0"
                onChange={(v) => setField("backup", v)}
                onSelectSuggestion={(s) =>
                  setField("backup", typeof s === "string" ? s : s.model)
                }
              />
            </div>
            <div>
              <label style={styles.label}>Smart Meter</label>
              <AutoCompleteInput
                value={form.smartMeter}
                suggestions={sugestiiSmartMeter}
                placeholder="ex: Huawei Smart Meter DTSU666-H"
                onChange={(v) => setField("smartMeter", v)}
                onSelectSuggestion={(s) =>
                  setField("smartMeter", typeof s === "string" ? s : s.model)
                }
              />
            </div>
          </Grid>

        </Section>

        <div style={{ display: "flex", gap: 12, marginTop: 16 }}>
          <button type="submit" disabled={submitting} style={styles.btnPrimary}>
            {submitting
              ? "Se salvează..."
              : isEdit
              ? "Salvează modificările"
              : "Salvează proiect"}
          </button>

          <button
            type="button"
            onClick={() => {
              setForm((p) => ({
                ...p,
                dateInvertoare: [{ nume: "", nrInvertoare: 0, putereInstalata: 0, serie: "" }],
                datePanouri: [{ nume: "", nrPanouri: 0, putereInstalata: 0, serie: "" }],
                dateBaterii: [],
              }));
              setResult({ ok: false, message: "" });
            }}
            style={styles.btn}
          >
            Reset liste
          </button>
        </div>
      </form>
    </div>
  );
}

function Section({ title, children }) {
  return (
    <div style={{ border: "1px solid #ddd", borderRadius: 12, padding: 12, marginBottom: 12 }}>
      <h3 style={{ margin: "4px 0 12px" }}>{title}</h3>
      {children}
    </div>
  );
}

function Grid({ children }) {
  return <div style={{ display: "grid", gridTemplateColumns: "repeat(3, minmax(240px, 1fr))", gap: 12 }}>{children}</div>;
}

function Input({ label, value, onChange, type = "text", step, placeholder }) {
  return (
    <label style={styles.label}>
      <span style={styles.labelText}>{label}</span>
      <input value={value} type={type} step={step} placeholder={placeholder} onChange={(e) => onChange(e.target.value)} style={styles.input} />
    </label>
  );
}

function Select({ label, value, onChange, options }) {
  return (
    <label style={styles.label}>
      <span style={styles.labelText}>{label}</span>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        style={styles.input}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label ?? o}
          </option>
        ))}
      </select>
    </label>
  );
}

function ListTable({ headers, rows, renderRow }) {
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
          {rows.map((row, idx) => (
            <tr key={idx} style={{ borderTop: "1px solid #eee" }}>
              {renderRow(row, idx)}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

const styles = {
  label: { display: "flex", flexDirection: "column", gap: 6 },
  labelText: { fontSize: 12, color: "#444" },
  input: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #ccc",
    outline: "none",
    width: "100%",
    boxSizing: "border-box",
  },
  th: { textAlign: "left", padding: 10, fontSize: 12, color: "#666", borderBottom: "1px solid #eee" },
  btn: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #bbb",
    background: "#fff",
    cursor: "pointer",
  },
  btnPrimary: {
    padding: "10px 12px",
    borderRadius: 10,
    border: "1px solid #111",
    background: "#111",
    color: "#fff",
    cursor: "pointer",
  },
  btnDanger: {
    padding: "8px 10px",
    borderRadius: 10,
    border: "1px solid #e74c3c",
    background: "#fdecea",
    color: "#111",
    cursor: "pointer",
  }

};

function AutoCompleteInput({
  value,
  onChange,
  suggestions = [],
  onSelectSuggestion,
  labelKey = "model",
  placeholder,
}) {
  const inputRef = React.useRef(null);
  const [open, setOpen] = React.useState(false);
  const [rect, setRect] = React.useState(null);

  const getLabel = (item) => {
    if (typeof item === "string") return item;
    return item?.[labelKey] ?? "";
  };

  const filtered = React.useMemo(() => {
    if (!value) return [];
    return suggestions
      .filter((s) => getLabel(s).toLowerCase().includes(value.toLowerCase()))
      .slice(0, 8);
  }, [value, suggestions, labelKey]);

  const updatePosition = React.useCallback(() => {
    if (!inputRef.current) return;
    const r = inputRef.current.getBoundingClientRect();
    setRect({
      top: r.bottom + 4,
      left: r.left,
      width: r.width,
    });
  }, []);

  const openList = () => {
    updatePosition();
    setOpen(true);
  };

  React.useEffect(() => {
    if (!open) return;

    const handleUpdate = () => updatePosition();

    window.addEventListener("scroll", handleUpdate, true);
    window.addEventListener("resize", handleUpdate);

    return () => {
      window.removeEventListener("scroll", handleUpdate, true);
      window.removeEventListener("resize", handleUpdate);
    };
  }, [open, updatePosition]);

  return (
    <>
      <input
        ref={inputRef}
        value={value}
        placeholder={placeholder}
        onFocus={openList}
        onChange={(e) => {
          onChange(e.target.value);
          openList();
        }}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
        style={styles.input}
      />

      {open &&
        rect &&
        filtered.length > 0 &&
        createPortal(
          <div
            style={{
              position: "fixed",
              top: rect.top,
              left: rect.left,
              width: rect.width,
              zIndex: 99999,
              background: "#fff",
              borderRadius: 10,
              boxShadow: "0 8px 24px rgba(0,0,0,0.15)",
              maxHeight: 220,
              overflowY: "auto",
            }}
          >
            {filtered.map((s, index) => {
              const label = getLabel(s);

              return (
                <div
                  key={typeof s === "string" ? `${label}-${index}` : s.id ?? label}
                  onMouseDown={() => {
                    onChange(label);
                    onSelectSuggestion?.(s);
                    setOpen(false);
                  }}
                  style={{
                    padding: "8px 12px",
                    cursor: "pointer",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = "#f5f5f5";
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = "transparent";
                  }}
                >
                  {label}
                </div>
              );
            })}
          </div>,
          document.body
        )}
    </>
  );
}
