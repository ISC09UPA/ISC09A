import type { ProblemDetails } from '../../types/api';

// setPrototypeOf keeps `instanceof` working when Babel transpiles classes that extend Error.

/** The API answered with a non-success status. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.title ?? `Request failed with status ${status}`);
    Object.setPrototypeOf(this, new.target.prototype);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  /** Validation messages by field, flattened. Empty when the error is not a validation error. */
  get validationMessages(): string[] {
    return Object.values(this.problem?.errors ?? {}).flat();
  }
}

export type NetworkErrorReason = 'timeout' | 'unreachable';

/** The request never got a response. */
export class NetworkError extends Error {
  readonly reason: NetworkErrorReason;

  constructor(reason: NetworkErrorReason, cause?: unknown) {
    super(reason === 'timeout' ? 'The request timed out.' : 'The server could not be reached.', { cause });
    Object.setPrototypeOf(this, new.target.prototype);
    this.name = 'NetworkError';
    this.reason = reason;
  }
}

/** The app is missing required configuration (e.g. EXPO_PUBLIC_API_URL). */
export class ConfigurationError extends Error {
  constructor(message: string) {
    super(message);
    Object.setPrototypeOf(this, new.target.prototype);
    this.name = 'ConfigurationError';
  }
}
