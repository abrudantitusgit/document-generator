import React from "react";
import { Link } from "react-router-dom";
import { FiArrowUpRight, FiBriefcase, FiFileText, FiFolder, FiMail, FiUsers, FiZap } from "react-icons/fi";

const shortcuts = [
  { title: "Clienți", description: "Date și informații de contact", path: "/client", icon: FiUsers, tone: "blue" },
  { title: "Companii", description: "Administrarea firmelor", path: "/firma", icon: FiBriefcase, tone: "violet" },
  { title: "Proiecte", description: "Branșamente și lucrări", path: "/proiect", icon: FiFolder, tone: "amber" },
  { title: "Documente", description: "Generează acte pentru proiecte", path: "/word", icon: FiFileText, tone: "green" },
  { title: "Fotovoltaice", description: "Proiecte și registru PV", path: "/proiectfotovoltaice", icon: FiZap, tone: "orange" },
  { title: "E-mail", description: "Mesaje și notificări", path: "/email", icon: FiMail, tone: "slate" },
];

export default function Acasa() {
  return (
    <div className="home-page">
      <section className="welcome-panel">
        <div className="welcome-copy">
          <div className="eyebrow"><span className="eyebrow-line" /> BINE AI VENIT</div>
          <h1>Documentele tale,<br /><span>în ordine.</span></h1>
          <p>Gestionează clienți, proiecte și documente tehnice dintr-un spațiu de lucru simplu.</p>
          <Link className="primary-action" to="/proiect">Deschide proiectele <FiArrowUpRight /></Link>
        </div>
        <div className="welcome-art" aria-hidden="true">
          <div className="art-orbit orbit-one" /><div className="art-orbit orbit-two" />
          <div className="art-document"><div className="art-doc-mark">O</div><i /><i /><i /><div className="art-doc-stamp">OK</div></div>
          <div className="art-chip chip-one"><FiFolder /> Proiecte</div>
          <div className="art-chip chip-two"><FiFileText /> Documente</div>
        </div>
      </section>

      <section className="section-heading">
        <div><div className="eyebrow">ACCES RAPID</div><h2>Unde vrei să ajungi?</h2></div>
        <span className="section-note">Alege o zonă de lucru pentru a continua.</span>
      </section>
      <section className="shortcut-grid" aria-label="Acces rapid">
        {shortcuts.map(({ title, description, path, icon: Icon, tone }) => (
          <Link key={title} to={path} className="shortcut-card">
            <div className={`shortcut-icon ${tone}`}><Icon /></div>
            <div className="shortcut-copy"><h3>{title}</h3><p>{description}</p></div>
            <FiArrowUpRight className="shortcut-arrow" />
          </Link>
        ))}
      </section>
      <div className="home-note"><span className="status-dot" /> Spațiu de lucru pregătit <span className="note-separator">·</span> OVInstal Electric</div>
    </div>
  );
}
