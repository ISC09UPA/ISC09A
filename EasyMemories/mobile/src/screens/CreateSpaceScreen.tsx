import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useState } from 'react';
import { Alert, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { createSpace } from '../api/spaces';
import { extractErrorMessage } from '../api/client';
import { RootStackParamList } from '../navigation/types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'CreateSpace'>;

export default function CreateSpaceScreen({ navigation }: Props) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function handleCreate() {
    if (!name.trim()) {
      Alert.alert('Falta el nombre', 'Ponle un nombre a tu evento.');
      return;
    }
    setSubmitting(true);
    try {
      const space = await createSpace(name.trim(), description.trim() || undefined);
      navigation.replace('SpaceDetail', { spaceId: space.id });
    } catch (err) {
      Alert.alert('No se pudo crear el espacio', extractErrorMessage(err, 'Intenta de nuevo.'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Nuevo espacio</Text>
      <Text style={styles.subtitle}>Se generará un código QR único para que tus invitados se unan.</Text>

      <TextInput
        style={styles.input}
        placeholder="Nombre del evento (ej. Fiesta XV Ana)"
        placeholderTextColor={colors.textMuted}
        value={name}
        onChangeText={setName}
      />
      <TextInput
        style={[styles.input, styles.multiline]}
        placeholder="Descripción (opcional)"
        placeholderTextColor={colors.textMuted}
        multiline
        value={description}
        onChangeText={setDescription}
      />

      <Pressable style={styles.primaryButton} onPress={handleCreate} disabled={submitting}>
        <Text style={styles.primaryButtonText}>{submitting ? 'Creando...' : 'Crear espacio'}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, padding: spacing.lg, justifyContent: 'center' },
  title: { fontSize: 26, fontWeight: '800', color: colors.text },
  subtitle: { color: colors.textMuted, marginTop: spacing.xs, marginBottom: spacing.lg },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    color: colors.text,
    marginBottom: spacing.md,
  },
  multiline: { minHeight: 90, textAlignVertical: 'top' },
  primaryButton: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
    marginTop: spacing.sm,
  },
  primaryButtonText: { color: '#1A1024', fontSize: 16, fontWeight: '700' },
});
