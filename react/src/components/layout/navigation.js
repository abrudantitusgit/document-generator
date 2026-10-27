import {
  FiArchive, FiBarChart2, FiBookOpen, FiBriefcase, FiClipboard, FiFile, FiFilePlus,
  FiFolder, FiHash, FiHome, FiLayers, FiMail, FiUsers, FiZap,
} from "react-icons/fi";

export const navigation = [
  { title: "Spațiu de lucru", items: [
    { label: "Acasă", path: "/", icon: FiHome },
  ] },
  { title: "Date", items: [
    { label: "Clienți", path: "/client", icon: FiUsers },
    { label: "Companii", path: "/firma", icon: FiBriefcase },
  ] },
  { title: "Proiecte", items: [
    { label: "Proiecte", path: "/proiect", icon: FiFolder },
    { label: "ATR", path: "/atr", icon: FiClipboard },
    { label: "Proiecte fotovoltaice", path: "/proiectfotovoltaice", icon: FiZap, aliases: ["/registrufotovoltaice"] },
  ] },
  { title: "Documente", items: [
    { label: "Generare documente", path: "/word", icon: FiFilePlus },
    { label: "Numere de registru", path: "/clientcontor", icon: FiHash },
    { label: "E-mailuri", path: "/email", icon: FiMail },
  ] },
  { title: "Rapoarte", items: [
    { label: "Privire de ansamblu", path: "/rapoarte", icon: FiBarChart2 },
    { label: "Registru ATR", path: "/registruatr", icon: FiBookOpen },
  ] },
  { title: "Șabloane", items: [
    { label: "Grupuri", path: "/grup", icon: FiLayers },
    { label: "Foldere", path: "/folder", icon: FiArchive },
    { label: "Fișiere", path: "/fisier", icon: FiFile },
  ] },
];

export const isActivePath = (item, pathname) => item.path === pathname || (item.aliases || []).includes(pathname);

export const findPage = (pathname) => {
  for (const group of navigation) {
    const item = group.items.find((entry) => isActivePath(entry, pathname));
    if (item) return { group: group.title, label: item.label };
  }
  return { group: "Spațiu de lucru", label: "Acasă" };
};
