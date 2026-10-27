const clientOptions = { optionsEndpoint: "/client/clienti", optionText: "nume", filterable: true };
const projectOptions = { optionsEndpoint: "/proiect/proiecteinclude", optionLabel: (project) => `${project.nrProiect || "—"} · ${project.denumire || project.strada || "Proiect"}`, filterable: true };
const groupOptions = { optionsEndpoint: "/grup/grupe", optionText: "nume", filterable: true };
const folderOptions = { optionsEndpoint: "/folder/foldere", optionLabel: (folder) => `${folder.nume || "Folder"}`, filterable: true };

export const resourceDefinitions = {
  client: {
    title: "Clienți", singular: "Client", plural: "Clienți", description: "Centralizează datele de contact și identificare ale clienților.", listEndpoint: "/client/clienti",
    importType: "romanianIdCard",
    create: { endpoint: "/client/create" }, update: { endpoint: "/client/modify" }, remove: { endpoint: (row) => `/client/${row.id}` },
    fields: [
      { name: "nume", label: "Nume complet", required: true }, { name: "cnp", label: "CNP", type: "number", required: true },
      { name: "telefon", label: "Telefon", type: "tel" }, { name: "seria", label: "Serie act" }, { name: "nrSeria", label: "Număr act" },
      { name: "dataBuletin", label: "Data emiterii", type: "date" }, { name: "eliberat", label: "Eliberat de" },
      { name: "judet", label: "Județ", filterable: true, filterByData: true }, { name: "localitatea", label: "Localitate", filterable: true, filterByData: true }, { name: "comuna", label: "Comună" },
      { name: "strada", label: "Stradă" }, { name: "nrStrada", label: "Număr" }, { name: "sector", label: "Sector" },
      { name: "bloc", label: "Bloc" }, { name: "scara", label: "Scară" }, { name: "etaj", label: "Etaj" },
      { name: "apartament", label: "Apartament" }, { name: "codPostal", label: "Cod poștal" },
    ],
    columns: [{ label: "Nume", path: "nume" }, { label: "CNP", path: "cnp" }, { label: "Telefon", path: "telefon" }, { label: "Localitate", path: "localitatea" }, { label: "Județ", path: "judet" }],
  },
  firma: {
    title: "Companii", singular: "Companie", plural: "Companii", description: "Gestionează firmele și datele lor de identificare fiscală.", listEndpoint: "/firma/list",
    create: { endpoint: "/firma/create" }, update: { endpoint: "/firma/modify" }, remove: { endpoint: (row) => `/firma/${row.id}` },
    fields: [
      { name: "clientID", label: "Reprezentant / client", type: "select", required: true, ...clientOptions }, { name: "nume", label: "Denumire companie", required: true },
      { name: "cui", label: "CUI", type: "number", required: true }, { name: "dataCUI", label: "Data înregistrării", type: "date", required: true }, { name: "orc", label: "Număr Registrul Comerțului", required: true },
      { name: "functia", label: "Funcția reprezentantului" }, { name: "telefon", label: "Telefon", type: "tel" },
      { name: "judet", label: "Județ", filterable: true, filterByData: true }, { name: "localitatea", label: "Localitate", filterable: true, filterByData: true }, { name: "comuna", label: "Comună" }, { name: "strada", label: "Stradă" },
      { name: "nrStrada", label: "Număr" }, { name: "sector", label: "Sector" }, { name: "bloc", label: "Bloc" }, { name: "scara", label: "Scară" },
      { name: "etaj", label: "Etaj" }, { name: "apartament", label: "Apartament" }, { name: "codPostal", label: "Cod poștal" },
    ],
    columns: [{ label: "Companie", path: "nume" }, { label: "CUI", path: "cui" }, { label: "Reprezentant", getValue: (row) => row.client?.nume || row.clientID }, { label: "Telefon", path: "telefon" }, { label: "Localitate", path: "localitatea" }],
  },
  proiect: {
    title: "Proiecte", singular: "Proiect", plural: "Proiecte", description: "Caută și administrează proiectele de branșament.", listEndpoint: "/proiect/proiecteinclude",
    create: { endpoint: "/proiect/create" }, update: { endpoint: "/proiect/modify" }, remove: { endpoint: (row) => `/proiect/${row.id}` },
    fields: [
      { name: "clientID", label: "Client", type: "select", required: true, ...clientOptions },
      { name: "firmaID", label: "Companie", type: "select", optionsEndpoint: "/firma/list", optionText: "nume", filterable: true },
      { name: "denumire", label: "Denumirea proiectului", required: true }, { name: "data", label: "Data proiectului", type: "date" },
      { name: "judet", label: "Județ", filterable: true, filterByData: true }, { name: "localitatea", label: "Localitate", filterable: true, filterByData: true }, { name: "comuna", label: "Comună" }, { name: "sector", label: "Sector" },
      { name: "strada", label: "Stradă" }, { name: "nrStrada", label: "Număr" }, { name: "bloc", label: "Bloc" }, { name: "scara", label: "Scară" },
      { name: "etaj", label: "Etaj" }, { name: "apartament", label: "Apartament" }, { name: "pi", label: "Putere instalată", type: "number", step: "0.01" },
      { name: "pa", label: "Putere absorbită", type: "number", step: "0.01" }, { name: "locConsum", label: "Loc de consum" }, { name: "tensiuneUtilizare", label: "Tensiune de utilizare" },
    ],
    columns: [{ label: "Nr. proiect", path: "nrProiect" }, { label: "Denumire", path: "denumire" }, { label: "Client", getValue: (row) => row.client?.nume || row.clientID }, { label: "Adresă", getValue: (row) => [row.localitatea, row.strada, row.nrStrada].filter(Boolean).join(", ") }, { label: "Data", path: "data", type: "date" }],
  },
  atr: {
    title: "ATR", singular: "ATR", plural: "ATR-uri", description: "Administrează avizele tehnice de racordare și datele aferente.", listEndpoint: "/atr/list",
    create: { endpoint: "/atr/create" }, update: { endpoint: "/atr/modify" }, remove: { endpoint: (row) => `/atr/${row.id}` },
    fields: [
      { name: "proiectID", label: "Proiect", type: "select", required: true, ...projectOptions }, { name: "nrAtr", label: "Număr ATR", type: "number", required: true }, { name: "dataAtr", label: "Data ATR", type: "date", required: true },
      { name: "nrCadastral", label: "Număr cadastral" }, { name: "codPostal", label: "Cod poștal" }, { name: "punctRacordare", label: "Punct de racordare" }, { name: "punctAlimentare", label: "Punct de alimentare" },
      { name: "punctBransament", label: "Punct de branșament" }, { name: "cablu", label: "Cablu" }, { name: "lungimeCablu", label: "Lungime cablu (m)", type: "number" }, { name: "ohmi", label: "Rezistență (Ω)", type: "number" },
      { name: "contor", label: "Contor" }, { name: "temperatura", label: "Temperatură", type: "number", step: "0.1" }, { name: "descriere", label: "Descriere", type: "textarea", wide: true },
      { name: "pret", label: "Preț", type: "number", step: "0.01" }, { name: "tva", label: "TVA (%)", type: "number", step: "0.01" },
    ],
    columns: [{ label: "Nr. ATR", path: "nrAtr" }, { label: "Proiect", path: "proiectID" }, { label: "Data", path: "dataAtr", type: "date" }, { label: "Punct de racordare", path: "punctRacordare" }, { label: "Cablu", path: "cablu" }],
  },
  grup: {
    title: "Grupuri de documente", singular: "Grup", plural: "Grupuri", description: "Organizează șabloanele și documentele pe categorii.", listEndpoint: "/grup/grupe",
    create: { endpoint: "/grup/create" }, update: { endpoint: "/grup/edit", method: "PUT" }, remove: { endpoint: (row) => `/grup/${row.id}` },
    fields: [{ name: "nume", label: "Numele grupului", required: true }],
    columns: [{ label: "Nume", path: "nume" }, { label: "Foldere", getValue: (row) => row.foldere?.length ?? 0 }, { label: "Identificator", path: "id" }],
  },
  folder: {
    title: "Foldere", singular: "Folder", plural: "Foldere", description: "Organizează fișierele în foldere și grupuri.", listEndpoint: "/folder/foldere",
    create: { endpoint: "/folder/create" }, update: { endpoint: (row) => `/folder/${row.id}`, method: "PUT" }, remove: { endpoint: (row) => `/folder/${row.id}` },
    fields: [{ name: "grupID", label: "Grup", type: "select", required: true, ...groupOptions }, { name: "nume", label: "Numele folderului", required: true }],
    columns: [{ label: "Folder", path: "nume" }, { label: "Grup", path: "grupID" }, { label: "Fișiere", getValue: (row) => row.fisiere?.length ?? 0 }],
  },
  fisier: {
    title: "Fișiere", singular: "Fișier", plural: "Fișiere", description: "Gestionează fișierele și șabloanele disponibile pentru generare.", listEndpoint: "/fisier/list",
    create: { endpoint: "/fisier/create" }, update: { endpoint: (row) => `/fisier/${row.id}`, method: "PUT" }, remove: { endpoint: (row) => `/fisier/${row.id}` },
    fields: [{ name: "folderID", label: "Folder", type: "select", required: true, ...folderOptions }, { name: "nume", label: "Numele fișierului", required: true }, { name: "denumireContor", label: "Denumire contor / tip document", required: true }],
    columns: [{ label: "Fișier", path: "nume" }, { label: "Folder", path: "folderID" }, { label: "Tip document", path: "denumireContor" }],
  },
  email: {
    title: "E-mailuri programate", singular: "E-mail", plural: "E-mailuri", description: "Gestionează mesajele care urmează să fie trimise automat.", listEndpoint: "/email/list",
    create: { endpoint: "/email/create" }, update: { endpoint: (row) => `/email/${row.id}`, method: "PUT" }, remove: { endpoint: (row) => `/email/${row.id}` },
    fields: [{ name: "adresa", label: "Adresă e-mail", type: "email", required: true }, { name: "data", label: "Data trimiterii", type: "datetime-local", required: true }, { name: "activ", label: "Activ", type: "checkbox", defaultValue: true, filterable: true, options: [{ id: "true", nume: "Activ" }, { id: "false", nume: "Inactiv" }] }, { name: "text", label: "Mesaj", type: "textarea", required: true, wide: true }],
    columns: [{ label: "Destinatar", path: "adresa" }, { label: "Mesaj", path: "text" }, { label: "Data trimiterii", path: "data", type: "date" }, { label: "Stare", path: "activ", type: "boolean" }],
  },
  contor: {
    title: "Numere de registru", singular: "Număr de registru", plural: "Numere de registru", description: "Alocă și corectează numerele documentelor asociate proiectelor.", listEndpoint: "/proiectcontor/records",
    create: { endpoint: "/proiectcontor/records" }, update: { endpoint: (row) => `/proiectcontor/records/${row.type}/${row.id}`, method: "PUT" }, remove: { endpoint: (row) => `/proiectcontor/records/${row.type}/${row.id}` },
    fields: [
      { name: "type", label: "Tip document", type: "select", required: true, options: [{ id: "cablu", nume: "Buletin cablu" }, { id: "priza", nume: "Buletin priză" }, { id: "contract", nume: "Contract" }, { id: "procesVerbal", nume: "Proces verbal" }], filterable: true },
      { name: "proiectID", label: "Proiect", type: "select", required: true, ...projectOptions },
      { name: "contor", label: "Număr alocat", type: "number", required: true, readOnlyOnCreate: true },
      { name: "tag", label: "Descriere / categorie" },
    ],
    columns: [{ label: "Număr", path: "contor" }, { label: "Tip document", path: "typeLabel" }, { label: "Proiect", getValue: (row) => row.nrProiect || row.proiectID }, { label: "Descriere", path: "tag" }],
  },
};
