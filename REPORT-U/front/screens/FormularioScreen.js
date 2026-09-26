import { useState } from 'react';
import {
  View,
  Text,
  ScrollView,
  Pressable,
  TextInput,
  Image,
  Alert,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import RadioGroup from '../components/RadioGroup';
import Dropdown from '../components/Dropdown';
import { api } from '../src/api';
import { TYPE_OPTIONS, IDENTITY_OPTIONS, MAX_IMAGES, CATEGORIES } from '../src/constants';
import { categoryLabel, categoryValue } from '../src/labels';
import { pickFromGallery, takeFromCamera } from '../src/imagePicker';

// Pantalla de nueva publicación (mockup #screen-create)
// POST /api/posts y luego POST /api/posts/{id}/images por cada imagen.
export default function FormularioScreen({ navigation }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState('Incidencia');
  const [category, setCategory] = useState(''); // valor del enum (p. ej. "Infraestructura")
  const [location, setLocation] = useState('');
  const [images, setImages] = useState([]); // [{ uri }]
  const [identity, setIdentity] = useState('Default');
  const [submitting, setSubmitting] = useState(false);

  const addImages = (uris) => {
    setImages((prev) => {
      const room = MAX_IMAGES - prev.length;
      if (uris.length > room) {
        Alert.alert(
          'Límite de imágenes',
          `Solo puedes agregar ${MAX_IMAGES} imágenes por publicación.`
        );
      }
      return [...prev, ...uris.slice(0, Math.max(0, room)).map((uri) => ({ uri }))];
    });
  };

  const openCamera = async () => {
    const uri = await takeFromCamera();
    if (uri) addImages([uri]);
  };

  const openGallery = async () => {
    const room = MAX_IMAGES - images.length;
    const uris = await pickFromGallery(Math.max(1, room));
    if (uris?.length) addImages(uris);
  };

  const handleSubmit = async () => {
    if (!title.trim() || !description.trim() || !category) {
      Alert.alert('Faltan datos', 'Escribe título, descripción y selecciona una categoría.');
      return;
    }
    setSubmitting(true);
    let created = null;
    try {
      created = await api.createPost({
        title: title.trim(),
        description: description.trim(),
        type,
        category,
        location: location.trim() || null,
        identityMode: identity,
      });
    } catch (e) {
      Alert.alert('No se pudo publicar', e.message);
      setSubmitting(false);
      return;
    }

    // Sube las imágenes una por una; un fallo no cancela lo ya publicado
    const failed = [];
    for (const img of images) {
      try {
        await api.uploadImage(created.id, img.uri);
      } catch (e) {
        failed.push(e.message);
      }
    }
    setSubmitting(false);

    // Limpia el formulario para la próxima publicación
    setTitle('');
    setDescription('');
    setType('Incidencia');
    setCategory('');
    setLocation('');
    setImages([]);
    setIdentity('Default');

    if (failed.length > 0) {
      Alert.alert(
        'Publicación creada',
        `Pero ${failed.length} imagen(es) no se subieron:\n${failed.join('\n')}`
      );
    }
    navigation.navigate('Inicio');
  };

  return (
    <View style={styles.container}>
      <AppHeader title="Nueva publicación" onBack={() => navigation.navigate('Inicio')} />

      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.group}>
          <Text style={styles.label}>Título</Text>
          <TextInput
            style={styles.input}
            placeholder="Ej: Fuga de agua en edificio B"
            placeholderTextColor={colors.gray400}
            maxLength={150}
            value={title}
            onChangeText={setTitle}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Descripción</Text>
          <TextInput
            style={[styles.input, styles.textarea]}
            placeholder="Describe tu incidencia, queja o discusión..."
            placeholderTextColor={colors.gray400}
            multiline
            maxLength={5000}
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
          <Dropdown
            options={CATEGORIES.map((c) => c.label)}
            value={category ? categoryLabel(category) : ''}
            placeholder="Selecciona una categoría"
            onChange={(label) => setCategory(categoryValue(label))}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Ubicación (opcional)</Text>
          <TextInput
            style={styles.input}
            placeholder="Ej: Edificio B, segundo piso"
            placeholderTextColor={colors.gray400}
            maxLength={200}
            value={location}
            onChangeText={setLocation}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Imágenes ({images.length}/{MAX_IMAGES})</Text>
          <View style={styles.uploadRow}>
            <Pressable
              style={styles.upload}
              onPress={openCamera}
              disabled={images.length >= MAX_IMAGES}
            >
              <Ionicons name="camera" size={24} color={colors.gray500} />
              <Text style={styles.uploadText}>Tomar foto</Text>
            </Pressable>
            <Pressable
              style={styles.upload}
              onPress={openGallery}
              disabled={images.length >= MAX_IMAGES}
            >
              <Ionicons name="images" size={24} color={colors.gray500} />
              <Text style={styles.uploadText}>Galería</Text>
            </Pressable>
          </View>

          {images.length > 0 ? (
            <View style={styles.previewRow}>
              {images.map((img, index) => (
                <View key={`${img.uri}-${index}`} style={styles.previewItem}>
                  <Image source={{ uri: img.uri }} style={styles.previewImage} />
                  <Pressable
                    style={styles.removeBtn}
                    onPress={() => setImages((imgs) => imgs.filter((_, i) => i !== index))}
                  >
                    <Ionicons name="close" size={12} color={colors.white} />
                  </Pressable>
                </View>
              ))}
            </View>
          ) : null}
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Modo de identidad</Text>
          <RadioGroup options={IDENTITY_OPTIONS} value={identity} onChange={setIdentity} />
        </View>

        <Pressable
          style={[styles.button, submitting && styles.buttonDisabled]}
          onPress={handleSubmit}
          disabled={submitting}
        >
          {submitting ? (
            <ActivityIndicator color={colors.white} />
          ) : (
            <Text style={styles.buttonText}>Publicar</Text>
          )}
        </Pressable>
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
    height: 120,
    textAlignVertical: 'top',
  },
  uploadRow: {
    flexDirection: 'row',
    gap: 10,
    marginBottom: 10,
  },
  upload: {
    flex: 1,
    borderWidth: 2,
    borderStyle: 'dashed',
    borderColor: colors.gray300,
    borderRadius: 8,
    paddingVertical: 20,
    paddingHorizontal: 12,
    alignItems: 'center',
    gap: 4,
    backgroundColor: colors.white,
  },
  uploadText: {
    fontSize: 13,
    color: colors.gray500,
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
  button: {
    backgroundColor: colors.primary,
    paddingVertical: 14,
    borderRadius: 8,
    alignItems: 'center',
    marginTop: 4,
  },
  buttonDisabled: {
    opacity: 0.6,
  },
  buttonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: '600',
  },
});
