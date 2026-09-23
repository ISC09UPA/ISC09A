import { View, Text, Button, StyleSheet } from 'react-native';

export default function ({ navigation }) {
    return (
        <View style={styles.container}>
            <Text style={styles.title}>Pantalla de Inicio</Text>
            <Button title="Ir a Detalles" onPress={() => navigation.navigate('Detalle')} />
        </View>
    )
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
});