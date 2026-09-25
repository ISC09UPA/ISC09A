import type { DueCardsResponse, Flashcard, ReviewState } from '../types/api';

export function flashcard(overrides: Partial<Flashcard> = {}): Flashcard {
  return {
    id: 'card-1',
    imageUrl: 'https://blob.test/card-images/1.png?sig=x',
    translation: 'Apple',
    language: 'en',
    exampleSentence: 'I eat an apple every day.',
    nextReviewAt: '2026-01-15T10:00:00Z',
    ...overrides,
  };
}

export function dueResponse(cards: Flashcard[], totalDue = cards.length): DueCardsResponse {
  return { totalDue, cards };
}

export function reviewState(cardId: string): ReviewState {
  return {
    cardId,
    nextReviewAt: '2026-01-16T10:00:00Z',
    lastReviewedAt: '2026-01-15T10:00:00Z',
    intervalDays: 1,
    easeFactor: 2.5,
    repetitions: 1,
    lapses: 0,
  };
}

/** A promise that the test resolves or rejects when it wants, to observe intermediate states. */
export function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return { promise, resolve, reject };
}
