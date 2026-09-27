import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  Pressable,
  ScrollView,
  Share,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { deleteSpace, getSpace, getSpaceQrCodeDataUri } from '../api/spaces';
import { extractErrorMessage } from '../api/client';
import { RootStackParamList } from '../navigation/types';
import { SpaceResponse } from '../types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'SpaceDetail'>;

export default function SpaceDetailScreen({ route, navigation }: Props) {
  const { spaceId } = route.params;
  const [space, setSpace] = useState<SpaceResponse | null>(null);
  const [qrUri, setQrUri] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    try {
      const [spaceData, qr] = await Promise.all([getSpace(spaceId), getSpaceQrCodeDataUri(spaceId)]);
      setSpace(spaceData);
      setQrUri(qr);
      navigation.setOptions({ title: spaceData.name });
    } catch (err) {
      Alert.alert('Error', extractErrorMessage(err, 'No se pudo cargar el espacio.'));
    } finally {
      setLoading(false);
    }
  }, [spaceId, navigation]);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load])
  );

  async function handleShare() {
    if (!space) return;
    await Share.share({
      message: `Únete a "${space.name}" en EasyMemories.\nCódigo: ${space.joinCode}\n${space.joinUrl}`,
    });
  }

  function handleDelete() {
    if (!space) return;
    Alert.alert('Eliminar espacio', `Se borrarán "${space.name}" y todos sus recuerdos. ¿Continuar?`, [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await deleteSpace(space.id);
            navigation.goBack();
          } catch (err) {
            Alert.alert('Error', extractErrorMessage(err, 'No se pudo eliminar.'));
          }
        },
      },
    ]);
  }

  if (loading || !space) {
    return (
      <View style={styles.center}>
        <ActivityIndicator color={colors.primary} />
      </View>
    );
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      {space.description ? <Text style={styles.description}>{space.description}</Text> : null}

      <View style={styles.qrCard}>
        {qrUri && <Image source={{ uri: qrUri }} style={styles.qrImage} resizeMode="contain" />}
        <Text style={styles.joinCode}>{space.joinCode}</Text>
        <Text style={styles.joinHint}>Los invitados escanean este código para entrar</Text>
      </View>

      <Pressable style={styles.primaryButton} onPress={handleShare}>
        <Text style={styles.primaryButtonText}>Compartir código</Text>
      </Pressable>

      <Pressable
        style={styles.secondaryButton}
        onPress={() => navigation.navigate('SpaceMemoryWall', { spaceId: space.id, spaceName: space.name })}
      >
        <Text style={styles.secondaryButtonText}>
          Ver muro de recuerdos ({space.memoryCount})
        </Text>
      </Pressable>

      <Pressable style={styles.dangerButton} onPress={handleDelete}>
        <Text style={styles.dangerButtonText}>Eliminar espacio</Text>
      </Pressable>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg },
  center: { flex: 1, backgroundColor: colors.background, alignItems: 'center', justifyContent: 'center' },
  description: { color: colors.textMuted, marginBottom: spacing.lg, textAlign: 'center' },
  qrCard: {
    backgroundColor: '#FFFFFF',
    borderRadius: 20,
    padding: spacing.lg,
    alignItems: 'center',
    marginBottom: spacing.lg,
  },
  qrImage: { width: 220, height: 220 },
  joinCode: {
    marginTop: spacing.md,
    fontSize: 22,
    fontWeight: '800',
    letterSpacing: 4,
    color: '#1A1024',
  },
  joinHint: { color: '#6B6478', marginTop: spacing.xs, fontSize: 12 },
  primaryButton: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  primaryButtonText: { color: '#1A1024', fontSize: 16, fontWeight: '700' },
  secondaryButton: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  secondaryButtonText: { color: colors.text, fontSize: 16, fontWeight: '600' },
  dangerButton: { padding: spacing.md, alignItems: 'center' },
  dangerButtonText: { color: colors.danger, fontWeight: '600' },
});
