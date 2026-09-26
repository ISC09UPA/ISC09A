import { useState } from 'react';
import { View, Text, ScrollView, Pressable, Switch, Alert, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, shadow } from '../theme';
import AppHeader from '../components/AppHeader';
import Dropdown from '../components/Dropdown';
import { useAuth } from '../src/AuthContext';
import { displayNameLabel, displayNameValue } from '../src/labels';

// Pantalla de Perfil (mockup #screen-profile)
// GET /api/users/me al montar; PATCH al cambiar privacidad. Email nunca es público.
export default function PerfilScreen({ navigation, onLogout }) {
  const { user, updateProfile, logout } = useAuth();
  const [savingPref, setSavingPref] = useState(false);

  if (!user) {
    return (
      <View style={styles.container}>
        <AppHeader title="Perfil" />
      </View>
    );
  }

  const initials = (user.fullName || user.username || '?')
    .split(' ')
    .map((w) => w[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase();

  const changePreference = async (label) => {
    const value = displayNameValue(label);
    setSavingPref(true);
    try {
      await updateProfile({ displayNamePreference: value });
    } catch (e) {
      Alert.alert('No se pudo guardar la preferencia', e.message);
    } finally {
      setSavingPref(false);
    }
  };

  const toggleEnrollment = async (show) => {
    setSavingPref(true);
    try {
      await updateProfile({ showEnrollmentNumber: show });
    } catch (e) {
      Alert.alert('No se pudo guardar', e.message);
    } finally {
      setSavingPref(false);
    }
  };

  const handleLogout = () => {
    Alert.alert('Cerrar sesión', '¿Seguro que quieres salir?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Salir',
        style: 'destructive',
        onPress: async () => {
          await logout();
          onLogout?.();
        },
      },
    ]);
  };

  return (
    <View style={styles.container}>
      <AppHeader title="Perfil" />

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.avatarWrap}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>{initials || '?'}</Text>
          </View>
        </View>

        <Text style={styles.name}>{user.fullName}</Text>
        <Text style={styles.username}>@{user.username}</Text>
        <Text style={styles.career}>{user.career}</Text>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Privacidad</Text>
          <View style={styles.option}>
            <Text style={styles.optionLabel}>Mostrar como</Text>
            <Dropdown
              compact
              options={DISPLAY_OPTIONS}
              value={displayNameLabel(user.displayNamePreference)}
              onChange={changePreference}
            />
          </View>
          <View style={[styles.option, styles.optionLast]}>
            <Text style={styles.optionLabel}>Mostrar matrícula</Text>
            <Switch
              value={user.showEnrollmentNumber}
              onValueChange={toggleEnrollment}
              disabled={savingPref}
              trackColor={{ false: colors.gray300, true: colors.primary }}
            />
          </View>
        </View>

        <View style={styles.menu}>
          <Pressable style={styles.menuItem} onPress={() => navigation.navigate('MisPublicaciones')}>
            <View style={styles.menuLeft}>
              <Ionicons name="document-text-outline" size={18} color={colors.gray700} />
              <Text style={styles.menuText}>Mis publicaciones</Text>
            </View>
            <Ionicons name="chevron-forward" size={20} color={colors.gray400} />
          </Pressable>

          <Pressable style={styles.menuItem} onPress={() => navigation.navigate('Guardados')}>
            <View style={styles.menuLeft}>
              <Ionicons name="bookmark-outline" size={18} color={colors.gray700} />
              <Text style={styles.menuText}>Guardados</Text>
            </View>
            <Ionicons name="chevron-forward" size={20} color={colors.gray400} />
          </Pressable>

          <View style={[styles.menuItem, styles.menuItemLast]}>
            <View style={styles.menuLeft}>
              <Ionicons name="mail-outline" size={18} color={colors.gray700} />
              <Text style={styles.menuText}>{user.email}</Text>
            </View>
          </View>
        </View>

        <Text style={styles.disclaimer}>
          Tu email nunca se muestra públicamente en la app.
        </Text>

        <Pressable style={[styles.logoutBtn]} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={18} color={colors.danger} />
          <Text style={styles.logoutText}>Cerrar sesión</Text>
        </Pressable>
      </ScrollView>
    </View>
  );
}

const DISPLAY_OPTIONS = ['Nombre real', 'Username', 'Anónimo'];

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.gray100,
  },
  content: {
    padding: 20,
    paddingBottom: 40,
  },
  avatarWrap: {
    alignItems: 'center',
    marginBottom: 16,
  },
  avatar: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: colors.primary,
    alignItems: 'center',
    justifyContent: 'center',
  },
  avatarText: {
    fontSize: 28,
    fontWeight: '700',
    color: colors.white,
  },
  name: {
    fontSize: 20,
    fontWeight: '700',
    color: colors.gray900,
    textAlign: 'center',
  },
  username: {
    fontSize: 14,
    color: colors.gray500,
    textAlign: 'center',
    marginTop: 4,
  },
  career: {
    fontSize: 13,
    color: colors.gray400,
    textAlign: 'center',
    marginTop: 4,
    marginBottom: 24,
  },
  section: {
    backgroundColor: colors.white,
    borderRadius: 12,
    padding: 16,
    marginBottom: 16,
    ...shadow,
  },
  sectionTitle: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.gray600,
    marginBottom: 12,
  },
  option: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 10,
    borderBottomWidth: 1,
    borderBottomColor: colors.gray100,
  },
  optionLast: {
    borderBottomWidth: 0,
  },
  optionLabel: {
    fontSize: 14,
    color: colors.gray800,
  },
  menu: {
    backgroundColor: colors.white,
    borderRadius: 12,
    overflow: 'hidden',
    ...shadow,
  },
  menuItem: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 16,
    paddingHorizontal: 18,
    borderBottomWidth: 1,
    borderBottomColor: colors.gray100,
  },
  menuItemLast: {
    borderBottomWidth: 0,
  },
  menuLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    flexShrink: 1,
  },
  menuText: {
    fontSize: 15,
    color: colors.gray800,
    flexShrink: 1,
  },
  disclaimer: {
    fontSize: 12,
    color: colors.gray400,
    textAlign: 'center',
    marginTop: 12,
    marginBottom: 16,
  },
  logoutBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: 8,
    backgroundColor: colors.white,
    borderRadius: 12,
    paddingVertical: 14,
    ...shadow,
  },
  logoutText: {
    color: colors.danger,
    fontSize: 15,
    fontWeight: '600',
  },
});
