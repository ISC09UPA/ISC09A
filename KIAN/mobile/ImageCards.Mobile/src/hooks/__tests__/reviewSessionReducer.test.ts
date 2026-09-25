import { dueResponse, flashcard } from '../../test-utils/fixtures';
import { initialReviewSessionState, reviewSessionReducer, type ReviewSessionState } from '../reviewSessionReducer';

const twoCards = dueResponse([flashcard({ id: 'a' }), flashcard({ id: 'b' })], 7);

function reviewing(): Extract<ReviewSessionState, { status: 'reviewing' }> {
  const state = reviewSessionReducer(initialReviewSessionState, { type: 'load_success', response: twoCards });
  if (state.status !== 'reviewing') throw new Error('expected reviewing');
  return state;
}

describe('reviewSessionReducer', () => {
  it('starts reviewing the first card, hidden, when cards are due', () => {
    expect(reviewing()).toMatchObject({ index: 0, revealed: false, reviewed: 0, totalDue: 7 });
  });

  it('is empty when nothing is due', () => {
    expect(reviewSessionReducer(initialReviewSessionState, { type: 'load_success', response: dueResponse([]) })).toEqual({
      status: 'empty',
    });
  });

  it('advances to the next hidden card after a successful review', () => {
    let state = reviewSessionReducer(reviewing(), { type: 'reveal' });
    state = reviewSessionReducer(state, { type: 'submit_start' });
    state = reviewSessionReducer(state, { type: 'submit_success' });

    expect(state).toMatchObject({ status: 'reviewing', index: 1, revealed: false, submitting: false, reviewed: 1 });
  });

  it('completes the session after the last card', () => {
    let state: ReviewSessionState = reviewing();
    for (let i = 0; i < 2; i++) {
      state = reviewSessionReducer(state, { type: 'reveal' });
      state = reviewSessionReducer(state, { type: 'submit_start' });
      state = reviewSessionReducer(state, { type: 'submit_success' });
    }

    expect(state).toEqual({ status: 'completed', reviewed: 2 });
  });

  it('keeps the same revealed card with an error when the review fails', () => {
    let state = reviewSessionReducer(reviewing(), { type: 'reveal' });
    state = reviewSessionReducer(state, { type: 'submit_start' });
    state = reviewSessionReducer(state, { type: 'submit_failure', message: 'offline' });

    expect(state).toMatchObject({ index: 0, revealed: true, submitting: false, submitError: 'offline', reviewed: 0 });
  });

  it('ignores card actions outside a session', () => {
    expect(reviewSessionReducer({ status: 'empty' }, { type: 'reveal' })).toEqual({ status: 'empty' });
    expect(reviewSessionReducer({ status: 'loading' }, { type: 'submit_success' })).toEqual({ status: 'loading' });
  });
});
