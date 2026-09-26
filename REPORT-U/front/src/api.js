// Cliente de la API ReportU (ASP.NET Core).
//
// Configuración de la base URL: crea front/.env con una de estas líneas
// (Expo la expone como EXPO_PUBLIC_*, se recarga al reiniciar `npm start`):
//
//   Android Studio (emulador):  EXPO_PUBLIC_API_URL=http://10.0.2.2:5034
//   Expo Go (teléfono físico):  EXPO_PUBLIC_API_URL=http://TU_IP_LAN:5034
//   iOS Simulator:              EXPO_PUBLIC_API_URL=http://localhost:5034
//
import AsyncStorage from '@react-native-async-storage/async-storage';

export const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_URL || 'http://10.0.2.2:5034';

const TOKEN_KEY = 'reportu.accessToken';
const EXPIRES_KEY = 'reportu.expiresAt';
const USER_KEY = 'reportu.user';

// ---------------------------------------------------------------------------
// Token
// ---------------------------------------------------------------------------

export async function loadSession() {
  // Devuelve { token, user } o null. Si el token ya expiró, se descarta.
  const [token, expiresAt, userJson] = await Promise.all([
    AsyncStorage.getItem(TOKEN_KEY),
    AsyncStorage.getItem(EXPIRES_KEY),
    AsyncStorage.getItem(USER_KEY),
  ]);
  if (!token) return null;
  if (expiresAt && new Date(expiresAt).getTime() < Date.now()) {
    await clearSession();
    return null;
  }
  return { token, user: userJson ? JSON.parse(userJson) : null };
}

export async function saveSession(auth) {
  // auth = { accessToken, expiresAt, user } (respuesta de /api/auth/*)
  await Promise.all([
    AsyncStorage.setItem(TOKEN_KEY, auth.accessToken),
    AsyncStorage.setItem(EXPIRES_KEY, new Date(auth.expiresAt).toISOString()),
    AsyncStorage.setItem(USER_KEY, JSON.stringify(auth.user)),
  ]);
}

export async function setCachedUser(user) {
  await AsyncStorage.setItem(USER_KEY, JSON.stringify(user));
}

export async function clearSession() {
  await Promise.all([
    AsyncStorage.removeItem(TOKEN_KEY),
    AsyncStorage.removeItem(EXPIRES_KEY),
    AsyncStorage.removeItem(USER_KEY),
  ]);
}

// ---------------------------------------------------------------------------
// uploadWithXhr(): multipart con XMLHttpRequest
// ---------------------------------------------------------------------------

// El fetch de Expo (expo/winter) no soporta las partes {uri} de React Native en
// FormData (falla con "Unsupported FormDataPart implementation"), así que la
// subida de archivos usa XMLHttpRequest, que sí las soporta de forma nativa.
function uploadWithXhr(path, formData) {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open('POST', `${API_BASE_URL}${path}`);
    if (authToken) xhr.setRequestHeader('Authorization', `Bearer ${authToken}`);
    xhr.setRequestHeader('Accept', 'application/json');
    xhr.onload = () => {
      let payload = null;
      try {
        payload = xhr.responseText ? JSON.parse(xhr.responseText) : null;
      } catch {
        payload = null;
      }
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(payload);
      } else {
        reject(new ApiError(xhr.status, payload, `Error ${xhr.status} en POST ${path}`));
      }
    };
    xhr.onerror = () => reject(new ApiError(0, null, 'Fallo de red al subir la imagen'));
    xhr.send(formData);
  });
}

// ---------------------------------------------------------------------------
// request(): fetch + JSON + ProblemDetails
// ---------------------------------------------------------------------------

export class ApiError extends Error {
  // `code` es el código propio de la API (p. ej. "already_supported") para que
  // la app ramifique por código y no por mensajes (ver api/README.md).
  constructor(status, payload, fallbackMessage) {
    super((payload && (payload.detail || payload.title)) || fallbackMessage);
    this.status = status;
    this.code = (payload && payload.code) || null;
    this.errors = (payload && payload.errors) || null;
  }
}

let authToken = null;

export function setAuthToken(token) {
  authToken = token;
}

export function getAuthToken() {
  return authToken;
}

async function parseBody(res) {
  const text = await res.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return null; // respuestas binarias (imágenes) no se parsean como JSON
  }
}

