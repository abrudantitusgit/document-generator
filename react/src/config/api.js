// API routes use a consistent lower camelCase JSON contract. CRA can override
// the origin at build time; the local Docker deployment uses the host API port.
const getBaseUrl = () => {
  if (process.env.REACT_APP_API_URL) return process.env.REACT_APP_API_URL.replace(/\/$/, "");
  return `${window.location.protocol}//${window.location.hostname}:5000`;
};

export const baseUrl = getBaseUrl();

// Helper function to build full API URLs
export const buildApiUrl = (endpoint) => {
  // Remove leading slash if present to avoid double slashes
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${baseUrl}/${cleanEndpoint}`;
};
