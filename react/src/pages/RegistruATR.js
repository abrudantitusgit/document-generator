import React, { useCallback, useEffect, useMemo, useState } from "react";
import * as FileSaver from "file-saver";
import * as XLSX from "xlsx";
import { FiCheckCircle, FiCreditCard, FiDownload, FiFileText, FiFolder, FiPrinter, FiRefreshCw } from "react-icons/fi";
import { apiList } from "../lib/api";
import { formatAddress, formatDate, formatMoney, formatNumber } from "../lib/format";
import { DataTable, FilterSelect, Notice, PageHeader, SearchInput, StatCard, StatGrid, Toolbar, distinctOptions } from "../components/ui";

const yearOf = (value) => value ? String(new Date(value).getFullYear()) : "";
const numbers = (items) => (items || []).map((item) => item.contor).filter((value) => value != null);
const documentCount = (row) => (row.contorContract ? 1 : 0) + (row.contorProcesVerbal ? 1 : 0) + (row.contorBuletinCabluri?.length || 0) + (row.contorBuletinPrize?.length || 0);

const NumberList = ({ values }) => values.length ? <span className="number-list">{values.map((value, index) => <span key={`${value}-${index}`} className="tag">{value}</span>)}</span> : <span className="cell-empty">—</span>;

export default function RegistruATR() {
  const [rows, setRows] = useState([]);
  const [exportRows, setExportRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [search, setSearch] = useState("");
  const [locality, setLocality] = useState("");
  const [year, setYear] = useState("");
  const [atrStatus, setAtrStatus] = useState("");

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [projects, registry] = await Promise.all([apiList("/proiect/proiecteinclude"), apiList("/proiect/registru")]);
      setRows(projects);
      setExportRows(registry);
    } catch (loadError) {
      setError(loadError.message || "Registrul nu a putut fi încărcat.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const visible = useMemo(() => {
    const query = search.trim().toLocaleLowerCase("ro");
    return rows.filter((row) => {
      if (locality && row.localitatea !== locality) return false;
      if (year && yearOf(row.data) !== year) return false;
      if (atrStatus === "cu" && !row.atr) return false;
      if (atrStatus === "fara" && row.atr) return false;
      if (!query) return true;
      return [row.nrProiect, row.client?.nume, row.firma?.nume, row.denumire, row.atr?.nrAtr, formatAddress(row)]
        .filter((value) => value != null).join(" ").toLocaleLowerCase("ro").includes(query);
    });
  }, [rows, search, locality, year, atrStatus]);

  const totals = useMemo(() => ({
    withAtr: visible.filter((row) => row.atr).length,
    value: visible.reduce((sum, row) => sum + Number(row.atr?.pret || 0), 0),
    documents: visible.reduce((sum, row) => sum + documentCount(row), 0),
  }), [visible]);

  const filtered = Boolean(search || locality || year || atrStatus);

  // Export uses the API's flat registry rows, limited to the projects on screen.
  const exportExcel = () => {
    const shown = new Set(visible.map((row) => String(row.nrProiect)));
    const data = filtered ? exportRows.filter((row) => shown.has(String(row.nrProiect))) : exportRows;
    const sheet = XLSX.utils.json_to_sheet(data);
    const workbook = { Sheets: { Clienti: sheet }, SheetNames: ["Clienti"] };
    const buffer = XLSX.write(workbook, { bookType: "xlsx", type: "array" });
    FileSaver.saveAs(new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8" }), `Registru Clienti - ${formatDate(new Date())}.xlsx`);
  };

  const columns = useMemo(() => [
    { key: "nr", label: "Nr. proiect", value: (row) => row.nrProiect, render: (row) => <span className="tag">{row.nrProiect ?? "—"}</span> },
    { key: "client", label: "Beneficiar", primary: true, value: (row) => row.client?.nume, render: (row) => <>{row.client?.nume || "—"}{row.firma && <span className="cell-sub">{row.firma.nume}</span>}</> },
    { key: "atr", label: "ATR", nowrap: true, value: (row) => row.atr?.nrAtr, render: (row) => row.atr ? <>{row.atr.nrAtr}<span className="cell-sub">{formatDate(row.atr.dataAtr)}</span></> : <span className="pill neutral">Fără ATR</span> },
    { key: "denumire", label: "Denumire", value: (row) => row.denumire },
    { key: "adresa", label: "Adresă", value: (row) => formatAddress(row), render: (row) => <span className="cell-clip" title={formatAddress(row)}>{formatAddress(row) || "—"}</span> },
    { key: "data", label: "Data", nowrap: true, value: (row) => row.data ? new Date(row.data).getTime() : null, render: (row) => formatDate(row.data) || <span className="cell-empty">—</span> },
    { key: "pret", label: "Preț ATR", align: "right", nowrap: true, value: (row) => row.atr?.pret, render: (row) => row.atr?.pret != null ? formatMoney(row.atr.pret) : <span className="cell-empty">—</span> },
    { key: "contract", label: "Contract", align: "center", value: (row) => row.contorContract?.contor, render: (row) => <NumberList values={numbers(row.contorContract ? [row.contorContract] : [])} /> },
    { key: "pv", label: "Proces verbal", align: "center", value: (row) => row.contorProcesVerbal?.contor, render: (row) => <NumberList values={numbers(row.contorProcesVerbal ? [row.contorProcesVerbal] : [])} /> },
    { key: "pvCablu", label: "PV cablu", align: "center", value: (row) => row.contorProcesVerbal?.cablu },
    { key: "cablu", label: "Buletine cablu", render: (row) => <NumberList values={numbers(row.contorBuletinCabluri)} /> },
    { key: "priza", label: "Buletine priză", render: (row) => <NumberList values={numbers(row.contorBuletinPrize)} /> },
  ], []);

  return (
    <section className="page report-page">
      <PageHeader
        eyebrow="Rapoarte"
        title="Registru ATR"
        description="Proiectele de branșament cu avizele ATR și numerele documentelor emise."
        actions={<>
          <button type="button" className="btn btn-secondary btn-icon no-print" onClick={load} disabled={loading} aria-label="Reîncarcă" title="Reîncarcă"><FiRefreshCw /></button>
          <button type="button" className="btn btn-secondary no-print" onClick={() => window.print()}><FiPrinter /> Printează</button>
          <button type="button" className="btn btn-primary no-print" onClick={exportExcel} disabled={loading || !exportRows.length}><FiDownload /> Export Excel{filtered ? " (filtrat)" : ""}</button>
        </>}
      />

      {error && <Notice type="error" onClose={() => setError(null)}>{error}</Notice>}

      <StatGrid>
        <StatCard label="Proiecte" value={formatNumber(visible.length)} loading={loading} icon={FiFolder} tone="green" foot={filtered ? `din ${formatNumber(rows.length)} în total` : "Toate proiectele de branșament"} />
        <StatCard label="Cu aviz ATR" value={formatNumber(totals.withAtr)} loading={loading} icon={FiCheckCircle} tone="blue" foot={visible.length ? `${Math.round((totals.withAtr / visible.length) * 100)}% din proiecte` : "—"} />
        <StatCard label="Valoare ATR" value={formatNumber(totals.value, 2)} unit="lei" loading={loading} icon={FiCreditCard} tone="violet" foot="Suma prețurilor din avize" />
        <StatCard label="Documente numerotate" value={formatNumber(totals.documents)} loading={loading} icon={FiFileText} tone="amber" foot="Contracte, procese verbale, buletine" />
      </StatGrid>

      <Toolbar filters={<>
        <FilterSelect label="Localitate" value={locality} onChange={setLocality} options={distinctOptions(rows, (row) => row.localitatea)} />
        <FilterSelect label="An" value={year} onChange={setYear} options={distinctOptions(rows, (row) => yearOf(row.data)).reverse()} />
        <FilterSelect label="ATR" value={atrStatus} onChange={setAtrStatus} options={[{ value: "cu", label: "Cu ATR" }, { value: "fara", label: "Fără ATR" }]} />
      </>}>
        <SearchInput value={search} onChange={setSearch} placeholder="Caută după nr. proiect, client, ATR, adresă…" />
      </Toolbar>

      <DataTable
        title="Registru"
        subtitle={filtered ? "rezultate filtrate" : undefined}
        columns={columns}
        rows={visible}
        rowKey={(row) => row.id}
        loading={loading}
        filtered={filtered}
        initialSort={{ key: "nr", dir: "desc" }}
        pageSizes={[50, 100, 500]}
        emptyTitle="Registrul este gol"
        emptyText="Proiectele de branșament adăugate vor apărea aici."
      />
    </section>
  );
}
