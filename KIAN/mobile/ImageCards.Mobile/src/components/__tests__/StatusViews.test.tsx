import { fireEvent, render, screen } from '@testing-library/react-native';

import { strings } from '../../constants/strings';
import { ErrorView } from '../ErrorView';
import { LoadingView } from '../LoadingView';
import { ProgressBar } from '../ProgressBar';

describe('LoadingView', () => {
  it('shows a spinner with the given message', async () => {
    await render(<LoadingView message="Cargando tarjetas…" />);

    expect(screen.getByTestId('loading-view')).toBeOnTheScreen();
    expect(screen.getByText('Cargando tarjetas…')).toBeOnTheScreen();
  });
});

describe('ErrorView', () => {
  it('shows the error message and retries on demand', async () => {
    const onRetry = jest.fn();
    await render(<ErrorView message="No se pudo conectar con el servidor." onRetry={onRetry} />);

    expect(screen.getByRole('alert')).toHaveTextContent(/No se pudo conectar con el servidor\./);

    await fireEvent.press(screen.getByRole('button', { name: strings.errors.retry }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it('has no retry button when no retry action is given', async () => {
    await render(<ErrorView message="Error" />);

    expect(screen.queryByRole('button')).not.toBeOnTheScreen();
  });
});

describe('ProgressBar', () => {
  it('exposes progress to accessibility and shows it as text', async () => {
    await render(<ProgressBar current={2} total={5} />);

    expect(screen.getByRole('progressbar')).toHaveAccessibilityValue({ min: 0, max: 5, now: 2 });
    expect(screen.getByText('2 / 5')).toBeOnTheScreen();
    expect(screen.getByTestId('progress-fill')).toHaveStyle({ width: '40%' });
  });

  it('clamps values outside the valid range', async () => {
    await render(<ProgressBar current={9} total={3} />);

    expect(screen.getByText('3 / 3')).toBeOnTheScreen();
    expect(screen.getByTestId('progress-fill')).toHaveStyle({ width: '100%' });
  });

  it('handles an empty session', async () => {
    await render(<ProgressBar current={0} total={0} />);

    expect(screen.getByTestId('progress-fill')).toHaveStyle({ width: '0%' });
  });
});
