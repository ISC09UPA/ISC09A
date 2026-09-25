import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { colors, spacing } from './src/theme/theme';

export default function App() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Mi clóset</Text>
      <Text style={styles.subtitle}>¡Bienvenido!</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    alignItems: 'center',
    justifyContent: 'center',
    padding: spacing.lg,
  },
  title: { fontSize: 28, fontWeight: '700', color: colors.textPrimary },
  subtitle: { fontSize: 16, color: colors.textSecondary, marginTop: spacing.sm },
});
