// Constantes compartidas. Los enums viajan como strings desde la API
// (JsonStringEnumConverter en Program.cs), respetando sus nombres exactos.

export const CATEGORIES = [
  { value: 'Infraestructura', label: 'Infraestructura' },
  { value: 'Limpieza', label: 'Limpieza' },
  { value: 'Seguridad', label: 'Seguridad' },
  { value: 'Servicios', label: 'Servicios' },
  { value: 'Academico', label: 'Académico' },
  { value: 'Tecnologia', label: 'Tecnología' },
  { value: 'Cafeteria', label: 'Cafetería' },
  { value: 'Estacionamiento', label: 'Estacionamiento' },
  { value: 'Comunidad', label: 'Comunidad' },
  { value: 'Otro', label: 'Otro' },
];

export const TYPE_OPTIONS = [
  { value: 'Incidencia', label: 'Incidencia' },
  { value: 'Queja', label: 'Queja' },
  { value: 'Discusion', label: 'Discusión' },
];

export const IDENTITY_OPTIONS = [
  { value: 'Default', label: 'Según mi perfil' },
  { value: 'RealName', label: 'Nombre real' },
  { value: 'Username', label: 'Username' },
  { value: 'Anonymous', label: 'Anónimo' },
];

// Preferencias del perfil (DisplayNamePreference).
export const DISPLAY_NAME_OPTIONS = [
  { value: 'RealName', label: 'Nombre real' },
  { value: 'Username', label: 'Username' },
  { value: 'Anonymous', label: 'Anónimo' },
];

export const CAREERS = [
  'Ingeniería en Sistemas Computacionales',
  'Ingeniería en Inteligencia Artificial',
  'Ingeniería en Datos',
  'Lic. en Tecnologías de la Información',
];

export const MAX_IMAGES = 4;
