import React, { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { FiBookOpen, FiCpu, FiCreditCard, FiFolder, FiPrinter, FiRefreshCw, FiSun, FiZap } from "react-icons/fi";
import { apiList } from "../lib/api";
import { formatMoney, formatNumber, lastMonths, monthKey, monthLabel, plural } from "../lib/format";
import { ChartCard, ChartTable, ColumnChart, HBarList, Notice, PageHeader, StatCard, StatGrid } from "../components/ui";

const SERIES = {
  branch: { name: "Branșament", color: "var(--series-1)" },
  pv: { name: "Fotovoltaic", color: "var(--series-2)" },
};
const periods = [6, 12, 24];
const kwp = (project) => Number(project.putereTotalaPanouri || 0) / 1000;

export default function Rapoarte() {
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);
  const [months, setMonths] = useState(12);

  const load = useCallback(() => {
    setData(null);
    setError(null);
    Promise.all([apiList("/proiect/proiecteinclude"), apiList("/proiectfotovoltaice"), apiList("/atr/list")])
      .then(([projects, pv, atr]) => setData({ projects, pv, atr }))
      .catch((loadError) => { setError(loadError.message); setData({ projects: [], pv: [], atr: [] }); });
  }, []);
  useEffect(() => { load(); }, [load]);

  const report = useMemo(() => {
    if (!data) return null;
    const keys = lastMonths(months);
    const inPeriod = new Set(keys);
    const branch = data.projects.filter((item) => inPeriod.has(monthKey(item.data)));
    const pv = data.pv.filter((item) => inPeriod.has(monthKey(item.data)));
    const atr = data.atr.filter((item) => inPeriod.has(monthKey(item.dataAtr)));
    const byMonth = (rows, dateOf, valueOf = () => 1) => keys.map((key) => rows.filter((row) => monthKey(dateOf(row)) === key).reduce((sum, row) => sum + valueOf(row), 0));

    const branchMonthly = byMonth(branch, (row) => row.data);
    const pvMonthly = byMonth(pv, (row) => row.data);
    const kwpMonthly = byMonth(pv, (row) => row.data, kwp);
    const atrMonthly = byMonth(atr, (row) => row.dataAtr, (row) => Number(row.pret || 0));

    const counties = {};
    branch.forEach((row) => { const key = row.judet || "Nespecificat"; counties[key] = counties[key] || [0, 0]; counties[key][0] += 1; });
    pv.forEach((row) => { const key = row.judet || "Nespecificat"; counties[key] = counties[key] || [0, 0]; counties[key][1] += 1; });
    let countyRows = Object.entries(counties).map(([label, values]) => ({ label, values })).sort((a, b) => (b.values[0] + b.values[1]) - (a.values[0] + a.values[1]));
    if (countyRows.length > 8) {
      const rest = countyRows.slice(7).reduce((sum, row) => [sum[0] + row.values[0], sum[1] + row.values[1]], [0, 0]);
      countyRows = [...countyRows.slice(0, 7), { label: "Alte județe", values: rest }];
    }

    const systems = {};
    pv.forEach((row) => { const key = row.tipSistem || "Nespecificat"; systems[key] = (systems[key] || 0) + 1; });

    return {
      keys,
      branch, pv, atr,
      monthly: keys.map((key, index) => ({ key, label: monthLabel(key), tooltipLabel: monthLabel(key, true), values: [branchMonthly[index], pvMonthly[index]] })),
      kwp: keys.map((key, index) => ({ key, label: monthLabel(key), tooltipLabel: monthLabel(key, true), values: [kwpMonthly[index]] })),
      atrValue: keys.map((key, index) => ({ key, label: monthLabel(key), tooltipLabel: monthLabel(key, true), values: [atrMonthly[index]] })),
      counties: countyRows,
      systems: Object.entries(systems).map(([label, value]) => ({ label, values: [value] })).sort((a, b) => b.values[0] - a.values[0]),
      totals: {
        projects: branch.length + pv.length,
        kwp: pv.reduce((sum, row) => sum + kwp(row), 0),
        inverter: pv.reduce((sum, row) => sum + Number(row.putereTotalaInvertor || 0), 0),
        atrValue: atr.reduce((sum, row) => sum + Number(row.pret || 0), 0),
      },
    };
  }, [data, months]);

  const loading = !report;
  const periodLabel = `ultimele ${months} luni`;
  const monthHead = (label) => ["Lună", label];

  return (
    <section className="page report-page">
      <PageHeader
        eyebrow="Rapoarte"
        title="Privire de ansamblu"
        description={`Activitatea pe ${periodLabel}: proiecte noi, putere fotovoltaică instalată și valoarea avizelor ATR.`}
        actions={<>
          <div className="segmented" role="group" aria-label="Perioadă">
            {periods.map((value) => <button key={value} type="button" className={months === value ? "is-active" : ""} aria-pressed={months === value} onClick={() => setMonths(value)}>{value} luni</button>)}
          </div>
          <button type="button" className="btn btn-secondary btn-icon no-print" onClick={load} aria-label="Reîncarcă datele" title="Reîncarcă"><FiRefreshCw /></button>
          <button type="button" className="btn btn-secondary no-print" onClick={() => window.print()}><FiPrinter /> Printează</button>
        </>}
      />

      {error && <Notice type="error">{error}</Notice>}

      <StatGrid>
        <StatCard label="Proiecte noi" value={formatNumber(report?.totals.projects)} loading={loading} icon={FiFolder} tone="green" foot={report && `${report.branch.length} branșament · ${report.pv.length} fotovoltaice`} />
        <StatCard label="Putere panouri instalată" value={formatNumber(report?.totals.kwp, 2)} unit="kWp" loading={loading} icon={FiSun} tone="orange" foot={report && plural(report.pv.length, "sistem fotovoltaic", "sisteme fotovoltaice")} />
        <StatCard label="Putere invertoare" value={formatNumber(report?.totals.inverter, 2)} unit="kW" loading={loading} icon={FiCpu} tone="amber" foot="Însumată pe toate proiectele PV" />
        <StatCard label="Valoare ATR" value={formatNumber(report?.totals.atrValue, 2)} unit="lei" loading={loading} icon={FiCreditCard} tone="blue" foot={report && plural(report.atr.length, "aviz emis", "avize emise")} />
      </StatGrid>

      {report && (
        <div className="report-grid">
          <div className="span-2">
            <ChartCard
              title="Proiecte noi pe lună"
              description={`Branșament și fotovoltaic, ${periodLabel}`}
              series={[SERIES.branch, SERIES.pv]}
              table={<ChartTable head={["Lună", "Branșament", "Fotovoltaic", "Total"]} rows={report.monthly.map((row) => [row.tooltipLabel, row.values[0], row.values[1], row.values[0] + row.values[1]])} />}
            >
              <ColumnChart data={report.monthly} series={[SERIES.branch, SERIES.pv]} integer />
            </ChartCard>
          </div>

          <ChartCard
            title="Putere fotovoltaică instalată"
            description="kWp panouri, pe luna proiectului"
            table={<ChartTable head={monthHead("kWp")} rows={report.kwp.map((row) => [row.tooltipLabel, formatNumber(row.values[0], 2)])} />}
          >
            <ColumnChart data={report.kwp} series={[{ ...SERIES.pv, name: "kWp instalați" }]} format={(value) => `${formatNumber(value, 2)} kWp`} />
          </ChartCard>

          <ChartCard
            title="Valoare ATR pe lună"
            description="Suma prețurilor ATR, după data avizului"
            table={<ChartTable head={monthHead("Valoare")} rows={report.atrValue.map((row) => [row.tooltipLabel, formatMoney(row.values[0]) || "0,00 lei"])} />}
          >
            <ColumnChart data={report.atrValue} series={[{ ...SERIES.branch, name: "Valoare ATR" }]} format={(value) => formatMoney(value) || "0 lei"} />
          </ChartCard>

          <ChartCard
            title="Proiecte pe județe"
            description="Primele județe după numărul de proiecte"
            series={[SERIES.branch, SERIES.pv]}
            table={<ChartTable head={["Județ", "Branșament", "Fotovoltaic", "Total"]} rows={report.counties.map((row) => [row.label, row.values[0], row.values[1], row.values[0] + row.values[1]])} />}
          >
            {report.counties.length ? <HBarList rows={report.counties} series={[SERIES.branch, SERIES.pv]} /> : <p className="muted">Nu există proiecte în perioada aleasă.</p>}
          </ChartCard>

          <ChartCard
            title="Sisteme fotovoltaice după tip"
            description="Numărul de sisteme pe fiecare tip"
            table={<ChartTable head={["Tip sistem", "Proiecte"]} rows={report.systems.map((row) => [row.label, row.values[0]])} />}
          >
            {report.systems.length ? <HBarList rows={report.systems} series={[SERIES.pv]} /> : <p className="muted">Nu există proiecte fotovoltaice în perioada aleasă.</p>}
          </ChartCard>
        </div>
      )}

      <div className="report-links no-print">
        <Link to="/registruatr" className="card report-link"><span className="stat-icon tone-blue"><FiBookOpen /></span><span className="recent-copy"><strong>Registru ATR</strong><span>Toate proiectele de branșament, cu numerele documentelor și export Excel</span></span></Link>
        <Link to="/proiectfotovoltaice" className="card report-link"><span className="stat-icon tone-orange"><FiZap /></span><span className="recent-copy"><strong>Registru fotovoltaic</strong><span>Proiecte PV cu echipamente, PRM și export Excel</span></span></Link>
      </div>
    </section>
  );
}
