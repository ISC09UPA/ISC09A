import type { DueCardsResponse, Review, ReviewState } from '../../types/api';
import type { LanguageCode } from '../../types/language';
import { apiRequest } from './apiClient';

export const DEFAULT_DUE_LIMIT = 50;

export const reviewsApi = {
  getDueCards: (language: LanguageCode, limit = DEFAULT_DUE_LIMIT) =>
    apiRequest<DueCardsResponse>('/api/reviews/due', { query: { language, limit } }),

  submitReview: (review: Review) =>
    apiRequest<ReviewState>('/api/reviews', { method: 'POST', body: review }),
};
