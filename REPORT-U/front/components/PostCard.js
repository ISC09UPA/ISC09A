import { Pressable, View, Text, Image, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, radius, shadow } from '../theme';
import { TypeBadge, CategoryChip } from './PostBadges';
import { thumbUrl } from '../data/posts';

// .post-card del mockup
export default function PostCard({ post, onPress, showAuthor = true, showComments = false }) {
  return (
    <Pressable style={({ pressed }) => [styles.card, pressed && styles.pressed]} onPress={onPress}>
      <View style={styles.header}>
        <TypeBadge type={post.type} />
        <CategoryChip category={post.category} />
      </View>

      <Text style={styles.title}>{post.title}</Text>
      <Text style={styles.excerpt} numberOfLines={2}>
        {post.excerpt}
      </Text>

      {post.images && post.images.length > 0 ? (
        <View style={styles.imagesPreview}>
          {post.images.map((seed) => (
            <Image key={seed} source={{ uri: thumbUrl(seed) }} style={styles.thumb} />
          ))}
        </View>
      ) : null}

      <View style={styles.footer}>
        <View style={styles.footerGroup}>
          {showAuthor ? (
            <View style={styles.metaItem}>
              <Ionicons name="person" size={12} color={colors.gray500} />
              <Text style={styles.metaText}>{post.author}</Text>
            </View>
          ) : null}
          <Text style={styles.metaText}>{post.date}</Text>
        </View>

        <View style={styles.footerGroup}>
          <View style={styles.metaItem}>
            <Ionicons name="thumbs-up" size={12} color={colors.gray500} />
            <Text style={styles.supports}>{post.supports}</Text>
          </View>
          {showComments ? (
            <View style={styles.metaItem}>
              <Ionicons name="chatbubble-outline" size={12} color={colors.gray500} />
              <Text style={styles.supports}>
                {post.commentCount != null ? post.commentCount : post.comments.length}
              </Text>
            </View>
          ) : null}
        </View>
      </View>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.white,
    borderRadius: radius.lg,
    padding: 16,
    marginBottom: 12,
    ...shadow,
  },
  pressed: {
    transform: [{ translateY: -1 }],
    ...shadow,
    shadowOpacity: 0.15,
    elevation: 4,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginBottom: 8,
  },
  title: {
    fontSize: 16,
    fontWeight: '700',
    color: colors.gray900,
    marginBottom: 6,
  },
  excerpt: {
    fontSize: 14,
    color: colors.gray500,
    lineHeight: 21,
    marginBottom: 8,
  },
  imagesPreview: {
    flexDirection: 'row',
    gap: 6,
    marginBottom: 8,
  },
  thumb: {
    width: 60,
    height: 60,
    borderRadius: radius.sm,
    backgroundColor: colors.gray200,
  },
  footer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  footerGroup: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  metaItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  metaText: {
    fontSize: 12,
    color: colors.gray500,
  },
  supports: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.gray600,
  },
});
