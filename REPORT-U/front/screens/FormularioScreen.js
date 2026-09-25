import { useState } from 'react';
import { View, Text, ScrollView, Pressable, TextInput, Image, Alert, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';
import AppHeader from '../components/AppHeader';
import RadioGroup from '../components/RadioGroup';
import Dropdown from '../components/Dropdown';
import { categories, TYPE_OPTIONS, IDENTITY_OPTIONS, thumbUrl } from '../data/posts';

// Pantalla de nueva publicación (mockup #screen-create)
export default function FormularioScreen({ navigation }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState('incidencia');
  const [category, setCategory] = useState('');
  const [location, setLocation] = useState('');
  const [images, setImages] = useState(['upload1']);
  const [identity, setIdentity] = useState('real');

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
            options={categories}
            value={category}
            placeholder="Selecciona una categoría"
            onChange={setCategory}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Ubicación</Text>
          <TextInput
            style={styles.input}
            placeholder="Ej: Edificio B, segundo piso"
            placeholderTextColor={colors.gray400}
            value={location}
            onChangeText={setLocation}
          />
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Imágenes (0–4)</Text>
          <View style={styles.uploadRow}>
            <Pressable style={styles.upload} onPress={() => Alert.alert('📷 Abrir cámara o galería')}>
              <Ionicons name="camera" size={24} color={colors.gray500} />
              <Text style={styles.uploadText}>Tomar foto</Text>
            </Pressable>
            <Pressable style={styles.upload} onPress={() => Alert.alert('🖼️ Abrir galería')}>
              <Ionicons name="images" size={24} color={colors.gray500} />
              <Text style={styles.uploadText}>Galería</Text>
            </Pressable>
          </View>

          {images.length > 0 ? (
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
            </View>
          ) : null}
        </View>

        <View style={styles.group}>
          <Text style={styles.label}>Modo de identidad</Text>
          <RadioGroup options={IDENTITY_OPTIONS} value={identity} onChange={setIdentity} />
        </View>

        <Pressable style={styles.button} onPress={() => navigation.navigate('Inicio')}>
          <Text style={styles.buttonText}>Publicar</Text>
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
  buttonText: {
    color: colors.white,
    fontSize: 16,
    fontWeight: '600',
  },
});
