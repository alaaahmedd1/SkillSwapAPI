import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorBody } from '../models/api.models';

/** Normalized error thrown by the api interceptor for every failed backend call. */
export class ApiRequestError extends Error {
  readonly status: number;
  readonly code: string | null;
  /** Property name -> validation messages (400 responses only). */
  readonly fieldErrors: Record<string, string[]>;

  constructor(message: string, status = 0, code: string | null = null, fieldErrors: Record<string, string[]> = {}) {
    super(message);
    this.name = 'ApiRequestError';
    this.status = status;
    this.code = code;
    this.fieldErrors = fieldErrors;
  }

  fieldError(field: string): string | null {
    return this.fieldErrors[field]?.[0] ?? null;
  }
}

export function extractApiError(err: unknown): ApiRequestError {
  if (err instanceof ApiRequestError) {
    return err;
  }
  if (err instanceof HttpErrorResponse) {
    const body = (err.error ?? {}) as ApiErrorBody;
    const fieldErrors: Record<string, string[]> = {};
    for (const item of body.errors ?? []) {
      if (!item?.code) continue;
      (fieldErrors[item.code] ??= []).push(item.description);
    }
    const message =
      body.message ??
      body.title ??
      (err.status === 0
        ? 'Cannot reach the server. Make sure the SkillSwapAPI is running.'
        : `Unexpected error (${err.status}).`);
    return new ApiRequestError(message, err.status, body.code ?? null, fieldErrors);
  }
  if (err instanceof Error) {
    return new ApiRequestError(err.message);
  }
  return new ApiRequestError('Something went wrong. Please try again.');
}
