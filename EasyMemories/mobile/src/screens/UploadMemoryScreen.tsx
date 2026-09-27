import * as ImagePicker from 'expo-image-picker';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  Image,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { PickedPhoto, uploadMemory } from '../api/join';
import { extractErrorMessage } from '../api/client';
import { RootStackParamList } from '../navigation/types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'UploadMemory'>;

export default function UploadMemoryScreen({ route, navigation }: Props) {
  const { joinCode, spaceName } = route.params;
  const [photo, setPhoto] = useState<PickedPhoto | null>(null);
  const [guestName, setGuestName] = useState('');
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  function toPickedPhoto(asset: ImagePicker.ImagePickerAsset): PickedPhoto {
    const fileName = asset.fileName ?? asset.uri.split('/').pop() ?? `foto-${Date.now()}.jpg`;
    const mimeType = asset.mimeType ?? guessMimeType(fileName);
    return { uri: asset.uri, fileName, mimeType };
  }

  async function pickFromCamera() {
    const permission = await ImagePicker.requestCameraPermissionsAsync();
    if (!permission.granted) {
      Alert.alert('Permiso necesario', 'Activa el permiso de cámara para tomar la foto.');
      return;
    }
    const result = await ImagePicker.launchCameraAsync({ quality: 0.7 });
    if (!result.canceled) setPhoto(toPickedPhoto(result.assets[0]));
  }

  async function pickFromGallery() {
    const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permission.granted) {
      Alert.alert('Permiso necesario', 'Activa el permiso de galería para elegir la foto.');
      return;
    }
    const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: ['images'], quality: 0.7 });
    if (!result.canceled) setPhoto(toPickedPhoto(result.assets[0]));
  }

  async function handleSubmit() {
    if (!photo) {
      Alert.alert('Falta la foto', 'Toma o elige una foto para subirla al muro.');
      return;
    }
    setSubmitting(true);
    try {
      await uploadMemory(joinCode, photo, guestName, comment);
      Alert.alert('¡Listo!', 'Tu recuerdo se subió al muro.', [
        {
          text: 'Ver muro',
          onPress: () => navigation.replace('MemoryWall', { joinCode, spaceName }),
        },
      ]);
    } catch (err) {
      Alert.alert('No se pudo subir', extractErrorMessage(err, 'Intenta de nuevo.'));
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <Text style={styles.title}>Nuevo recuerdo para {spaceName}</Text>

      {photo ? (
        <Image source={{ uri: photo.uri }} style={styles.preview} resizeMode="cover" />
      ) : (
        <View style={styles.previewPlaceholder}>
          <Text style={styles.previewPlaceholderText}>Sin foto seleccionada</Text>
        </View>
      )}

      <View style={styles.pickerRow}>
        <Pressable style={styles.secondaryButton} onPress={pickFromCamera}>
          <Text style={styles.secondaryButtonText}>Tomar foto</Text>
        </Pressable>
        <Pressable style={styles.secondaryButton} onPress={pickFromGallery}>
          <Text style={styles.secondaryButtonText}>Elegir de galería</Text>
        </Pressable>
      </View>

      <TextInput
        style={styles.input}
        placeholder="Tu nombre (opcional)"
        placeholderTextColor={colors.textMuted}
        value={guestName}
        onChangeText={setGuestName}
      />
      <TextInput
        style={[styles.input, styles.multiline]}
        placeholder="Un comentario para recordar este momento..."
        placeholderTextColor={colors.textMuted}
        multiline
        value={comment}
        onChangeText={setComment}
      />

      <Pressable style={styles.primaryButton} onPress={handleSubmit} disabled={submitting}>
        {submitting ? <ActivityIndicator color="#1A1024" /> : <Text style={styles.primaryButtonText}>Subir al muro</Text>}
      </Pressable>
    </ScrollView>
  );
}

function guessMimeType(fileName: string): string {
  const ext = fileName.split('.').pop()?.toLowerCase();
  switch (ext) {
    case 'png':
      return 'image/png';
    case 'webp':
      return 'image/webp';
    case 'heic':
      return 'image/heic';
    default:
      return 'image/jpeg';
  }
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  content: { padding: spacing.lg },
  title: { color: colors.text, fontSize: 22, fontWeight: '800', marginBottom: spacing.lg },
  preview: { width: '100%', aspectRatio: 1, borderRadius: 16, marginBottom: spacing.md },
  previewPlaceholder: {
    width: '100%',
    aspectRatio: 1,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: colors.border,
    borderStyle: 'dashed',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: spacing.md,
  },
  previewPlaceholderText: { color: colors.textMuted },
  pickerRow: { flexDirection: 'row', gap: spacing.sm, marginBottom: spacing.lg },
  secondaryButton: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
  },
  secondaryButtonText: { color: colors.text, fontWeight: '600' },
  input: {
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    color: colors.text,
    marginBottom: spacing.md,
  },
  multiline: { minHeight: 90, textAlignVertical: 'top' },
  primaryButton: { backgroundColor: colors.primary, borderRadius: 12, padding: spacing.md, alignItems: 'center' },
  primaryButtonText: { color: '#1A1024', fontSize: 16, fontWeight: '700' },
});
