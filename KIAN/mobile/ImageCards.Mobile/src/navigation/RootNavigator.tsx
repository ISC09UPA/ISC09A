import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';

import { strings } from '../constants/strings';
import { colors } from '../constants/theme';
import { CardsScreen } from '../screens/CardsScreen';
import { ReviewScreen } from '../screens/ReviewScreen';
import { SettingsScreen } from '../screens/SettingsScreen';
import type { RootTabParamList } from './types';

const Tab = createBottomTabNavigator<RootTabParamList>();

export function RootNavigator() {
  return (
    <Tab.Navigator
      initialRouteName="Review"
      screenOptions={{
        tabBarActiveTintColor: colors.primary,
        tabBarInactiveTintColor: colors.textMuted,
        tabBarIconStyle: { display: 'none' },
        tabBarLabelStyle: { fontSize: 15, fontWeight: '600' },
      }}
    >
      <Tab.Screen name="Review" component={ReviewScreen} options={{ title: strings.tabs.review }} />
      <Tab.Screen name="Cards" component={CardsScreen} options={{ title: strings.tabs.cards }} />
      <Tab.Screen name="Settings" component={SettingsScreen} options={{ title: strings.tabs.settings }} />
    </Tab.Navigator>
  );
}
