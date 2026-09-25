import { useState } from 'react';
import { View, Text, ScrollView, Pressable, TextInput, Image, Alert, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import RadioGroup from '../components/RadioGroup';
import Dropdown from '../components/Dropdown';
import { getPost, posts, categories, TYPE_OPTIONS, IDENTITY_OPTIONS, thumbUrl } from '../data/posts';

// Pantalla de edición (mockup #screen-edit)
export default function EditarScreen({ route, navigation }) {
  const post = getPost(route.params?.postId) || posts[0];

  const [title, setTitle] = useState(post.title);
  const [description, setDescription] = useState(post.body);
  const [type, setType] = useState(post.type);
  const [category, setCategory] = useState(post.category);
  const [location, setLocation] = useState(post.location || '');
  const [images, setImages] = useState([...(post.images || [])]);
  const [identity, setIdentity] = useState('real');

  const confirmDelete = () => {
    Alert.alert('Eliminar publicación', '¿Eliminar esta publicación?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: () => navigation.navigate('MisPublicaciones'),
      },
    ]);
  };

  return (
    <View style={styles.container}>
      <AppHeader title="Editar publicación" onBack={() => navigation.goBack()} />

      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.group}>
          <Text style={styles.label}>Título</Text>
          <TextInput style={styles.input} value={title} onChangeText={setTitle} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Descripción</Text>
          <TextInput
            style={[styles.input, styles.textarea]}
            multiline
            value={description}
            onChangeText={setDescription}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Tipo</Text>
          <RadioGroup options={TYPE_OPTIONS} value={type} onChange={setType} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Categoría</Text>
          <Dropdown options={categories} value={category} onChange={setCategory} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Ubicación</Text>
          <TextInput style={styles.input} value={location} onChangeText={setLocation} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Imágenes (0–4)</Text>
          <View style={styles.previewRow}>
            {images.map((seed, index) => (
              <View key={`${seed}-${index}`} style={styles.previewItem}>
                <Image source={{ uri: thumbUrl(seed) }} style={styles.previewImage} />
                <Pressable
                  style={styles.removeBtn}
                  onPress={() => setImages((imgs) => imgs.filter((_, i) => i !== index))}
                >
                  <Ionicons name="close" size={12} color={colors.white} />
                </Pressable>
              </View>
            ))}
            <Pressable
              style={styles.uploadSmall}
              onPress={() => Alert.alert('📷 Abrir cámara o galería')}
            >
              <Ionicons name="add" size={24} color={colors.gray500} />
            </Pressable>
          </View>
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Modo de identidad</Text>
          <RadioGroup options={IDENTITY_OPTIONS} value={identity} onChange={setIdentity} />
        </View>

        <View style={styles.actions}>
          <Pressable style={styles.button} onPress={() => navigation.goBack()}>
            <Text style={styles.buttonText}>Guardar cambios</Text>
          </Pressable>
          <Pressable style={styles.dangerButton} onPress={confirmDelete}>
            <Text style={styles.dangerText}>Eliminar publicación</Text>
          </Pressable>
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.gray100,
  },
  content: {
    padding: 20,
    paddingBottom: 40,
  },
  group: {
    marginBottom: 16,
  },
  label: {
    fontSize: 13,
    fontWeight: '600',
    color: colors.gray600,
    marginBottom: 6,
  },
  input: {
    borderWidth: 1.5,
    borderColor: colors.gray300,
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 12,
    fontSize: 15,
    backgroundColor: colors.white,
    color: colors.gray900,
  },
  textarea: {
    height: 140,
    textAlignVertical: 'top',
  },
  previewRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  previewItem: {
    width: 80,
    height: 80,
    borderRadius: 8,
    overflow: 'hidden',
  },
  previewImage: {
    width: '100%',
    height: '100%',
    backgroundColor: colors.gray200,
  },
  removeBtn: {
    position: 'absolute',
    top: 2,
    right: 2,
    width: 20,
    height: 20,
    borderRadius: 10,
    backgroundColor: colors.danger,
    alignItems: 'center',
    justifyContent: 'center',
  },
  uploadSmall: {
    width: 80,
    height: 80,
    borderWidth: 2,
    borderStyle: 'dashed',
    borderColor: colors.gray300,
    borderRadius: 8,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.white,
  },
  actions: {
    gap: 10,
  },
  button: {
    backgroundColor: colors.primary,
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
  },
  buttonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: '600',
  },
  dangerButton: {
    backgroundColor: colors.dangerLight,
    borderWidth: 1.5,
    borderColor: colors.danger,
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
  },
  dangerText: {
    color: colors.danger,
    fontSize: 16,
    fontWeight: '600',
  },
});
