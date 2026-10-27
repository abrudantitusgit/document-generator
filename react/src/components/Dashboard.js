import React from "react";
import { Link } from "react-router-dom";
import * as Tooltip from "@radix-ui/react-tooltip";
import { FiHome, FiUsers, FiBriefcase, FiFolder, FiFileText, FiZap, FiMail } from "react-icons/fi";
import logo from "../logo.png";

const navigation = [
  { label: "Acasă", path: "/", icon: FiHome, section: "Spațiu de lucru" },
  { label: "Clienți", path: "/client", icon: FiUsers },
  { label: "Companii", path: "/firma", icon: FiBriefcase },
  { label: "Proiecte", path: "/proiect", icon: FiFolder },
  { label: "Proiecte fotovoltaice", path: "/proiectfotovoltaice", icon: FiZap },
  { label: "ATR", path: "/atr", icon: FiFileText },
  { label: "Registru ATR", path: "/registruatr", icon: FiFileText },
  { label: "Generare documente", path: "/word", icon: FiFileText },
  { label: "Clienți contor", path: "/clientcontor", icon: FiUsers },
  { label: "E-mail", path: "/email", icon: FiMail },
];

const utilityNavigation = [
  { label: "Grupuri", path: "/grup" },
  { label: "Foldere", path: "/folder" },
  { label: "Fișiere", path: "/fisier" },
];

export const pageTitle = (pathname) => navigation.find((item) => item.path === pathname)?.label || (pathname === "/registrufotovoltaice" ? "Proiecte fotovoltaice" : null) || utilityNavigation.find((item) => item.path === pathname)?.label || "Spațiu de lucru";

export default function Dashboard({ children, location = { pathname: "/" } }) {
  const title = pageTitle(location.pathname);
  return (
    <Tooltip.Provider delayDuration={250}>
      <div className="app-shell">
        <aside className="sidebar">
          <Link className="brand" to="/" aria-label="OVINSTAL — Acasă">
            <img src={logo} alt="" />
            <span><strong>OVINSTAL</strong><small>Documente & proiecte</small></span>
          </Link>
          <div className="nav-caption">SPAȚIU DE LUCRU</div>
          <nav className="main-nav" aria-label="Navigare principală">
            {navigation.map(({ label, path, icon: Icon }) => (
              <Tooltip.Root key={path}>
                <Tooltip.Trigger asChild>
                  <span className="nav-tooltip-anchor">
                    <Link className={`nav-link${location.pathname === path ? " active" : ""}`} to={path} aria-current={location.pathname === path ? "page" : undefined}>
                      <Icon aria-hidden="true" /><span>{label}</span>
                    </Link>
                  </span>
                </Tooltip.Trigger>
                <Tooltip.Portal><Tooltip.Content className="nav-tooltip" side="right">{label}<Tooltip.Arrow className="tooltip-arrow" /></Tooltip.Content></Tooltip.Portal>
              </Tooltip.Root>
            ))}
          </nav>
          <div className="nav-caption utility-caption">ORGANIZARE</div>
          <nav className="utility-nav" aria-label="Organizare">
            {utilityNavigation.map(({ label, path }) => <Link key={path} className={`utility-link${location.pathname === path ? " active" : ""}`} to={path}>{label}</Link>)}
          </nav>
          <div className="sidebar-footer"><span className="status-dot" /> Aplicație locală</div>
        </aside>
        <div className="workspace">
          <header className="topbar">
            <div className="breadcrumb"><span>OVINSTAL</span><span className="crumb-divider">/</span><strong>{title}</strong></div>
            <div className="topbar-actions"><span className="workspace-label">Administrare documente</span><div className="avatar" aria-label="Utilizator">O</div></div>
          </header>
          <main className="page-content">{children}</main>
          <footer className="page-footer"><span>OVINSTAL ELECTRIC</span><span>Documente și proiecte, într-un singur loc.</span></footer>
        </div>
      </div>
    </Tooltip.Provider>
  );
}
