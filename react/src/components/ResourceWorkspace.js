import React, { useCallback, useEffect, useMemo, useState } from "react";
import { FiEdit2, FiPlus, FiRefreshCw, FiSearch, FiTrash2, FiX, FiUploadCloud } from "react-icons/fi";
import { baseUrl } from "../config/api";
import "../css/components/resource-workspace.css";

const readJson = async (response) => {
  const content = await response.text();
  if (!content) return null;
  try { return JSON.parse(content); } catch { return content; }
};

const readPath = (value, path) => path.split(".").reduce((current, key) => current == null ? undefined : current[key], value);
const humanizeError = (body, fallback) => body?.message || body?.title || (typeof body === "string" ? body : fallback);
const formatInputValue = (field, value) => {
  if (!value) return field.type === "checkbox" ? false : "";
  if (field.type === "date") return String(value).slice(0, 10);
  if (field.type === "datetime-local") {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? "" : new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  }
  return value;
};

export default function ResourceWorkspace({ definition }) {
  const [records, setRecords] = useState([]);
  const [options, setOptions] = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [filters, setFilters] = useState({});
  const [dialog, setDialog] = useState(null);
  const [draft, setDraft] = useState({});
  const [notice, setNotice] = useState(null);
  const [ocrBusy, setOcrBusy] = useState(false);
  const [ocrMessage, setOcrMessage] = useState(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await fetch(`${baseUrl}${definition.listEndpoint}`);
      const body = await readJson(response);
      if (!response.ok) throw new Error(humanizeError(body, `Nu s-au putut încărca ${definition.plural.toLowerCase()}.`));
      setRecords(Array.isArray(body) ? body : []);
    } catch (error) {
      setRecords([]);
      setNotice({ type: "error", text: error.message || "Nu s-au putut încărca înregistrările." });
    } finally {
      setLoading(false);
    }
  }, [definition]);

  useEffect(() => { loadData(); }, [loadData]);

  useEffect(() => {
    let active = true;
    const sources = [...new Set(definition.fields.map((field) => field.optionsEndpoint).filter(Boolean))];
    Promise.all(sources.map(async (endpoint) => {
      try {
        const response = await fetch(`${baseUrl}${endpoint}`);
        const body = await readJson(response);
        return [endpoint, response.ok && Array.isArray(body) ? body : []];
      } catch { return [endpoint, []]; }
    })).then((entries) => { if (active) setOptions(Object.fromEntries(entries)); });
    return () => { active = false; };
  }, [definition]);

  const fieldOptions = (field) => field.options || options[field.optionsEndpoint] || [];
  const getOptionLabel = (field, option) => field.optionLabel ? field.optionLabel(option) : option?.[field.optionText || "nume"] || option?.name || "Fără nume";
  const filterOptions = (field) => field.filterByData
    ? [...new Set(records.map((record) => readPath(record, field.name)).filter(Boolean))].sort().map((value) => ({ id: value, nume: value }))
    : fieldOptions(field);

  const visibleRecords = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("ro");
    return records.filter((record) => {
      const matchesSearch = !query || definition.columns.some((column) => {
        const value = column.getValue ? column.getValue(record) : readPath(record, column.path);
        return String(value ?? "").toLocaleLowerCase("ro").includes(query);
      });
      const matchesFilters = definition.fields.filter((field) => field.filterable && filters[field.name]).every((field) => String(readPath(record, field.name) ?? "") === String(filters[field.name]));
      return matchesSearch && matchesFilters;
    });
  }, [records, search, filters, definition]);

  const openCreate = () => {
    const initial = Object.fromEntries(definition.fields.map((field) => [field.name, field.defaultValue ?? (field.type === "checkbox" ? false : "")]));
    setDraft({ ...initial, ...(definition.defaults || {}) });
    setDialog("create");
    setNotice(null);
    setOcrMessage(null);
  };

  const openEdit = (record) => {
    setDraft(Object.fromEntries(definition.fields.map((field) => [field.name, formatInputValue(field, record[field.name])] )));
    setDialog(record);
    setNotice(null);
    setOcrMessage(null);
  };

  const closeDialog = () => { if (!saving && !ocrBusy) { setDialog(null); setOcrMessage(null); } };

  const importIdCard = async (event) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;
    setOcrBusy(true);
    setOcrMessage(null);
    try {
      const form = new FormData();
      form.append("file", file);
      const ocrUrl = `${window.location.protocol}//${window.location.hostname}:5001/extract`;
      const response = await fetch(ocrUrl, { method: "POST", body: form });
      const result = await readJson(response);
      if (!response.ok) throw new Error(humanizeError(result, "Nu s-au putut citi datele din imagine."));
      const fields = result?.fields || {};
      const filled = Object.entries(fields).filter(([, value]) => value != null && String(value).trim() !== "");
      if (!filled.length) {
        setOcrMessage({ type: "error", text: "Nu am identificat câmpuri. Încearcă o fotografie clară, bine luminată, cu textul lizibil." });
        return;
      }
      setDraft((current) => {
        const next = { ...current };
        filled.forEach(([key, value]) => { if (next[key] == null || String(next[key]).trim() === "") next[key] = value; });
        return next;
      });
      setOcrMessage({ type: result.warnings?.length ? "warning" : "success", text: `${filled.length} câmpuri detectate. Verifică și corectează datele înainte de salvare.${result.warnings?.length ? ` ${result.warnings.join(" ")}` : ""}` });
    } catch (error) {
      setOcrMessage({ type: "error", text: error.message || "Serviciul local de citire nu este disponibil." });
    } finally { setOcrBusy(false); }
  };

  const saveRecord = async (event) => {
    event.preventDefault();
    setSaving(true);
    setNotice(null);
    const isEdit = dialog !== "create";
    const config = isEdit ? definition.update : definition.create;
    const body = {};
    definition.fields.forEach((field) => {
      const value = draft[field.name];
      if (field.type === "number") body[field.name] = value === "" || value == null ? 0 : Number(value);
      else if (field.type === "date" || field.type === "datetime-local") { if (value) body[field.name] = value; }
      else body[field.name] = value;
    });
    if (isEdit && definition.idField !== false) body[definition.idField || "id"] = dialog[definition.idField || "id"];
    try {
      const response = await fetch(`${baseUrl}${typeof config.endpoint === "function" ? config.endpoint(dialog) : config.endpoint}`, {
        method: config.method || "POST",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(body),
      });
      const result = await readJson(response);
      if (!response.ok) throw new Error(humanizeError(result, "Salvarea nu a reușit. Verifică datele și încearcă din nou."));
      setDialog(null);
      setNotice({ type: "success", text: isEdit ? "Înregistrarea a fost actualizată." : "Înregistrarea a fost adăugată." });
      await loadData();
    } catch (error) {
      setNotice({ type: "error", text: error.message || "Salvarea nu a reușit." });
    } finally { setSaving(false); }
  };

  const deleteRecord = async (record) => {
    const label = definition.columns.map((column) => column.getValue ? column.getValue(record) : readPath(record, column.path)).find(Boolean);
    if (!window.confirm(`Ștergi ${definition.singular.toLowerCase()}${label ? ` „${label}”` : ""}?`)) return;
    try {
      const endpoint = typeof definition.remove.endpoint === "function" ? definition.remove.endpoint(record) : definition.remove.endpoint;
      const response = await fetch(`${baseUrl}${endpoint}`, { method: definition.remove.method || "DELETE" });
      const body = await readJson(response);
      if (!response.ok) throw new Error(humanizeError(body, "Ștergerea nu a reușit."));
      setNotice({ type: "success", text: "Înregistrarea a fost ștearsă." });
      await loadData();
    } catch (error) { setNotice({ type: "error", text: error.message || "Ștergerea nu a reușit." }); }
  };

  const renderCell = (column, record) => {
    const value = column.getValue ? column.getValue(record) : readPath(record, column.path);
    if (column.render) return column.render(value, record);
    if (value == null || value === "") return <span className="cell-muted">—</span>;
    if (column.type === "date") return new Date(value).toLocaleDateString("ro-RO");
    if (column.type === "boolean") return <span className={`state-pill ${value ? "is-active" : "is-inactive"}`}>{value ? "Activ" : "Inactiv"}</span>;
    return String(value);
  };

  return (
    <section className="resource-page">
      <div className="resource-heading">
        <div><div className="eyebrow">ADMINISTRARE DATE</div><h1>{definition.title}</h1><p>{definition.description}</p></div>
        <button type="button" className="resource-primary" onClick={openCreate}><FiPlus /> Adaugă {definition.singular.toLowerCase()}</button>
      </div>

      {notice && <div className={`resource-notice ${notice.type}`} role="status">{notice.text}<button type="button" aria-label="Închide mesajul" onClick={() => setNotice(null)}><FiX /></button></div>}

      <div className="resource-toolbar">
        <label className="resource-search"><FiSearch /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder={`Caută în ${definition.plural.toLowerCase()}...`} aria-label={`Caută în ${definition.plural.toLowerCase()}`} /></label>
        <div className="resource-filters">
          {definition.fields.filter((field) => field.filterable).map((field) => <select key={field.name} value={filters[field.name] || ""} onChange={(event) => setFilters((current) => ({ ...current, [field.name]: event.target.value }))} aria-label={`Filtru: ${field.label}`}><option value="">Toate: {field.label.toLowerCase()}</option>{filterOptions(field).map((option) => <option key={option.id} value={option.id}>{getOptionLabel(field, option)}</option>)}</select>)}
          <button type="button" className="resource-icon-button" onClick={loadData} aria-label="Reîncarcă lista" title="Reîncarcă"><FiRefreshCw /></button>
        </div>
      </div>

      <div className="resource-table-card">
        <div className="resource-table-header"><div><strong>{definition.plural}</strong><span>{loading ? "Se încarcă…" : `${visibleRecords.length} ${visibleRecords.length === 1 ? "înregistrare" : "înregistrări"}`}</span></div></div>
        <div className="resource-table-scroll"><table className="resource-table"><thead><tr>{definition.columns.map((column) => <th key={column.label}>{column.label}</th>)}<th className="actions-header">Acțiuni</th></tr></thead>
          <tbody>{loading ? <tr><td colSpan={definition.columns.length + 1} className="table-message">Se încarcă datele…</td></tr> : visibleRecords.length === 0 ? <tr><td colSpan={definition.columns.length + 1} className="table-empty"><span className="empty-mark"><FiSearch /></span><strong>{search || Object.values(filters).some(Boolean) ? "Nu am găsit rezultate" : `Nu există ${definition.plural.toLowerCase()} încă`}</strong><span>{search || Object.values(filters).some(Boolean) ? "Încearcă altă căutare sau elimină filtrele." : `Adaugă prima înregistrare folosind butonul „Adaugă ${definition.singular.toLowerCase()}”.`}</span></td></tr> : visibleRecords.map((record) => <tr key={record[definition.idField || "id"]}>{definition.columns.map((column) => <td key={column.label}>{renderCell(column, record)}</td>)}<td className="row-actions"><button type="button" className="row-action edit" onClick={() => openEdit(record)} title="Modifică" aria-label={`Modifică ${definition.singular.toLowerCase()}`}><FiEdit2 /></button><button type="button" className="row-action delete" onClick={() => deleteRecord(record)} title="Șterge" aria-label={`Șterge ${definition.singular.toLowerCase()}`}><FiTrash2 /></button></td></tr>)}</tbody>
        </table></div>
        <div className="resource-table-footer">Se afișează <strong>{visibleRecords.length}</strong> din <strong>{records.length}</strong> înregistrări</div>
      </div>

      {dialog && <div className="resource-dialog-backdrop" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget) closeDialog(); }}><section className="resource-dialog" role="dialog" aria-modal="true" aria-labelledby="resource-dialog-title"><header><div><div className="eyebrow">{dialog === "create" ? "ÎNREGISTRARE NOUĂ" : "EDITARE ÎNREGISTRĂRI"}</div><h2 id="resource-dialog-title">{dialog === "create" ? `Adaugă ${definition.singular.toLowerCase()}` : `Modifică ${definition.singular.toLowerCase()}`}</h2></div><button type="button" className="resource-icon-button" onClick={closeDialog} aria-label="Închide"><FiX /></button></header>
        {definition.importType === "romanianIdCard" && dialog === "create" && <div className="id-import-panel"><div className="id-import-copy"><strong>Completează din cartea de identitate</strong><span>Fotografia este procesată local. Verifică toate câmpurile detectate înainte să salvezi.</span></div><label className={`resource-secondary id-import-button ${ocrBusy ? "is-busy" : ""}`}><FiUploadCloud />{ocrBusy ? "Se citesc datele…" : "Încarcă fotografie"}<input type="file" accept="image/jpeg,image/png,image/webp" onChange={importIdCard} disabled={ocrBusy || saving} /></label>{ocrMessage && <div className={`id-import-message ${ocrMessage.type}`} role="status">{ocrMessage.text}</div>}</div>}
        <form onSubmit={saveRecord}><div className="resource-form-grid">{definition.fields.map((field) => <label key={field.name} className={`resource-field ${field.wide ? "wide" : ""}`}><span>{field.label}{field.required && <i> *</i>}</span>{field.type === "select" ? <select required={field.required} value={draft[field.name] ?? ""} onChange={(event) => setDraft((current) => ({ ...current, [field.name]: event.target.value || null }))}><option value="">Alege {field.label.toLowerCase()}</option>{fieldOptions(field).map((option) => <option key={option.id} value={option.id}>{getOptionLabel(field, option)}</option>)}</select> : field.type === "textarea" ? <textarea required={field.required} rows={4} value={draft[field.name] ?? ""} onChange={(event) => setDraft((current) => ({ ...current, [field.name]: event.target.value }))} /> : field.type === "checkbox" ? <span className="resource-checkbox"><input type="checkbox" checked={Boolean(draft[field.name])} onChange={(event) => setDraft((current) => ({ ...current, [field.name]: event.target.checked }))} /> {field.helpText || "Da"}</span> : <input required={field.required} type={field.type || "text"} min={field.min} step={field.step} value={draft[field.name] ?? ""} placeholder={field.placeholder || ""} disabled={field.readOnlyOnCreate && dialog === "create"} onChange={(event) => setDraft((current) => ({ ...current, [field.name]: event.target.value }))} />}</label>)}</div>
        <footer><span><i>*</i> Câmp obligatoriu</span><div><button type="button" className="resource-secondary" onClick={closeDialog} disabled={saving || ocrBusy}>Anulează</button><button type="submit" className="resource-primary" disabled={saving || ocrBusy}>{saving ? "Se salvează…" : dialog === "create" ? "Adaugă înregistrarea" : "Salvează modificările"}</button></div></footer></form></section></div>}
    </section>
  );
}
