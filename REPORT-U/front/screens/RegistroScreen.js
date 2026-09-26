import { useState } from 'react';
import {
  ScrollView,
  View,
  Text,
  TextInput,
  Pressable,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, shadowMd } from '../theme';
import Dropdown from '../components/Dropdown';
import { useAuth } from '../src/AuthContext';
import { ApiError } from '../src/api';
import { CAREERS } from '../src/constants';

// Pantalla de Registro (mockup #screen-register)
export default function RegistroScreen({ navigation }) {
  const { register } = useAuth();
  const [form, setForm] = useState({
    fullName: '',
    username: '',
    email: '',
    password: '',
    enrollmentNumber: '',
  });
  const [career, setCareer] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const set = (key) => (value) => setForm((f) => ({ ...f, [key]: value }));

  const handleRegister = async () => {
    const f = {
      ...form,
      fullName: form.fullName.trim(),
      username: form.username.trim(),
      email: form.email.trim(),
      enrollmentNumber: form.enrollmentNumber.trim(),
    };
    if (!f.fullName || !f.username || !f.email || !f.password || !career || !f.enrollmentNumber) {
      setError('Completa todos los campos.');
      return;
    }
    if (f.password.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }
    if (!/^[a-zA-Z0-9_.]+$/.test(f.username) || f.username.length < 3 || f.username.length > 30) {
      setError('Username: 3–30 caracteres, solo letras, números, guion bajo o punto.');
      return;
    }
    setError('');
    setSubmitting(true);
    try {
      await register({ ...f, career });
      // El cambio de sesión lo detecta App.js y muestra el feed
    } catch (e) {
      if (e instanceof ApiError) {
        if (e.code === 'email_taken') setError('Ese email ya está registrado.');
        else if (e.code === 'username_taken') setError('Ese username ya está en uso.');
        else if (e.errors) {
          const first = Object.values(e.errors)[0];
          setError(
            `Revisa los datos: ${Array.isArray(first) ? first.join(' ') : String(first)}`
          );
        } else setError(e.message);
      } else {
        setError(`No se pudo crear la cuenta: ${e.message}`);
      }
    } finally {
      setSubmitting(false);
    }
  };

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
              value={form.fullName}
              onChangeText={set('fullName')}
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Username</Text>
            <TextInput
              style={styles.input}
              placeholder="juanperez"
              placeholderTextColor={colors.gray400}
              autoCapitalize="none"
              value={form.username}
              onChangeText={set('username')}
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
              value={form.email}
              onChangeText={set('email')}
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Contraseña</Text>
            <TextInput
              style={styles.input}
              placeholder="••••••••"
              placeholderTextColor={colors.gray400}
              secureTextEntry
              value={form.password}
              onChangeText={set('password')}
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Carrera</Text>
            <Dropdown
              options={CAREERS}
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
              value={form.enrollmentNumber}
              onChangeText={set('enrollmentNumber')}
            />
          </View>

          {error ? <Text style={styles.error}>{error}</Text> : null}

          <Pressable
            style={[styles.button, submitting && styles.buttonDisabled]}
            onPress={handleRegister}
            disabled={submitting}
          >
            {submitting ? (
              <ActivityIndicator color={colors.white} />
            ) : (
              <Text style={styles.buttonText}>Crear cuenta</Text>
            )}
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
  error: {
    color: colors.danger,
    fontSize: 13,
    marginBottom: 12,
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
