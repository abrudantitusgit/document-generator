import React, { useEffect, useMemo, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { FiCheck, FiFileText, FiFolder, FiX } from "react-icons/fi";
import { apiList, apiRequest } from "../lib/api";
import { formatAddress, formatDate } from "../lib/format";
import { Combobox, Notice } from "./ui";

function FolderCheckbox({ checked, indeterminate, onChange, label }) {
  const ref = useRef(null);
  useEffect(() => { if (ref.current) ref.current.indeterminate = indeterminate; }, [indeterminate]);
  return <input ref={ref} type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} aria-label={label} />;
}

const projectLabel = (project) => `${project.nrProiect ?? "—"} · ${project.denumire || project.strada || "Proiect"}`;
const projectOwner = (project) => project.firma?.nume || project.client?.nume || "";

/**
 * Picks a project and a subset of template files from a document group, then
 * asks the API to generate the documents. Used for branșament (page, with a
 * project picker) and fotovoltaic projects (dialog, project fixed).
 */
export default function DocumentGenerator({ groupName, endpoint, project: fixedProject, variant = "page", onClose }) {
  const [group, setGroup] = useState(null);
  const [loadingGroup, setLoadingGroup] = useState(true);
  const [selected, setSelected] = useState(() => new Set());
  const [projects, setProjects] = useState([]);
  const [project, setProject] = useState(fixedProject || null);
  const [query, setQuery] = useState("");
  const [generating, setGenerating] = useState(false);
  const [notice, setNotice] = useState(null);

  useEffect(() => {
    let active = true;
    setLoadingGroup(true);
    apiRequest(`/grup/${groupName}`)
      .catch(() => null)
      .then((data) => {
        if (!active) return;
        const loaded = data && Array.isArray(data.foldere) ? data : null;
        setGroup(loaded);
        setSelected(new Set((loaded?.foldere || []).flatMap((folder) => (folder.fisiere || []).map((file) => file.id))));
        setLoadingGroup(false);
      });
    return () => { active = false; };
  }, [groupName]);

  useEffect(() => {
    if (fixedProject) return;
    apiList("/proiect/proiecteinclude").then(setProjects).catch(() => setProjects([]));
  }, [fixedProject]);

  const folders = useMemo(() => group?.foldere || [], [group]);
  const totalFiles = folders.reduce((sum, folder) => sum + (folder.fisiere?.length || 0), 0);

  const toggleFile = (id, checked) => setSelected((current) => {
    const next = new Set(current);
    if (checked) next.add(id); else next.delete(id);
    return next;
  });
  const toggleFolder = (folder, checked) => setSelected((current) => {
    const next = new Set(current);
    (folder.fisiere || []).forEach((file) => { if (checked) next.add(file.id); else next.delete(file.id); });
    return next;
  });
  const setAll = (checked) => setSelected(checked ? new Set(folders.flatMap((folder) => (folder.fisiere || []).map((file) => file.id))) : new Set());

  const generate = async () => {
    if (!project) { setNotice({ type: "error", text: "Alege mai întâi proiectul pentru care generezi documentele." }); return; }
    if (!selected.size) { setNotice({ type: "error", text: "Selectează cel puțin un document." }); return; }
    setGenerating(true);
    setNotice(null);
    // The API expects the group with only the chosen files left in each folder.
    const payload = {
      proiect: project,
      grup: { ...group, foldere: folders.map((folder) => ({ ...folder, fisiere: (folder.fisiere || []).filter((file) => selected.has(file.id)) })) },
    };
    try {
      const result = await apiRequest(endpoint, { method: "POST", body: payload, fallbackError: "Generarea documentelor nu a reușit." });
      if (result === false || result?.success === false) throw new Error(result?.messages?.join("\n") || "Generarea documentelor nu a reușit.");
      const messages = result?.messages?.filter(Boolean) || [];
      setNotice({ type: messages.length ? "warning" : "success", text: `${selected.size} ${selected.size === 1 ? "document generat" : "documente generate"} pentru proiectul ${project.nrProiect}.${messages.length ? `\n${messages.join("\n")}` : ""}` });
    } catch (error) {
      setNotice({ type: "error", text: error.message });
    } finally {
      setGenerating(false);
    }
  };

  const projectPicker = !fixedProject && (
    <section className="card generator-step">
      <div className="card-header"><div className="card-title"><span className="step-number">1</span><div><h3>Proiect</h3><p>Caută după număr, denumire, client sau adresă.</p></div></div></div>
      <div className="card-body">
        {project ? (
          <div className="project-summary">
            <span className="project-summary-icon"><FiFolder /></span>
            <div className="project-summary-copy">
              <strong>{projectLabel(project)}</strong>
              <span>{[projectOwner(project), formatAddress(project), project.atr && `ATR ${project.atr.nrAtr}/${formatDate(project.atr.dataAtr)}`].filter(Boolean).join(" · ")}</span>
            </div>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => { setProject(null); setQuery(""); }}><FiX /> Schimbă</button>
          </div>
        ) : (
          <Combobox
            value={query}
            onChange={setQuery}
            onSelect={(item) => { setProject(item); setNotice(null); }}
            suggestions={projects}
            getLabel={projectLabel}
            getDescription={(item) => [projectOwner(item), formatAddress(item), item.atr && `ATR ${item.atr.nrAtr}`].filter(Boolean).join(" · ")}
            placeholder={projects.length ? `Caută printre ${projects.length} proiecte…` : "Se încarcă proiectele…"}
            showAllOnFocus
            maxItems={12}
          />
        )}
      </div>
    </section>
  );

  const documentList = (
    <section className={variant === "page" ? "card generator-step" : "generator-documents"}>
      <div className={variant === "page" ? "card-header" : "form-section-header"}>
        <div className="card-title">
          {variant === "page" && <span className="step-number">{fixedProject ? 1 : 2}</span>}
          <div><h3>Documente</h3><p>{loadingGroup ? "Se încarcă șabloanele…" : `${selected.size} din ${totalFiles} selectate`}</p></div>
        </div>
        {totalFiles > 0 && (
          <div className="generator-bulk">
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => setAll(true)}>Toate</button>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => setAll(false)}>Niciunul</button>
          </div>
        )}
      </div>
      <div className={variant === "page" ? "card-body" : undefined}>
        {loadingGroup ? <div className="folder-grid">{[0, 1].map((key) => <div key={key} className="folder-card"><span className="skeleton" style={{ width: "40%" }} /><span className="skeleton" style={{ width: "75%", marginTop: 14 }} /><span className="skeleton" style={{ width: "60%", marginTop: 10 }} /></div>)}</div>
          : !group ? (
            <div className="table-state">
              <span className="table-state-icon"><FiFileText /></span>
              <strong>Grupul „{groupName}” nu există încă</strong>
              <span>Creează grupul și adaugă foldere și fișiere șablon din <Link to="/grup">Șabloane → Grupuri</Link>.</span>
            </div>
          ) : folders.length === 0 ? (
            <div className="table-state">
              <span className="table-state-icon"><FiFolder /></span>
              <strong>Grupul „{groupName}” nu are foldere</strong>
              <span>Adaugă foldere și fișiere din <Link to="/folder">Șabloane → Foldere</Link>.</span>
            </div>
          ) : (
            <div className="folder-grid">
              {folders.map((folder) => {
                const files = folder.fisiere || [];
                const count = files.filter((file) => selected.has(file.id)).length;
                return (
                  <div key={folder.id} className={`folder-card${count ? " has-selection" : ""}`}>
                    <label className="folder-card-header">
                      <FolderCheckbox checked={files.length > 0 && count === files.length} indeterminate={count > 0 && count < files.length} onChange={(checked) => toggleFolder(folder, checked)} label={`Selectează tot din ${folder.nume}`} />
                      <strong>{folder.nume}</strong>
                      <span className="count-badge">{count}/{files.length}</span>
                    </label>
                    {files.length ? (
                      <ul className="file-list">
                        {files.map((file) => (
                          <li key={file.id}>
                            <label className="checkbox"><input type="checkbox" checked={selected.has(file.id)} onChange={(event) => toggleFile(file.id, event.target.checked)} />{file.nume}</label>
                          </li>
                        ))}
                      </ul>
                    ) : <p className="muted folder-empty">Folderul nu conține fișiere.</p>}
                  </div>
                );
              })}
            </div>
          )}
      </div>
    </section>
  );

  const generateButton = (
    <button type="button" className="btn btn-primary" onClick={generate} disabled={generating || !group || !selected.size}>
      {generating ? "Se generează…" : <><FiCheck /> Generează {selected.size ? `(${selected.size})` : ""}</>}
    </button>
  );

  if (variant === "dialog") {
    return (
      <>
        <div className="dialog-body">
          {notice && <div className="form-error"><Notice type={notice.type} onClose={() => setNotice(null)}>{notice.text}</Notice></div>}
          {documentList}
        </div>
        <footer className="dialog-footer">
          <span className="dialog-footer-note">Documentele se salvează în folderul proiectului.</span>
          <div className="dialog-footer-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={generating}>Închide</button>
            {generateButton}
          </div>
        </footer>
      </>
    );
  }

  return (
    <div className="generator-layout">
      <div className="generator-main">
        {projectPicker}
        {documentList}
      </div>
      <aside className="card generator-aside">
        <div className="card-body">
          <span className="eyebrow">Rezumat</span>
          <dl className="summary-list">
            <div><dt>Proiect</dt><dd>{project ? projectLabel(project) : <span className="muted">Neales</span>}</dd></div>
            {project && projectOwner(project) && <div><dt>Beneficiar</dt><dd>{projectOwner(project)}</dd></div>}
            <div><dt>Documente</dt><dd className="tabular">{selected.size} / {totalFiles}</dd></div>
          </dl>
          {notice && <Notice type={notice.type} onClose={() => setNotice(null)}>{notice.text}</Notice>}
          {generateButton}
        </div>
      </aside>
    </div>
  );
}
