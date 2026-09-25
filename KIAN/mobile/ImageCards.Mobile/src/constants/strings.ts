import type { ReviewRating } from '../types/api';

/** User-facing texts, in one place so they can be translated later. */
export const strings = {
  tabs: {
    review: 'Repasar',
    cards: 'Tarjetas',
    settings: 'Ajustes',
  },
  review: {
    loading: 'Cargando tarjetas…',
    tapToReveal: 'Toca la tarjeta para ver la traducción',
    imageLabel: 'Imagen de la tarjeta',
    revealHint: 'Muestra la traducción',
    progress: (current: number, total: number) => `${current} / ${total}`,
    pending: (count: number) => `Pendientes: ${count}`,
    emptyTitle: '¡Todo al día!',
    emptyMessage: 'No hay tarjetas pendientes en este idioma.',
    completedTitle: 'Sesión terminada',
    completedMessage: (count: number) => `Repasaste ${count} ${count === 1 ? 'tarjeta' : 'tarjetas'}.`,
    checkAgain: 'Buscar más tarjetas',
  },
  ratings: {
    Again: 'Otra vez',
    Hard: 'Difícil',
    Good: 'Bien',
    Easy: 'Fácil',
  } satisfies Record<ReviewRating, string>,
  cards: {
    loading: 'Cargando tarjetas…',
    empty: 'Todavía no hay tarjetas en este idioma.',
    total: (count: number) => `${count} ${count === 1 ? 'tarjeta' : 'tarjetas'}`,
  },
  settings: {
    languageTitle: 'Idioma de estudio',
    languageHint: 'Las tarjetas se muestran con la traducción en este idioma.',
    apiTitle: 'Servidor',
    apiNotConfigured: 'No configurado (EXPO_PUBLIC_API_URL)',
  },
  language: {
    selectorLabel: 'Idioma',
  },
  errors: {
    title: 'Algo salió mal',
    retry: 'Reintentar',
    configuration: 'La app no está configurada: falta la URL del servidor (EXPO_PUBLIC_API_URL).',
    timeout: 'El servidor tardó demasiado en responder. Inténtalo de nuevo.',
    unreachable: 'No se pudo conectar con el servidor. Revisa tu conexión.',
    badRequest: 'La solicitud no es válida.',
    unauthorized: 'Tu sesión no es válida. Vuelve a iniciar sesión.',
    notFound: 'No se encontró lo que buscabas.',
    unavailable: 'El servicio no está disponible en este momento. Inténtalo más tarde.',
    server: 'Error del servidor. Inténtalo de nuevo.',
    unexpected: 'Ocurrió un error inesperado.',
    reviewNotSaved: 'No se pudo guardar tu respuesta.',
  },
} as const;
