import React, { useCallback, useEffect, useMemo, useState } from "react";
import { FiBattery, FiChevronDown, FiChevronRight, FiCpu, FiDownload, FiEdit2, FiFileText, FiPlus, FiRefreshCw, FiSun, FiTrash2, FiZap } from "react-icons/fi";
import { baseUrl } from "../config/api";
import { apiList, apiRequest } from "../lib/api";
import { formatAddress, formatDate, formatNumber } from "../lib/format";
import DocumentGenerator from "../components/DocumentGenerator";
import { ConfirmDialog, DataTable, Dialog, FilterSelect, Notice, PageHeader, SearchInput, StatCard, StatGrid, Toolbar, distinctOptions } from "../components/ui";
import ProiectFotovoltaice from "./ProiectFotovoltaice";

const projectId = (project) => project.id ?? project.ID;
const kwp = (project) => project.putereTotalaPanouri != null ? Number(project.putereTotalaPanouri) / 1000 : null;
const fullAddress = (project) => [formatAddress(project), project.judet && `jud. ${project.judet}`].filter(Boolean).join(", ");
const show = (value) => value == null || value === "" ? <span className="cell-empty">—</span> : value;

function EquipmentCard({ icon: Icon, title, head, rows, empty }) {
  return (
    <div className="equipment-card">
      <div className="equipment-card-title"><Icon aria-hidden="true" /><strong>{title}</strong><span className="count-badge">{rows.length}</span></div>
      {rows.length ? (
        <div className="table-scroll">
          <table className="mini-table">
            <thead><tr>{head.map((label, index) => <th key={label} className={index && index < head.length - 1 ? "align-right" : undefined}>{label}</th>)}</tr></thead>
            <tbody>{rows.map((row, rowIndex) => <tr key={rowIndex}>{row.map((cell, index) => <td key={index} className={index && index < row.length - 1 ? "align-right" : "cell-wrap"}>{show(cell)}</td>)}</tr>)}</tbody>
          </table>
        </div>
      ) : <p className="muted equipment-empty">{empty}</p>}
    </div>
  );
}

function ProjectDetails({ project }) {
  return (
    <div className="pv-details">
      <dl className="pv-facts">
        <div><dt>Amplasare</dt><dd>{show(project.amplasare)}</dd></div>
        <div><dt>Nr. cadastral</dt><dd>{show(project.nrCadastral)}</dd></div>
        <div><dt>Nr. CF</dt><dd>{show(project.nrCF)}</dd></div>
        <div><dt>Coeficient PRM</dt><dd>{show(project.prM_Coeficient)}</dd></div>
        <div><dt>Sistem backup</dt><dd>{show(project.backup)}</dd></div>
        <div><dt>Smart meter</dt><dd>{show(project.smartMeter)}</dd></div>
      </dl>
      <div className="equipment-grid">
        <EquipmentCard icon={FiSun} title="Panouri" empty="Nu există panouri." head={["Model", "Nr.", "Putere (W)", "Total (kWp)", "Serii"]}
          rows={(project.datePanouri || []).map((item) => [item.model, item.nrPanouri, formatNumber(item.putereInstalata, 0), formatNumber((Number(item.nrPanouri || 0) * Number(item.putereInstalata || 0)) / 1000, 3), item.serie])} />
        <EquipmentCard icon={FiCpu} title="Invertoare" empty="Nu există invertoare." head={["Model", "Nr.", "Putere (kW)", "Serie"]}
          rows={(project.dateInvertoare || []).map((item) => [item.model, item.nrInvertoare, formatNumber(item.putereInstalata, 2), item.serie])} />
        <EquipmentCard icon={FiBattery} title="Baterii" empty="Nu există baterii." head={["Model", "Nr.", "Putere (kW)", "Capacitate (Ah)", "Serie"]}
          rows={(project.dateBaterii || []).map((item) => [item.model, item.nrBaterii, formatNumber(item.putereInstalata, 2), formatNumber(item.capacitate, 2), item.serie])} />
      </div>
    </div>
  );
}

