import AsyncStorage from '@react-native-async-storage/async-storage';
import React, { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { AUTH_TOKEN_KEY } from '../api/client';
import * as authApi from '../api/auth';

interface AuthState {
  isLoading: boolean;
  isAuthenticated: boolean;
  displayName: string | null;
  email: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, displayName: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

const PROFILE_KEY = 'easymemories.authProfile';

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isLoading, setIsLoading] = useState(true);
  const [displayName, setDisplayName] = useState<string | null>(null);
  const [email, setEmail] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      const [token, profileJson] = await Promise.all([
        AsyncStorage.getItem(AUTH_TOKEN_KEY),
        AsyncStorage.getItem(PROFILE_KEY),
      ]);

      if (token && profileJson) {
        const profile = JSON.parse(profileJson) as { displayName: string; email: string };
        setDisplayName(profile.displayName);
        setEmail(profile.email);
      }
      setIsLoading(false);
    })();
  }, []);

  async function persistSession(token: string, name: string, mail: string) {
    await AsyncStorage.setItem(AUTH_TOKEN_KEY, token);
    await AsyncStorage.setItem(PROFILE_KEY, JSON.stringify({ displayName: name, email: mail }));
    setDisplayName(name);
    setEmail(mail);
  }

  const value = useMemo<AuthState>(
    () => ({
      isLoading,
      isAuthenticated: !!displayName,
      displayName,
      email,
      async login(loginEmail, password) {
        const res = await authApi.login(loginEmail, password);
        await persistSession(res.token, res.displayName, res.email);
      },
      async register(regEmail, password, name) {
        const res = await authApi.register(regEmail, password, name);
        await persistSession(res.token, res.displayName, res.email);
      },
      async logout() {
        await AsyncStorage.multiRemove([AUTH_TOKEN_KEY, PROFILE_KEY]);
        setDisplayName(null);
        setEmail(null);
      },
    }),
    [isLoading, displayName, email]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de <AuthProvider>');
  return ctx;
}
