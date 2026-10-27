import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { FiArrowRight, FiArrowUpRight, FiBarChart2, FiBookOpen, FiBriefcase, FiFilePlus, FiFolder, FiUsers, FiZap } from "react-icons/fi";
import { apiList } from "../lib/api";
import { formatDate, formatNumber } from "../lib/format";
import { StatCard, StatGrid } from "../components/ui";

const shortcuts = [
  { title: "Generează documente", description: "Acte de branșament pentru un proiect", path: "/word", icon: FiFilePlus, tone: "green" },
  { title: "Proiecte fotovoltaice", description: "Registru, echipamente și documente PV", path: "/proiectfotovoltaice", icon: FiZap, tone: "orange" },
  { title: "Registru ATR", description: "Raport cu numerele documentelor", path: "/registruatr", icon: FiBookOpen, tone: "blue" },
  { title: "Rapoarte", description: "Evoluție lunară și distribuție pe județe", path: "/rapoarte", icon: FiBarChart2, tone: "violet" },
];

const sources = { clients: "/client/clienti", companies: "/firma/list", projects: "/proiect/proiecteinclude", pv: "/proiectfotovoltaice" };

export default function Acasa() {
  const [data, setData] = useState(null);

  useEffect(() => {
    let active = true;
    Promise.all(Object.entries(sources).map(([key, endpoint]) => apiList(endpoint).then((rows) => [key, rows], () => [key, null])))
      .then((entries) => { if (active) setData(Object.fromEntries(entries)); });
    return () => { active = false; };
  }, []);

  const recent = useMemo(() => {
    if (!data) return [];
    const branch = (data.projects || []).map((item) => ({ id: item.id, kind: "Branșament", path: "/proiect", number: item.nrProiect, name: item.denumire || item.strada, owner: item.firma?.nume || item.client?.nume, date: item.data }));
    const pv = (data.pv || []).map((item) => ({ id: item.id, kind: "Fotovoltaic", path: "/proiectfotovoltaice", number: item.nrProiect, name: [item.localitatea, item.strada].filter(Boolean).join(", "), owner: item.firma?.nume || item.client?.nume, date: item.data }));
    return [...branch, ...pv].sort((a, b) => new Date(b.date || 0) - new Date(a.date || 0)).slice(0, 6);
  }, [data]);

  const count = (key) => data?.[key] ? formatNumber(data[key].length) : "—";

  return (
    <div className="home-page">
      <section className="welcome-panel">
        <div className="welcome-copy">
          <span className="eyebrow welcome-eyebrow"><span className="eyebrow-line" /> Bine ai venit</span>
          <h1>Documentele tale,<br /><span>în ordine.</span></h1>
          <p>Gestionează clienți, proiecte și documente tehnice dintr-un spațiu de lucru simplu.</p>
          <div className="welcome-actions">
            <Link className="welcome-button primary" to="/proiect">Deschide proiectele <FiArrowUpRight /></Link>
            <Link className="welcome-button" to="/word">Generează documente</Link>
          </div>
        </div>
        <div className="welcome-art" aria-hidden="true">
          <div className="art-orbit orbit-one" /><div className="art-orbit orbit-two" />
          <div className="art-document"><div className="art-doc-mark">O</div><i /><i /><i /><div className="art-doc-stamp">OK</div></div>
          <div className="art-chip chip-one"><FiFolder /> Proiecte</div>
          <div className="art-chip chip-two"><FiZap /> Fotovoltaice</div>
        </div>
      </section>

      <StatGrid>
        <StatCard label="Clienți" value={count("clients")} icon={FiUsers} tone="blue" loading={!data} foot={<Link className="stat-link" to="/client">Vezi clienții <FiArrowRight /></Link>} />
        <StatCard label="Companii" value={count("companies")} icon={FiBriefcase} tone="violet" loading={!data} foot={<Link className="stat-link" to="/firma">Vezi companiile <FiArrowRight /></Link>} />
        <StatCard label="Proiecte branșament" value={count("projects")} icon={FiFolder} tone="green" loading={!data} foot={<Link className="stat-link" to="/proiect">Vezi proiectele <FiArrowRight /></Link>} />
        <StatCard label="Proiecte fotovoltaice" value={count("pv")} icon={FiZap} tone="orange" loading={!data} foot={<Link className="stat-link" to="/proiectfotovoltaice">Vezi registrul PV <FiArrowRight /></Link>} />
      </StatGrid>

      <div className="home-columns">
        <section className="card">
          <div className="card-header"><div className="card-title"><div><h3>Activitate recentă</h3><p>Ultimele proiecte, după dată</p></div></div></div>
          {!data ? <div className="card-body"><span className="skeleton" style={{ width: "70%" }} /><span className="skeleton" style={{ width: "50%", marginTop: 14 }} /></div>
            : recent.length === 0 ? <div className="table-state"><span className="table-state-icon"><FiFolder /></span><strong>Niciun proiect încă</strong><span>Proiectele adăugate vor apărea aici.</span></div>
              : (
                <ul className="recent-list">
                  {recent.map((item) => (
                    <li key={`${item.kind}-${item.id}`}>
                      <Link to={item.path} className="recent-item">
                        <span className={`recent-kind ${item.kind === "Fotovoltaic" ? "tone-orange" : "tone-green"}`}>{item.kind === "Fotovoltaic" ? <FiZap /> : <FiFolder />}</span>
                        <span className="recent-copy"><strong>Nr. {item.number ?? "—"} · {item.name || item.kind}</strong><span>{[item.kind, item.owner].filter(Boolean).join(" · ")}</span></span>
                        <span className="recent-date">{formatDate(item.date)}</span>
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
        </section>

        <section className="card">
          <div className="card-header"><div className="card-title"><div><h3>Acces rapid</h3><p>Cele mai folosite acțiuni</p></div></div></div>
          <ul className="shortcut-list">
            {shortcuts.map(({ title, description, path, icon: Icon, tone }) => (
              <li key={path}>
                <Link to={path} className="shortcut-item">
                  <span className={`stat-icon tone-${tone}`}><Icon /></span>
                  <span className="recent-copy"><strong>{title}</strong><span>{description}</span></span>
                  <FiArrowUpRight className="shortcut-arrow" />
                </Link>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
