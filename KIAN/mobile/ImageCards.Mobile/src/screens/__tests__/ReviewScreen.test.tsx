import { fireEvent, render, screen } from '@testing-library/react-native';

import { strings } from '../../constants/strings';
import { LanguageProvider } from '../../hooks/useLanguage';
import { ApiError, NetworkError, reviewsApi } from '../../services/api';
import { deferred, dueResponse, flashcard, reviewState } from '../../test-utils/fixtures';
import type { DueCardsResponse } from '../../types/api';
import { ReviewScreen } from '../ReviewScreen';

jest.mock('../../services/api/reviewsApi', () => ({
  reviewsApi: { getDueCards: jest.fn(), submitReview: jest.fn() },
}));

const getDueCards = jest.mocked(reviewsApi.getDueCards);
const submitReview = jest.mocked(reviewsApi.submitReview);

const apple = flashcard({ id: 'apple', translation: 'Apple', exampleSentence: 'I eat an apple.' });
const dog = flashcard({ id: 'dog', translation: 'Dog', exampleSentence: null, imageUrl: 'https://blob.test/dog.png' });

async function renderScreen() {
  await render(
    <LanguageProvider>
      <ReviewScreen />
    </LanguageProvider>,
  );
}

async function revealCard() {
  await fireEvent.press(screen.getByTestId('flashcard'));
}

describe('ReviewScreen', () => {
  beforeEach(() => {
    getDueCards.mockReset();
    submitReview.mockReset();
    submitReview.mockImplementation((review) => Promise.resolve(reviewState(review.cardId)));
  });

  it('shows a loading state while due cards are fetched', async () => {
    const pending = deferred<DueCardsResponse>();
    getDueCards.mockReturnValue(pending.promise);

    await renderScreen();

    expect(screen.getByText(strings.review.loading)).toBeOnTheScreen();
    expect(getDueCards).toHaveBeenCalledWith('en');
  });

  it('runs the full flow: image, reveal, rate, next card, session finished', async () => {
    getDueCards.mockResolvedValue(dueResponse([apple, dog], 5));

    await renderScreen();

    // First card: only the image.
    expect(await screen.findByTestId('flashcard-image')).toHaveProp('source', { uri: apple.imageUrl });
    expect(screen.getByText(strings.review.pending(5))).toBeOnTheScreen();
    expect(screen.getByText('0 / 2')).toBeOnTheScreen();
    expect(screen.queryByText('Apple')).not.toBeOnTheScreen();
    expect(screen.queryByTestId('rate-Good')).not.toBeOnTheScreen();

    // Tap: translation and rating buttons appear.
    await revealCard();
    expect(screen.getByText('Apple')).toBeOnTheScreen();
    expect(screen.getByText('I eat an apple.')).toBeOnTheScreen();

    // Rate: the review is sent and the next card appears hidden.
    await fireEvent.press(screen.getByText(strings.ratings.Good));
    expect(submitReview).toHaveBeenCalledWith({ cardId: 'apple', rating: 'Good' });
    expect(await screen.findByText('1 / 2')).toBeOnTheScreen();
    expect(screen.getByTestId('flashcard-image')).toHaveProp('source', { uri: dog.imageUrl });
    expect(screen.queryByText('Dog')).not.toBeOnTheScreen();
    expect(screen.getByText(strings.review.pending(4))).toBeOnTheScreen();

    // Last card: the session ends.
    await revealCard();
    await fireEvent.press(screen.getByText(strings.ratings.Again));
    expect(submitReview).toHaveBeenLastCalledWith({ cardId: 'dog', rating: 'Again' });
    expect(await screen.findByText(strings.review.completedTitle)).toBeOnTheScreen();
    expect(screen.getByText(strings.review.completedMessage(2))).toBeOnTheScreen();
  });

  it('can look for more cards after finishing', async () => {
    getDueCards.mockResolvedValueOnce(dueResponse([apple])).mockResolvedValueOnce(dueResponse([dog]));
    await renderScreen();
    await screen.findByTestId('flashcard');
    await revealCard();
    await fireEvent.press(screen.getByText(strings.ratings.Easy));
    await screen.findByTestId('review-completed');

    await fireEvent.press(screen.getByText(strings.review.checkAgain));

    await revealCard();
    expect(await screen.findByText('Dog')).toBeOnTheScreen();
    expect(getDueCards).toHaveBeenCalledTimes(2);
  });

  it('shows an empty state when nothing is due', async () => {
    getDueCards.mockResolvedValue(dueResponse([]));

    await renderScreen();

    expect(await screen.findByText(strings.review.emptyTitle)).toBeOnTheScreen();
    expect(screen.queryByTestId('flashcard')).not.toBeOnTheScreen();
  });

  it('shows the error and recovers when the user retries', async () => {
    getDueCards.mockRejectedValueOnce(new NetworkError('unreachable')).mockResolvedValueOnce(dueResponse([apple]));

    await renderScreen();

    expect(await screen.findByText(strings.errors.unreachable)).toBeOnTheScreen();
    await fireEvent.press(screen.getByRole('button', { name: strings.errors.retry }));

    expect(await screen.findByTestId('flashcard')).toBeOnTheScreen();
    expect(getDueCards).toHaveBeenCalledTimes(2);
  });

  it('keeps the card and explains the problem when a review cannot be saved', async () => {
    getDueCards.mockResolvedValue(dueResponse([apple, dog]));
    submitReview.mockRejectedValueOnce(new ApiError(503, null));
    await renderScreen();
    await screen.findByTestId('flashcard');
    await revealCard();

    await fireEvent.press(screen.getByText(strings.ratings.Hard));

    expect(await screen.findByText(new RegExp(strings.errors.reviewNotSaved))).toBeOnTheScreen();
    expect(screen.getByText('Apple')).toBeOnTheScreen();
    expect(screen.getByText('0 / 2')).toBeOnTheScreen();

    // Retrying the rating succeeds and moves on.
    await fireEvent.press(screen.getByText(strings.ratings.Hard));
    expect(await screen.findByText('1 / 2')).toBeOnTheScreen();
    expect(submitReview).toHaveBeenCalledTimes(2);
  });

  it('reloads due cards in the newly selected language', async () => {
    getDueCards.mockImplementation((language) =>
      Promise.resolve(dueResponse([flashcard({ id: language, translation: language === 'es' ? 'Manzana' : 'Apple', language })])),
    );
    await renderScreen();
    await screen.findByTestId('flashcard');

    await fireEvent.press(screen.getByRole('radio', { name: 'Español' }));
    await screen.findByTestId('flashcard');
    await revealCard();

    expect(getDueCards).toHaveBeenLastCalledWith('es');
    expect(await screen.findByText('Manzana')).toBeOnTheScreen();
  });
});
