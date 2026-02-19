// Dynamic API configuration that automatically detects the current domain
const getBaseUrl = () => {
  // Get the current origin (protocol + hostname + port)
  const origin = window.location.origin;
  
  // For development, if running on localhost, use port 5000 for HTTP backend
  if (origin.includes('localhost') || origin.includes('127.0.0.1')) {
    return 'http://localhost:5000';
  }
  
  // For production or other environments, use HTTP with port 5000
  const hostname = window.location.hostname;
  
  // Use HTTP with port 5000 for the backend API (allow all IPs)
  return `http://${hostname}:5000`;
};

// Export the base URL for use throughout the application
export const baseUrl = getBaseUrl();

// Helper function to build full API URLs
export const buildApiUrl = (endpoint) => {
  // Remove leading slash if present to avoid double slashes
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${baseUrl}/${cleanEndpoint}`;
};

// Log the base URL for debugging
console.log('API Base URL:', baseUrl);
