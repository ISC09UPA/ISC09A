import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useState } from 'react';
import { Alert, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { extractErrorMessage } from '../api/client';
import { useAuth } from '../context/AuthContext';
import { RootStackParamList } from '../navigation/types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'Register'>;

export default function RegisterScreen({ navigation }: Props) {
  const { register } = useAuth();
  const [displayName, setDisplayName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function handleRegister() {
    if (!displayName.trim() || !/^\S+@\S+\.\S+$/.test(email.trim()) || password.length < 6) {
      Alert.alert(
        'Revisa tus datos',
        'Nombre, un correo válido (ej. nombre@correo.com) y una contraseña de al menos 6 caracteres.'
      );
      return;
    }
    setSubmitting(true);
    try {
      await register(email.trim(), password, displayName.trim());
      navigation.reset({ index: 0, routes: [{ name: 'SpacesList' }] });
    } catch (err) {
      Alert.alert('No se pudo crear la cuenta', extractErrorMessage(err, 'Intenta de nuevo.'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Crear cuenta de anfitrión</Text>

      <TextInput
        style={styles.input}
        placeholder="Tu nombre"
        placeholderTextColor={colors.textMuted}
        value={displayName}
        onChangeText={setDisplayName}
      />
      <TextInput
        style={styles.input}
        placeholder="Correo"
        placeholderTextColor={colors.textMuted}
        autoCapitalize="none"
        keyboardType="email-address"
        value={email}
        onChangeText={setEmail}
      />
      <TextInput
        style={styles.input}
        placeholder="Contraseña (mínimo 6 caracteres)"
        placeholderTextColor={colors.textMuted}
        secureTextEntry
        value={password}
        onChangeText={setPassword}
      />

      <Pressable style={styles.primaryButton} onPress={handleRegister} disabled={submitting}>
        <Text style={styles.primaryButtonText}>{submitting ? 'Creando...' : 'Crear cuenta'}</Text>
      </Pressable>

      <Pressable onPress={() => navigation.navigate('Login')}>
        <Text style={styles.link}>Ya tengo cuenta, iniciar sesión</Text>
      </Pressable>
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
    fontSize: 26,
    fontWeight: '800',
    color: colors.text,
    marginBottom: spacing.lg,
  },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    color: colors.text,
    marginBottom: spacing.md,
  },
  primaryButton: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
    marginTop: spacing.sm,
  },
  primaryButtonText: {
    color: '#1A1024',
    fontSize: 16,
    fontWeight: '700',
  },
  link: {
    color: colors.primary,
    textAlign: 'center',
    marginTop: spacing.lg,
  },
});
