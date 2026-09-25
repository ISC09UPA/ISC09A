import type { DueCardsResponse, Flashcard } from '../types/api';

/** State machine of a study session. Pure, so it can be tested without React. */
export type ReviewSessionState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'empty' }
  | {
      status: 'reviewing';
      cards: Flashcard[];
      index: number;
      revealed: boolean;
      submitting: boolean;
      submitError: string | null;
      reviewed: number;
      totalDue: number;
    }
  | { status: 'completed'; reviewed: number };

export type ReviewSessionAction =
  | { type: 'load_start' }
  | { type: 'load_success'; response: DueCardsResponse }
  | { type: 'load_failure'; message: string }
  | { type: 'reveal' }
  | { type: 'submit_start' }
  | { type: 'submit_success' }
  | { type: 'submit_failure'; message: string };

export const initialReviewSessionState: ReviewSessionState = { status: 'loading' };

export function reviewSessionReducer(state: ReviewSessionState, action: ReviewSessionAction): ReviewSessionState {
  switch (action.type) {
    case 'load_start':
      return { status: 'loading' };

    case 'load_success': {
      const { cards, totalDue } = action.response;
      if (cards.length === 0) {
        return { status: 'empty' };
      }
      return { status: 'reviewing', cards, index: 0, revealed: false, submitting: false, submitError: null, reviewed: 0, totalDue };
    }

    case 'load_failure':
      return { status: 'error', message: action.message };

    case 'reveal':
      return state.status === 'reviewing' ? { ...state, revealed: true } : state;

    case 'submit_start':
      return state.status === 'reviewing' ? { ...state, submitting: true, submitError: null } : state;

    case 'submit_success': {
      if (state.status !== 'reviewing') {
        return state;
      }
      const reviewed = state.reviewed + 1;
      const nextIndex = state.index + 1;
      if (nextIndex >= state.cards.length) {
        return { status: 'completed', reviewed };
      }
      return { ...state, index: nextIndex, revealed: false, submitting: false, reviewed };
    }

    case 'submit_failure':
      return state.status === 'reviewing' ? { ...state, submitting: false, submitError: action.message } : state;
  }
}
