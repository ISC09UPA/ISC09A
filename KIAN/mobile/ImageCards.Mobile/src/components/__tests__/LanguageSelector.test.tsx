import { fireEvent, render, screen } from '@testing-library/react-native';

import { SUPPORTED_LANGUAGES } from '../../types/language';
import { LanguageSelector } from '../LanguageSelector';

describe('LanguageSelector', () => {
  it('offers every supported language, not only English', async () => {
    await render(<LanguageSelector value="en" onChange={jest.fn()} />);

    expect(screen.getAllByRole('radio')).toHaveLength(SUPPORTED_LANGUAGES.length);
    for (const code of ['en', 'es', 'fr', 'de', 'it', 'pt']) {
      expect(screen.getByTestId(`language-${code}`)).toBeOnTheScreen();
    }
  });

  it('marks only the current language as selected', async () => {
    await render(<LanguageSelector value="fr" onChange={jest.fn()} />);

    expect(screen.getByRole('radio', { name: 'Français' })).toBeSelected();
    expect(screen.getByRole('radio', { name: 'English' })).not.toBeSelected();
  });

  it('reports the chosen language code', async () => {
    const onChange = jest.fn();
    await render(<LanguageSelector value="en" onChange={onChange} />);

    await fireEvent.press(screen.getByRole('radio', { name: 'Deutsch' }));

    expect(onChange).toHaveBeenCalledWith('de');
  });

  it('does not report a change when the selected language is pressed again', async () => {
    const onChange = jest.fn();
    await render(<LanguageSelector value="en" onChange={onChange} />);

    await fireEvent.press(screen.getByRole('radio', { name: 'English' }));

    expect(onChange).not.toHaveBeenCalled();
  });

  it('ignores presses while disabled', async () => {
    const onChange = jest.fn();
    await render(<LanguageSelector value="en" onChange={onChange} disabled />);

    await fireEvent.press(screen.getByRole('radio', { name: 'Español' }));

    expect(onChange).not.toHaveBeenCalled();
  });
});
