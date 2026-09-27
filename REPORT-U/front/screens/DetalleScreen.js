import { useCallback, useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  Pressable,
  Image,
  TextInput,
  ActivityIndicator,
  Alert,
  StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import { TypeBadge, CategoryChip } from '../components/PostBadges';
import { api, imageUrl } from '../src/api';
import { mapPost, timeAgo } from '../src/format';

// Pantalla de Detalle (mockup #screen-detail): GET /api/posts/{id} + comentarios
export default function DetalleScreen({ route, navigation }) {
  const postId = route.params?.postId;

  const [post, setPost] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false); // apoyo/guardado en vuelo
  const [commentText, setCommentText] = useState('');
  const [sendingComment, setSendingComment] = useState(false);

  const load = useCallback(async () => {
    if (!postId) return;
    setError('');
    try {
      const [detail, commentList] = await Promise.all([
        api.getPost(postId),
        api.getComments(postId),
      ]);
      setPost(mapPost(detail));
      setComments(commentList);
    } catch (e) {
      setError(`No se pudo cargar la publicación: ${e.message}`);
    }
  }, [postId]);

  // Carga al montar y refresca cada vez que la pantalla recupera el foco
  // (p. ej. al volver del visor de imágenes).
  useFocusEffect(
    useCallback(() => {
      load().finally(() => setLoading(false));
    }, [load])
  );

  const toggleSupport = async () => {
    if (!post || busy) return;
    setBusy(true);
    const wasSupported = post.supportedByMe;
    // Optimista: la API confirma con el contador real
    setPost((p) => ({
      ...p,
      supportedByMe: !wasSupported,
      supports: p.supports + (wasSupported ? -1 : 1),
    }));
    try {
      const res = wasSupported ? await api.unsupport(postId) : await api.support(postId);
      setPost((p) => ({ ...p, supports: res.supportCount, supportedByMe: res.supportedByMe }));
    } catch (e) {
      setPost((p) => ({
        ...p,
        supportedByMe: wasSupported,
        supports: p.supports + (wasSupported ? 1 : -1),
      }));
      Alert.alert('No se pudo actualizar el apoyo', e.message);
    } finally {
      setBusy(false);
    }
  };

  const toggleBookmark = async () => {
    if (!post || busy) return;
    setBusy(true);
    const wasSaved = post.bookmarkedByMe;
    setPost((p) => ({ ...p, bookmarkedByMe: !wasSaved }));
    try {
      const res = wasSaved ? await api.unbookmark(postId) : await api.bookmark(postId);
      setPost((p) => ({ ...p, bookmarkedByMe: res.bookmarkedByMe }));
    } catch (e) {
      setPost((p) => ({ ...p, bookmarkedByMe: wasSaved }));
      Alert.alert('No se pudo actualizar guardados', e.message);
    } finally {
      setBusy(false);
    }
  };

  const sendComment = async () => {
    const text = commentText.trim();
    if (!text || sendingComment) return;
    setSendingComment(true);
    try {
      const created = await api.createComment(postId, text);
      setComments((list) => [...list, created]);
      setCommentText('');
      setPost((p) => ({ ...p, commentCount: p.commentCount + 1 }));
    } catch (e) {
      Alert.alert('No se pudo comentar', e.message);
    } finally {
      setSendingComment(false);
    }
  };

  if (loading) {
    return (
      <View style={[styles.container, styles.center]}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  if (error || !post) {
    return (
      <View style={[styles.container, styles.center]}>
        <Ionicons name="alert-circle-outline" size={40} color={colors.gray400} />
        <Text style={styles.errorText}>{error || 'Publicación no encontrada.'}</Text>
        <Pressable style={styles.retryBtn} onPress={load}>
          <Text style={styles.retryText}>Reintentar</Text>
        </Pressable>
      </View>
    );
  }

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
          {post.authorEnrollment ? (
            <View style={styles.infoItem}>
              <Ionicons name="school-outline" size={14} color={colors.gray500} />
              <Text style={styles.infoText}>Matrícula {post.authorEnrollment}</Text>
            </View>
          ) : null}
          {post.location ? (
            <View style={styles.infoItem}>
              <Ionicons name="location-outline" size={14} color={colors.gray500} />
              <Text style={styles.infoText}>{post.location}</Text>
            </View>
          ) : null}
          <View style={styles.infoItem}>
            <Ionicons name="time-outline" size={14} color={colors.gray500} />
            <Text style={styles.infoText}>{timeAgo(post.createdAt)}</Text>
          </View>
        </View>

        {post.images.length > 0 ? (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            style={styles.images}
            contentContainerStyle={styles.imagesContent}
          >
            {post.images.map((image) => (
              <Pressable
                key={image.id}
                onPress={() =>
                  navigation.navigate('VisorImagen', {
                    imageId: image.id,
                    uri: imageUrl(image),
                  })
                }
              >
                <Image
                  source={{ uri: imageUrl(image) }}
                  style={styles.image}
                  // El JWT va en la query; sin caché para no mostrar imágenes viejas tras editar
                  cache="reload"
                />
              </Pressable>
            ))}
          </ScrollView>
        ) : null}

        <View style={styles.body}>
          {post.description
            .split('\n')
            .filter((line) => line.trim().length > 0)
            .map((paragraph, index) => (
              <Text key={index} style={styles.paragraph}>
                {paragraph}
              </Text>
            ))}
        </View>

        <View style={styles.actions}>
          <Pressable
            style={[styles.pill, post.supportedByMe && styles.pillSupportActive]}
            onPress={toggleSupport}
            disabled={busy}
          >
            <Ionicons
              name={post.supportedByMe ? 'thumbs-up' : 'thumbs-up-outline'}
              size={16}
              color={post.supportedByMe ? colors.primary : colors.gray600}
            />
            <Text style={[styles.pillText, post.supportedByMe && styles.pillTextSupport]}>
              {post.supports}
            </Text>
          </Pressable>

          <Pressable
            style={[styles.pill, post.bookmarkedByMe && styles.pillBookmarkActive]}
            onPress={toggleBookmark}
            disabled={busy}
          >
            <Ionicons
              name={post.bookmarkedByMe ? 'bookmark' : 'bookmark-outline'}
              size={16}
              color={post.bookmarkedByMe ? colors.queja : colors.gray600}
            />
            <Text style={styles.pillText}>{post.bookmarkedByMe ? 'Guardado' : 'Guardar'}</Text>
          </Pressable>
        </View>

        <View style={styles.comments}>
          <Text style={styles.commentsTitle}>Comentarios ({comments.length})</Text>

          {comments.length === 0 ? (
            <Text style={styles.noComments}>Sé el primero en comentar.</Text>
          ) : (
            comments.map((comment) => (
              <View key={comment.id} style={styles.comment}>
                <View style={styles.commentHeader}>
                  <Text style={styles.commentAuthor}>{comment.author?.displayName}</Text>
                  <Text style={styles.commentDate}>{timeAgo(comment.createdAt)}</Text>
                </View>
                <Text style={styles.commentText}>{comment.body}</Text>
              </View>
            ))
          )}

          <View style={styles.commentInput}>
            <TextInput
              style={styles.commentField}
              placeholder="Escribe un comentario..."
              placeholderTextColor={colors.gray400}
              value={commentText}
              onChangeText={setCommentText}
              multiline
              maxLength={1000}
            />
            <Pressable
              style={[styles.sendBtn, sendingComment && styles.sendBtnDisabled]}
              onPress={sendComment}
              disabled={sendingComment}
            >
              {sendingComment ? (
                <ActivityIndicator size="small" color={colors.white} />
              ) : (
                <Text style={styles.sendText}>Enviar</Text>
              )}
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
  center: {
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    padding: 24,
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
  noComments: {
    color: colors.gray400,
    fontSize: 14,
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
    maxHeight: 120,
  },
  sendBtn: {
    backgroundColor: colors.primary,
    paddingVertical: 10,
    paddingHorizontal: 18,
    borderRadius: 24,
  },
  sendBtnDisabled: {
    opacity: 0.6,
  },
  sendText: {
    color: colors.white,
    fontSize: 14,
    fontWeight: '600',
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
});
