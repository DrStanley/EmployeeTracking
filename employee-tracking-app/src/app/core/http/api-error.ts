import { HttpErrorResponse } from '@angular/common/http';

export function apiErrorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  const body = error instanceof HttpErrorResponse ? error.error : error;
  const parsed = parseBody(body);
  if (!parsed) return fallback;
  if (typeof parsed === 'string') return parsed.trim() || fallback;

  const detail = text(parsed.detail) ?? text(parsed.Detail);
  if (detail) return detail;

  const fieldErrors = messages(parsed.errors ?? parsed.Errors ?? parsed.extensions?.errors);
  if (fieldErrors) return fieldErrors;

  const title = text(parsed.title) ?? text(parsed.Title);
  return title || fallback;
}

function parseBody(body: unknown): any {
  if (typeof body !== 'string') return body;
  const trimmed = body.trim();
  if (!trimmed) return null;
  try {
    return JSON.parse(trimmed);
  } catch {
    return trimmed;
  }
}

function text(value: unknown): string | null {
  return typeof value === 'string' && value.trim() ? value.trim() : null;
}

function messages(errors: unknown): string | null {
  if (!errors || typeof errors !== 'object') return null;
  const lines = Object.values(errors as Record<string, unknown>)
    .flatMap(value => Array.isArray(value) ? value : [value])
    .map(text)
    .filter((line): line is string => !!line);
  return lines.length ? lines.join(' ') : null;
}
