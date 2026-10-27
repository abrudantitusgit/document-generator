export const readPath = (value, path) => path.split(".").reduce((current, key) => current == null ? undefined : current[key], value);

const toDate = (value) => {
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
};

export const formatDate = (value) => toDate(value)?.toLocaleDateString("ro-RO", { day: "2-digit", month: "2-digit", year: "numeric" }) || "";

export const formatNumber = (value, digits = 0) => {
  const number = Number(value);
  if (value === null || value === undefined || value === "" || !Number.isFinite(number)) return "";
  return number.toLocaleString("ro-RO", { minimumFractionDigits: digits, maximumFractionDigits: digits });
};

export const formatMoney = (value) => {
  const formatted = formatNumber(value, 2);
  return formatted ? `${formatted} lei` : "";
};

export const formatAddress = (record) => [
  record.localitatea,
  record.comuna && `com. ${record.comuna}`,
  record.strada && `str. ${record.strada}`,
  record.nrStrada && `nr. ${record.nrStrada}`,
  record.bloc && `bl. ${record.bloc}`,
  record.apartament && `ap. ${record.apartament}`,
].filter(Boolean).join(", ");

export const monthKey = (value) => {
  const date = toDate(value);
  return date ? `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}` : null;
};

const monthNames = ["ian", "feb", "mar", "apr", "mai", "iun", "iul", "aug", "sep", "oct", "nov", "dec"];
export const monthLabel = (key, withYear = false) => {
  const [year, month] = key.split("-");
  return `${monthNames[Number(month) - 1]}${withYear ? ` ${year}` : ""}`;
};

// The last `count` calendar months ending with the current one, oldest first.
export const lastMonths = (count) => {
  const now = new Date();
  return Array.from({ length: count }, (_, index) => {
    const date = new Date(now.getFullYear(), now.getMonth() - (count - 1 - index), 1);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}`;
  });
};

export const plural = (count, one, many) => `${formatNumber(count)} ${count === 1 ? one : many}`;

export const compareValues = (a, b) => {
  if (a == null || a === "") return b == null || b === "" ? 0 : 1;
  if (b == null || b === "") return -1;
  if (typeof a === "number" && typeof b === "number") return a - b;
  return String(a).localeCompare(String(b), "ro", { numeric: true, sensitivity: "base" });
};
