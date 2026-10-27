import React, { useCallback, useEffect, useMemo, useState } from "react";
import { FiEdit2, FiPlus, FiRefreshCw, FiTrash2, FiUploadCloud } from "react-icons/fi";
import { ocrUrl } from "../config/api";
import { apiList, apiRequest, humanizeError, readJson } from "../lib/api";
import { formatDate, formatMoney, formatNumber, readPath } from "../lib/format";
import { ConfirmDialog, DataTable, Dialog, Field, FilterSelect, FormGrid, FormSection, Notice, PageHeader, SearchInput, Toolbar } from "./ui";

const formatInputValue = (field, value) => {
  if (value == null || value === "") return field.type === "checkbox" ? false : "";
  if (field.type === "date") return String(value).slice(0, 10);
  if (field.type === "datetime-local") {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? "" : new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  }
  return value;
};

const isBlank = (value) => value == null || value === "";

/**
 * The CRUD template behind every simple resource page. A definition in
 * pages/resourceDefinitions.js describes endpoints, form fields (grouped into
 * sections) and table columns; this component renders the same header,
 * toolbar, table, form dialog and delete confirmation for all of them.
 */
export default function ResourceWorkspace({ definition }) {
  const idField = definition.idField || "id";
  const [records, setRecords] = useState([]);
  const [options, setOptions] = useState({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [search, setSearch] = useState("");
  const [filters, setFilters] = useState({});
  const [dialog, setDialog] = useState(null);
  const [draft, setDraft] = useState({});
  const [notice, setNotice] = useState(null);
  const [formError, setFormError] = useState(null);
  const [pendingDelete, setPendingDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [ocrBusy, setOcrBusy] = useState(false);
  const [ocrMessage, setOcrMessage] = useState(null);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      setRecords(await apiList(definition.listEndpoint));
    } catch (error) {
      setRecords([]);
      setNotice({ type: "error", text: error.message || `Nu s-au putut încărca ${definition.plural.toLowerCase()}.` });
    } finally {
      setLoading(false);
    }
  }, [definition]);

  useEffect(() => {
    setSearch("");
    setFilters({});
    setNotice(null);
    loadData();
  }, [loadData]);

  useEffect(() => {
    let active = true;
    const sources = [...new Set(definition.fields.map((field) => field.optionsEndpoint).filter(Boolean))];
    Promise.all(sources.map(async (endpoint) => {
      try { return [endpoint, await apiList(endpoint)]; } catch { return [endpoint, []]; }
    })).then((entries) => { if (active) setOptions(Object.fromEntries(entries)); });
    return () => { active = false; };
  }, [definition]);

  const fieldByName = useMemo(() => Object.fromEntries(definition.fields.map((field) => [field.name, field])), [definition]);
  const fieldOptions = useCallback((field) => field.options || options[field.optionsEndpoint] || [], [options]);
  const optionLabel = (field, option) => field.optionLabel ? field.optionLabel(option) : option?.[field.optionText || "nume"] || option?.name || "Fără nume";

  // Columns can show the label of a related record instead of its raw id.
  const lookupLabel = useCallback((fieldName, id) => {
    const field = fieldByName[fieldName];
    const option = field && fieldOptions(field).find((item) => String(item.id) === String(id));
    return option ? (field.optionLabel ? field.optionLabel(option) : option[field.optionText || "nume"]) : undefined;
  }, [fieldByName, fieldOptions]);

  const columnValue = useCallback((column, record) => {
    const raw = column.getValue ? column.getValue(record) : readPath(record, column.path);
    if (column.lookup && !isBlank(raw)) return lookupLabel(column.lookup, raw) ?? raw;
    return raw;
  }, [lookupLabel]);

  const filterFields = useMemo(() => definition.fields.filter((field) => field.filterable), [definition]);
  const filterOptions = (field) => field.filterByData
    ? [...new Set(records.map((record) => readPath(record, field.name)).filter(Boolean))].sort((a, b) => String(a).localeCompare(String(b), "ro")).map((value) => ({ value: String(value), label: String(value) }))
    : fieldOptions(field).map((option) => ({ value: String(option.id), label: optionLabel(field, option) }));

  const visibleRecords = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("ro");
    return records.filter((record) => {
      const matchesSearch = !query || definition.columns.some((column) => String(columnValue(column, record) ?? "").toLocaleLowerCase("ro").includes(query));
      const matchesFilters = filterFields.every((field) => !filters[field.name] || String(readPath(record, field.name) ?? "") === String(filters[field.name]));
      return matchesSearch && matchesFilters;
    });
  }, [records, search, filters, definition, filterFields, columnValue]);

  const tableColumns = useMemo(() => definition.columns.map((column, index) => ({
    key: column.label,
    label: column.label,
    primary: index === 0,
    align: column.align || (["number", "money"].includes(column.type) ? "right" : undefined),
    nowrap: column.type === "date",
    value: (record) => {
      const value = columnValue(column, record);
      return column.type === "date" && value ? new Date(value).getTime() : value;
    },
    render: (record) => {
      const value = columnValue(column, record);
      if (column.type === "boolean") return <span className={`pill ${value ? "success" : "neutral"}`}>{value ? "Activ" : "Inactiv"}</span>;
      if (isBlank(value)) return <span className="cell-empty">—</span>;
      if (column.type === "date") return formatDate(value);
      if (column.type === "money") return formatMoney(value);
      if (column.type === "number") return formatNumber(value, column.digits || 0);
      if (column.type === "tag") return <span className="tag">{String(value)}</span>;
      if (column.clip) return <span className="cell-clip" title={String(value)}>{String(value)}</span>;
      return String(value);
    },
  })), [definition, columnValue]);

  const sections = useMemo(() => {
    const declared = definition.sections || [{ id: "main", title: "Detalii" }];
    return declared
      .map((section, index) => ({ ...section, fields: definition.fields.filter((field) => (field.section || declared[0].id) === section.id || (index === 0 && !declared.some((item) => item.id === field.section))) }))
      .filter((section) => section.fields.length);
  }, [definition]);

  const openCreate = () => {
    const initial = Object.fromEntries(definition.fields.map((field) => [field.name, field.defaultValue ?? (field.type === "checkbox" ? false : "")]));
    setDraft({ ...initial, ...(definition.defaults || {}) });
    setDialog("create");
    setFormError(null);
    setOcrMessage(null);
  };

  const openEdit = (record) => {
    setDraft(Object.fromEntries(definition.fields.map((field) => [field.name, formatInputValue(field, record[field.name])])));
    setDialog(record);
    setFormError(null);
    setOcrMessage(null);
  };

  const closeDialog = () => { if (!saving && !ocrBusy) { setDialog(null); setOcrMessage(null); } };
  const setValue = (name, value) => setDraft((current) => ({ ...current, [name]: value }));

  const importIdCard = async (event) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) return;
    setOcrBusy(true);
    setOcrMessage(null);
    try {
      const form = new FormData();
      form.append("file", file);
      const response = await fetch(`${ocrUrl}/extract`, { method: "POST", body: form });
      const result = await readJson(response);
      if (!response.ok) throw new Error(humanizeError(result, "Nu s-au putut citi datele din imagine."));
      const filled = Object.entries(result?.fields || {}).filter(([, value]) => value != null && String(value).trim() !== "");
      if (!filled.length) {
        setOcrMessage({ type: "error", text: "Nu am identificat câmpuri. Încearcă o fotografie clară, bine luminată, cu textul lizibil." });
        return;
      }
      setDraft((current) => {
        const next = { ...current };
        filled.forEach(([key, value]) => { if (isBlank(next[key]) || String(next[key]).trim() === "") next[key] = value; });
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
    setFormError(null);
    const isEdit = dialog !== "create";
    const config = isEdit ? definition.update : definition.create;
    const body = {};
    definition.fields.forEach((field) => {
      const value = draft[field.name];
      if (field.type === "number") body[field.name] = isBlank(value) ? 0 : Number(value);
      else if (field.type === "date" || field.type === "datetime-local") { if (value) body[field.name] = value; }
      else body[field.name] = value;
    });
    if (isEdit && definition.idField !== false) body[idField] = dialog[idField];
    try {
      await apiRequest(typeof config.endpoint === "function" ? config.endpoint(dialog) : config.endpoint, {
        method: config.method || "POST",
        body,
        fallbackError: "Salvarea nu a reușit. Verifică datele și încearcă din nou.",
      });
      setDialog(null);
      setNotice({ type: "success", text: isEdit ? `${definition.singular} actualizat${definition.feminine ? "ă" : ""}.` : `${definition.singular} adăugat${definition.feminine ? "ă" : ""}.` });
      await loadData();
    } catch (error) {
      setFormError(error.message || "Salvarea nu a reușit.");
    } finally { setSaving(false); }
  };

  const confirmDelete = async () => {
    const record = pendingDelete;
    setDeleting(true);
    try {
      const endpoint = typeof definition.remove.endpoint === "function" ? definition.remove.endpoint(record) : definition.remove.endpoint;
      await apiRequest(endpoint, { method: definition.remove.method || "DELETE", fallbackError: "Ștergerea nu a reușit." });
      setNotice({ type: "success", text: "Înregistrarea a fost ștearsă." });
      await loadData();
    } catch (error) {
      setNotice({ type: "error", text: error.message || "Ștergerea nu a reușit." });
    } finally {
      setDeleting(false);
      setPendingDelete(null);
    }
  };

  const recordLabel = (record) => definition.columns.map((column) => columnValue(column, record)).find((value) => !isBlank(value));
  const isFiltered = Boolean(search) || Object.values(filters).some(Boolean);
  const singular = definition.singular.toLowerCase();

  const renderInput = (field) => {
    const value = draft[field.name];
    if (field.type === "select") return (
      <select className="select" required={field.required} value={value ?? ""} onChange={(event) => setValue(field.name, event.target.value || null)}>
        <option value="">Alege…</option>
        {fieldOptions(field).map((option) => <option key={option.id} value={option.id}>{optionLabel(field, option)}</option>)}
      </select>
    );
    if (field.type === "textarea") return <textarea className="textarea" required={field.required} rows={4} value={value ?? ""} onChange={(event) => setValue(field.name, event.target.value)} />;
    if (field.type === "checkbox") return <span className="checkbox"><input type="checkbox" checked={Boolean(value)} onChange={(event) => setValue(field.name, event.target.checked)} />{field.helpText || "Da"}</span>;
    return <input className="input" required={field.required} type={field.type || "text"} min={field.min} step={field.step} value={value ?? ""} placeholder={field.placeholder || ""} disabled={field.readOnlyOnCreate && dialog === "create"} onChange={(event) => setValue(field.name, event.target.value)} />;
  };

  return (
    <section className="page">
      <PageHeader
        eyebrow={definition.eyebrow || "Administrare date"}
        title={definition.title}
        description={definition.description}
        actions={<>
          <button type="button" className="btn btn-secondary btn-icon" onClick={loadData} disabled={loading} aria-label="Reîncarcă lista" title="Reîncarcă"><FiRefreshCw /></button>
          <button type="button" className="btn btn-primary" onClick={openCreate}><FiPlus /> Adaugă {singular}</button>
        </>}
      />

      {notice && <Notice type={notice.type} onClose={() => setNotice(null)}>{notice.text}</Notice>}

      <Toolbar filters={filterFields.length > 0 && filterFields.map((field) => (
        <FilterSelect key={field.name} label={field.label} value={filters[field.name] || ""} onChange={(value) => setFilters((current) => ({ ...current, [field.name]: value }))} options={filterOptions(field)} />
      ))}>
        <SearchInput value={search} onChange={setSearch} placeholder={`Caută în ${definition.plural.toLowerCase()}…`} />
      </Toolbar>

      <DataTable
        title={definition.plural}
        columns={tableColumns}
        rows={visibleRecords}
        rowKey={(record) => [record.type, record[idField]].filter(Boolean).join(":")}
        loading={loading}
        filtered={isFiltered}
        emptyTitle={`Nu există ${definition.plural.toLowerCase()} încă`}
        emptyText={`Adaugă prima înregistrare cu butonul „Adaugă ${singular}”.`}
        actions={(record) => <>
          <button type="button" className="icon-action is-brand" onClick={() => openEdit(record)} title="Modifică" aria-label={`Modifică ${singular}`}><FiEdit2 /></button>
          <button type="button" className="icon-action is-danger" onClick={() => setPendingDelete(record)} title="Șterge" aria-label={`Șterge ${singular}`}><FiTrash2 /></button>
        </>}
      />

      <Dialog
        open={Boolean(dialog)}
        onClose={closeDialog}
        busy={saving || ocrBusy}
        eyebrow={dialog === "create" ? "Înregistrare nouă" : "Editare"}
        title={dialog === "create" ? `Adaugă ${singular}` : `Modifică ${singular}`}
        size={definition.dialogSize || "md"}
      >
        <form onSubmit={saveRecord}>
          <div className="dialog-body">
            {definition.importType === "romanianIdCard" && dialog === "create" && (
              <div className="import-panel">
                <span className="import-icon"><FiUploadCloud /></span>
                <div className="import-copy"><strong>Completează din cartea de identitate</strong><span>Fotografia este procesată local. Verifică toate câmpurile detectate înainte să salvezi.</span></div>
                <label className={`btn btn-secondary import-button${ocrBusy ? " is-busy" : ""}`}>
                  {ocrBusy ? "Se citesc datele…" : "Încarcă fotografie"}
                  <input type="file" accept="image/jpeg,image/png,image/webp" onChange={importIdCard} disabled={ocrBusy || saving} />
                </label>
                {ocrMessage && <div className="import-message"><Notice type={ocrMessage.type}>{ocrMessage.text}</Notice></div>}
              </div>
            )}
            {formError && <div className="form-error"><Notice type="error" onClose={() => setFormError(null)}>{formError}</Notice></div>}
            <div className="form-sections">
              {sections.map((section) => (
                <FormSection key={section.id} title={sections.length > 1 ? section.title : undefined} description={section.description}>
                  <FormGrid cols={section.cols || 2}>
                    {section.fields.map((field) => (
                      <Field key={field.name} label={field.label} required={field.required} hint={field.hint} wide={field.wide}>{renderInput(field)}</Field>
                    ))}
                  </FormGrid>
                </FormSection>
              ))}
            </div>
          </div>
          <footer className="dialog-footer">
            <span className="dialog-footer-note"><i>*</i> Câmp obligatoriu</span>
            <div className="dialog-footer-actions">
              <button type="button" className="btn btn-secondary" onClick={closeDialog} disabled={saving || ocrBusy}>Anulează</button>
              <button type="submit" className="btn btn-primary" disabled={saving || ocrBusy}>{saving ? "Se salvează…" : dialog === "create" ? `Adaugă ${singular}` : "Salvează modificările"}</button>
            </div>
          </footer>
        </form>
      </Dialog>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        busy={deleting}
        title={`Ștergi ${singular}?`}
        message={<>{pendingDelete && recordLabel(pendingDelete) ? <><strong>„{recordLabel(pendingDelete)}”</strong> va fi șters definitiv. </> : "Înregistrarea va fi ștearsă definitiv. "}Acțiunea nu poate fi anulată.</>}
        onConfirm={confirmDelete}
        onCancel={() => setPendingDelete(null)}
      />
    </section>
  );
}
