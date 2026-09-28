import { Platform } from 'react-native';
import Constants from 'expo-constants';

export type BodyPart = 'Head' | 'Torso' | 'Legs' | 'Feet';

export type Garment = {
    id: number;
    name: string;
    bodyPart: BodyPart;
    fileName: string | null;
    createdAtUtc: string;
    imageUrl: string | null;
};

function resolveApiUrl() {
    if (Platform.OS === 'web') {
        return 'http://localhost:5179';
    }

    const constants = Constants as unknown as {
        expoConfig?: { hostUri?: string };
        expoGoConfig?: { debuggerHost?: string; developer?: { host?: string } };
    };

    const hostUri =
        constants.expoConfig?.hostUri ??
        constants.expoGoConfig?.debuggerHost ??
        constants.expoGoConfig?.developer?.host;

    const host = hostUri?.split(':')[0];

    return host ? `http://${host}:5179` : 'http://localhost:5179';
}

export const API_URL = resolveApiUrl();

export function withApiUrl(url: string) {
  return {
    getGarments: () => getGarments(url),
    uploadGarment: (name: string, bodyPart: BodyPart, asset: { uri: string; fileName?: string | null; mimeType?: string | null }) => uploadGarment(name, bodyPart, url, asset),
    deleteGarment: (id: number) => deleteGarment(id, url),
  };
}

export async function getGarments(url: string = API_URL): Promise<Garment[]> {
    const response = await fetch(`${url}/api/garments`);

    if (!response.ok) {
        throw new Error(`Error al cargar prendas: ${response.status}`);
    }

    return response.json();
}

function buildPhotoAsset(
    asset: { uri: string; fileName?: string | null; mimeType?: string | null }
) {
    const mimeType = asset.mimeType ?? 'image/jpeg';
    const extension =
        mimeType === 'image/png' ? 'png'
            : mimeType === 'image/webp' ? 'webp'
            : 'jpg';

    return {
        uri: asset.uri ?? '',
        name: asset.fileName ?? `prenda-${Date.now()}.${extension}`,
        type: mimeType,
    };
}

function uploadWithXhr(url: string, form: FormData): Promise<void> {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();

        xhr.open('POST', `${url}/api/garments`);
        xhr.timeout = 30000;
        xhr.ontimeout = () => reject(new Error('Tiempo de espera agotado al subir la foto.'));
        xhr.onerror = () => reject(new Error('No se pudo conectar con la API.'));
        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve();
            } else {
                reject(new Error(`Error al subir prenda: ${xhr.status}`));
            }
        };
        xhr.send(form);
    });
}

export async function uploadGarment(
    name: string,
    bodyPart: BodyPart,
    url: string = API_URL,
    asset: { uri: string; fileName?: string | null; mimeType?: string | null }
): Promise<void> {
    if (Platform.OS === 'web') {
        const response = await fetch(asset.uri);
        const blob = await response.blob();

        const form = new FormData();
        form.append('name', name);
        form.append('bodyPart', bodyPart);
        form.append('photo', blob, asset.fileName ?? 'prenda.jpg');

        const result = await fetch(`${url}/api/garments`, {
            method: 'POST',
            body: form,
        });

        if (!result.ok) {
            throw new Error(`Error al subir prenda: ${result.status}`);
        }

        return;
    }

    const photo = buildPhotoAsset(asset);
    const form = new FormData();
    form.append('name', name);
    form.append('bodyPart', bodyPart);
    form.append('photo', photo as unknown as Blob);

    await uploadWithXhr(url, form);
}

export async function deleteGarment(id: number, url: string = API_URL): Promise<void> {
    const response = await fetch(`${url}/api/garments/${id}`, {
        method: 'DELETE',
    } as RequestInit);

    if (!response.ok) {
        throw new Error(`Error al borrar prenda: ${response.status}`);
    }
}
