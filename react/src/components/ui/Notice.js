import React from "react";
import { FiAlertCircle, FiAlertTriangle, FiCheckCircle, FiInfo, FiX } from "react-icons/fi";

const icons = { success: FiCheckCircle, error: FiAlertCircle, warning: FiAlertTriangle, info: FiInfo };

export default function Notice({ type = "info", children, onClose }) {
  const Icon = icons[type] || FiInfo;
  return (
    <div className={`notice ${type}`} role={type === "error" ? "alert" : "status"}>
      <Icon aria-hidden="true" />
      <div className="notice-text">{children}</div>
      {onClose && <button type="button" className="notice-close" onClick={onClose} aria-label="Închide mesajul"><FiX /></button>}
    </div>
  );
}
