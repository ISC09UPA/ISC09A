// Formateo de fechas y mapeo de DTOs de la API a la forma que consumen las
// pantallas y componentes (la misma forma que tenían los mocks de data/posts.js
// para minimizar cambios visuales).

export function timeAgo(iso) {
  const then = new Date(iso).getTime();
  if (Number.isNaN(then)) return '';
  const minutes = Math.max(1, Math.floor((Date.now() - then) / 60000));
  if (minutes < 60) return `Hace ${minutes} min`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `Hace ${hours} h`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `Hace ${days} día${days === 1 ? '' : 's'}`;
  const weeks = Math.floor(days / 7);
  if (weeks < 5) return `Hace ${weeks} sem`;
  return new Date(iso).toLocaleDateString('es-MX');
}

// PostSummaryResponse / PostDetailResponse → forma del PostCard
export function mapPost(dto) {
  return {
    id: dto.id,
    type: dto.type,
    category: dto.category,
    title: dto.title,
    description: dto.description ?? '',
    location: dto.location ?? '',
    author: dto.author?.displayName || 'Anónimo',
    authorEnrollment: dto.author?.enrollmentNumber ?? null,
    isAnonymous: !!dto.author?.isAnonymous,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
    date: timeAgo(dto.createdAt),
    supports: dto.supportCount ?? 0,
    commentCount: dto.commentCount ?? 0,
    imageCount: dto.imageCount ?? 0,
    images: dto.images ?? (dto.coverImage ? [dto.coverImage] : []),
    coverImage: dto.coverImage ?? null,
    supportedByMe: !!dto.supportedByMe,
    bookmarkedByMe: !!dto.bookmarkedByMe,
    isOwner: !!dto.isOwner,
  };
}
