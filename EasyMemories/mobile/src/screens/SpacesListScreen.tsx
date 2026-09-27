import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useCallback, useState } from 'react';
import { FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { listMySpaces } from '../api/spaces';
import { extractErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { RootStackParamList } from '../navigation/types';
import { SpaceResponse } from '../types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'SpacesList'>;

export default function SpacesListScreen({ navigation }: Props) {
  const { displayName, logout } = useAuth();
  const [spaces, setSpaces] = useState<SpaceResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setError(null);
      setSpaces(await listMySpaces());
    } catch (err) {
      setError(extractErrorMessage(err, 'No se pudieron cargar tus espacios.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load])
  );

  async function handleLogout() {
    await logout();
    navigation.reset({ index: 0, routes: [{ name: 'Home' }] });
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <View>
          <Text style={styles.greeting}>Hola, {displayName}</Text>
          <Text style={styles.title}>Tus espacios</Text>
        </View>
        <Pressable onPress={handleLogout}>
          <Text style={styles.logout}>Salir</Text>
        </Pressable>
      </View>

      {error && <Text style={styles.error}>{error}</Text>}

      <FlatList
        data={spaces}
        keyExtractor={(item) => String(item.id)}
        refreshControl={<RefreshControl refreshing={loading} onRefresh={load} tintColor={colors.primary} />}
        contentContainerStyle={spaces.length === 0 && styles.emptyContainer}
        ListEmptyComponent={
          !loading ? <Text style={styles.emptyText}>Aún no has creado ningún espacio.</Text> : null
        }
        renderItem={({ item }) => (
          <Pressable
            style={styles.card}
            onPress={() => navigation.navigate('SpaceDetail', { spaceId: item.id })}
          >
            <Text style={styles.cardTitle}>{item.name}</Text>
            {item.description ? <Text style={styles.cardDescription}>{item.description}</Text> : null}
            <Text style={styles.cardMeta}>
              {item.memoryCount} recuerdo{item.memoryCount === 1 ? '' : 's'}
            </Text>
          </Pressable>
        )}
      />

      <Pressable style={styles.fab} onPress={() => navigation.navigate('CreateSpace')}>
        <Text style={styles.fabText}>+ Nuevo espacio</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, padding: spacing.lg },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
    marginBottom: spacing.lg,
  },
  greeting: { color: colors.textMuted, fontSize: 13 },
  title: { color: colors.text, fontSize: 26, fontWeight: '800' },
  logout: { color: colors.danger, fontWeight: '600' },
  error: { color: colors.danger, marginBottom: spacing.md },
  emptyContainer: { flexGrow: 1, justifyContent: 'center' },
  emptyText: { color: colors.textMuted, textAlign: 'center' },
  card: {
    backgroundColor: colors.surface,
    borderRadius: 14,
    padding: spacing.md,
    marginBottom: spacing.md,
  },
  cardTitle: { color: colors.text, fontSize: 18, fontWeight: '700' },
  cardDescription: { color: colors.textMuted, marginTop: spacing.xs },
  cardMeta: { color: colors.primary, marginTop: spacing.sm, fontWeight: '600' },
  fab: {
    backgroundColor: colors.primary,
    borderRadius: 14,
    padding: spacing.md,
    alignItems: 'center',
  },
  fabText: { color: '#1A1024', fontWeight: '700', fontSize: 16 },
});
