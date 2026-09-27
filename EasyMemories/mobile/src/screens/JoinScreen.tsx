import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { getSpaceInfo } from '../api/join';
import { extractErrorMessage } from '../api/client';
import { RootStackParamList } from '../navigation/types';
import { SpacePublicInfo } from '../types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'Join'>;

export default function JoinScreen({ route, navigation }: Props) {
  const { joinCode } = route.params;
  const [info, setInfo] = useState<SpacePublicInfo | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getSpaceInfo(joinCode)
      .then(setInfo)
      .catch((err) => setError(extractErrorMessage(err, 'Código inválido o el espacio ya no está activo.')));
  }, [joinCode]);

  if (error) {
    return (
      <View style={styles.container}>
        <Text style={styles.errorTitle}>No pudimos encontrar ese espacio</Text>
        <Text style={styles.errorText}>{error}</Text>
        <Pressable style={styles.secondaryButton} onPress={() => navigation.goBack()}>
          <Text style={styles.secondaryButtonText}>Volver a escanear</Text>
        </Pressable>
      </View>
    );
  }

  if (!info) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.eyebrow}>Te uniste a</Text>
      <Text style={styles.title}>{info.name}</Text>
      {info.description ? <Text style={styles.description}>{info.description}</Text> : null}

      <View style={styles.actions}>
        <Pressable
          style={styles.primaryButton}
          onPress={() => navigation.navigate('UploadMemory', { joinCode, spaceName: info.name })}
        >
          <Text style={styles.primaryButtonText}>Subir foto y comentario</Text>
        </Pressable>

        <Pressable
          style={styles.secondaryButton}
          onPress={() => navigation.navigate('MemoryWall', { joinCode, spaceName: info.name })}
        >
          <Text style={styles.secondaryButtonText}>Ver muro de recuerdos</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, padding: spacing.lg, justifyContent: 'center' },
  center: { flex: 1, backgroundColor: colors.background, alignItems: 'center', justifyContent: 'center' },
  eyebrow: { color: colors.primary, textAlign: 'center', fontWeight: '700' },
  title: { color: colors.text, fontSize: 30, fontWeight: '800', textAlign: 'center', marginTop: spacing.xs },
  description: { color: colors.textMuted, textAlign: 'center', marginTop: spacing.md },
  actions: { marginTop: spacing.xl, gap: spacing.md },
  primaryButton: { backgroundColor: colors.primary, borderRadius: 12, padding: spacing.md, alignItems: 'center' },
  primaryButtonText: { color: '#1A1024', fontSize: 16, fontWeight: '700' },
  secondaryButton: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
  },
  secondaryButtonText: { color: colors.text, fontSize: 16, fontWeight: '600' },
  errorTitle: { color: colors.text, fontSize: 20, fontWeight: '700', textAlign: 'center' },
  errorText: { color: colors.textMuted, textAlign: 'center', marginTop: spacing.sm, marginBottom: spacing.lg },
});
