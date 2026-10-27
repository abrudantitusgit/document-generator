import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { FiMenu, FiMoon, FiSun } from "react-icons/fi";
import logo from "../../logo.png";
import { findPage, isActivePath, navigation } from "./navigation";

const THEME_KEY = "ovinstal-theme";

const readStoredTheme = () => {
  try { return localStorage.getItem(THEME_KEY); } catch { return null; }
};

const systemPrefersDark = () => window.matchMedia?.("(prefers-color-scheme: dark)").matches;

function useTheme() {
  const [theme, setTheme] = useState(() => readStoredTheme() || (systemPrefersDark() ? "dark" : "light"));
  useEffect(() => {
    document.documentElement.dataset.theme = theme;
    try { localStorage.setItem(THEME_KEY, theme); } catch { /* private mode: theme just isn't remembered */ }
  }, [theme]);
  return [theme, () => setTheme((current) => current === "dark" ? "light" : "dark")];
}

export default function AppShell({ children, location = { pathname: "/" } }) {
  const [menuOpen, setMenuOpen] = useState(false);
  const [theme, toggleTheme] = useTheme();
  const page = findPage(location.pathname);

  useEffect(() => { setMenuOpen(false); }, [location.pathname]);
  useEffect(() => { document.title = `${page.label} · OVINSTAL`; }, [page.label]);

  return (
    <div className="app-shell">
      <aside className={`sidebar${menuOpen ? " open" : ""}`} aria-label="Navigare">
        <Link className="brand" to="/" aria-label="OVINSTAL, pagina principală">
          <img src={logo} alt="" />
          <span className="brand-text"><strong>OVINSTAL</strong><small>Documente și proiecte</small></span>
        </Link>
        <nav className="sidebar-scroll">
          {navigation.map((group) => (
            <div key={group.title} className="nav-group">
              <div className="nav-group-title">{group.title}</div>
              <ul className="nav-list">
                {group.items.map((item) => {
                  const active = isActivePath(item, location.pathname);
                  const Icon = item.icon;
                  return (
                    <li key={item.path}>
                      <Link className={`nav-link${active ? " active" : ""}`} to={item.path} aria-current={active ? "page" : undefined}>
                        <Icon aria-hidden="true" /><span>{item.label}</span>
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>
          ))}
        </nav>
        <div className="sidebar-footer"><span className="status-dot" /> Aplicație locală</div>
      </aside>
      <div className={`sidebar-backdrop${menuOpen ? " open" : ""}`} onClick={() => setMenuOpen(false)} aria-hidden="true" />

      <div className="workspace">
        <header className="topbar">
          <div className="topbar-left">
            <button type="button" className="btn btn-ghost btn-icon menu-button" onClick={() => setMenuOpen(true)} aria-label="Deschide meniul"><FiMenu /></button>
            <div className="breadcrumb"><span>{page.group}</span><span className="crumb-divider">/</span><strong>{page.label}</strong></div>
          </div>
          <div className="topbar-actions">
            <button type="button" className="btn btn-ghost btn-icon" onClick={toggleTheme} aria-label={theme === "dark" ? "Comută pe tema luminoasă" : "Comută pe tema întunecată"} title="Schimbă tema">
              {theme === "dark" ? <FiSun /> : <FiMoon />}
            </button>
            <div className="avatar" aria-hidden="true">O</div>
          </div>
        </header>
        <main className="page-content">{children}</main>
        <footer className="page-footer"><strong>OVINSTAL ELECTRIC</strong><span>Documente și proiecte, într-un singur loc.</span></footer>
      </div>
    </div>
  );
}
