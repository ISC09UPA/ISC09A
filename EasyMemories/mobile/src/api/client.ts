import AsyncStorage from '@react-native-async-storage/async-storage';
import Constants from 'expo-constants';
import axios from 'axios';

const BACKEND_PORT = 5215;

/**
 * En desarrollo con Expo Go, "localhost" apunta al propio teléfono, no a la PC.
 * Usamos la IP de la PC que Metro ya conoce (hostUri) para que funcione sin configurar nada.
 * Para producción, define EXPO_PUBLIC_API_URL en el entorno de build.
 */
function resolveBaseUrl(): string {
  if (process.env.EXPO_PUBLIC_API_URL) {
    return process.env.EXPO_PUBLIC_API_URL;
  }

  const hostUri = Constants.expoConfig?.hostUri ?? Constants.expoGoConfig?.debuggerHost;
  const host = hostUri?.split(':')[0];

  return host ? `http://${host}:${BACKEND_PORT}/api` : `http://localhost:${BACKEND_PORT}/api`;
}

export const API_BASE_URL = resolveBaseUrl();

export const AUTH_TOKEN_KEY = 'easymemories.authToken';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 20000,
});

apiClient.interceptors.request.use(async (config) => {
  const token = await AsyncStorage.getItem(AUTH_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export function extractErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { message?: string; errors?: string[] | Record<string, string[]> }
      | undefined;

    if (data?.message) return data.message;

    if (Array.isArray(data?.errors) && data.errors.length) {
      return data.errors.join('\n');
    }

    // ASP.NET Core devuelve los errores de validación automática (DataAnnotations)
    // como un diccionario { campo: [mensajes] }, no como un arreglo plano.
    if (data?.errors && typeof data.errors === 'object') {
      const messages = Object.values(data.errors).flat();
      if (messages.length) return messages.join('\n');
    }
  }
  return fallback;
}
