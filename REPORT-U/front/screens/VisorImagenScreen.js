import { View, Text, Pressable, Image, Alert, StyleSheet } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Ionicons } from '@expo/vector-icons';
import { fullUrl } from '../data/posts';

// Visor de imágenes en pantalla completa (mockup #screen-imageviewer)
export default function VisorImagenScreen({ route, navigation }) {
  const insets = useSafeAreaInsets();
  const seed = route.params?.seed;

  return (
    <View style={styles.container}>
      <Image source={{ uri: fullUrl(seed) }} style={styles.image} resizeMode="contain" />

      <Pressable
        style={[styles.close, { top: insets.top + 16 }]}
        onPress={() => navigation.goBack()}
        hitSlop={8}
      >
        <Ionicons name="close" size={24} color="#fff" />
      </Pressable>

      <View style={[styles.actions, { paddingBottom: insets.bottom + 16 }]}>
        <Pressable
          style={styles.downloadBtn}
          onPress={() => Alert.alert('⬇️ Descargando imagen...')}
        >
          <Ionicons name="download-outline" size={16} color="#fff" />
          <Text style={styles.downloadText}>Descargar</Text>
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
