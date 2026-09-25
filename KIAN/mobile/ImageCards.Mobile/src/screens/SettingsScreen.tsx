import { StyleSheet, Text, View } from 'react-native';

import { LanguageSelector } from '../components';
import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';
import { getApiBaseUrl } from '../config/env';
import { useLanguage } from '../hooks/useLanguage';

function describeApiUrl(): string {
  try {
    return getApiBaseUrl();
  } catch {
    return strings.settings.apiNotConfigured;
  }
}

/** Initial settings. More options (account, notifications, ...) will be added here. */
export function SettingsScreen() {
  const { language, setLanguage } = useLanguage();

  return (
    <View style={styles.screen}>
      <View style={styles.section}>
        <Text style={styles.title}>{strings.settings.languageTitle}</Text>
        <Text style={styles.hint}>{strings.settings.languageHint}</Text>
        <LanguageSelector value={language} onChange={setLanguage} />
      </View>

      <View style={styles.section}>
        <Text style={styles.title}>{strings.settings.apiTitle}</Text>
        <Text style={styles.hint} selectable>
          {describeApiUrl()}
        </Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: colors.background,
    padding: spacing.md,
    gap: spacing.md,
  },
  section: {
    backgroundColor: colors.surface,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    gap: spacing.sm,
  },
  title: {
    fontSize: 17,
    fontWeight: '600',
    color: colors.text,
  },
  hint: {
    color: colors.textMuted,
  },
});
