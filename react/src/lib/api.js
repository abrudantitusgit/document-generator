import { baseUrl } from "../config/api";

// Responses can be empty, JSON, or a plain-text error from ASP.NET.
export const readJson = async (response) => {
  const content = await response.text();
  if (!content) return null;
  try { return JSON.parse(content); } catch { return content; }
};

export const humanizeError = (body, fallback) => body?.message || body?.title || (typeof body === "string" && body.length < 400 ? body : fallback);

export async function apiRequest(endpoint, { method = "GET", body, fallbackError = "Cererea nu a reușit." } = {}) {
  const response = await fetch(`${baseUrl}${endpoint}`, {
    method,
    headers: body === undefined ? { Accept: "application/json" } : { "Content-Type": "application/json", Accept: "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const result = await readJson(response);
  if (!response.ok) throw new Error(humanizeError(result, fallbackError));
  return result;
}

export const apiList = async (endpoint) => {
  const result = await apiRequest(endpoint, { fallbackError: "Nu s-au putut încărca datele." });
  return Array.isArray(result) ? result : [];
};