async function request(path, { method = 'GET', body, auth = true, headers } = {}) {
  const finalHeaders = { Accept: 'application/json', ...headers };
  if (body !== undefined && !(body instanceof FormData)) {
    finalHeaders['Content-Type'] = 'application/json';
    body = JSON.stringify(body);
  }
  if (auth && authToken) finalHeaders.Authorization = `Bearer ${authToken}`;

  const res = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: finalHeaders,
    body,
  });

  const payload = await parseBody(res);
  if (!res.ok) {
    throw new ApiError(res.status, payload, `Error ${res.status} en ${method} ${path}`);
  }
  return payload;
}

// ---------------------------------------------------------------------------
// Auth
// ---------------------------------------------------------------------------

export const api = {
  register: (data) =>
    request('/api/auth/register', { method: 'POST', body: data, auth: false }),
  login: (identifier, password) =>
    request('/api/auth/login', {
      method: 'POST',
      body: { identifier, password },
      auth: false,
    }),

  // Usuario
  getMe: () => request('/api/users/me'),
  updateMe: (data) => request('/api/users/me', { method: 'PATCH', body: data }),

  // Publicaciones
  getFeed: (sort = 'recent', page = 1, pageSize = 20) =>
    request(`/api/posts?sort=${sort}&page=${page}&pageSize=${pageSize}`),
  getPost: (id) => request(`/api/posts/${id}`),
  createPost: (data) => request('/api/posts', { method: 'POST', body: data }),
  updatePost: (id, data) => request(`/api/posts/${id}`, { method: 'PATCH', body: data }),
  deletePost: (id) => request(`/api/posts/${id}`, { method: 'DELETE' }),

  // Imágenes: multipart vía XMLHttpRequest (ver uploadWithXhr)
  uploadImage: (postId, uri) => uploadWithXhr(`/api/posts/${postId}/images`, toFormData(uri)),
  listImages: (postId) => request(`/api/posts/${postId}/images`),
  deleteImage: (imageId) => request(`/api/media/${imageId}`, { method: 'DELETE' }),

  // Comentarios
  getComments: (postId) => request(`/api/posts/${postId}/comments`),
  createComment: (postId, body) =>
    request(`/api/posts/${postId}/comments`, { method: 'POST', body: { body } }),

  // Apoyos y guardados
  support: (postId) => request(`/api/posts/${postId}/support`, { method: 'PUT' }),
  unsupport: (postId) => request(`/api/posts/${postId}/support`, { method: 'DELETE' }),
  bookmark: (postId) => request(`/api/posts/${postId}/bookmark`, { method: 'PUT' }),
  unbookmark: (postId) => request(`/api/posts/${postId}/bookmark`, { method: 'DELETE' }),

  // Mis publicaciones / guardados
  getMyPosts: (page = 1, pageSize = 50) =>
    request(`/api/me/posts?page=${page}&pageSize=${pageSize}`),
  getMyBookmarks: (page = 1, pageSize = 50) =>
    request(`/api/me/bookmarks?page=${page}&pageSize=${pageSize}`),
};

// multipart/form-data con el archivo en el campo `file` (IFormFile file).
export function toFormData(uri) {
  const ext = (uri.split('.').pop() || 'jpg').toLowerCase();
  const typeMap = {
    jpg: 'image/jpeg',
    jpeg: 'image/jpeg',
    png: 'image/png',
    webp: 'image/webp',
    gif: 'image/gif',
    heic: 'image/heic',
  };
  const formData = new FormData();
  formData.append('file', {
    uri,
    name: `imagen.${ext === 'jpeg' ? 'jpg' : ext}`,
    type: typeMap[ext] || 'image/jpeg',
  });
  return formData;
}

// URL absoluta de una imagen: la API devuelve rutas relativas (/api/media/{id})
// y los endpoints de media exigen Authorization: Bearer (ver MediaController).
export function imageUrl(image, {
  baseUrl = API_BASE_URL,
  accessToken = authToken,
} = {}) {
  if (!image) return null;
  const url = `${baseUrl}${image.url || `/api/media/${image.id}`}`;
  // React Native no permite headers por <Image>, así que pasamos el JWT en la
  // query solo para la descarga del binario del visor/listas.
  const sep = url.includes('?') ? '&' : '?';
  return `${url}${sep}access_token=${encodeURIComponent(accessToken || '')}`;
}
