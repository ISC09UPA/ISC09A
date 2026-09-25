import { StyleSheet, Text, View } from 'react-native';

import { strings } from '../constants/strings';
import { colors, spacing } from '../constants/theme';
import { PrimaryButton } from './PrimaryButton';

interface ErrorViewProps {
  message: string;
  onRetry?: () => void;
}

export function ErrorView({ message, onRetry }: ErrorViewProps) {
  return (
    <View style={styles.container} testID="error-view">
      <Text style={styles.title}>{strings.errors.title}</Text>
      <Text style={styles.message} accessibilityRole="alert">
        {message}
      </Text>
      {onRetry ? <PrimaryButton label={strings.errors.retry} onPress={onRetry} testID="retry-button" /> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: spacing.md,
    padding: spacing.lg,
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    color: colors.danger,
  },
  message: {
    fontSize: 16,
    color: colors.text,
    textAlign: 'center',
  },
});
