import { View, Text, ScrollView, StyleSheet } from 'react-native';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import PostCard from '../components/PostCard';
import { getPost, myPostIds } from '../data/posts';

// Pantalla "Mis publicaciones" (mockup #screen-myposts)
export default function MisPublicacionesScreen({ navigation }) {
  const myPosts = myPostIds.map(getPost).filter(Boolean);

  return (
    <View style={styles.container}>
      <AppHeader title="Mis publicaciones" onBack={() => navigation.goBack()} />

      <ScrollView contentContainerStyle={styles.list}>
        {myPosts.map((post) => (
          <PostCard
            key={post.id}
            post={post}
            showAuthor={false}
            showComments
            onPress={() => navigation.navigate('Editar', { postId: post.id })}
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
