import { ScrollView, View, Text, TextInput, Pressable, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, shadowMd } from '../theme';

// Pantalla de Login (mockup #screen-login)
export default function LoginScreen({ navigation, onLogin }) {
  return (
    <View style={styles.container}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <View style={styles.logo}>
          <Ionicons name="megaphone" size={56} color={colors.white} />
          <Text style={styles.logoTitle}>ReportU</Text>
          <Text style={styles.logoSubtitle}>Comunidad universitaria</Text>
        </View>

        <View style={styles.card}>
          <View style={styles.group}>
            <Text style={styles.label}>Email</Text>
            <TextInput
              style={styles.input}
              placeholder="tu@universidad.edu"
              placeholderTextColor={colors.gray400}
              keyboardType="email-address"
              autoCapitalize="none"
              defaultValue="juan@uni.mx"
            />
          </View>

          <View style={styles.group}>
            <Text style={styles.label}>Contraseña</Text>
            <TextInput
              style={styles.input}
              placeholder="••••••••"
              placeholderTextColor={colors.gray400}
              secureTextEntry
              defaultValue="123456"
            />
          </View>

          <Pressable style={styles.button} onPress={onLogin}>
            <Text style={styles.buttonText}>Iniciar sesión</Text>
          </Pressable>
        </View>

        <Text style={styles.footer}>
          ¿No tienes cuenta?{' '}
          <Text style={styles.link} onPress={() => navigation.navigate('Registro')}>
            Regístrate
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
