import { CameraView, useCameraPermissions } from 'expo-camera';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useState } from 'react';
import { Alert, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { RootStackParamList } from '../navigation/types';
import { colors, spacing } from '../theme';

type Props = NativeStackScreenProps<RootStackParamList, 'ScanQr'>;

/** Acepta tanto el deep link completo (easymemories://join/ABC123) como el código a secas. */
function extractJoinCode(scannedValue: string): string {
  const match = scannedValue.match(/join\/([A-Za-z0-9]+)/);
  return (match ? match[1] : scannedValue).trim().toUpperCase();
}

export default function ScanQrScreen({ navigation }: Props) {
  const [permission, requestPermission] = useCameraPermissions();
  const [scanned, setScanned] = useState(false);
  const [manualCode, setManualCode] = useState('');

  function goToJoinCode(raw: string) {
    const joinCode = extractJoinCode(raw);
    if (!joinCode) return;
    navigation.navigate('Join', { joinCode });
  }

  if (!permission) {
    return <View style={styles.container} />;
  }

  if (!permission.granted) {
    return (
      <View style={styles.container}>
        <Text style={styles.message}>Necesitamos permiso de cámara para escanear el código QR.</Text>
        <Pressable style={styles.primaryButton} onPress={requestPermission}>
          <Text style={styles.primaryButtonText}>Dar permiso</Text>
        </Pressable>

        <ManualEntry value={manualCode} onChangeText={setManualCode} onSubmit={() => goToJoinCode(manualCode)} />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <CameraView
        style={StyleSheet.absoluteFill}
        barcodeScannerSettings={{ barcodeTypes: ['qr'] }}
        onBarcodeScanned={({ data }) => {
          if (scanned) return;
          setScanned(true);
          goToJoinCode(data);
          setTimeout(() => setScanned(false), 2000);
        }}
      />
      <View style={styles.frame} pointerEvents="none" />
      <View style={styles.overlay}>
        <Text style={styles.overlayText}>Apunta la cámara al código QR del evento</Text>
        <ManualEntry value={manualCode} onChangeText={setManualCode} onSubmit={() => goToJoinCode(manualCode)} />
      </View>
    </View>
  );
}

function ManualEntry({
  value,
  onChangeText,
  onSubmit,
}: {
  value: string;
  onChangeText: (v: string) => void;
  onSubmit: () => void;
}) {
  return (
    <View style={styles.manualEntry}>
      <TextInput
        style={styles.manualInput}
        placeholder="O escribe el código"
        placeholderTextColor={colors.textMuted}
        autoCapitalize="characters"
        value={value}
        onChangeText={onChangeText}
      />
      <Pressable style={styles.manualButton} onPress={onSubmit}>
        <Text style={styles.primaryButtonText}>Ir</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background, justifyContent: 'center', padding: spacing.lg },
  message: { color: colors.text, textAlign: 'center', marginBottom: spacing.lg },
  frame: {
    position: 'absolute',
    top: '30%',
    left: '15%',
    right: '15%',
    height: '30%',
    borderWidth: 3,
    borderColor: colors.primary,
    borderRadius: 20,
  },
  overlay: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    padding: spacing.lg,
    backgroundColor: 'rgba(15, 11, 30, 0.85)',
  },
  overlayText: { color: colors.text, textAlign: 'center', marginBottom: spacing.md },
  manualEntry: { flexDirection: 'row', gap: spacing.sm },
  manualInput: {
    flex: 1,
    borderWidth: 1,
    borderColor: colors.border,
    borderRadius: 12,
    padding: spacing.md,
    color: colors.text,
  },
  manualButton: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    paddingHorizontal: spacing.lg,
    justifyContent: 'center',
  },
  primaryButton: {
    backgroundColor: colors.primary,
    borderRadius: 12,
    padding: spacing.md,
    alignItems: 'center',
  },
  primaryButtonText: { color: '#1A1024', fontSize: 16, fontWeight: '700' },
});
