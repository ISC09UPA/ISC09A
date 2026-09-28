import { Platform } from 'react-native';
import { apiClient } from './client';
import { MemoryResponse, SpacePublicInfo } from '../types';

export function getSpaceInfo(joinCode: string) {
  return apiClient.get<SpacePublicInfo>(`/join/${joinCode}`).then((res) => res.data);
}

export function getMemories(joinCode: string) {
  return apiClient.get<MemoryResponse[]>(`/join/${joinCode}/memories`).then((res) => res.data);
}

export interface PickedPhoto {
  uri: string;
  fileName: string;
  mimeType: string;
}

export async function uploadMemory(
  joinCode: string,
  photo: PickedPhoto,
  guestName: string,
  comment: string
) {
  const formData = new FormData();

  if (Platform.OS === 'web') {
    // En web, `{ uri, name, type }` no significa nada para el FormData nativo del navegador:
    // hay que convertir el uri (blob:/data:) en un Blob real primero.
    const blob = await fetch(photo.uri).then((res) => res.blob());
    formData.append('photo', blob, photo.fileName);
  } else {
    // En Android/iOS, React Native sí interpreta este objeto especial y lee el archivo del uri.
    formData.append('photo', {
      uri: photo.uri,
      name: photo.fileName,
      type: photo.mimeType,
    } as unknown as Blob);
  }

  if (guestName.trim()) formData.append('guestName', guestName.trim());
  if (comment.trim()) formData.append('comment', comment.trim());

  // Ojo: no fijamos "Content-Type" a mano. FormData necesita generar su propio
  // boundary; si lo pisamos, el servidor no puede parsear el cuerpo (por eso daba 400).
  return apiClient
    .post<MemoryResponse>(`/join/${joinCode}/memories`, formData)
    .then((res) => res.data);
}
