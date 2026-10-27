import React, { useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import { FiAlertTriangle, FiX } from "react-icons/fi";

// `busy` keeps the dialog open while a request runs, so a stray Escape or
// backdrop click cannot drop unsaved work mid-save.
export default function Dialog({ open, onClose, title, eyebrow, description, size = "md", busy = false, children }) {
  const panelRef = useRef(null);
  const closeRef = useRef(onClose);
  closeRef.current = busy ? null : onClose;

  useEffect(() => {
    if (!open) return undefined;
    const onKey = (event) => { if (event.key === "Escape" && closeRef.current) closeRef.current(); };
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", onKey);
    const panel = panelRef.current;
    const focusTarget = panel?.querySelector("[data-autofocus]") || panel?.querySelector("input:not([type=hidden]):not([disabled]), select, textarea");
    (focusTarget || panel)?.focus({ preventScroll: true });
    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener("keydown", onKey);
    };
  }, [open]);

  if (!open) return null;

  return createPortal(
    <div className="dialog-backdrop" role="presentation" onMouseDown={(event) => { if (event.target === event.currentTarget && closeRef.current) closeRef.current(); }}>
      <section ref={panelRef} tabIndex={-1} className={`dialog size-${size}`} role="dialog" aria-modal="true" aria-labelledby="dialog-title">
        {title && (
          <header className="dialog-header">
            <div>
              {eyebrow && <span className="eyebrow">{eyebrow}</span>}
              <h2 id="dialog-title">{title}</h2>
              {description && <p>{description}</p>}
            </div>
            <button type="button" className="icon-action" onClick={onClose} disabled={busy} aria-label="Închide"><FiX /></button>
          </header>
        )}
        {children}
      </section>
    </div>,
    document.body
  );
}

export function ConfirmDialog({ open, title, message, confirmLabel = "Șterge", busy = false, onConfirm, onCancel }) {
  return (
    <Dialog open={open} onClose={onCancel} size="sm" busy={busy}>
      <div className="dialog-body">
        <div className="confirm-body">
          <span className="confirm-icon"><FiAlertTriangle /></span>
          <div><h2 id="dialog-title">{title}</h2><p>{message}</p></div>
        </div>
      </div>
      <footer className="dialog-footer">
        <div className="dialog-footer-actions">
          <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={busy} data-autofocus>Anulează</button>
          <button type="button" className="btn btn-danger" onClick={onConfirm} disabled={busy}>{busy ? "Se șterge…" : confirmLabel}</button>
        </div>
      </footer>
    </Dialog>
  );
}
