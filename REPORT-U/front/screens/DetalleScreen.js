import { useState } from 'react';
import { View, Text, ScrollView, Pressable, Image, TextInput, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import { TypeBadge, CategoryChip } from '../components/PostBadges';
import { getPost, posts, detailUrl } from '../data/posts';

// Pantalla de Detalle (mockup #screen-detail)
export default function DetalleScreen({ route, navigation }) {
  const post = getPost(route.params?.postId) || posts[0];

  // En el mockup el botón de soporte arranca activo con el conteo base
  const [supported, setSupported] = useState(true);
  const [saved, setSaved] = useState(false);

  const supportCount = post.supports - (supported ? 0 : 1);

  return (
    <View style={styles.container}>
      <AppHeader title="Detalle" onBack={() => navigation.goBack()} />

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.meta}>
          <TypeBadge type={post.type} />
          <CategoryChip category={post.category} />
        </View>

        <Text style={styles.title}>{post.title}</Text>

        <View style={styles.info}>
          <View style={styles.infoItem}>
            <Ionicons name="person-outline" size={14} color={colors.gray500} />
            <Text style={styles.infoText}>{post.author}</Text>
          </View>
          {post.location ? (
            <View style={styles.infoItem}>
              <Ionicons name="location-outline" size={14} color={colors.gray500} />
              <Text style={styles.infoText}>{post.location}</Text>
            </View>
          ) : null}
          <View style={styles.infoItem}>
            <Ionicons name="time-outline" size={14} color={colors.gray500} />
            <Text style={styles.infoText}>{post.date}</Text>
          </View>
        </View>

        {post.images && post.images.length > 0 ? (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            style={styles.images}
            contentContainerStyle={styles.imagesContent}
          >
            {post.images.map((seed) => (
              <Pressable key={seed} onPress={() => navigation.navigate('VisorImagen', { seed })}>
                <Image source={{ uri: detailUrl(seed) }} style={styles.image} />
              </Pressable>
            ))}
          </ScrollView>
        ) : null}

        <View style={styles.body}>
          {post.body.split('\n\n').map((paragraph, index) => (
            <Text key={index} style={styles.paragraph}>
              {paragraph}
            </Text>
          ))}
        </View>

        <View style={styles.actions}>
          <Pressable
            style={[styles.pill, supported && styles.pillSupportActive]}
            onPress={() => setSupported((s) => !s)}
          >
            <Ionicons
              name="thumbs-up"
              size={16}
              color={supported ? colors.primary : colors.gray600}
            />
            <Text style={[styles.pillText, supported && styles.pillTextSupport]}>
              {supportCount}
            </Text>
          </Pressable>

          <Pressable
            style={[styles.pill, saved && styles.pillBookmarkActive]}
            onPress={() => setSaved((s) => !s)}
          >
            <Ionicons
              name={saved ? 'bookmark' : 'bookmark-outline'}
              size={16}
              color={saved ? colors.queja : colors.gray600}
            />
            <Text style={styles.pillText}>{saved ? 'Guardado' : 'Guardar'}</Text>
          </Pressable>
        </View>

        <View style={styles.comments}>
          <Text style={styles.commentsTitle}>Comentarios ({post.comments.length})</Text>

          {post.comments.map((comment, index) => (
            <View key={index} style={styles.comment}>
              <View style={styles.commentHeader}>
                <Text style={styles.commentAuthor}>{comment.author}</Text>
                <Text style={styles.commentDate}>{comment.date}</Text>
              </View>
              <Text style={styles.commentText}>{comment.text}</Text>
            </View>
          ))}

          <View style={styles.commentInput}>
            <TextInput
              style={styles.commentField}
              placeholder="Escribe un comentario..."
              placeholderTextColor={colors.gray400}
            />
            <Pressable style={styles.sendBtn}>
              <Text style={styles.sendText}>Enviar</Text>
            </Pressable>
          </View>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.gray100,
  },
  content: {
    padding: 20,
    paddingBottom: 40,
  },
  meta: {
    flexDirection: 'row',
    gap: 8,
    marginBottom: 12,
  },
  title: {
    fontSize: 22,
    fontWeight: '800',
    color: colors.gray900,
    marginBottom: 12,
  },
  info: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
    marginBottom: 16,
  },
  infoItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  infoText: {
    fontSize: 13,
    color: colors.gray500,
  },
  images: {
    marginBottom: 16,
  },
  imagesContent: {
    gap: 8,
  },
  image: {
    width: 280,
    height: 200,
    borderRadius: 12,
    backgroundColor: colors.gray200,
  },
  body: {
    marginBottom: 16,
  },
  paragraph: {
    fontSize: 15,
    color: colors.gray700,
    lineHeight: 25,
    marginBottom: 10,
  },
  actions: {
    flexDirection: 'row',
    gap: 10,
    marginBottom: 24,
  },
  pill: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 24,
    borderWidth: 1.5,
    borderColor: colors.gray300,
    backgroundColor: colors.white,
  },
  pillText: {
    fontSize: 15,
    fontWeight: '600',
    color: colors.gray700,
  },
  pillSupportActive: {
    backgroundColor: colors.primaryLight,
    borderColor: colors.primary,
  },
  pillTextSupport: {
    color: colors.primary,
  },
  pillBookmarkActive: {
    backgroundColor: '#fef3c7',
    borderColor: colors.queja,
  },
  comments: {
    borderTopWidth: 1,
    borderTopColor: colors.gray200,
    paddingTop: 20,
  },
  commentsTitle: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.gray800,
    marginBottom: 16,
  },
  comment: {
    backgroundColor: colors.gray50,
    borderRadius: 8,
    paddingVertical: 12,
    paddingHorizontal: 14,
    marginBottom: 10,
  },
  commentHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 6,
  },
  commentAuthor: {
    fontSize: 13,
    fontWeight: '700',
    color: colors.gray800,
  },
  commentDate: {
    fontSize: 13,
    color: colors.gray400,
  },
  commentText: {
    fontSize: 14,
    color: colors.gray600,
    lineHeight: 21,
  },
  commentInput: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginTop: 16,
  },
  commentField: {
    flex: 1,
    borderWidth: 1.5,
    borderColor: colors.gray300,
    borderRadius: 24,
    paddingHorizontal: 14,
    paddingVertical: 10,
    fontSize: 14,
    backgroundColor: colors.white,
    color: colors.gray900,
  },
  sendBtn: {
    backgroundColor: colors.primary,
    paddingVertical: 10,
    paddingHorizontal: 18,
    borderRadius: 24,
  },
  sendText: {
    color: colors.white,
    fontSize: 14,
    fontWeight: '600',
  },
});
