import { fireEvent, render, screen } from '@testing-library/react-native';

import { strings } from '../../constants/strings';
import { flashcard } from '../../test-utils/fixtures';
import { Flashcard } from '../Flashcard';

describe('Flashcard', () => {
  it('shows only the image and a hint before being revealed', async () => {
    await render(<Flashcard card={flashcard()} revealed={false} onReveal={jest.fn()} />);

    expect(screen.getByTestId('flashcard-image')).toHaveProp('source', {
      uri: 'https://blob.test/card-images/1.png?sig=x',
    });
    expect(screen.getByText(strings.review.tapToReveal)).toBeOnTheScreen();
    expect(screen.queryByText('Apple')).not.toBeOnTheScreen();
    expect(screen.queryByText('I eat an apple every day.')).not.toBeOnTheScreen();
  });

  it('asks the parent to reveal when tapped', async () => {
    const onReveal = jest.fn();
    await render(<Flashcard card={flashcard()} revealed={false} onReveal={onReveal} />);

    await fireEvent.press(screen.getByTestId('flashcard'));

    expect(onReveal).toHaveBeenCalledTimes(1);
  });

  it('shows translation and example sentence once revealed, keeping the image', async () => {
    await render(<Flashcard card={flashcard()} revealed onReveal={jest.fn()} />);

    expect(screen.getByText('Apple')).toBeOnTheScreen();
    expect(screen.getByText('I eat an apple every day.')).toBeOnTheScreen();
    expect(screen.getByTestId('flashcard-image')).toBeOnTheScreen();
    expect(screen.queryByText(strings.review.tapToReveal)).not.toBeOnTheScreen();
  });

  it('omits the example sentence when the card has none', async () => {
    await render(<Flashcard card={flashcard({ exampleSentence: null })} revealed onReveal={jest.fn()} />);

    expect(screen.getByTestId('flashcard-back')).toHaveTextContent('Apple', { exact: true });
  });

  it('does not reveal again once already revealed', async () => {
    const onReveal = jest.fn();
    await render(<Flashcard card={flashcard()} revealed onReveal={onReveal} />);

    await fireEvent.press(screen.getByTestId('flashcard'));

    expect(onReveal).not.toHaveBeenCalled();
  });
});
