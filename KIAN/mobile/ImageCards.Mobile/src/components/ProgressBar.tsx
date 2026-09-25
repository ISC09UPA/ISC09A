import { StyleSheet, Text, View } from 'react-native';

import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';

interface ProgressBarProps {
  /** Cards already reviewed in this session. */
  current: number;
  total: number;
}

export function ProgressBar({ current, total }: ProgressBarProps) {
  const safeTotal = Math.max(total, 0);
  const clamped = Math.min(Math.max(current, 0), safeTotal);
  const ratio = safeTotal === 0 ? 0 : clamped / safeTotal;

  return (
    <View
      style={styles.container}
      testID="progress-bar"
      accessible
      accessibilityRole="progressbar"
      accessibilityValue={{ min: 0, max: safeTotal, now: clamped }}
    >
      <View style={styles.track}>
        <View style={[styles.fill, { width: `${ratio * 100}%` }]} testID="progress-fill" />
      </View>
      <Text style={styles.label}>{strings.review.progress(clamped, safeTotal)}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.sm,
  },
  track: {
    flex: 1,
    height: 8,
    borderRadius: radius.sm,
    backgroundColor: colors.border,
    overflow: 'hidden',
  },
  fill: {
    height: '100%',
    backgroundColor: colors.primary,
  },
  label: {
    minWidth: 48,
    textAlign: 'right',
    color: colors.textMuted,
    fontVariant: ['tabular-nums'],
  },
});
