import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';

export const errorInterceptor: HttpInterceptorFn = (req, next) => next(req);

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const problemDetail = error.error?.detail;
    const legacyMessage = error.error?.message;

    if (typeof problemDetail === 'string' && problemDetail.length > 0) {
      return problemDetail;
    }

    if (typeof legacyMessage === 'string' && legacyMessage.length > 0) {
      return legacyMessage;
    }

    if (error.status === 0) {
      return 'Cannot connect to the backend. Start the ASP.NET Core API on http://localhost:5000.';
    }

    if (error.status === 409) {
      return 'The game changed before this action was saved. Refresh the current game and try again.';
    }

    return `Request failed with HTTP ${error.status}.`;
  }

  return 'An unexpected error occurred.';
}
