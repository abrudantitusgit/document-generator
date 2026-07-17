import * as XLSX from "xlsx";
import { saveAs } from "file-saver";
import { baseUrl } from "../config/api";

export async function exportProiecteFotovoltaiceExcel() {
  const res = await fetch(`${baseUrl}/proiectfotovoltaice`);
  if (!res.ok) throw new Error(await res.text());
  const proiecte = await res.json();

  const rows = [];

  proiecte.forEach((p) => {
    const baseData = {
      "Nr Proiect": p.nrProiect ?? "",
      "Nr PV": p.nrPV ?? "",
      "Data": p.data ? new Date(p.data).toLocaleDateString() : "",
      "Client": p.client?.nume ?? "",
      "Firma": p.firma?.denumire ?? "",
      "Judet": p.judet ?? "",
      "Tip Sistem": p.tipSistem ?? "",
    };

    // PANOURI
    (p.datePanouri ?? []).forEach((x) => {
      rows.push({
        ...baseData,
        "Tip Echipament": "Panou",
        "Model": x.model ?? "",
        "Cantitate": x.nrPanouri ?? 0,
        "Putere (kW)": x.putereInstalata ?? 0,
        "Capacitate (Ah)": "",
        "Serie": x.serie ?? "",
      });
    });

    // INVERTOARE
    (p.dateInvertoare ?? []).forEach((x) => {
      rows.push({
        ...baseData,
        "Tip Echipament": "Invertor",
        "Model": x.model ?? "",
        "Cantitate": x.nrInvertoare ?? 0,
        "Putere (kW)": x.putereInstalata ?? 0,
        "Capacitate (Ah)": "",
        "Serie": x.serie ?? "",
      });
    });

    // BATERII
    (p.dateBaterii ?? []).forEach((x) => {
      rows.push({
        ...baseData,
        "Tip Echipament": "Baterie",
        "Model": x.model ?? "",
        "Cantitate": x.nrBaterii ?? 0,
        "Putere (kW)": x.putereInstalata ?? 0,
        "Capacitate (Ah)": x.capacitate ?? 0,
        "Serie": x.serie ?? "",
      });
    });

    // dacă un proiect nu are echipamente
    if (
      (!p.datePanouri || p.datePanouri.length === 0) &&
      (!p.dateInvertoare || p.dateInvertoare.length === 0) &&
      (!p.dateBaterii || p.dateBaterii.length === 0)
    ) {
      rows.push({
        ...baseData,
        "Tip Echipament": "",
        "Model": "",
        "Cantitate": "",
        "Putere (kW)": "",
        "Capacitate (Ah)": "",
        "Serie": "",
      });
    }
  });

  const ws = XLSX.utils.json_to_sheet(rows);
  const wb = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(wb, ws, "Proiecte");

  const excelBuffer = XLSX.write(wb, { bookType: "xlsx", type: "array" });

  const blob = new Blob([excelBuffer], {
    type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  });

  saveAs(blob, "ProiecteFotovoltaice.xlsx");
}