export default function RegistruFotovoltaice() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [notice, setNotice] = useState(null);
  const [search, setSearch] = useState("");
  const [county, setCounty] = useState("");
  const [system, setSystem] = useState("");
  const [expanded, setExpanded] = useState(() => new Set());
  const [editor, setEditor] = useState(null); // { id } for edit, {} for create
  const [editorBusy, setEditorBusy] = useState(false);
  const [generating, setGenerating] = useState(null);
  const [pendingDelete, setPendingDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setItems(await apiList("/proiectfotovoltaice"));
    } catch (error) {
      setItems([]);
      setNotice({ type: "error", text: error.message || "Proiectele nu au putut fi încărcate." });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const visible = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("ro");
    return items.filter((project) => {
      if (county && project.judet !== county) return false;
      if (system && project.tipSistem !== system) return false;
      if (!query) return true;
      return [
        project.nrProiect, project.nrPV, project.judet, project.localitatea, project.comuna, project.strada, project.pod, project.tipSistem,
        project.client?.nume, project.client?.cnp, project.firma?.nume,
        ...(project.dateInvertoare || []).map((item) => item.model),
        ...(project.datePanouri || []).map((item) => item.model),
        ...(project.dateBaterii || []).map((item) => item.model),
      ].filter((value) => value != null).join(" ").toLocaleLowerCase("ro").includes(query);
    });
  }, [items, search, county, system]);

  const totals = useMemo(() => ({
    kwp: visible.reduce((sum, project) => sum + (kwp(project) || 0), 0),
    inverter: visible.reduce((sum, project) => sum + Number(project.putereTotalaInvertor || 0), 0),
    withBattery: visible.filter((project) => project.dateBaterii?.length).length,
  }), [visible]);

  const toggle = (id) => setExpanded((current) => {
    const next = new Set(current);
    if (next.has(id)) next.delete(id); else next.add(id);
    return next;
  });

  const confirmDelete = async () => {
    const id = projectId(pendingDelete);
    setDeleting(true);
    try {
      await apiRequest(`/proiectfotovoltaice/${id}`, { method: "DELETE", fallbackError: "Ștergerea nu a reușit." });
      setItems((current) => current.filter((project) => projectId(project) !== id));
      setNotice({ type: "success", text: `Proiectul ${pendingDelete.nrProiect} a fost șters.` });
    } catch (error) {
      setNotice({ type: "error", text: error.message });
    } finally {
      setDeleting(false);
      setPendingDelete(null);
    }
  };

  const onSaved = (isEdit) => {
    setEditor(null);
    setNotice({ type: "success", text: isEdit ? "Proiectul a fost actualizat." : "Proiectul a fost creat." });
    load();
  };

  const filtered = Boolean(search || county || system);

  const columns = useMemo(() => [
    { key: "nr", label: "Nr. proiect", nowrap: true, value: (row) => Number(row.nrProiect) || row.nrProiect, render: (row) => <><span className="tag">{row.nrProiect ?? "—"}</span><span className="cell-sub">PV {row.nrPV ?? "—"}</span></> },
    { key: "data", label: "Data", nowrap: true, value: (row) => row.data ? new Date(row.data).getTime() : null, render: (row) => show(formatDate(row.data)) },
    { key: "client", label: "Beneficiar", primary: true, value: (row) => row.client?.nume, render: (row) => <>{row.client?.nume || "—"}{row.firmaID && row.firma && <span className="cell-sub">{row.firma.nume}</span>}</> },
    { key: "id", label: "CNP / CUI", nowrap: true, value: (row) => row.client?.cnp, render: (row) => <>{show(row.client?.cnp)}{row.firmaID && row.firma?.cui && <span className="cell-sub">CUI {row.firma.cui}</span>}</> },
    { key: "tip", label: "Sistem", value: (row) => row.tipSistem, render: (row) => row.tipSistem ? <span className={`pill plain ${row.tipSistem === "Trifazat" ? "info" : "neutral"}`}>{row.tipSistem}</span> : show(null) },
    { key: "pod", label: "POD", nowrap: true, value: (row) => row.pod },
    { key: "adresa", label: "Adresă", value: (row) => fullAddress(row), render: (row) => <span className="cell-clip" title={fullAddress(row)}>{show(fullAddress(row))}</span> },
    { key: "telefon", label: "Telefon", nowrap: true, value: (row) => row.client?.telefon },
    { key: "kwp", label: "Panouri (kWp)", align: "right", value: kwp, render: (row) => show(formatNumber(kwp(row), 3)) },
    { key: "inv", label: "Invertor (kW)", align: "right", value: (row) => row.putereTotalaInvertor, render: (row) => show(formatNumber(row.putereTotalaInvertor, 2)) },
    { key: "bat", label: "Baterii (Ah)", align: "right", value: (row) => row.capacitateTotalaBaterii, render: (row) => show(formatNumber(row.capacitateTotalaBaterii, 2)) },
    { key: "prm", label: "PRM măs. / calc.", align: "right", nowrap: true, value: (row) => row.prM_Calculat, render: (row) => <>{show(row.prM_Masurat)}<span className="cell-sub">{row.prM_Calculat ?? "—"}</span></> },
  ], []);

  return (
    <section className="page">
      <PageHeader
        eyebrow="Proiecte"
        title="Proiecte fotovoltaice"
        description="Registrul proiectelor PV: beneficiari, echipamente, PRM și documente generate."
        actions={<>
          <button type="button" className="btn btn-secondary btn-icon" onClick={load} disabled={loading} aria-label="Reîncarcă lista" title="Reîncarcă"><FiRefreshCw /></button>
          <button type="button" className="btn btn-secondary" onClick={() => window.location.assign(`${baseUrl}/proiectfotovoltaice/export`)}><FiDownload /> Export Excel</button>
          <button type="button" className="btn btn-primary" onClick={() => setEditor({})}><FiPlus /> Adaugă proiect</button>
        </>}
      />

      {notice && <Notice type={notice.type} onClose={() => setNotice(null)}>{notice.text}</Notice>}

      <StatGrid>
        <StatCard label="Proiecte" value={formatNumber(visible.length)} loading={loading} icon={FiZap} tone="orange" foot={filtered ? `din ${formatNumber(items.length)} în total` : "Toate proiectele PV"} />
        <StatCard label="Putere panouri" value={formatNumber(totals.kwp, 2)} unit="kWp" loading={loading} icon={FiSun} tone="amber" foot="Însumată pe proiectele afișate" />
        <StatCard label="Putere invertoare" value={formatNumber(totals.inverter, 2)} unit="kW" loading={loading} icon={FiCpu} tone="blue" foot="Însumată pe proiectele afișate" />
        <StatCard label="Cu stocare" value={formatNumber(totals.withBattery)} loading={loading} icon={FiBattery} tone="green" foot={visible.length ? `${Math.round((totals.withBattery / visible.length) * 100)}% au baterii` : "—"} />
      </StatGrid>

      <Toolbar filters={<>
        <FilterSelect label="Județ" value={county} onChange={setCounty} options={distinctOptions(items, (item) => item.judet)} />
        <FilterSelect label="Sistem" value={system} onChange={setSystem} options={distinctOptions(items, (item) => item.tipSistem)} />
      </>}>
        <SearchInput value={search} onChange={setSearch} placeholder="Caută după nr., client, localitate, POD, echipament…" />
      </Toolbar>

      <DataTable
        title="Proiecte"
        columns={columns}
        rows={visible}
        rowKey={projectId}
        loading={loading}
        filtered={filtered}
        initialSort={{ key: "nr", dir: "desc" }}
        expandedKeys={expanded}
        renderDetail={(project) => <ProjectDetails project={project} />}
        emptyTitle="Nu există proiecte fotovoltaice încă"
        emptyText="Adaugă primul proiect cu butonul „Adaugă proiect”."
        actions={(project) => {
          const id = projectId(project);
          const open = expanded.has(id);
          return <>
            <button type="button" className={`icon-action${open ? " is-active" : ""}`} onClick={() => toggle(id)} aria-expanded={open} title={open ? "Ascunde echipamentele" : "Vezi echipamentele"} aria-label="Detalii echipamente">{open ? <FiChevronDown /> : <FiChevronRight />}</button>
            <button type="button" className="icon-action is-brand" onClick={() => setGenerating(project)} title="Generează documente" aria-label="Generează documente"><FiFileText /></button>
            <button type="button" className="icon-action is-brand" onClick={() => setEditor({ id })} title="Modifică" aria-label="Modifică proiectul"><FiEdit2 /></button>
            <button type="button" className="icon-action is-danger" onClick={() => setPendingDelete(project)} title="Șterge" aria-label="Șterge proiectul"><FiTrash2 /></button>
          </>;
        }}
      />

      <Dialog
        open={Boolean(editor)}
        onClose={() => setEditor(null)}
        busy={editorBusy}
        size="lg"
        eyebrow={editor?.id ? "Editare" : "Proiect nou"}
        title={editor?.id ? "Modifică proiectul fotovoltaic" : "Adaugă proiect fotovoltaic"}
      >
        {editor && <ProiectFotovoltaice id={editor.id} onSuccess={() => onSaved(Boolean(editor.id))} onCancel={() => setEditor(null)} onBusyChange={setEditorBusy} />}
      </Dialog>

      <Dialog
        open={Boolean(generating)}
        onClose={() => setGenerating(null)}
        size="lg"
        eyebrow="Generare documente"
        title={generating ? `Proiect nr. ${generating.nrProiect}` : ""}
        description={generating ? [generating.firma?.nume || generating.client?.nume, fullAddress(generating)].filter(Boolean).join(" · ") : undefined}
      >
        {generating && <DocumentGenerator groupName="Fotovoltaice" endpoint="/word/createfotovoltaice" project={generating} variant="dialog" onClose={() => setGenerating(null)} />}
      </Dialog>

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        busy={deleting}
        title="Ștergi proiectul fotovoltaic?"
        message={pendingDelete && <>Proiectul <strong>nr. {pendingDelete.nrProiect}</strong>{pendingDelete.client?.nume ? ` (${pendingDelete.client.nume})` : ""} și echipamentele lui vor fi șterse definitiv.</>}
        onConfirm={confirmDelete}
        onCancel={() => setPendingDelete(null)}
      />
    </section>
  );
}
