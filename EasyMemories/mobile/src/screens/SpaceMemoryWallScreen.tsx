import { NativeStackScreenProps } from '@react-navigation/native-stack';
import React, { useCallback, useState } from 'react';
import { StyleSheet, View } from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { getSpaceMemories } from '../api/spaces';
import { RootStackParamList } from '../navigation/types';
import { MemoryResponse } from '../types';
import { colors } from '../theme';
import MemoryGrid from '../components/MemoryGrid';

type Props = NativeStackScreenProps<RootStackParamList, 'SpaceMemoryWall'>;

export default function SpaceMemoryWallScreen({ route, navigation }: Props) {
  const { spaceId, spaceName } = route.params;
  const [memories, setMemories] = useState<MemoryResponse[]>([]);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setMemories(await getSpaceMemories(spaceId));
    setLoading(false);
  }, [spaceId]);

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
        emptyText="Todavía no hay recuerdos subidos."
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: colors.background },
});
