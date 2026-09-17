import { HttpErrorResponse } from '@angular/common/http';

const DEFAULT_MESSAGE = 'Something went wrong. Please try again.';

export function isNotFound(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 404;
}

// Turns an API error into a message that can be shown to the user
export function getErrorMessage(error: unknown, fallback = DEFAULT_MESSAGE): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  if (error.status === 0) {
    return 'Cannot reach the server. Make sure the backend is running.';
  }

  const body = error.error;

  // The backend returns { message } for its own errors
  if (typeof body?.message === 'string') {
    return body.message;
  }

  // ASP.NET model validation returns ProblemDetails with an errors map
  if (body?.errors && typeof body.errors === 'object') {
    const firstError = Object.values(body.errors as Record<string, string[]>).flat()[0];

    if (firstError) {
      return firstError;
    }
  }

  if (typeof body?.title === 'string') {
    return body.title;
  }

  return fallback;
}
