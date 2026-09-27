import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useAuth } from '../context/AuthContext';
import { RootStackParamList } from '../navigation/types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'Home'>;

export default function HomeScreen({ navigation }: Props) {
  const { isAuthenticated } = useAuth();

  return (
    <View style={styles.container}>
      <Text style={styles.title}>EasyMemories</Text>
      <Text style={styles.subtitle}>El muro de recuerdos de tu fiesta</Text>

      <View style={styles.actions}>
        <Pressable style={styles.primaryButton} onPress={() => navigation.navigate('ScanQr')}>
          <Text style={styles.primaryButtonText}>Escanear código QR</Text>
          <Text style={styles.buttonHint}>Soy invitado de un evento</Text>
        </Pressable>

        <Pressable
          style={styles.secondaryButton}
          onPress={() => navigation.navigate(isAuthenticated ? 'SpacesList' : 'Login')}
        >
          <Text style={styles.secondaryButtonText}>Soy anfitrión</Text>
          <Text style={styles.buttonHintMuted}>Crear y administrar espacios</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background,
    padding: spacing.lg,
    justifyContent: 'center',
  },
  title: {
    fontSize: 36,
    fontWeight: '800',
    color: colors.text,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 15,
    color: colors.textMuted,
    textAlign: 'center',
    marginTop: spacing.xs,
    marginBottom: spacing.xl,
  },
  actions: {
    gap: spacing.md,
  },
  primaryButton: {
    backgroundColor: colors.primary,
    borderRadius: 16,
    padding: spacing.lg,
    alignItems: 'center',
  },
  primaryButtonText: {
    color: '#1A1024',
    fontSize: 18,
    fontWeight: '700',
  },
  buttonHint: {
    color: '#1A1024',
    opacity: 0.7,
    marginTop: spacing.xs,
  },
  secondaryButton: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 16,
    padding: spacing.lg,
    alignItems: 'center',
  },
  secondaryButtonText: {
    color: colors.text,
    fontSize: 18,
    fontWeight: '700',
  },
  buttonHintMuted: {
    color: colors.textMuted,
    marginTop: spacing.xs,
  },
});
