// Datos estáticos extraídos de los mockups (Mockups/index.html)

export const posts = [
  {
    id: 1,
    type: 'incidencia',
    category: 'Infraestructura',
    title: 'Fuga de agua en el edificio B',
    excerpt:
      'Hay una fuga grande en el segundo piso del edificio B, el agua está cayendo sobre las escaleras y es peligroso caminar por ahí...',
    body:
      'Hay una fuga grande en el segundo piso del edificio B, el agua está cayendo sobre las escaleras y es peligroso caminar por ahí. Ya reporté esto a administración hace una semana pero no han hecho nada.\n\nSe necesita una reparación urgente antes de que alguien se lastime. El piso está resbaloso y el agua empieza a llegar al primer piso.',
    author: 'Juan Pérez',
    date: 'Hace 2 horas',
    supports: 24,
    location: 'Edificio B, segundo piso',
    images: ['fuga1', 'fuga2'],
    commentCount: 3,
    comments: [
      {
        author: 'María García',
        date: 'Hace 1 hora',
        text: 'A mí también me pasó, casi me resbalo ayer. Es muy peligroso.',
      },
      {
        author: 'Carlos Ruiz',
        date: 'Hace 45 min',
        text: 'Yo mandé un correo a mantenimiento y me dijeron que "están evaluando la situación". Ya van 2 semanas.',
      },
      {
        author: 'Anónimo',
        date: 'Hace 30 min',
        text: 'Esto pasa cada semestre. La infraestructura del edificio B está muy mal cuidada.',
      },
    ],
  },
  {
    id: 2,
    type: 'queja',
    category: 'Limpieza',
    title: 'Baños sin papel higiénico',
    excerpt:
      'Los baños del primer piso llevan más de una semana sin papel higiénico. La situación es incómoda para todos los estudiantes...',
    body:
      'Los baños del primer piso llevan más de una semana sin papel higiénico. La situación es incómoda para todos los estudiantes...',
    author: 'María García',
    date: 'Hace 5 horas',
    supports: 18,
    location: '',
    images: [],
    comments: [],
  },
  {
    id: 3,
    type: 'discusion',
    category: 'Académico',
    title: 'Horarios del semestre siguiente',
    excerpt:
      '¿Alguien sabe cuándo publican los horarios del próximo semestre? Necesito planificar mis clases lo antes posible...',
    body:
      '¿Alguien sabe cuándo publican los horarios del próximo semestre? Necesito planificar mis clases lo antes posible...',
    author: 'Anónimo',
    date: 'Hace 1 día',
    supports: 42,
    location: '',
    images: [],
    comments: [],
  },
  {
    id: 4,
    type: 'incidencia',
    category: 'Seguridad',
    title: 'Luz fundida en el estacionamiento',
    excerpt:
      'La luz del estacionamiento subterráneo nivel 2 está completamente apagada. Es inseguro salir de noche después de clases...',
    body:
      'La luz del estacionamiento subterráneo nivel 2 está completamente apagada. Es inseguro salir de noche después de clases...',
    author: 'Carlos Ruiz',
    date: 'Hace 1 día',
    supports: 31,
    location: '',
    images: ['luz1'],
    comments: [],
  },
  {
    id: 5,
    type: 'queja',
    category: 'Cafetería',
    title: 'Precio excesivo en la cafetería',
    excerpt:
      'Los precios de la cafetería subieron un 30% este semestre sin ninguna justificación...',
    body:
      'Los precios de la cafetería subieron un 30% este semestre sin ninguna justificación...',
    author: 'Juan Pérez',
    date: 'Hace 3 días',
    supports: 15,
    location: '',
    images: [],
    commentCount: 7,
    comments: [],
  },
  {
    id: 6,
    type: 'incidencia',
    category: 'Tecnología',
    title: 'WiFi no funciona en la biblioteca',
    excerpt: 'El WiFi de la biblioteca lleva caído todo el semana. Es imposible hacer tareas...',
    body: 'El WiFi de la biblioteca lleva caído todo el semana. Es imposible hacer tareas...',
    author: 'Laura Díaz',
    date: 'Hace 2 días',
    supports: 38,
    location: '',
    images: [],
    comments: [],
  },
];

// Pantallas "Mis publicaciones" y "Guardados" del mockup
export const myPostIds = [1, 5];
export const bookmarkIds = [3, 6];

export const categories = [
  'Infraestructura',
  'Limpieza',
  'Seguridad',
  'Servicios',
  'Académico',
  'Tecnología',
  'Cafetería',
  'Estacionamiento',
  'Comunidad',
  'Otro',
];

export const careers = [
  'Ingeniería en Sistemas Computacionales',
  'Ingeniería en Inteligencia Artificial',
  'Ingeniería en Datos',
  'Lic. en Tecnologías de la Información',
];

export const TYPE_OPTIONS = [
  { value: 'incidencia', label: 'Incidencia' },
  { value: 'queja', label: 'Queja' },
  { value: 'discusion', label: 'Discusión' },
];

export const IDENTITY_OPTIONS = [
  { value: 'real', label: 'Nombre real' },
  { value: 'username', label: 'Username' },
  { value: 'anon', label: 'Anónimo' },
];

export function getPost(id) {
  return posts.find((p) => p.id === id);
}

// Imágenes de ejemplo (picsum.photos) igual que en los mockups
export const thumbUrl = (seed) => `https://picsum.photos/seed/${seed}/200/200`;
export const detailUrl = (seed) => `https://picsum.photos/seed/${seed}/600/400`;
export const fullUrl = (seed) => `https://picsum.photos/seed/${seed}/800/600`;
