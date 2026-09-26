import { useCallback, useEffect, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  RefreshControl,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import PostCard from '../components/PostCard';
import { api } from '../src/api';
import { mapPost } from '../src/format';
import useRefreshOnFocus from '../src/useRefreshOnFocus';

// Pantalla "Guardados" (mockup #screen-bookmarks): GET /api/me/bookmarks
export default function GuardadosScreen({ navigation }) {
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setError('');
    try {
      const data = await api.getMyBookmarks(1, 50);
      setList(data.items.map(mapPost));
    } catch (e) {
      setError(`No se pudieron cargar tus guardados: ${e.message}`);
    }
  }, []);

  // Refresca al volver (p. ej. tras quitar un guardado desde el Detalle)
  useRefreshOnFocus(load);

  // Primera carga
  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const onRefresh = () => {
    setRefreshing(true);
    load().finally(() => setRefreshing(false));
  };

  return (
    <View style={styles.container}>
      <AppHeader title="Guardados" />

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      ) : error ? (
        <View style={styles.center}>
          <Ionicons name="cloud-offline-outline" size={40} color={colors.gray400} />
          <Text style={styles.message}>{error}</Text>
        </View>
      ) : list.length === 0 ? (
        <View style={styles.center}>
          <Ionicons name="bookmark-outline" size={40} color={colors.gray400} />
          <Text style={styles.message}>Guarda publicaciones para verlas aquí.</Text>
        </View>
      ) : (
        <ScrollView
          contentContainerStyle={styles.list}
          refreshControl={
            <RefreshControl refreshing={refreshing} onRefresh={onRefresh} colors={[colors.primary]} />
          }
        >
          {list.map((post) => (
            <PostCard
              key={post.id}
              post={post}
              onPress={() => navigation.navigate('Detalle', { postId: post.id })}
            />
          ))}
        </ScrollView>
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.gray100,
  },
  list: {
    padding: 12,
    paddingBottom: 24,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    padding: 24,
  },
  message: {
    color: colors.gray500,
    textAlign: 'center',
  },
});
