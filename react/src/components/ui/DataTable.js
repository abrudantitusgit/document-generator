import React, { useEffect, useMemo, useRef, useState } from "react";
import { FiChevronDown, FiChevronLeft, FiChevronRight, FiChevronUp, FiInbox, FiSearch } from "react-icons/fi";
import { compareValues, formatNumber } from "../../lib/format";

const isBlank = (value) => value == null || value === "";

/**
 * Shared table used by every list and report page.
 * columns: [{ key, label, value(row), render(row), align, primary, nowrap, sortable }]
 * `value` feeds sorting; `render` (optional) decides what is shown.
 */
export default function DataTable({
  title, subtitle, headerActions, columns, rows, rowKey, loading = false,
  actions, renderDetail, expandedKeys, initialSort, pageSizes = [25, 50, 100],
  filtered = false, emptyTitle, emptyText, footer,
}) {
  const [sort, setSort] = useState(initialSort || null);
  const [pageSize, setPageSize] = useState(pageSizes[0]);
  const [page, setPage] = useState(0);
  const scrollRef = useRef(null);

  // Detail rows span the full (scrollable) table; expose the visible width so
  // their content can stay in view instead of stretching past the viewport.
  const hasDetail = Boolean(renderDetail);
  useEffect(() => {
    const element = scrollRef.current;
    if (!element || !hasDetail || typeof ResizeObserver === "undefined") return undefined;
    const observer = new ResizeObserver(() => element.style.setProperty("--visible-width", `${element.clientWidth}px`));
    observer.observe(element);
    return () => observer.disconnect();
  }, [hasDetail]);

  const sortedRows = useMemo(() => {
    const column = sort && columns.find((item) => item.key === sort.key);
    if (!column?.value) return rows;
    const direction = sort.dir === "asc" ? 1 : -1;
    return [...rows].sort((a, b) => {
      const va = column.value(a);
      const vb = column.value(b);
      // Blank values stay at the bottom in both directions.
      if (isBlank(va) || isBlank(vb)) return compareValues(va, vb);
      return compareValues(va, vb) * direction;
    });
  }, [rows, sort, columns]);

  const pageCount = Math.max(1, Math.ceil(sortedRows.length / pageSize));
  const currentPage = Math.min(page, pageCount - 1);
  const pageRows = sortedRows.slice(currentPage * pageSize, (currentPage + 1) * pageSize);
  const colSpan = columns.length + (actions ? 1 : 0);

  const toggleSort = (key) => {
    setSort((current) => current?.key === key ? { key, dir: current.dir === "asc" ? "desc" : "asc" } : { key, dir: "asc" });
    setPage(0);
  };

  const renderCell = (column, row) => {
    if (column.render) return column.render(row);
    const value = column.value ? column.value(row) : undefined;
    return isBlank(value) ? <span className="cell-empty">—</span> : String(value);
  };

  return (
    <div className="card table-card">
      {(title || headerActions) && (
        <div className="card-header">
          <div className="card-title">
            <h2>{title}</h2>
            <span className="count-badge">{loading ? "…" : formatNumber(rows.length)}</span>
            {subtitle && <p>{subtitle}</p>}
          </div>
          {headerActions}
        </div>
      )}
      <div className="table-scroll" ref={scrollRef}>
        <table className="data-table">
          <thead>
            <tr>
              {columns.map((column) => {
                const sorted = sort?.key === column.key;
                const sortable = column.sortable !== false && column.value;
                return (
                  <th key={column.key} className={[column.align && `align-${column.align}`].filter(Boolean).join(" ")} aria-sort={sorted ? (sort.dir === "asc" ? "ascending" : "descending") : undefined}>
                    {sortable ? (
                      <button type="button" className={`sort-button${sorted ? " is-sorted" : ""}`} onClick={() => toggleSort(column.key)}>
                        {column.label}{sorted && sort.dir === "desc" ? <FiChevronDown /> : <FiChevronUp />}
                      </button>
                    ) : column.label}
                  </th>
                );
              })}
              {actions && <th className="align-right actions-cell"><span className="sr-only">Acțiuni</span></th>}
            </tr>
          </thead>
          <tbody>
            {loading ? Array.from({ length: 5 }, (_, index) => (
              <tr key={index} className="skeleton-row">
                {Array.from({ length: colSpan }, (_, cell) => <td key={cell}><span className="skeleton" style={{ width: `${45 + ((index + cell) % 4) * 12}%` }} /></td>)}
              </tr>
            )) : pageRows.length === 0 ? (
              <tr><td colSpan={colSpan} className="table-state">
                <span className="table-state-icon">{filtered ? <FiSearch /> : <FiInbox />}</span>
                <strong>{filtered ? "Nu am găsit rezultate" : emptyTitle || "Nu există înregistrări"}</strong>
                <span>{filtered ? "Încearcă altă căutare sau elimină filtrele." : emptyText}</span>
              </td></tr>
            ) : pageRows.map((row) => {
              const key = rowKey(row);
              const expanded = renderDetail && expandedKeys?.has(key);
              return (
                <React.Fragment key={key}>
                  <tr className={expanded ? "is-expanded" : undefined}>
                    {columns.map((column) => (
                      <td key={column.key} className={[column.primary && "is-primary", column.nowrap && "is-nowrap", column.align && `align-${column.align}`].filter(Boolean).join(" ") || undefined}>
                        {renderCell(column, row)}
                      </td>
                    ))}
                    {actions && <td className="align-right actions-cell"><div className="row-actions">{actions(row)}</div></td>}
                  </tr>
                  {expanded && <tr className="detail-row"><td colSpan={colSpan}><div className="detail-inner">{renderDetail(row)}</div></td></tr>}
                </React.Fragment>
              );
            })}
          </tbody>
        </table>
      </div>
      <div className="table-footer">
        <span>{footer || <>Se afișează <strong>{pageRows.length ? currentPage * pageSize + 1 : 0}–{currentPage * pageSize + pageRows.length}</strong> din <strong>{formatNumber(sortedRows.length)}</strong></>}</span>
        {sortedRows.length > pageSizes[0] && (
          <div className="pagination">
            <select className="select" value={pageSize} onChange={(event) => { setPageSize(Number(event.target.value)); setPage(0); }} aria-label="Rânduri pe pagină">
              {pageSizes.map((size) => <option key={size} value={size}>{size} / pagină</option>)}
            </select>
            <button type="button" className="icon-action" onClick={() => setPage(currentPage - 1)} disabled={currentPage === 0} aria-label="Pagina anterioară"><FiChevronLeft /></button>
            <span className="pagination-label">{currentPage + 1} / {pageCount}</span>
            <button type="button" className="icon-action" onClick={() => setPage(currentPage + 1)} disabled={currentPage >= pageCount - 1} aria-label="Pagina următoare"><FiChevronRight /></button>
          </div>
        )}
      </div>
    </div>
  );
}
