import { ScrollView, StyleSheet, Text, View } from 'react-native';

import { ErrorView, Flashcard, LanguageSelector, LoadingView, MessageView, ProgressBar, ReviewButtons } from '../components';
import { strings } from '../constants/strings';
import { colors, spacing } from '../constants/theme';
import { useLanguage } from '../hooks/useLanguage';
import { useReviewSession } from '../hooks/useReviewSession';

export function ReviewScreen() {
  const { language, setLanguage } = useLanguage();
  const { state, currentCard, reveal, rate, reload } = useReviewSession(language);
  const submitting = state.status === 'reviewing' && state.submitting;

  return (
    <View style={styles.screen}>
      <View style={styles.header}>
        <LanguageSelector value={language} onChange={setLanguage} disabled={submitting} />
      </View>

      {state.status === 'loading' && <LoadingView message={strings.review.loading} />}

      {state.status === 'error' && <ErrorView message={state.message} onRetry={reload} />}

      {state.status === 'empty' && (
        <MessageView
          testID="review-empty"
          title={strings.review.emptyTitle}
          message={strings.review.emptyMessage}
          actionLabel={strings.review.checkAgain}
          onAction={reload}
        />
      )}

      {state.status === 'completed' && (
        <MessageView
          testID="review-completed"
          title={strings.review.completedTitle}
          message={strings.review.completedMessage(state.reviewed)}
          actionLabel={strings.review.checkAgain}
          onAction={reload}
        />
      )}

      {state.status === 'reviewing' && currentCard && (
        <ScrollView contentContainerStyle={styles.content}>
          <Text style={styles.pending} testID="pending-count">
            {strings.review.pending(state.totalDue - state.reviewed)}
          </Text>
          <ProgressBar current={state.reviewed} total={state.cards.length} />
          <Flashcard key={currentCard.id} card={currentCard} revealed={state.revealed} onReveal={reveal} />
          {state.revealed && <ReviewButtons onRate={rate} disabled={submitting} />}
          {state.submitError && (
            <Text style={styles.submitError} accessibilityRole="alert">
              {`${strings.errors.reviewNotSaved} ${state.submitError}`}
            </Text>
          )}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: colors.background,
  },
  header: {
    paddingHorizontal: spacing.md,
    paddingTop: spacing.sm,
  },
  content: {
    padding: spacing.md,
    gap: spacing.md,
  },
  pending: {
    color: colors.textMuted,
    fontSize: 14,
  },
  submitError: {
    color: colors.danger,
    textAlign: 'center',
  },
});
