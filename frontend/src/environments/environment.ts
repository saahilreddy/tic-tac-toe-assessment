/**
 * Production/default Angular build-time configuration.
 * API URLs are configuration, not secrets.
 */
export const environment = {
  production: true,
  apiUrl: 'http://localhost:62167/api/v1'
} as const;
