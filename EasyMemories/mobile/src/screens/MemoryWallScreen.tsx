import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useCallback, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { getMemories } from '../api/join';
import { RootStackParamList } from '../navigation/types';
import { MemoryResponse } from '../types';
import { colors, spacing } from '../theme';
import MemoryGrid from '../components/MemoryGrid';

type Props = NativeStackScreenProps<RootStackParamList, 'MemoryWall'>;

export default function MemoryWallScreen({ route, navigation }: Props) {
  const { joinCode, spaceName } = route.params;
  const [memories, setMemories] = useState<MemoryResponse[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setMemories(await getMemories(joinCode));
    setLoading(false);
  }, [joinCode]);

  useFocusEffect(
    useCallback(() => {
      navigation.setOptions({ title: spaceName });
      load();
    }, [load, navigation, spaceName])
  );

  return (
    <View style={styles.container}>
      <MemoryGrid
        memories={memories}
        refreshing={loading}
        onRefresh={load}
        emptyText="Todavía no hay recuerdos. ¡Sé el primero en subir uno!"
      />
      <Pressable
        style={styles.fab}
        onPress={() => navigation.navigate('UploadMemory', { joinCode, spaceName })}
      >
        <Text style={styles.fabText}>+ Subir recuerdo</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
  fab: {
    position: 'absolute',
    bottom: spacing.lg,
    left: spacing.lg,
    right: spacing.lg,
    backgroundColor: colors.primary,
    borderRadius: 14,
    padding: spacing.md,
    alignItems: 'center',
  },
  fabText: { color: '#1A1024', fontWeight: '700', fontSize: 16 },
});
