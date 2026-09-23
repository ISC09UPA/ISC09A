import React from 'react';
import { StyleSheet, View } from 'react-native';

const PerfilScreen = () => {
    return (
        <View style={styles.container}>
            <Text style={styles.title}>Pantalla de Perfil</Text>
        </View>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        alignContent: 'center',
        justifyContent: 'center',
        backgroundColor: '#fff',
        /* alignItems: 'center',
        padding: 20 */
    },
    title: {
        fontSize: 24,
        /* fontWeight: 'bold', */
        marginBottom: 20
    }
})

export default PerfilScreen;
