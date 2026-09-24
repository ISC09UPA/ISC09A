import React from 'react';
import { StyleSheet, Text, View, TextInput, TouchableOpacity, SafeAreaView, ScrollView } from 'react-native';

export default function FormularioScreen() {
  return (
    // SafeAreaView asegura que el contenido no choque con la barra del celular
    <ScrollView style={styles.container}>
      
      {/* Contenedor principal con margen */}
      <View style={styles.content}>
        
        {/* Título y Subtítulo */}
        <Text style={styles.name}>Juan Ángel</Text>
        <Text style={styles.subtitle}>Se la *****************</Text>

        {/* Campo de Correo */}
        <Text style={styles.label}>Correo:</Text>
        <TextInput 
          style={styles.input} 
          placeholder="ejemplo@correo.com" 
          keyboardType="email-address"
        />

        {/* Campo de Teléfono */}
        <Text style={styles.label}>Teléfono:</Text>
        <TextInput 
          style={styles.input} 
          placeholder="12345678" 
          keyboardType="phone-pad"
        />

        {/* Botón Azul */}
        <TouchableOpacity style={styles.button}>
          <Text style={styles.buttonText}>GUARDAR CAMBIOS</Text>
        </TouchableOpacity>

      </View>
    </ScrollView>
  );
}

// Aquí le damos el diseño y colores para que quede igual a tu foto
const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#e9f1f7', // Color de fondo azul/gris clarito
  },
  content: {
    flex: 1,
    justifyContent: 'center', // Centra todo verticalmente
    paddingHorizontal: 30,
    paddingVertical: 80, // Margen a los lados
  },
  name: {
    fontSize: 26,
    fontWeight: 'bold',
    color: '#2d2220', // Un tono oscuro casi negro
    marginBottom: 5,
    textAlign: 'center'
  },
  subtitle: {
    fontSize: 14,
    color: '#333',
    marginBottom: 40, // Espacio grande antes del formulario
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
    borderColor: '#c0cad5', // Borde gris claro
    backgroundColor: '#dce5f0', // Fondo de la caja de texto
    borderRadius: 8, // Bordes redondeados
    paddingVertical: 12,
    paddingHorizontal: 15,
    fontSize: 16,
    marginBottom: 20, // Espacio entre cada campo
  },
  button: {
    backgroundColor: '#008ce6', // El color azul brillante del botón
    paddingVertical: 15,
    borderRadius: 5, // Bordes ligeramente redondeados
    alignItems: 'center', // Centra el texto dentro del botón
    marginTop: 10,
  },
  buttonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: 'bold',
  }
});