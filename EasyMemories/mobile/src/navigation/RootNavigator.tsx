import { LinkingOptions, NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import React from 'react';

import HomeScreen from '../screens/HomeScreen';
import LoginScreen from '../screens/LoginScreen';
import RegisterScreen from '../screens/RegisterScreen';
import SpacesListScreen from '../screens/SpacesListScreen';
import CreateSpaceScreen from '../screens/CreateSpaceScreen';
import SpaceDetailScreen from '../screens/SpaceDetailScreen';
import SpaceMemoryWallScreen from '../screens/SpaceMemoryWallScreen';
import ScanQrScreen from '../screens/ScanQrScreen';
import JoinScreen from '../screens/JoinScreen';
import UploadMemoryScreen from '../screens/UploadMemoryScreen';
import MemoryWallScreen from '../screens/MemoryWallScreen';
import { RootStackParamList } from './types';
import { colors } from '../theme';

const Stack = createNativeStackNavigator<RootStackParamList>();

// Permite abrir la app directamente en la pantalla de unión al escanear el QR
// con la cámara nativa del teléfono (en builds standalone con el scheme registrado).
const linking: LinkingOptions<RootStackParamList> = {
  prefixes: ['easymemories://'],
  config: {
    screens: {
      Join: 'join/:joinCode',
      Home: '*',
    },
  },
};

export default function RootNavigator() {
  return (
    <NavigationContainer linking={linking}>
      <Stack.Navigator
        initialRouteName="Home"
        screenOptions={{
          headerStyle: { backgroundColor: colors.background },
          headerTintColor: colors.text,
          headerShadowVisible: false,
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="Home" component={HomeScreen} options={{ headerShown: false }} />
        <Stack.Screen name="Login" component={LoginScreen} options={{ title: '' }} />
        <Stack.Screen name="Register" component={RegisterScreen} options={{ title: '' }} />
        <Stack.Screen name="SpacesList" component={SpacesListScreen} options={{ headerShown: false }} />
        <Stack.Screen name="CreateSpace" component={CreateSpaceScreen} options={{ title: '' }} />
        <Stack.Screen name="SpaceDetail" component={SpaceDetailScreen} options={{ title: 'Espacio' }} />
        <Stack.Screen name="SpaceMemoryWall" component={SpaceMemoryWallScreen} />
        <Stack.Screen name="ScanQr" component={ScanQrScreen} options={{ title: 'Escanear QR' }} />
        <Stack.Screen name="Join" component={JoinScreen} options={{ title: '' }} />
        <Stack.Screen name="UploadMemory" component={UploadMemoryScreen} options={{ title: 'Subir recuerdo' }} />
        <Stack.Screen name="MemoryWall" component={MemoryWallScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
