import React from "react";

export function StatGrid({ children }) {
  return <div className="stat-grid">{children}</div>;
}

export default function StatCard({ label, value, unit, foot, icon: Icon, tone = "green", loading = false }) {
  return (
    <div className="card stat-card">
      <div className="stat-head">
        <span>{label}</span>
        {Icon && <span className={`stat-icon tone-${tone}`}><Icon aria-hidden="true" /></span>}
      </div>
      <div className="stat-value">{loading ? <span className="skeleton" style={{ width: 70, height: 24 }} /> : <>{value}{unit && <small>{unit}</small>}</>}</div>
      {foot && <div className="stat-foot">{foot}</div>}
    </div>
  );
}
