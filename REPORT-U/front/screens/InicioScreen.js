import { useState } from 'react';
import { View, Text, ScrollView, Pressable, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import PostCard from '../components/PostCard';
import { posts } from '../data/posts';

// Pantalla de Feed (mockup #screen-feed)
export default function InicioScreen({ navigation }) {
  const [tab, setTab] = useState('recientes');

  // "Populares" ordena por soportes DESC, igual que el mockup
  const list =
    tab === 'recientes' ? posts : [...posts].sort((a, b) => b.supports - a.supports);

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

      <ScrollView contentContainerStyle={styles.list}>
        {list.map((post) => (
          <PostCard
            key={post.id}
            post={post}
            onPress={() => navigation.navigate('Detalle', { postId: post.id })}
          />
        ))}
      </ScrollView>

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
