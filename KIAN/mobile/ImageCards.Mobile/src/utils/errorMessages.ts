import { strings } from '../constants/strings';
import { ApiError, ConfigurationError, NetworkError } from '../services/api/errors';

/** Turns any thrown value into a message that is safe and useful to show to the user. */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ConfigurationError) {
    return strings.errors.configuration;
  }

  if (error instanceof NetworkError) {
    return error.reason === 'timeout' ? strings.errors.timeout : strings.errors.unreachable;
  }

  if (error instanceof ApiError) {
    return messageForStatus(error);
  }

  return strings.errors.unexpected;
}

function messageForStatus(error: ApiError): string {
  switch (error.status) {
    case 400:
      return error.validationMessages[0] ?? strings.errors.badRequest;
    case 401:
    case 403:
      return strings.errors.unauthorized;
    case 404:
      return strings.errors.notFound;
    case 503:
      return strings.errors.unavailable;
    default:
      return error.status >= 500 ? strings.errors.server : strings.errors.unexpected;
  }
}
