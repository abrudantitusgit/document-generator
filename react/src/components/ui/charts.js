import React, { useEffect, useRef, useState } from "react";
import { formatNumber } from "../../lib/format";

// Round an axis maximum up to 1, 2, 2.5 or 5 × 10^n so gridlines land on clean values.
const niceMax = (value) => {
  if (value <= 0) return 4;
  const magnitude = 10 ** Math.floor(Math.log10(value));
  const step = [1, 2, 2.5, 5, 10].find((candidate) => candidate * magnitude >= value);
  return step * magnitude;
};

// Integer data (counts) gets a maximum divisible by the 4 grid steps.
const integerMax = (value) => Math.max(4, Math.ceil(value / 4) * 4);

// Track the rendered width so the SVG is drawn 1:1 and its text never scales.
function useWidth(fallback) {
  const ref = useRef(null);
  const [width, setWidth] = useState(fallback);
  useEffect(() => {
    const element = ref.current;
    if (!element) return undefined;
    const update = () => { if (element.clientWidth) setWidth(element.clientWidth); };
    update();
    if (typeof ResizeObserver === "undefined") return undefined;
    const observer = new ResizeObserver(update);
    observer.observe(element);
    return () => observer.disconnect();
  }, []);
  return [ref, width];
}

// A bar with only its top corners rounded, anchored to the baseline.
const topRoundedBar = (x, y, width, height, radius) => {
  const r = Math.min(radius, width / 2, height);
  return `M${x},${y + height}V${y + r}Q${x},${y} ${x + r},${y}H${x + width - r}Q${x + width},${y} ${x + width},${y + r}V${y + height}Z`;
};

export function ChartLegend({ series }) {
  return (
    <div className="chart-legend">
      {series.map((item) => <span key={item.name} className="legend-item"><span className="legend-swatch" style={{ background: item.color }} />{item.name}</span>)}
    </div>
  );
}

/**
 * Stacked column chart. data: [{ key, label, tooltipLabel, values: [n, ...] }],
 * series: [{ name, color }] in the same order as values.
 */
export function ColumnChart({ data, series, format = (value) => formatNumber(value), integer = false, height = 240 }) {
  const [hover, setHover] = useState(null);
  const [wrapRef, width] = useWidth(640);
  const padding = { top: 12, right: 8, bottom: 26, left: 40 };
  const plotWidth = width - padding.left - padding.right;
  const plotHeight = height - padding.top - padding.bottom;
  const totals = data.map((item) => item.values.reduce((sum, value) => sum + (value || 0), 0));
  const rawMax = Math.max(...totals, 0);
  const max = integer ? integerMax(rawMax) : niceMax(rawMax);
  const slot = plotWidth / Math.max(data.length, 1);
  const barWidth = Math.max(4, Math.min(32, slot * 0.56));
  // Skip every other month label when slots get too narrow to fit them.
  const labelEvery = slot < 34 ? 2 : 1;
  const y = (value) => padding.top + plotHeight - (value / max) * plotHeight;
  const ticks = [0, 0.25, 0.5, 0.75, 1].map((ratio) => max * ratio);

  return (
    <div className="chart-wrap" ref={wrapRef} onMouseLeave={() => setHover(null)}>
      <svg className="chart-svg" width={width} height={height} viewBox={`0 0 ${width} ${height}`} role="img" aria-label={`Grafic: ${series.map((item) => item.name).join(", ")}`}>
        {ticks.map((tick) => (
          <g key={tick}>
            <line className="grid-line" x1={padding.left} x2={width - padding.right} y1={y(tick)} y2={y(tick)} />
            <text className="axis-label" x={padding.left - 8} y={y(tick)} dy="0.32em" textAnchor="end">{tick.toLocaleString("ro-RO", { maximumFractionDigits: 2 })}</text>
          </g>
        ))}
        {data.map((item, index) => {
          const x = padding.left + slot * index + (slot - barWidth) / 2;
          let stackTop = y(0);
          const segments = item.values.map((value, seriesIndex) => ({ value, seriesIndex })).filter((segment) => segment.value > 0);
          return (
            <g key={item.key} className="bar-group" onMouseEnter={() => setHover(index)}>
              <rect className="bar-hit" x={padding.left + slot * index} y={padding.top} width={slot} height={plotHeight} rx="4" />
              {segments.map((segment, segmentIndex) => {
                const barHeight = (segment.value / max) * plotHeight;
                const isTop = segmentIndex === segments.length - 1;
                // 2px surface gap between stacked segments.
                const gap = segmentIndex > 0 ? 2 : 0;
                const top = stackTop - barHeight;
                const shape = isTop
                  ? <path key={segment.seriesIndex} d={topRoundedBar(x, top, barWidth, Math.max(barHeight - gap, 1), 4)} fill={series[segment.seriesIndex].color} />
                  : <rect key={segment.seriesIndex} x={x} y={top} width={barWidth} height={Math.max(barHeight - gap, 1)} fill={series[segment.seriesIndex].color} />;
                stackTop = top;
                return shape;
              })}
              {(data.length - 1 - index) % labelEvery === 0 && <text className="axis-label" x={x + barWidth / 2} y={height - 8} textAnchor="middle">{item.label}</text>}
            </g>
          );
        })}
      </svg>
      {hover !== null && data[hover] && (
        <div className="chart-tooltip" style={{ left: `${((padding.left + slot * hover + slot / 2) / width) * 100}%`, top: `${(y(totals[hover]) / height) * 100}%` }}>
          <strong>{data[hover].tooltipLabel || data[hover].label}</strong>
          {series.map((item, seriesIndex) => (
            <div key={item.name} className="tooltip-row"><span><span className="legend-swatch" style={{ background: item.color }} />{item.name}</span><b>{format(data[hover].values[seriesIndex] || 0)}</b></div>
          ))}
          {series.length > 1 && <div className="tooltip-row"><span>Total</span><b>{format(totals[hover])}</b></div>}
        </div>
      )}
    </div>
  );
}

