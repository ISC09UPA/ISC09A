import { useState } from 'react';
import { View, Text, ScrollView, Pressable, Switch, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors, shadow } from '../theme';
import AppHeader from '../components/AppHeader';
import Dropdown from '../components/Dropdown';

// Pantalla de Perfil (mockup #screen-profile)
export default function PerfilScreen({ navigation, onLogout }) {
  const [displayName, setDisplayName] = useState('Nombre real');
  const [showEnrollment, setShowEnrollment] = useState(false);

  return (
    <View style={styles.container}>
      <AppHeader title="Perfil" />

      <ScrollView contentContainerStyle={styles.content}>
        <View style={styles.avatarWrap}>
          <View style={styles.avatar}>
            <Text style={styles.avatarText}>JP</Text>
          </View>
        </View>

        <Text style={styles.name}>Juan Pérez López</Text>
        <Text style={styles.username}>@juanperez</Text>
        <Text style={styles.career}>Ingeniería en Sistemas Computacionales</Text>

        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Privacidad</Text>
          <View style={styles.option}>
            <Text style={styles.optionLabel}>Mostrar como</Text>
            <Dropdown
              compact
              options={['Nombre real', 'Username', 'Anónimo']}
              value={displayName}
              onChange={setDisplayName}
            />
          </View>
          <View style={[styles.option, styles.optionLast]}>
            <Text style={styles.optionLabel}>Mostrar matrícula</Text>
            <Switch
              value={showEnrollment}
              onValueChange={setShowEnrollment}
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

          <View style={styles.menuItem}>
            <View style={styles.menuLeft}>
              <Ionicons name="settings-outline" size={18} color={colors.gray700} />
              <Text style={styles.menuText}>Configuración</Text>
            </View>
            <Ionicons name="chevron-forward" size={20} color={colors.gray400} />
          </View>

          <Pressable style={[styles.menuItem, styles.menuItemLast]} onPress={onLogout}>
            <View style={styles.menuLeft}>
              <Ionicons name="log-out-outline" size={18} color={colors.danger} />
              <Text style={[styles.menuText, styles.logoutText]}>Cerrar sesión</Text>
            </View>
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
  },
  menuText: {
    fontSize: 15,
    color: colors.gray800,
  },
  logoutText: {
    color: colors.danger,
  },
});
