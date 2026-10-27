import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createPortal } from "react-dom";

const defaultLabel = (item) => typeof item === "string" ? item : item?.model ?? "";
let nextListId = 0;

/**
 * Text input with a suggestion list. The list renders in a portal with fixed
 * positioning so it is not clipped inside dialogs or scrolling tables.
 */
export default function Combobox({
  value, onChange, onSelect, suggestions = [], getLabel = defaultLabel, getDescription,
  placeholder, showAllOnFocus = false, maxItems = 8, inputProps,
}) {
  const inputRef = useRef(null);
  const [open, setOpen] = useState(false);
  const [rect, setRect] = useState(null);
  const [active, setActive] = useState(0);
  const listId = useRef(null);
  if (!listId.current) listId.current = `combo-list-${(nextListId += 1)}`;

  const matches = useMemo(() => {
    const query = String(value || "").trim().toLocaleLowerCase("ro");
    if (!query && !showAllOnFocus) return [];
    return suggestions
      .filter((item) => !query || `${getLabel(item)} ${getDescription ? getDescription(item) : ""}`.toLocaleLowerCase("ro").includes(query))
      .slice(0, maxItems);
  }, [value, suggestions, getLabel, getDescription, showAllOnFocus, maxItems]);

  const updatePosition = useCallback(() => {
    if (!inputRef.current) return;
    const box = inputRef.current.getBoundingClientRect();
    setRect({ top: box.bottom + 4, left: box.left, width: box.width });
  }, []);

  useEffect(() => {
    if (!open) return undefined;
    window.addEventListener("scroll", updatePosition, true);
    window.addEventListener("resize", updatePosition);
    return () => {
      window.removeEventListener("scroll", updatePosition, true);
      window.removeEventListener("resize", updatePosition);
    };
  }, [open, updatePosition]);

  const show = () => { updatePosition(); setOpen(true); setActive(0); };
  const choose = (item) => {
    onChange(getLabel(item));
    onSelect?.(item);
    setOpen(false);
  };

  const onKeyDown = (event) => {
    if (!open || matches.length === 0) return;
    if (event.key === "ArrowDown") { event.preventDefault(); setActive((index) => (index + 1) % matches.length); }
    else if (event.key === "ArrowUp") { event.preventDefault(); setActive((index) => (index - 1 + matches.length) % matches.length); }
    else if (event.key === "Enter") { event.preventDefault(); choose(matches[active]); }
    else if (event.key === "Escape") { event.stopPropagation(); setOpen(false); }
  };

  return (
    <div className="combo">
      <input
        ref={inputRef}
        className="input"
        value={value ?? ""}
        placeholder={placeholder}
        autoComplete="off"
        role="combobox"
        aria-expanded={open && matches.length > 0}
        aria-controls={listId.current}
        aria-autocomplete="list"
        onFocus={show}
        onChange={(event) => { onChange(event.target.value); show(); }}
        onBlur={() => setTimeout(() => setOpen(false), 120)}
        onKeyDown={onKeyDown}
        {...inputProps}
      />
      {open && rect && matches.length > 0 && createPortal(
        <ul id={listId.current} className="combo-list" role="listbox" style={{ top: rect.top, left: rect.left, width: rect.width }}>
          {matches.map((item, index) => (
            <li
              key={typeof item === "string" ? `${item}-${index}` : item.id ?? `${getLabel(item)}-${index}`}
              role="option"
              aria-selected={index === active}
              className={`combo-option${index === active ? " is-active" : ""}`}
              onMouseDown={(event) => { event.preventDefault(); choose(item); }}
              onMouseEnter={() => setActive(index)}
            >
              {getLabel(item)}
              {getDescription && <small>{getDescription(item)}</small>}
            </li>
          ))}
        </ul>,
        document.body
      )}
    </div>
  );
}
