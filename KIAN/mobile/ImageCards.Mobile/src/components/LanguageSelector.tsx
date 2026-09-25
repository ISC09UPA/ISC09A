import { Pressable, ScrollView, StyleSheet, Text } from 'react-native';

import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';
import { SUPPORTED_LANGUAGES, type LanguageCode } from '../types/language';

interface LanguageSelectorProps {
  value: LanguageCode;
  onChange: (language: LanguageCode) => void;
  disabled?: boolean;
}

export function LanguageSelector({ value, onChange, disabled = false }: LanguageSelectorProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      contentContainerStyle={styles.row}
      accessibilityRole="radiogroup"
      accessibilityLabel={strings.language.selectorLabel}
    >
      {SUPPORTED_LANGUAGES.map(({ code, label }) => {
        const selected = code === value;
        return (
          <Pressable
            key={code}
            testID={`language-${code}`}
            onPress={() => {
              if (!selected) {
                onChange(code);
              }
            }}
            disabled={disabled}
            accessibilityRole="radio"
            accessibilityLabel={label}
            accessibilityState={{ selected, disabled }}
            style={[styles.chip, selected && styles.chipSelected]}
          >
            <Text style={[styles.code, selected && styles.textSelected]}>{code.toUpperCase()}</Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  row: {
    gap: spacing.sm,
    paddingVertical: spacing.xs,
  },
  chip: {
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    backgroundColor: colors.surface,
  },
  chipSelected: {
    backgroundColor: colors.primary,
    borderColor: colors.primary,
  },
  code: {
    fontWeight: '600',
    color: colors.text,
  },
  textSelected: {
    color: colors.onPrimary,
  },
});
