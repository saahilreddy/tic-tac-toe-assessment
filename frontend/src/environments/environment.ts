/**
 * Production/default Angular build-time configuration.
 * API URLs are configuration, not secrets.
 */
export const environment = {
  production: true,
  apiUrl: 'https://localhost:62167/api/v1'
} as const;
