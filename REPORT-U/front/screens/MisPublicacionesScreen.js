import { useCallback, useState } from 'react';
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

// Pantalla "Mis publicaciones" (mockup #screen-myposts): GET /api/me/posts
export default function MisPublicacionesScreen({ navigation }) {
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    setError('');
    try {
      const data = await api.getMyPosts(1, 50);
      setList(data.items.map(mapPost));
    } catch (e) {
      setError(`No se pudieron cargar tus publicaciones: ${e.message}`);
    }
  }, []);

  useFocusEffect(
    useCallback(() => {
      // Se recarga cada vez que la pantalla toma foco (al entrar y al volver
      // de Editar tras guardar o eliminar una publicación).
      load().finally(() => setLoading(false));
    }, [load])
  );

  const onRefresh = () => {
    setRefreshing(true);
    load().finally(() => setRefreshing(false));
  };

  return (
    <View style={styles.container}>
      <AppHeader title="Mis publicaciones" onBack={() => navigation.goBack()} />

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
          <Ionicons name="document-text-outline" size={40} color={colors.gray400} />
          <Text style={styles.message}>Todavía no has publicado nada.</Text>
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
              showAuthor={false}
              showComments
              onPress={() => navigation.navigate('Editar', { postId: post.id })}
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
