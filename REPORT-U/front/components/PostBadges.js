import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme';

const TYPE_STYLES = {
  incidencia: { background: '#fef2f2', color: colors.incidencia, label: 'Incidencia' },
  queja: { background: '#fffbeb', color: colors.queja, label: 'Queja' },
  discusion: { background: '#f5f3ff', color: colors.discusion, label: 'Discusión' },
};

// .post-type / .type-* del mockup
export function TypeBadge({ type }) {
  const t = TYPE_STYLES[type] || TYPE_STYLES.incidencia;
  return (
    <View style={[styles.type, { backgroundColor: t.background }]}>
      <Text style={[styles.typeText, { color: t.color }]}>{t.label}</Text>
    </View>
  );
}

// .post-category del mockup
export function CategoryChip({ category }) {
  return (
    <View style={styles.category}>
      <Text style={styles.categoryText}>{category}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  type: {
    paddingVertical: 3,
    paddingHorizontal: 8,
    borderRadius: 20,
  },
  typeText: {
    fontSize: 11,
    fontWeight: '700',
    textTransform: 'uppercase',
    letterSpacing: 0.3,
  },
  category: {
    backgroundColor: colors.gray100,
    paddingVertical: 2,
    paddingHorizontal: 8,
    borderRadius: 10,
  },
  categoryText: {
    fontSize: 12,
    color: colors.gray500,
  },
});
