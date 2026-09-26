import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme';
import { typeLabel, categoryLabel } from '../src/labels';

const TYPE_STYLES = {
  Incidencia: { background: '#fef2f2', color: colors.incidencia, label: 'Incidencia' },
  Queja: { background: '#fffbeb', color: colors.queja, label: 'Queja' },
  Discusion: { background: '#f5f3ff', color: colors.discusion, label: 'Discusión' },
};

// .post-type / .type-* del mockup. `type` es el enum de la API ("Incidencia").
export function TypeBadge({ type }) {
  const t = TYPE_STYLES[type] || { background: '#f3f4f6', color: colors.gray600, label: typeLabel(type) };
  return (
    <View style={[styles.type, { backgroundColor: t.background }]}>
      <Text style={[styles.typeText, { color: t.color }]}>{t.label}</Text>
    </View>
  );
}

// .post-category del mockup. `category` es el enum de la API ("Academico");
// se muestra con acentos vía labels ("Académico").
export function CategoryChip({ category }) {
  return (
    <View style={styles.category}>
      <Text style={styles.categoryText}>{categoryLabel(category)}</Text>
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
