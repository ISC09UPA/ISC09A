import type { Card, PagedResponse } from '../../types/api';
import type { LanguageCode } from '../../types/language';
import { apiRequest } from './apiClient';

export interface GetCardsParams {
  language?: LanguageCode;
  page?: number;
  pageSize?: number;
}

export const cardsApi = {
  getCards: ({ language, page = 1, pageSize = 20 }: GetCardsParams = {}) =>
    apiRequest<PagedResponse<Card>>('/api/cards', { query: { language, page, pageSize } }),

  getCard: (id: string, language?: LanguageCode) =>
    apiRequest<Card>(`/api/cards/${encodeURIComponent(id)}`, { query: { language } }),

  deleteCard: (id: string) =>
    apiRequest<void>(`/api/cards/${encodeURIComponent(id)}`, { method: 'DELETE' }),
};
