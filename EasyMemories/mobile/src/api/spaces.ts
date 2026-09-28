import { apiClient } from './client';
import { MemoryResponse, SpaceResponse } from '../types';

export function createSpace(name: string, description?: string) {
  return apiClient.post<SpaceResponse>('/spaces', { name, description }).then((res) => res.data);
}

export function listMySpaces() {
  return apiClient.get<SpaceResponse[]>('/spaces').then((res) => res.data);
}

export function getSpace(id: number) {
  return apiClient.get<SpaceResponse>(`/spaces/${id}`).then((res) => res.data);
}

export function deleteSpace(id: number) {
  return apiClient.delete(`/spaces/${id}`);
}

export function getSpaceMemories(id: number) {
  return apiClient.get<MemoryResponse[]>(`/spaces/${id}/memories`).then((res) => res.data);
}

/** Descarga el PNG del QR como data URI base64, listo para <Image source={{ uri }} />. */
export async function getSpaceQrCodeDataUri(id: number): Promise<string> {
  const res = await apiClient.get(`/spaces/${id}/qrcode`, { responseType: 'arraybuffer' });
  const base64 = arrayBufferToBase64(res.data as ArrayBuffer);
  return `data:image/png;base64,${base64}`;
}

// Hermes (el motor JS de React Native) no expone `btoa` de forma confiable, así que
// codificamos base64 manualmente en lugar de depender de un polyfill global.
const BASE64_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';

function arrayBufferToBase64(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer);
  let result = '';

  for (let i = 0; i < bytes.length; i += 3) {
    const b0 = bytes[i];
    const b1 = bytes[i + 1];
    const b2 = bytes[i + 2];

    result += BASE64_CHARS[b0 >> 2];
    result += BASE64_CHARS[((b0 & 0x03) << 4) | (b1 === undefined ? 0 : b1 >> 4)];
    result += b1 === undefined ? '=' : BASE64_CHARS[((b1 & 0x0f) << 2) | (b2 === undefined ? 0 : b2 >> 6)];
    result += b2 === undefined ? '=' : BASE64_CHARS[b2 & 0x3f];
  }

  return result;
}
