import { useState } from 'react';
import { Modal, Pressable, Text, View, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { colors } from '../theme';

// Select custom: RN no trae <select>, así replicamos el comportamiento del mockup
export default function Dropdown({ options, value, placeholder, onChange, compact = false }) {
  const [open, setOpen] = useState(false);
  const hasValue = !!value;

  return (
    <>
      <Pressable
        style={compact ? [styles.triggerCompact, !hasValue && styles.triggerCompactEmpty] : [styles.trigger, !hasValue && styles.triggerEmpty]}
        onPress={() => setOpen(true)}
      >
        <Text
          numberOfLines={1}
          style={compact ? [styles.triggerTextCompact, !hasValue && styles.placeholderText] : [styles.triggerText, !hasValue && styles.placeholderText]}
        >
          {value || placeholder}
        </Text>
        <Ionicons name="chevron-down" size={compact ? 14 : 16} color={colors.gray400} />
      </Pressable>

      <Modal visible={open} transparent animationType="fade" onRequestClose={() => setOpen(false)}>
        <Pressable style={styles.backdrop} onPress={() => setOpen(false)}>
          <View style={styles.sheet}>
            {options.map((option, index) => (
              <Pressable
                key={option}
                style={[styles.option, index === options.length - 1 && styles.optionLast]}
                onPress={() => {
                  onChange(option);
                  setOpen(false);
                }}
              >
                <Text style={[styles.optionText, value === option && styles.optionTextActive]}>{option}</Text>
                {value === option ? <Ionicons name="checkmark" size={18} color={colors.primary} /> : null}
              </Pressable>
            ))}
          </View>
        </Pressable>
      </Modal>
    </>
  );
}

const styles = StyleSheet.create({
  trigger: {
    width: '100%',
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    borderWidth: 1.5,
    borderColor: colors.gray300,
    borderRadius: 8,
    paddingHorizontal: 14,
    paddingVertical: 12,
    backgroundColor: colors.white,
  },
  triggerEmpty: {
    borderColor: colors.gray300,
  },
  triggerText: {
    fontSize: 15,
    color: colors.gray900,
    flex: 1,
    marginRight: 8,
  },
  triggerCompact: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    borderWidth: 1,
    borderColor: colors.gray300,
    borderRadius: 6,
    paddingHorizontal: 10,
    paddingVertical: 6,
    backgroundColor: colors.white,
    maxWidth: 190,
  },
  triggerTextCompact: {
    fontSize: 13,
    color: colors.gray800,
  },
  placeholderText: {
    color: colors.gray400,
  },
  backdrop: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.4)',
    justifyContent: 'center',
    paddingHorizontal: 32,
  },
  sheet: {
    backgroundColor: colors.white,
    borderRadius: 12,
    paddingVertical: 4,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.2,
    shadowRadius: 8,
    elevation: 8,
  },
  option: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 14,
    borderBottomWidth: 1,
    borderBottomColor: colors.gray100,
  },
  optionLast: {
    borderBottomWidth: 0,
  },
  optionText: {
    fontSize: 15,
    color: colors.gray800,
    flex: 1,
  },
  optionTextActive: {
    color: colors.primary,
    fontWeight: '600',
  },
});
