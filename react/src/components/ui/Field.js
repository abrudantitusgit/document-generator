import React from "react";

export function FormSection({ title, description, actions, children }) {
  return (
    <section className="form-section">
      {(title || actions) && (
        <div className="form-section-header">
          <div><h3>{title}</h3>{description && <p>{description}</p>}</div>
          {actions}
        </div>
      )}
      {children}
    </section>
  );
}

export function FormGrid({ cols = 2, children }) {
  return <div className={`form-grid cols-${cols}`}>{children}</div>;
}

export function Field({ label, required, hint, wide, children }) {
  return (
    <label className={`field${wide ? " wide" : ""}`}>
      <span className="field-label">{label}{required && <i> *</i>}</span>
      {children}
      {hint && <span className="field-hint">{hint}</span>}
    </label>
  );
}

export function TextField({ label, required, hint, wide, value, onChange, ...inputProps }) {
  return (
    <Field label={label} required={required} hint={hint} wide={wide}>
      <input className="input" required={required} value={value ?? ""} onChange={(event) => onChange(event.target.value)} {...inputProps} />
    </Field>
  );
}

// Options may be plain strings or { value, label } objects.
export function SelectField({ label, required, hint, wide, value, onChange, options, placeholder, ...selectProps }) {
  return (
    <Field label={label} required={required} hint={hint} wide={wide}>
      <select className="select" required={required} value={value ?? ""} onChange={(event) => onChange(event.target.value)} {...selectProps}>
        {placeholder !== undefined && <option value="">{placeholder}</option>}
        {options.map((option) => {
          const item = typeof option === "string" ? { value: option, label: option } : option;
          return <option key={item.value} value={item.value}>{item.label}</option>;
        })}
      </select>
    </Field>
  );
}
