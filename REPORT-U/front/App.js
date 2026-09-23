import React from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ScrollView } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Ionicons } from '@expo/vector-icons';
import InicioScreen from './screens/InicioScreen';
import DetalleScreen from './screens/DetalleScreen';
import FormularioScreen from './screens/FormularioScreen';
import PerfilScreen from './screens/PerfilScreen';


// const Stack = createNativeStackNavigator();
const Tab = createBottomTabNavigator();

export default function App() {
  return (
    <NavigationContainer>
      <Tab.Navigator 
        screenOptions={({ route }) => ({
          tabBarIcon: ({ focused, color, size }) => {
            let iconName;

            if (route.name === 'Inicio') {
              iconName = focused ? 'home' : 'home-outline';
            } else if (route.name === 'Detalle') {
              iconName = focused ? 'list' : 'list-outline';
            } else if (route.name === 'Formulario') {
              iconName = focused ? 'create' : 'create-outline';
            } else if (route.name === 'Perfil') {
              iconName = focused ? 'person' : 'person-outline';
            }

            // You can return any component that you like here!
            return <Ionicons name={iconName} size={size} color={color} />;
          },
          tabBarActiveTintColor: 'blue',
          tabBarInactiveTintColor: 'gray',
        })}
      >
        <Tab.Screen name="Inicio" component={InicioScreen} />
        <Tab.Screen name="Detalle" component={DetalleScreen} />
        <Tab.Screen name="Formulario" component={FormularioScreen} />
        <Tab.Screen name="Perfil" component={PerfilScreen} />
      </Tab.Navigator>
    </NavigationContainer>
  );
}


const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#e9f1f7', 
  },
  content: {
    flex: 1,
    justifyContent: 'center', 
    paddingHorizontal: 30,
    paddingVertical: 80, 
  },
  name: {
    fontSize: 26,
    fontWeight: 'bold',
    color: '#2d2220', 
    marginBottom: 5,
    textAlign: 'center'
  },
  subtitle: {
    fontSize: 14,
    color: '#333',
    marginBottom: 40, 
    textAlign: 'center'
  },
  label: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#2d2220',
    marginBottom: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: '#c0cad5', 
    backgroundColor: '#dce5f0', 
    borderRadius: 8, 
    paddingVertical: 12,
    paddingHorizontal: 15,
    fontSize: 16,
    marginBottom: 20, 
  },
  button: {
    backgroundColor: '#008ce6', 
    paddingVertical: 15,
    borderRadius: 5, 
    alignItems: 'center', 
    marginTop: 10,
  },
  buttonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: 'bold',
  }
});