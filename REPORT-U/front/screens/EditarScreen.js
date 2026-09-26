import { useEffect, useState } from 'react';
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
import { api, imageUrl } from '../src/api';
import { TYPE_OPTIONS, IDENTITY_OPTIONS, MAX_IMAGES, CATEGORIES } from '../src/constants';
import { categoryLabel, categoryValue } from '../src/labels';
import { mapPost } from '../src/format';
import { pickFromGallery, takeFromCamera } from '../src/imagePicker';

// Pantalla de edición (mockup #screen-edit)
// PATCH /api/posts/{id} + POST/DELETE de imágenes + DELETE de la publicación.
export default function EditarScreen({ route, navigation }) {
  const postId = route.params?.postId;

  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState('Incidencia');
  const [category, setCategory] = useState('');
  const [location, setLocation] = useState('');
  const [identity, setIdentity] = useState(null); // null = no cambiar el modo guardado
  const [existingImages, setExistingImages] = useState([]); // PostImageResponse[]
  const [newImages, setNewImages] = useState([]); // [{ uri }] aún no subidas
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let alive = true;
    (async () => {
      try {
        const detail = await api.getPost(postId);
        if (!alive) return;
        const post = mapPost(detail);
        setTitle(post.title);
        setDescription(post.description);
        setType(post.type);
        setCategory(post.category);
        setLocation(post.location || '');
        setExistingImages(detail.images || []);
      } catch (e) {
        if (alive) setLoadError(`No se pudo cargar la publicación: ${e.message}`);
      } finally {
        if (alive) setLoading(false);
      }
    })();
    return () => {
      alive = false;
    };
  }, [postId]);

  const addNewImages = (uris) => {
    setNewImages((prev) => {
      const room = MAX_IMAGES - existingImages.length - prev.length;
      if (uris.length > room) {
        Alert.alert('Límite de imágenes', `Solo puedes tener ${MAX_IMAGES} imágenes por publicación.`);
      }
      return [...prev, ...uris.slice(0, Math.max(0, room))].map((uri) => ({ uri }));
    });
  };

  const openCamera = async () => {
    const uri = await takeFromCamera();
    if (uri) addNewImages([uri]);
  };

  const openGallery = async () => {
    const room = MAX_IMAGES - existingImages.length - newImages.length;
    const uris = await pickFromGallery(Math.max(1, room));
    if (uris?.length) addNewImages(uris);
  };

  const removeExisting = (image) => {
    Alert.alert('Eliminar imagen', '¿Quitar esta imagen de la publicación?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Quitar',
        style: 'destructive',
        onPress: async () => {
          try {
            await api.deleteImage(image.id);
            setExistingImages((list) => list.filter((i) => i.id !== image.id));
          } catch (e) {
            Alert.alert('No se pudo eliminar la imagen', e.message);
          }
        },
      },
    ]);
  };

  const handleSave = async () => {
    if (!title.trim() || !description.trim() || !category) {
      Alert.alert('Faltan datos', 'El título, la descripción y la categoría son obligatorios.');
      return;
    }
    setSaving(true);
    try {
      // identity solo se envía si el usuario cambió el modo en esta edición
      await api.updatePost(postId, {
        title: title.trim(),
        description: description.trim(),
        type,
        category,
        location: location.trim() || null,
        ...(identity ? { identityMode: identity } : {}),
      });

      const failed = [];
      for (const img of newImages) {
        try {
          await api.uploadImage(postId, img.uri);
        } catch (e) {
          failed.push(e.message);
        }
      }
      setNewImages([]);
      setSaving(false);

      if (failed.length > 0) {
        Alert.alert('Cambios guardados', `Pero ${failed.length} imagen(es) no se subieron.`);
      } else {
        Alert.alert('Cambios guardados', 'La publicación se actualizó correctamente.');
      }
      navigation.goBack(); // MisPublicaciones se refresca al recuperar el foco
    } catch (e) {
      setSaving(false);
      Alert.alert('No se pudo guardar', e.message);
    }
  };

  const confirmDelete = () => {
    Alert.alert('Eliminar publicación', 'Se borrarán también sus imágenes y comentarios. ¿Continuar?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Eliminar',
        style: 'destructive',
        onPress: async () => {
          try {
            await api.deletePost(postId); // el backend borra también los blobs
            Alert.alert('Eliminada', 'Tu publicación fue eliminada.');
            navigation.navigate('MisPublicaciones');
          } catch (e) {
            Alert.alert('No se pudo eliminar', e.message);
          }
        },
      },
    ]);
  };

  if (loading) {
    return (
      <View style={[styles.container, styles.center]}>
        <ActivityIndicator size="large" color={colors.primary} />
      </View>
    );
  }

  if (loadError) {
    return (
      <View style={[styles.container, styles.center]}>
        <Ionicons name="alert-circle-outline" size={40} color={colors.gray400} />
        <Text style={styles.centerText}>{loadError}</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <AppHeader title="Editar publicación" onBack={() => navigation.goBack()} />

      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.group}>
          <Text style={styles.label}>Título</Text>
          <TextInput style={styles.input} value={title} onChangeText={setTitle} maxLength={150} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Descripción</Text>
          <TextInput
            style={[styles.input, styles.textarea]}
            multiline
            value={description}
            onChangeText={setDescription}
            maxLength={5000}
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
            value={categoryLabel(category)}
            onChange={(label) => setCategory(categoryValue(label))}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Ubicación</Text>
          <TextInput style={styles.input} value={location} onChangeText={setLocation} maxLength={200} />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>
            Imágenes ({existingImages.length + newImages.length}/{MAX_IMAGES})
          </Text>
          <View style={styles.previewRow}>
            {existingImages.map((image) => (
              <View key={image.id} style={styles.previewItem}>
                <Image source={{ uri: imageUrl(image) }} style={styles.previewImage} />
                <Pressable style={styles.removeBtn} onPress={() => removeExisting(image)}>
                  <Ionicons name="close" size={12} color={colors.white} />
                </Pressable>
              </View>
            ))}
            {newImages.map((img, index) => (
              <View key={`${img.uri}-${index}`} style={styles.previewItem}>
                <Image source={{ uri: img.uri }} style={styles.previewImage} />
                <Pressable
                  style={styles.removeBtn}
                  onPress={() => setNewImages((imgs) => imgs.filter((_, i) => i !== index))}
                >
                  <Ionicons name="close" size={12} color={colors.white} />
                </Pressable>
              </View>
            ))}
            {existingImages.length + newImages.length < MAX_IMAGES ? (
              <Pressable style={styles.uploadSmall} onPress={openGallery}>
                <Ionicons name="add" size={24} color={colors.gray500} />
              </Pressable>
            ) : null}
          </View>
          <View style={styles.uploadRow}>
            <Pressable style={styles.uploadSmallWide} onPress={openCamera}>
              <Ionicons name="camera" size={20} color={colors.gray500} />
              <Text style={styles.uploadTextSmall}>Tomar foto</Text>
            </Pressable>
            <Pressable style={styles.uploadSmallWide} onPress={openGallery}>
              <Ionicons name="images" size={20} color={colors.gray500} />
              <Text style={styles.uploadTextSmall}>Galería</Text>
            </Pressable>
          </View>
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Modo de identidad</Text>
          <RadioGroup options={IDENTITY_OPTIONS} value={identity} onChange={setIdentity} />
          {identity === null ? (
            <Text style={styles.hint}>Sin selección: se mantiene el modo guardado.</Text>
          ) : null}
        </View>

        <View style={styles.actions}>
          <Pressable
            style={[styles.button, saving && styles.buttonDisabled]}
            onPress={handleSave}
            disabled={saving}
          >
            {saving ? (
              <ActivityIndicator color={colors.white} />
            ) : (
              <Text style={styles.buttonText}>Guardar cambios</Text>
            )}
          </Pressable>
          <Pressable style={styles.dangerButton} onPress={confirmDelete} disabled={saving}>
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
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 10,
    padding: 24,
  },
  centerText: {
    color: colors.gray600,
    textAlign: 'center',
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
    marginBottom: 10,
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
  uploadRow: {
    flexDirection: 'row',
    gap: 10,
  },
  uploadSmallWide: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 6,
    borderWidth: 1.5,
    borderColor: colors.gray300,
    borderRadius: 8,
    paddingVertical: 10,
    backgroundColor: colors.white,
  },
  uploadTextSmall: {
    fontSize: 13,
    color: colors.gray500,
  },
  hint: {
    fontSize: 12,
    color: colors.gray400,
    marginTop: 4,
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
  buttonDisabled: {
    opacity: 0.6,
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
