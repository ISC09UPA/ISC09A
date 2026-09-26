import { useState } from 'react';
import { View, Text, Pressable, Image, Alert, ActivityIndicator, StyleSheet } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import * as FileSystem from 'expo-file-system/legacy';
import * as Sharing from 'expo-sharing';
import { API_BASE_URL, imageUrl, getAuthToken } from '../src/api';

// Visor de imágenes en pantalla completa (mockup #screen-imageviewer)
// Muestra el binario de GET /api/media/{id} y descarga GET /api/media/{id}/download.
export default function VisorImagenScreen({ route, navigation }) {
  const insets = useSafeAreaInsets();
  const imageId = route.params?.imageId;
  const initialUri = route.params?.uri || imageUrl({ id: imageId });
  const [downloading, setDownloading] = useState(false);

  const handleDownload = async () => {
    if (!imageId || downloading) return;
    setDownloading(true);
    try {
      // 1. Descarga el archivo original (attachment) con el JWT en la query
      const fileUri = `${FileSystem.cacheDirectory}${imageId}.img`;
      await FileSystem.downloadAsync(
        `${API_BASE_URL}/api/media/${imageId}/download?access_token=${encodeURIComponent(
          getAuthToken() || ''
        )}`,
        fileUri
      );

      // 2. "Guardar en el dispositivo" vía el sheet del sistema
      if (await Sharing.isAvailableAsync()) {
        await Sharing.shareAsync(fileUri, {
          mimeType: 'application/octet-stream',
          dialogTitle: 'Guardar imagen',
        });
      } else {
        Alert.alert('Descargada', `La imagen quedó en:\n${fileUri}`);
      }
    } catch (e) {
      Alert.alert('No se pudo descargar', e.message);
    } finally {
      setDownloading(false);
    }
  };

  return (
    <View style={styles.container}>
      <Image source={{ uri: initialUri }} style={styles.image} resizeMode="contain" />

      <Pressable
        style={[styles.close, { top: insets.top + 16 }]}
        onPress={() => navigation.goBack()}
        hitSlop={8}
      >
        <Ionicons name="close" size={24} color="#fff" />
      </Pressable>

      <View style={[styles.actions, { paddingBottom: insets.bottom + 16 }]}>
        <Pressable style={styles.downloadBtn} onPress={handleDownload} disabled={downloading}>
          {downloading ? (
            <ActivityIndicator size="small" color="#fff" />
          ) : (
            <>
              <Ionicons name="download-outline" size={16} color="#fff" />
              <Text style={styles.downloadText}>Descargar</Text>
            </>
          )}
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.95)',
    alignItems: 'center',
    justifyContent: 'center',
  },
  image: {
    width: '100%',
    height: '80%',
  },
  close: {
    position: 'absolute',
    right: 16,
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(255,255,255,0.2)',
    alignItems: 'center',
    justifyContent: 'center',
    zIndex: 10,
  },
  actions: {
    padding: 16,
  },
  downloadBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    backgroundColor: 'rgba(255,255,255,0.2)',
    paddingVertical: 10,
    paddingHorizontal: 20,
    borderRadius: 24,
  },
  downloadText: {
    color: '#fff',
    fontSize: 14,
    fontWeight: '600',
  },
});
