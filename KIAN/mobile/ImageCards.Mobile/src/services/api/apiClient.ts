import { getApiBaseUrl } from '../../config/env';
import type { ProblemDetails } from '../../types/api';
import { ApiError, NetworkError } from './errors';

export const REQUEST_TIMEOUT_MS = 15_000;

type QueryValue = string | number | undefined;

export interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  query?: Record<string, QueryValue>;
  body?: unknown;
}

/**
 * Single entry point for HTTP calls to the backend. Components never call fetch directly.
 * Throws ApiError, NetworkError or ConfigurationError.
 */
export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const url = buildUrl(getApiBaseUrl(), path, options.query);
  const hasBody = options.body !== undefined;

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  let response: Response;
  try {
    response = await fetch(url, {
      method: options.method ?? 'GET',
      headers: {
        Accept: 'application/json',
        ...(hasBody ? { 'Content-Type': 'application/json' } : {}),
      },
      body: hasBody ? JSON.stringify(options.body) : undefined,
      signal: controller.signal,
    });
  } catch (error) {
    throw new NetworkError(controller.signal.aborted ? 'timeout' : 'unreachable', error);
  } finally {
    clearTimeout(timeout);
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readProblem(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/** Built by hand: URL/URLSearchParams are only partially implemented in React Native. */
export function buildUrl(baseUrl: string, path: string, query?: Record<string, QueryValue>): string {
  const queryString = Object.entries(query ?? {})
    .filter((entry): entry is [string, string | number] => entry[1] !== undefined)
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`)
    .join('&');

  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return queryString ? `${baseUrl}${normalizedPath}?${queryString}` : `${baseUrl}${normalizedPath}`;
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    const body: unknown = await response.json();
    return typeof body === 'object' && body !== null ? (body as ProblemDetails) : null;
  } catch {
    return null;
  }
}
