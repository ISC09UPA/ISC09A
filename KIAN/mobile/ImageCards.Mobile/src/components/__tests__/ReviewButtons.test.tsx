import { fireEvent, render, screen } from '@testing-library/react-native';

import { strings } from '../../constants/strings';
import { REVIEW_RATINGS } from '../../types/api';
import { ReviewButtons } from '../ReviewButtons';

describe('ReviewButtons', () => {
  it('shows the four ratings in order', async () => {
    await render(<ReviewButtons onRate={jest.fn()} />);

    expect(screen.getAllByRole('button')).toHaveLength(4);
    for (const rating of REVIEW_RATINGS) {
      expect(screen.getByTestId(`rate-${rating}`)).toHaveTextContent(strings.ratings[rating]);
    }
  });

  it.each(REVIEW_RATINGS)('reports %s when its button is pressed', async (rating) => {
    const onRate = jest.fn();
    await render(<ReviewButtons onRate={onRate} />);

    await fireEvent.press(screen.getByText(strings.ratings[rating]));

    expect(onRate).toHaveBeenCalledTimes(1);
    expect(onRate).toHaveBeenCalledWith(rating);
  });

  it('ignores presses while disabled', async () => {
    const onRate = jest.fn();
    await render(<ReviewButtons onRate={onRate} disabled />);

    await fireEvent.press(screen.getByTestId('rate-Good'));

    expect(onRate).not.toHaveBeenCalled();
    expect(screen.getByTestId('rate-Good')).toBeDisabled();
  });
});
