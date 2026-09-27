import React from 'react';
import { FlatList, Image, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { MemoryResponse } from '../types';
import { colors, spacing } from '../theme';

interface Props {
  memories: MemoryResponse[];
  refreshing: boolean;
  onRefresh: () => void;
  emptyText: string;
}

export default function MemoryGrid({ memories, refreshing, onRefresh, emptyText }: Props) {
  return (
    <FlatList
      data={memories}
      keyExtractor={(item) => String(item.id)}
      numColumns={2}
      columnWrapperStyle={styles.row}
      contentContainerStyle={memories.length === 0 ? styles.emptyContainer : styles.list}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor={colors.primary} />}
      ListEmptyComponent={!refreshing ? <Text style={styles.emptyText}>{emptyText}</Text> : null}
      renderItem={({ item }) => (
        <View style={styles.card}>
          <Image source={{ uri: item.photoUrl }} style={styles.photo} resizeMode="cover" />
          {item.comment ? <Text style={styles.comment}>{item.comment}</Text> : null}
          {item.guestName ? <Text style={styles.guestName}>— {item.guestName}</Text> : null}
        </View>
      )}
    />
  );
}

const styles = StyleSheet.create({
  list: { padding: spacing.md },
  row: { gap: spacing.sm },
  emptyContainer: { flexGrow: 1, alignItems: 'center', justifyContent: 'center' },
  emptyText: { color: colors.textMuted, textAlign: 'center', paddingHorizontal: spacing.lg },
  card: {
    flex: 1,
    backgroundColor: colors.surface,
    borderRadius: 14,
    marginBottom: spacing.sm,
    overflow: 'hidden',
  },
  photo: { width: '100%', aspectRatio: 1, backgroundColor: colors.border },
  comment: { color: colors.text, fontSize: 13, padding: spacing.sm, paddingBottom: 0 },
  guestName: { color: colors.textMuted, fontSize: 12, padding: spacing.sm, paddingTop: spacing.xs },
});
