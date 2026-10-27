import React from "react";
import { FiSearch, FiX } from "react-icons/fi";

export function Toolbar({ children, filters }) {
  return (
    <div className="toolbar">
      {children}
      {filters && <div className="toolbar-filters">{filters}</div>}
    </div>
  );
}

export function SearchInput({ value, onChange, placeholder }) {
  return (
    <label className="search-input">
      <FiSearch aria-hidden="true" />
      <input value={value} onChange={(event) => onChange(event.target.value)} placeholder={placeholder} aria-label={placeholder} />
      {value && <button type="button" className="search-clear" onClick={() => onChange("")} aria-label="Șterge căutarea"><FiX /></button>}
    </label>
  );
}

// `options` are { value, label }; an empty value means "all".
export function FilterSelect({ label, value, onChange, options }) {
  return (
    <select className={`filter-select${value ? " is-set" : ""}`} value={value} onChange={(event) => onChange(event.target.value)} aria-label={`Filtru: ${label}`}>
      <option value="">{label}: toate</option>
      {options.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
    </select>
  );
}

// Distinct non-empty values of a field, sorted, as FilterSelect options.
export const distinctOptions = (rows, getValue) => [...new Set(rows.map(getValue).filter((value) => value != null && value !== ""))]
  .sort((a, b) => String(a).localeCompare(String(b), "ro"))
  .map((value) => ({ value: String(value), label: String(value) }));