/** Horizontal stacked bars with the total printed at the end of each row. */
export function HBarList({ rows, series, format = (value) => formatNumber(value) }) {
  const max = Math.max(...rows.map((row) => row.values.reduce((sum, value) => sum + value, 0)), 1);
  return (
    <ul className="hbar-list">
      {rows.map((row) => {
        const total = row.values.reduce((sum, value) => sum + value, 0);
        return (
          <li key={row.label} className="hbar-row" title={series.map((item, index) => `${item.name}: ${format(row.values[index])}`).join(" · ")}>
            <span className="hbar-label">{row.label}</span>
            <span className="hbar-track">
              {row.values.map((value, index) => value > 0 && <span key={series[index].name} className="hbar-seg" style={{ width: `${(value / max) * 100}%`, background: series[index].color }} />)}
            </span>
            <span className="hbar-value">{format(total)}</span>
          </li>
        );
      })}
    </ul>
  );
}

/** Card wrapper that offers the same data as a chart or as a table. */
export function ChartCard({ title, description, series, table, children }) {
  const [view, setView] = useState("chart");
  return (
    <section className="card chart-card">
      <div className="card-header">
        <div className="card-title"><div><h3>{title}</h3>{description && <p>{description}</p>}</div></div>
        <div className="segmented" role="group" aria-label="Mod de afișare">
          <button type="button" className={view === "chart" ? "is-active" : ""} onClick={() => setView("chart")} aria-pressed={view === "chart"}>Grafic</button>
          <button type="button" className={view === "table" ? "is-active" : ""} onClick={() => setView("table")} aria-pressed={view === "table"}>Tabel</button>
        </div>
      </div>
      <div className="card-body">
        {view === "chart" ? <>{series && series.length > 1 && <ChartLegend series={series} />}<div className="chart-space">{children}</div></> : table}
      </div>
    </section>
  );
}

export function ChartTable({ head, rows }) {
  return (
    <div className="table-scroll">
      <table className="mini-table">
        <thead><tr>{head.map((label, index) => <th key={label} className={index ? "align-right" : undefined}>{label}</th>)}</tr></thead>
        <tbody>{rows.map((row) => <tr key={row[0]}>{row.map((cell, index) => <td key={index} className={index ? "align-right" : undefined}>{cell}</td>)}</tr>)}</tbody>
      </table>
    </div>
  );
}
