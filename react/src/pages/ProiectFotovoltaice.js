import React, { useEffect, useMemo, useState } from "react";
import { FiPlus, FiTrash2 } from "react-icons/fi";
import { apiList, apiRequest } from "../lib/api";
import { formatNumber } from "../lib/format";
import { Combobox, Field, FormGrid, FormSection, Notice, SelectField, TextField } from "../components/ui";

const emptyInverter = () => ({ nume: "", nrInvertoare: 0, putereInstalata: 0, serie: "" });
const emptyPanel = () => ({ nume: "", nrPanouri: 0, putereInstalata: 0, serie: "" });
const emptyBattery = () => ({ nume: "", nrBaterii: 0, putereInstalata: 0, capacitate: 0, serie: "" });

const initialForm = () => ({
  clientID: "", nrProiect: "", nrPV: "", data: new Date().toISOString().slice(0, 16), firmaID: "",
  judet: "", sector: "", localitatea: "", comuna: "", strada: "", nrStrada: "", bloc: "", scara: "", etaj: "", apartament: "",
  nrCadastral: "", nrCF: "", amplasare: "",
  coeficientPRM: "1.1", prm_Masurat: "",
  tipSistem: "Monofazat", pod: "", backup: "", smartMeter: "",
  dateInvertoare: [emptyInverter()], datePanouri: [emptyPanel()], dateBaterii: [],
});

const prmCoefficients = [
  { value: "1.1", label: "1,1 — sol uscat" },
  { value: "1.3", label: "1,3 — sol umed" },
  { value: "1.5", label: "1,5 — sol foarte umed" },
];

// PRM calculat = PRM măsurat × coeficient, rounded to 2 decimals.
const computePrm = (measured, coefficient) => {
  const value = Number(measured);
  if (measured === "" || !Number.isFinite(value)) return "";
  return String(Math.round(value * Number(coefficient) * 100) / 100);
};

const guidRe = /^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$/;

const equipment = {
  dateInvertoare: {
    title: "Invertoare", addLabel: "Adaugă invertor", empty: emptyInverter, keepOne: true, suggestionKey: "invertoare",
    columns: [
      { key: "nume", label: "Model", autocomplete: true, placeholder: "ex: Huawei SUN2000-10KTL-M1" },
      { key: "nrInvertoare", label: "Nr.", type: "number", min: "0", width: 90 },
      { key: "putereInstalata", label: "Putere (kW)", type: "number", step: "0.01", min: "0", width: 130 },
      { key: "serie", label: "Serie" },
    ],
    onSuggestion: (item) => ({ putereInstalata: String(item.putereInstalata) }),
  },
  datePanouri: {
    title: "Panouri", addLabel: "Adaugă panou", empty: emptyPanel, keepOne: true, suggestionKey: "panouri",
    columns: [
      { key: "nume", label: "Model", autocomplete: true, placeholder: "ex: LR5-54HTH-455M LONGI" },
      { key: "nrPanouri", label: "Nr.", type: "number", min: "0", width: 90 },
      { key: "putereInstalata", label: "Putere panou (W)", type: "number", step: "0.001", min: "0", width: 150 },
      { key: "serie", label: "Serii", minWidth: 280 },
    ],
    onSuggestion: (item) => ({ putereInstalata: String(item.putereInstalata) }),
  },
  dateBaterii: {
    title: "Baterii", addLabel: "Adaugă baterie", empty: emptyBattery, keepOne: false, suggestionKey: "baterii",
    columns: [
      { key: "nume", label: "Model", autocomplete: true, placeholder: "ex: Huawei LUNA2000-5-S0" },
      { key: "nrBaterii", label: "Nr.", type: "number", min: "0", width: 90 },
      { key: "putereInstalata", label: "Putere (kW)", type: "number", step: "0.01", min: "0", width: 120 },
      { key: "capacitate", label: "Capacitate (Ah)", type: "number", step: "0.01", min: "0", width: 140 },
      { key: "serie", label: "Serie" },
    ],
    onSuggestion: (item) => ({ putereInstalata: String(item.putereInstalata), capacitate: String(item.capacitate) }),
  },
};

