import React from "react";
import { Link } from "react-router-dom";
import { FiZap } from "react-icons/fi";
import DocumentGenerator from "../components/DocumentGenerator";
import { PageHeader } from "../components/ui";

export default function Word() {
  return (
    <section className="page">
      <PageHeader
        eyebrow="Documente"
        title="Generare documente branșament"
        description="Alege proiectul și documentele din grupul „Bransament”, apoi generează-le dintr-un singur pas."
        actions={<Link className="btn btn-secondary" to="/proiectfotovoltaice"><FiZap /> Documente fotovoltaice</Link>}
      />
      <DocumentGenerator groupName="Bransament" endpoint="/word/create" />
    </section>
  );
}
