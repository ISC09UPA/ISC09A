import { Image, Pressable, StyleSheet, Text, View } from 'react-native';

import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';
import type { Flashcard as FlashcardData } from '../types/api';

interface FlashcardProps {
  card: FlashcardData;
  revealed: boolean;
  onReveal: () => void;
}

/**
 * Front: the image. Back (after tapping): the image plus translation and example.
 * Controlled component: the parent owns the `revealed` state.
 */
export function Flashcard({ card, revealed, onReveal }: FlashcardProps) {
  return (
    <Pressable
      testID="flashcard"
      onPress={onReveal}
      disabled={revealed}
      accessibilityRole="button"
      accessibilityHint={revealed ? undefined : strings.review.revealHint}
      accessibilityState={{ expanded: revealed }}
      style={({ pressed }) => [styles.card, pressed && !revealed && styles.pressed]}
    >
      <Image
        testID="flashcard-image"
        source={{ uri: card.imageUrl }}
        style={styles.image}
        resizeMode="contain"
        accessibilityLabel={strings.review.imageLabel}
      />

      {revealed ? (
        <View style={styles.back} testID="flashcard-back">
          <Text style={styles.translation} accessibilityRole="header">
            {card.translation}
          </Text>
          {card.exampleSentence ? <Text style={styles.example}>{card.exampleSentence}</Text> : null}
        </View>
      ) : (
        <Text style={styles.hint}>{strings.review.tapToReveal}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border,
    padding: spacing.md,
    alignItems: 'center',
    gap: spacing.md,
  },
  pressed: {
    opacity: 0.85,
  },
  image: {
    width: '100%',
    aspectRatio: 1,
    borderRadius: radius.md,
    backgroundColor: colors.background,
  },
  back: {
    alignItems: 'center',
    gap: spacing.sm,
  },
  translation: {
    fontSize: 28,
    fontWeight: '700',
    color: colors.text,
    textAlign: 'center',
  },
  example: {
    fontSize: 16,
    fontStyle: 'italic',
    color: colors.textMuted,
    textAlign: 'center',
  },
  hint: {
    fontSize: 15,
    color: colors.textMuted,
  },
});
