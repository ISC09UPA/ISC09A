// Selección de imágenes con expo-image-picker (cámara y galería).
// Los permisos se solicitan aquí; si el usuario los negó se explica con un Alert.
import * as ImagePicker from 'expo-image-picker';
import { Alert } from 'react-native';

export async function takeFromCamera() {
  const perm = await ImagePicker.requestCameraPermissionsAsync();
  if (!perm.granted) {
    Alert.alert('Permiso necesario', 'Permite el acceso a la cámara para tomar fotos.');
    return null;
  }
  const result = await ImagePicker.launchCameraAsync({
    mediaTypes: ['images'],
    quality: 0.7,
  });
  if (result.canceled || !result.assets?.length) return null;
  return result.assets[0].uri;
}

export async function pickFromGallery(max = 1) {
  const perm = await ImagePicker.requestMediaLibraryPermissionsAsync();
  if (!perm.granted) {
    Alert.alert('Permiso necesario', 'Permite el acceso a tus fotos para seleccionar imágenes.');
    return null;
  }
  const result = await ImagePicker.launchImageLibraryAsync({
    mediaTypes: ['images'],
    allowsMultipleSelection: max > 1,
    selectionLimit: max,
    quality: 0.7,
  });
  if (result.canceled || !result.assets?.length) return [];
  return result.assets.map((a) => a.uri);
}
