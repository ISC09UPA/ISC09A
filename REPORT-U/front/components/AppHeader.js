import { View, Text, Pressable, StyleSheet } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';

// Header azul replicado de .app-header (Mockups/style.css)
export default function AppHeader({ title, onBack }) {
  const insets = useSafeAreaInsets();

  return (
    <View style={[styles.header, { paddingTop: insets.top + 14 }]}>
      {onBack ? (
        <Pressable onPress={onBack} hitSlop={8} style={styles.back}>
          <Ionicons name="arrow-back" size={22} color={colors.white} />
        </Pressable>
      ) : null}
      <Text style={styles.title}>{title}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  header: {
    backgroundColor: colors.primary,
    paddingHorizontal: 20,
    paddingBottom: 16,
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    zIndex: 10,
  },
  back: {
    padding: 2,
  },
  title: {
    fontSize: 20,
    fontWeight: '700',
    color: colors.white,
  },
});
