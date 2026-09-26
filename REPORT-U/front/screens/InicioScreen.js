import { useCallback, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  Pressable,
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

// Pantalla de Feed (mockup #screen-feed): GET /api/posts?sort=recent|popular
export default function InicioScreen({ navigation }) {
  const [tab, setTab] = useState('recientes');
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(
    async (sort) => {
      setError('');
      try {
        const data = await api.getFeed(sort === 'populares' ? 'popular' : 'recent', 1, 50);
        setList(data.items.map(mapPost));
      } catch (e) {
        setError(`No se pudo cargar el feed: ${e.message}`);
      }
    },
    []
  );

  // Carga al montar, al cambiar de pestaña y cada vez que la pantalla
  // recupera el foco (p. ej. tras crear una publicación).
  useFocusEffect(
    useCallback(() => {
      load(tab).finally(() => setLoading(false));
    }, [tab, load])
  );

  const onRefresh = () => {
    setRefreshing(true);
    load(tab).finally(() => setRefreshing(false));
  };

  return (
    <View style={styles.container}>
      <AppHeader title="ReportU" />

      <View style={styles.tabs}>
        <Pressable
          style={[styles.tab, tab === 'recientes' && styles.tabActive]}
          onPress={() => setTab('recientes')}
        >
          <Text style={[styles.tabText, tab === 'recientes' && styles.tabTextActive]}>
            Recientes
          </Text>
        </Pressable>
        <Pressable
          style={[styles.tab, tab === 'populares' && styles.tabActive]}
          onPress={() => setTab('populares')}
        >
          <Text style={[styles.tabText, tab === 'populares' && styles.tabTextActive]}>
            Populares
          </Text>
        </Pressable>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={colors.primary} />
        </View>
      ) : error ? (
        <View style={styles.center}>
          <Ionicons name="cloud-offline-outline" size={40} color={colors.gray400} />
          <Text style={styles.errorText}>{error}</Text>
          <Pressable style={styles.retryBtn} onPress={() => load(tab)}>
            <Text style={styles.retryText}>Reintentar</Text>
          </Pressable>
        </View>
      ) : list.length === 0 ? (
        <View style={styles.center}>
          <Ionicons name="megaphone-outline" size={40} color={colors.gray400} />
          <Text style={styles.emptyText}>Aún no hay publicaciones. ¡Crea la primera!</Text>
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

      <Pressable style={styles.fab} onPress={() => navigation.navigate('Crear')}>
        <Ionicons name="add" size={28} color={colors.white} />
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.gray100,
  },
  tabs: {
    flexDirection: 'row',
    backgroundColor: colors.white,
    borderBottomWidth: 1,
    borderBottomColor: colors.gray200,
  },
  tab: {
    flex: 1,
    paddingVertical: 14,
    alignItems: 'center',
    borderBottomWidth: 3,
    borderBottomColor: 'transparent',
  },
  tabActive: {
    borderBottomColor: colors.primary,
  },
  tabText: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.gray400,
  },
  tabTextActive: {
    color: colors.primary,
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
  errorText: {
    color: colors.gray600,
    textAlign: 'center',
  },
  retryBtn: {
    borderWidth: 1.5,
    borderColor: colors.primary,
    borderRadius: 8,
    paddingVertical: 8,
    paddingHorizontal: 20,
  },
  retryText: {
    color: colors.primary,
    fontWeight: '600',
  },
  emptyText: {
    color: colors.gray500,
    textAlign: 'center',
  },
  fab: {
    position: 'absolute',
    right: 20,
    bottom: 20,
    width: 56,
    height: 56,
    borderRadius: 28,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
    shadowColor: '#2563eb',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.4,
    shadowRadius: 12,
    elevation: 6,
  },
});
