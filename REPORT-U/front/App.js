import { StatusBar } from 'expo-status-bar';
import { ActivityIndicator, View } from 'react-native';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';

import { colors } from './theme';
import { AuthProvider, useAuth } from './src/AuthContext';
import LoginScreen from './screens/LoginScreen';
import RegistroScreen from './screens/RegistroScreen';
import InicioScreen from './screens/InicioScreen';
import DetalleScreen from './screens/DetalleScreen';
import FormularioScreen from './screens/FormularioScreen';
import EditarScreen from './screens/EditarScreen';
import MisPublicacionesScreen from './screens/MisPublicacionesScreen';
import GuardadosScreen from './screens/GuardadosScreen';
import PerfilScreen from './screens/PerfilScreen';
import VisorImagenScreen from './screens/VisorImagenScreen';

const AuthStack = createNativeStackNavigator();
const RootStack = createNativeStackNavigator();
const InicioStack = createNativeStackNavigator();
const GuardadosStack = createNativeStackNavigator();
const PerfilStack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

const noHeader = { headerShown: false };

// Cada pestaña tiene su propio stack para que la barra inferior
// permanezca visible en pantallas secundarias (como en el mockup)
function InicioStackNav() {
  return (
    <InicioStack.Navigator screenOptions={noHeader}>
      {/* "InicioHome" evita duplicar el nombre con la pestaña "Inicio" (warning de navegación) */}
      <InicioStack.Screen name="InicioHome" component={InicioScreen} />
      <InicioStack.Screen name="Detalle" component={DetalleScreen} />
    </InicioStack.Navigator>
  );
}

function GuardadosStackNav() {
  return (
    <GuardadosStack.Navigator screenOptions={noHeader}>
      {/* "GuardadosHome" evita duplicar el nombre con la pestaña "Guardados" */}
      <GuardadosStack.Screen name="GuardadosHome" component={GuardadosScreen} />
      <GuardadosStack.Screen name="Detalle" component={DetalleScreen} />
    </GuardadosStack.Navigator>
  );
}

function PerfilStackNav() {
  return (
    <PerfilStack.Navigator screenOptions={noHeader}>
      {/* "PerfilHome" evita duplicar el nombre con la pestaña "Perfil" */}
      <PerfilStack.Screen name="PerfilHome" component={PerfilScreen} />
      <PerfilStack.Screen name="MisPublicaciones" component={MisPublicacionesScreen} />
      <PerfilStack.Screen name="Editar" component={EditarScreen} />
    </PerfilStack.Navigator>
  );
}

function MainTabs() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.gray400,
        tabBarLabelStyle: { fontSize: 11, fontWeight: '600' },
        tabBarStyle: { backgroundColor: colors.white, borderTopColor: colors.gray200 },
        tabBarIcon: ({ focused, color, size }) => {
          const icons = {
            Inicio: focused ? 'home' : 'home-outline',
            Crear: focused ? 'create' : 'create-outline',
            Guardados: focused ? 'bookmark' : 'bookmark-outline',
            Perfil: focused ? 'person' : 'person-outline',
          };
          return <Ionicons name={icons[route.name]} size={size} color={color} />;
        },
      })}
    >
      <Tab.Screen name="Inicio" component={InicioStackNav} />
      <Tab.Screen name="Crear" component={FormularioScreen} />
      <Tab.Screen name="Guardados" component={GuardadosStackNav} />
      <Tab.Screen name="Perfil" component={PerfilStackNav} />
    </Tab.Navigator>
  );
}

function MainNavigator() {
  return (
    <RootStack.Navigator screenOptions={noHeader}>
      <RootStack.Screen name="Main" component={MainTabs} />
      <RootStack.Screen
        name="VisorImagen"
        component={VisorImagenScreen}
        options={{ presentation: 'fullScreenModal', animation: 'fade' }}
      />
    </RootStack.Navigator>
  );
}

// Login y Registro se muestran sin la barra inferior (authScreens del mockup)
function AuthNavigator() {
  return (
    <AuthStack.Navigator screenOptions={noHeader}>
      <AuthStack.Screen name="Login" component={LoginScreen} />
      <AuthStack.Screen name="Registro" component={RegistroScreen} />
    </AuthStack.Navigator>
  );
}

function Root() {
  const { user, loading } = useAuth();

  // Espera el auto-login (token persistido) antes de navegar
  if (loading) {
    return (
      <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: colors.primary }}>
        <ActivityIndicator size="large" color={colors.white} />
      </View>
    );
  }

  return user ? <MainNavigator /> : <AuthNavigator />;
}

export default function App() {
  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <AuthProvider>
        <NavigationContainer>
          <Root />
        </NavigationContainer>
      </AuthProvider>
    </SafeAreaProvider>
  );
}
