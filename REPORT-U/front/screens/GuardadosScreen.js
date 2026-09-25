import { View, Text, ScrollView, StyleSheet } from 'react-native';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import PostCard from '../components/PostCard';
import { getPost, bookmarkIds } from '../data/posts';

// Pantalla "Guardados" (mockup #screen-bookmarks)
export default function GuardadosScreen({ navigation }) {
  const saved = bookmarkIds.map(getPost).filter(Boolean);

  return (
    <View style={styles.container}>
      <AppHeader title="Guardados" />

      <ScrollView contentContainerStyle={styles.list}>
        {saved.map((post) => (
          <PostCard
            key={post.id}
            post={post}
            onPress={() => navigation.navigate('Detalle', { postId: post.id })}
          />
        ))}
      </ScrollView>
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
});
