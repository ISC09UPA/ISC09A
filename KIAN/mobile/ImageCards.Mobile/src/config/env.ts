import { ConfigurationError } from '../services/api/errors';

/**
 * Base URL of the ImageCards API, from EXPO_PUBLIC_API_URL (see .env.example).
 * Read on every call so tests can change it. Expo only inlines `process.env.EXPO_PUBLIC_*`
 * when accessed exactly like this, so do not destructure process.env.
 */
export function getApiBaseUrl(): string {
  const value = process.env.EXPO_PUBLIC_API_URL?.trim();
  if (!value) {
    throw new ConfigurationError('EXPO_PUBLIC_API_URL is not set.');
  }
  return value.replace(/\/+$/, '');
}
