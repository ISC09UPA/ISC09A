import { View, Text, Pressable, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';

// .radio-group / .radio-option del mockup
export default function RadioGroup({ options, value, onChange }) {
  return (
    <View style={styles.group}>
      {options.map((option) => (
        <Pressable key={option.value} style={styles.option} onPress={() => onChange(option.value)} hitSlop={4}>
          <Ionicons
            name={value === option.value ? 'radio-button-on' : 'radio-button-off'}
            size={18}
            color={colors.primary}
          />
          <Text style={styles.label}>{option.label}</Text>
        </Pressable>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  group: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 12,
  },
  option: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  label: {
    fontSize: 14,
    color: colors.gray800,
  },
});
