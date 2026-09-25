import { useState } from 'react';
import { ScrollView, View, Text, TextInput, Pressable, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, shadowMd } from '../theme';
import Dropdown from '../components/Dropdown';
import { careers } from '../data/posts';

// Pantalla de Registro (mockup #screen-register)
export default function RegistroScreen({ navigation, onLogin }) {
  const [career, setCareer] = useState('');

  return (
    <View style={styles.container}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.logo}>
          <Ionicons name="megaphone" size={56} color={colors.white} />
          <Text style={styles.logoTitle}>ReportU</Text>
          <Text style={styles.logoSubtitle}>Crea tu cuenta</Text>
        </View>

        <View style={styles.card}>
          <View style={styles.group}>
            <Text style={styles.label}>Nombre completo</Text>
            <TextInput
              style={styles.input}
              placeholder="Juan Pérez López"
              placeholderTextColor={colors.gray400}
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Username</Text>
            <TextInput
              style={styles.input}
              placeholder="juanperez"
              placeholderTextColor={colors.gray400}
              autoCapitalize="none"
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Email</Text>
            <TextInput
              style={styles.input}
              placeholder="juan@universidad.edu"
              placeholderTextColor={colors.gray400}
              keyboardType="email-address"
              autoCapitalize="none"
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Contraseña</Text>
            <TextInput
              style={styles.input}
              placeholder="••••••••"
              placeholderTextColor={colors.gray400}
              secureTextEntry
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Carrera</Text>
            <Dropdown
              options={careers}
              value={career}
              placeholder="Selecciona tu carrera"
              onChange={setCareer}
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Matrícula</Text>
            <TextInput
              style={styles.input}
              placeholder="20240001"
              placeholderTextColor={colors.gray400}
              keyboardType="numeric"
            />
          </View>

          <Pressable style={styles.button} onPress={onLogin}>
            <Text style={styles.buttonText}>Crear cuenta</Text>
          </Pressable>
        </View>

        <Text style={styles.footer}>
          ¿Ya tienes cuenta?{' '}
          <Text style={styles.link} onPress={() => navigation.navigate('Login')}>
            Inicia sesión
          </Text>
        </Text>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.primary,
  },
  content: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: 24,
  },
  logo: {
    alignItems: 'center',
    marginBottom: 32,
  },
  logoTitle: {
    fontSize: 32,
    fontWeight: '800',
    color: colors.white,
    marginTop: 8,
  },
  logoSubtitle: {
    fontSize: 14,
    color: colors.white,
    opacity: 0.85,
    marginTop: 4,
  },
  card: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 24,
    width: '100%',
    ...shadowMd,
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
  footer: {
    color: colors.white,
    marginTop: 16,
    fontSize: 14,
    textAlign: 'center',
  },
  link: {
    fontWeight: '700',
    textDecorationLine: 'underline',
  },
});
