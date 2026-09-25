import { FlatList, Image, StyleSheet, Text, View } from 'react-native';

import { ErrorView, LanguageSelector, LoadingView, MessageView } from '../components';
import { strings } from '../constants/strings';
import { colors, radius, spacing } from '../constants/theme';
import { useCards } from '../hooks/useCards';
import { useLanguage } from '../hooks/useLanguage';
import type { Card } from '../types/api';
import type { LanguageCode } from '../types/language';

/** Read-only list of the user's cards. Card creation is available through the API (Swagger) for now. */
export function CardsScreen() {
  const { language, setLanguage } = useLanguage();
  const { state, reload, refresh } = useCards(language);

  return (
    <View style={styles.screen}>
      <View style={styles.header}>
        <LanguageSelector value={language} onChange={setLanguage} />
      </View>

      {state.status === 'loading' && <LoadingView message={strings.cards.loading} />}
      {state.status === 'error' && <ErrorView message={state.message} onRetry={reload} />}
      {state.status === 'loaded' && (
        <FlatList
          data={state.data.items}
          keyExtractor={(card) => card.id}
          renderItem={({ item }) => <CardRow card={item} language={language} />}
          contentContainerStyle={styles.list}
          refreshing={state.refreshing}
          onRefresh={refresh}
          ListHeaderComponent={<Text style={styles.total}>{strings.cards.total(state.data.totalCount)}</Text>}
          ListEmptyComponent={<MessageView title={strings.cards.empty} testID="cards-empty" />}
        />
      )}
    </View>
  );
}

function CardRow({ card, language }: { card: Card; language: LanguageCode }) {
  const translation = card.translations.find((t) => t.language === language) ?? card.translations[0];
  return (
    <View style={styles.row} testID={`card-${card.id}`}>
      <Image source={{ uri: card.imageUrl }} style={styles.thumbnail} accessibilityLabel={strings.review.imageLabel} />
      <View style={styles.rowText}>
        <Text style={styles.translation}>{translation?.translatedText}</Text>
        {translation?.exampleSentence ? (
          <Text style={styles.example} numberOfLines={2}>
            {translation.exampleSentence}
          </Text>
        ) : null}
      </View>
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
  list: {
    padding: spacing.md,
    gap: spacing.sm,
    flexGrow: 1,
  },
  total: {
    color: colors.textMuted,
    marginBottom: spacing.xs,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: spacing.md,
    padding: spacing.sm,
    backgroundColor: colors.surface,
    borderRadius: radius.md,
    borderWidth: 1,
    borderColor: colors.border,
  },
  thumbnail: {
    width: 64,
    height: 64,
    borderRadius: radius.sm,
    backgroundColor: colors.background,
  },
  rowText: {
    flex: 1,
    gap: spacing.xs,
  },
  translation: {
    fontSize: 17,
    fontWeight: '600',
    color: colors.text,
  },
  example: {
    color: colors.textMuted,
  },
});