const suggestionSources = ["panouri", "invertoare", "baterii", "smartmeter", "backup"];

export default function ProiectFotovoltaice({ id, onSuccess, onCancel, onBusyChange }) {
  const isEdit = Boolean(id);
  const [form, setForm] = useState(initialForm);
  const [clients, setClients] = useState([]);
  const [loadingClients, setLoadingClients] = useState(true);
  const [loadingProject, setLoadingProject] = useState(isEdit);
  const [suggestions, setSuggestions] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [result, setResult] = useState(null);

  const setField = (name, value) => setForm((current) => ({ ...current, [name]: value }));

  useEffect(() => { onBusyChange?.(submitting); }, [submitting, onBusyChange]);

  useEffect(() => {
    let active = true;
    Promise.all(suggestionSources.map((key) => apiList(`/proiectfotovoltaice/sugestii/${key}`).then((rows) => [key, rows], () => [key, []])))
      .then((entries) => { if (active) setSuggestions(Object.fromEntries(entries)); });
    apiList("/client/clientifirme")
      .then((rows) => { if (active) setClients(rows); })
      .catch(() => { if (active) setResult({ ok: false, message: "Lista de clienți nu a putut fi încărcată." }); })
      .finally(() => { if (active) setLoadingClients(false); });
    return () => { active = false; };
  }, []);

  useEffect(() => {
    if (isEdit) return;
    apiRequest("/proiectfotovoltaice/next-numbers")
      .then((data) => setForm((current) => ({ ...current, nrProiect: String(data.nrProiect), nrPV: String(data.nrPV) })))
      .catch(() => {});
  }, [isEdit]);

  useEffect(() => {
    if (!isEdit) return;
    setLoadingProject(true);
    apiRequest(`/proiectfotovoltaice/${id}`, { fallbackError: "Proiectul nu a putut fi încărcat." })
      .then((project) => setForm({
        id: project.id,
        clientID: project.clientID ?? "",
        nrProiect: project.nrProiect ?? "",
        nrPV: project.nrPV ?? "",
        data: project.data ? project.data.slice(0, 16) : "",
        firmaID: project.firmaID ?? "",
        judet: project.judet ?? "", sector: project.sector ?? "", localitatea: project.localitatea ?? "", comuna: project.comuna ?? "",
        strada: project.strada ?? "", nrStrada: project.nrStrada ?? "", bloc: project.bloc ?? "", scara: project.scara ?? "",
        etaj: project.etaj ?? "", apartament: project.apartament ?? "",
        nrCadastral: project.nrCadastral ?? "", nrCF: project.nrCF ?? "", amplasare: project.amplasare ?? "",
        backup: project.backup ?? "", smartMeter: project.smartMeter ?? "",
        coeficientPRM: String(project.prM_Coeficient ?? "1.1"),
        prm_Masurat: project.prM_Masurat?.toString() ?? "",
        tipSistem: project.tipSistem ?? "Monofazat",
        pod: project.pod ?? "",
        dateInvertoare: project.dateInvertoare?.map((item) => ({ nume: item.model, nrInvertoare: item.nrInvertoare, putereInstalata: item.putereInstalata, serie: item.serie ?? "" })) ?? [],
        datePanouri: project.datePanouri?.map((item) => ({ nume: item.model, nrPanouri: item.nrPanouri, putereInstalata: item.putereInstalata, serie: item.serie ?? "" })) ?? [],
        dateBaterii: project.dateBaterii?.map((item) => ({ nume: item.model, nrBaterii: item.nrBaterii, putereInstalata: item.putereInstalata, capacitate: item.capacitate, serie: item.serie ?? "" })) ?? [],
      }))
      .catch((error) => setResult({ ok: false, message: error.message }))
      .finally(() => setLoadingProject(false));
  }, [id, isEdit]);

  const selectedClient = useMemo(() => clients.find((client) => client.id === form.clientID) || null, [clients, form.clientID]);
  const companies = selectedClient?.firme ?? [];
  const prmCalculated = computePrm(form.prm_Masurat, form.coeficientPRM);

  const panelKwp = form.datePanouri.reduce((sum, item) => sum + Number(item.nrPanouri || 0) * Number(item.putereInstalata || 0), 0) / 1000;
  const inverterKw = form.dateInvertoare.reduce((sum, item) => sum + Number(item.nrInvertoare || 0) * Number(item.putereInstalata || 0), 0);

  const updateItem = (listName, index, changes) => setForm((current) => ({
    ...current,
    [listName]: current[listName].map((item, itemIndex) => itemIndex === index ? { ...item, ...changes } : item),
  }));
  const addItem = (listName) => setForm((current) => ({ ...current, [listName]: [...current[listName], equipment[listName].empty()] }));
  const removeItem = (listName, index) => setForm((current) => {
    const remaining = current[listName].filter((_, itemIndex) => itemIndex !== index);
    return { ...current, [listName]: remaining.length === 0 && equipment[listName].keepOne ? [equipment[listName].empty()] : remaining };
  });

  const validate = () => {
    const errors = [];
    if (!form.clientID) errors.push("Alege clientul.");
    if (!form.nrProiect) errors.push("Numărul proiectului este obligatoriu.");
    if (!form.nrPV) errors.push("Numărul PV este obligatoriu.");
    if (form.clientID && !guidRe.test(form.clientID)) errors.push("Clientul selectat nu are un identificator valid.");
    if (form.firmaID && !guidRe.test(form.firmaID)) errors.push("Firma selectată nu are un identificator valid.");
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
      judet: form.judet, sector: form.sector, localitatea: form.localitatea, comuna: form.comuna, strada: form.strada,
      nrStrada: form.nrStrada, bloc: form.bloc, scara: form.scara, etaj: form.etaj, apartament: form.apartament,
      nrCadastral: form.nrCadastral, nrCF: form.nrCF, amplasare: form.amplasare,
      prm_Masurat: form.prm_Masurat === "" ? 0 : Number(form.prm_Masurat),
      prm_Calculat: prmCalculated === "" ? 0 : Number(prmCalculated),
      prm_Coeficient: Number(form.coeficientPRM),
      tipSistem: form.tipSistem,
      pod: form.pod,
      backup: form.backup,
      smartMeter: form.smartMeter,
      dateInvertoare: form.dateInvertoare.map((item) => ({ model: item.nume, nrInvertoare: Number(item.nrInvertoare || 0), putereInstalata: Number(item.putereInstalata || 0), serie: item.serie })),
      datePanouri: form.datePanouri.map((item) => ({ model: item.nume, nrPanouri: Number(item.nrPanouri || 0), putereInstalata: Number(item.putereInstalata || 0), serie: item.serie })),
      dateBaterii: form.dateBaterii.map((item) => ({ model: item.nume, nrBaterii: Number(item.nrBaterii || 0), putereInstalata: Number(item.putereInstalata || 0), capacitate: Number(item.capacitate || 0), serie: item.serie })),
    };
    if (!payload.data) delete payload.data;
    return payload;
  };

  const onSubmit = async (event) => {
    event.preventDefault();
    setResult(null);
    const errors = validate();
    if (errors.length) { setResult({ ok: false, message: errors.join("\n") }); return; }
    setSubmitting(true);
    try {
      const saved = await apiRequest(isEdit ? `/proiectfotovoltaice/${id}` : "/proiectfotovoltaice", {
        method: isEdit ? "PUT" : "POST",
        body: buildPayload(),
        fallbackError: "Proiectul nu a putut fi salvat.",
      });
      onSuccess?.(saved);
    } catch (error) {
      setResult({ ok: false, message: error.message });
    } finally {
      setSubmitting(false);
    }
  };

  const renderEquipment = (listName) => {
    const config = equipment[listName];
    const rows = form[listName];
    return (
      <>
        {rows.length ? (
          <div className="table-scroll">
            <table className="line-table">
              <thead><tr>{config.columns.map((column) => <th key={column.key} style={{ width: column.width, minWidth: column.minWidth }}>{column.label}</th>)}<th><span className="sr-only">Acțiuni</span></th></tr></thead>
              <tbody>
                {rows.map((row, index) => (
                  <tr key={index}>
                    {config.columns.map((column) => (
                      <td key={column.key}>
                        {column.autocomplete ? (
                          <Combobox
                            value={row[column.key]}
                            onChange={(value) => updateItem(listName, index, { [column.key]: value })}
                            onSelect={(item) => updateItem(listName, index, config.onSuggestion(item))}
                            suggestions={suggestions[config.suggestionKey] || []}
                            placeholder={column.placeholder}
                            inputProps={{ "aria-label": `${config.title}: ${column.label}` }}
                          />
                        ) : (
                          <input className="input" type={column.type || "text"} step={column.step} min={column.min} value={row[column.key] ?? ""} onChange={(event) => updateItem(listName, index, { [column.key]: event.target.value })} aria-label={`${config.title}: ${column.label}`} />
                        )}
                      </td>
                    ))}
                    <td><button type="button" className="icon-action is-danger" onClick={() => removeItem(listName, index)} title="Elimină rândul" aria-label={`Elimină rândul ${index + 1}`}><FiTrash2 /></button></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : <div className="line-empty">Nu ai adăugat {config.title.toLowerCase()}.</div>}
        <button type="button" className="btn btn-ghost btn-sm line-add" onClick={() => addItem(listName)}><FiPlus /> {config.addLabel}</button>
      </>
    );
  };

  return (
    <form onSubmit={onSubmit}>
      <div className="dialog-body">
        {result && <div className="form-error"><Notice type={result.ok ? "success" : "error"} onClose={() => setResult(null)}>{result.message}</Notice></div>}
        {loadingProject ? (
          <div className="form-loading"><span className="skeleton" style={{ width: "40%" }} /><span className="skeleton" style={{ width: "80%" }} /><span className="skeleton" style={{ width: "65%" }} /></div>
        ) : (
          <div className="form-sections">
            <FormSection title="Date generale" description="Beneficiarul, numerotarea și tipul sistemului.">
              <FormGrid cols={4}>
                <Field label="Client" required>
                  <select className="select" value={form.clientID} disabled={loadingClients} onChange={(event) => setForm((current) => ({ ...current, clientID: event.target.value, firmaID: "" }))}>
                    <option value="">{loadingClients ? "Se încarcă…" : "Alege clientul"}</option>
                    {clients.map((client) => <option key={client.id} value={client.id}>{client.nume}{client.cnp ? ` (${client.cnp})` : ""}</option>)}
                  </select>
                </Field>
                <Field label="Firmă" hint={form.clientID && companies.length === 0 ? "Clientul nu are firme asociate." : undefined}>
                  <select className="select" value={form.firmaID} disabled={!form.clientID || companies.length === 0} onChange={(event) => setField("firmaID", event.target.value)}>
                    <option value="">{!form.clientID ? "Alege întâi clientul" : companies.length === 0 ? "Fără firmă" : "Persoană fizică (fără firmă)"}</option>
                    {companies.map((company) => <option key={company.id} value={company.id}>{company.denumire ?? company.nume ?? company.id}</option>)}
                  </select>
                </Field>
                <TextField label="Nr. proiect" required type="number" value={form.nrProiect} onChange={(value) => setField("nrProiect", value)} />
                <TextField label="Nr. PV" required type="number" value={form.nrPV} onChange={(value) => setField("nrPV", value)} />
                <TextField label="Data" type="datetime-local" value={form.data} onChange={(value) => setField("data", value)} />
                <SelectField label="Tip sistem" value={form.tipSistem} onChange={(value) => setField("tipSistem", value)} options={[...new Set(["Monofazat", "Trifazat", form.tipSistem].filter(Boolean))]} />
                <TextField label="POD" value={form.pod} onChange={(value) => setField("pod", value)} placeholder="ex: 59404020000…" />
                <TextField label="Amplasare" value={form.amplasare} onChange={(value) => setField("amplasare", value)} placeholder="ex: sol, acoperiș casă" />
              </FormGrid>
            </FormSection>

            <FormSection title="Adresa locului de producere">
              <FormGrid cols={4}>
                <TextField label="Județ" value={form.judet} onChange={(value) => setField("judet", value)} />
                <TextField label="Localitate" value={form.localitatea} onChange={(value) => setField("localitatea", value)} />
                <TextField label="Comună" value={form.comuna} onChange={(value) => setField("comuna", value)} />
                <TextField label="Sector" value={form.sector} onChange={(value) => setField("sector", value)} />
                <TextField label="Stradă" value={form.strada} onChange={(value) => setField("strada", value)} />
                <TextField label="Număr" value={form.nrStrada} onChange={(value) => setField("nrStrada", value)} />
                <TextField label="Bloc" value={form.bloc} onChange={(value) => setField("bloc", value)} />
                <TextField label="Scară" value={form.scara} onChange={(value) => setField("scara", value)} />
                <TextField label="Etaj" value={form.etaj} onChange={(value) => setField("etaj", value)} />
                <TextField label="Apartament" value={form.apartament} onChange={(value) => setField("apartament", value)} />
                <TextField label="Nr. cadastral" value={form.nrCadastral} onChange={(value) => setField("nrCadastral", value)} />
                <TextField label="Nr. CF" value={form.nrCF} onChange={(value) => setField("nrCF", value)} />
              </FormGrid>
            </FormSection>

            <FormSection title="Priza de pământ (PRM)" description="Valoarea calculată se actualizează automat: măsurat × coeficient.">
              <FormGrid cols={3}>
                <TextField label="PRM măsurat (Ω)" type="number" step="0.01" value={form.prm_Masurat} onChange={(value) => setField("prm_Masurat", value)} />
                <SelectField label="Coeficient sezonier" value={form.coeficientPRM} onChange={(value) => setField("coeficientPRM", value)} options={prmCoefficients} />
                <Field label="PRM calculat (Ω)"><input className="input is-computed" value={prmCalculated} readOnly tabIndex={-1} /></Field>
              </FormGrid>
            </FormSection>

            <FormSection title="Panouri" description="Puterea se completează din model când alegi o sugestie." actions={<span className="pill plain neutral tabular">Total {formatNumber(panelKwp, 3)} kWp</span>}>
              {renderEquipment("datePanouri")}
            </FormSection>

            <FormSection title="Invertoare" actions={<span className="pill plain neutral tabular">Total {formatNumber(inverterKw, 2)} kW</span>}>
              {renderEquipment("dateInvertoare")}
            </FormSection>

            <FormSection title="Baterii" description="Opțional, pentru sisteme cu stocare.">
              {renderEquipment("dateBaterii")}
            </FormSection>

            <FormSection title="Accesorii">
              <FormGrid cols={2}>
                <Field label="Sistem backup">
                  <Combobox value={form.backup} onChange={(value) => setField("backup", value)} suggestions={suggestions.backup || []} placeholder="ex: Huawei SmartGuard-63A-T0" />
                </Field>
                <Field label="Smart meter">
                  <Combobox value={form.smartMeter} onChange={(value) => setField("smartMeter", value)} suggestions={suggestions.smartmeter || []} placeholder="ex: Huawei Smart Meter DTSU666-H" />
                </Field>
              </FormGrid>
            </FormSection>
          </div>
        )}
      </div>
      <footer className="dialog-footer">
        <span className="dialog-footer-note"><i>*</i> Câmp obligatoriu</span>
        <div className="dialog-footer-actions">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={submitting}>Anulează</button>
          <button type="submit" className="btn btn-primary" disabled={submitting || loadingProject}>{submitting ? "Se salvează…" : isEdit ? "Salvează modificările" : "Salvează proiectul"}</button>
        </div>
      </footer>
    </form>
  );
}
