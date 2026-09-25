import { Pressable, StyleSheet, Text, View } from 'react-native';

import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';
import { REVIEW_RATINGS, type ReviewRating } from '../types/api';

interface ReviewButtonsProps {
  onRate: (rating: ReviewRating) => void;
  disabled?: boolean;
}

export function ReviewButtons({ onRate, disabled = false }: ReviewButtonsProps) {
  return (
    <View style={styles.row}>
      {REVIEW_RATINGS.map((rating) => (
        <Pressable
          key={rating}
          testID={`rate-${rating}`}
          onPress={() => onRate(rating)}
          disabled={disabled}
          accessibilityRole="button"
          accessibilityState={{ disabled }}
          style={({ pressed }) => [
            styles.button,
            { backgroundColor: colors.ratings[rating] },
            (pressed || disabled) && styles.dimmed,
          ]}
        >
          <Text style={styles.label}>{strings.ratings[rating]}</Text>
        </Pressable>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    gap: spacing.sm,
  },
  button: {
    flex: 1,
    paddingVertical: spacing.md,
    borderRadius: radius.md,
    alignItems: 'center',
  },
  dimmed: {
    opacity: 0.6,
  },
  label: {
    color: colors.onPrimary,
    fontWeight: '600',
    fontSize: 15,
  },
});
