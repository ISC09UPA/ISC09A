import { useCallback, useEffect, useReducer, useRef } from 'react';

import { reviewsApi } from '../services/api';
import type { Flashcard, ReviewRating } from '../types/api';
import type { LanguageCode } from '../types/language';
import { getErrorMessage } from '../utils/errorMessages';
import { initialReviewSessionState, reviewSessionReducer } from './reviewSessionReducer';

/**
 * Loads the due cards for a language and drives the study flow:
 * load -> reveal -> rate (POST /api/reviews) -> next card -> completed.
 * Reloads automatically when the language changes.
 */
export function useReviewSession(language: LanguageCode) {
  const [state, dispatch] = useReducer(reviewSessionReducer, initialReviewSessionState);
  // Identifies the current session so late responses from a previous language are ignored.
  const sessionRef = useRef(0);

  const load = useCallback(async () => {
    const session = ++sessionRef.current;
    dispatch({ type: 'load_start' });
    try {
      const response = await reviewsApi.getDueCards(language);
      if (session === sessionRef.current) {
        dispatch({ type: 'load_success', response });
      }
    } catch (error) {
      if (session === sessionRef.current) {
        dispatch({ type: 'load_failure', message: getErrorMessage(error) });
      }
    }
  }, [language]);

  useEffect(() => {
    void load();
  }, [load]);

  const currentCard: Flashcard | null = state.status === 'reviewing' ? (state.cards[state.index] ?? null) : null;
  const canRate = state.status === 'reviewing' && state.revealed && !state.submitting;

  const reveal = useCallback(() => dispatch({ type: 'reveal' }), []);

  const rate = useCallback(
    async (rating: ReviewRating) => {
      if (!canRate || !currentCard) {
        return;
      }
      const session = sessionRef.current;
      dispatch({ type: 'submit_start' });
      try {
        await reviewsApi.submitReview({ cardId: currentCard.id, rating });
        if (session === sessionRef.current) {
          dispatch({ type: 'submit_success' });
        }
      } catch (error) {
        if (session === sessionRef.current) {
          dispatch({ type: 'submit_failure', message: getErrorMessage(error) });
        }
      }
    },
    [canRate, currentCard],
  );

  return { state, currentCard, reveal, rate, reload: load };
}
