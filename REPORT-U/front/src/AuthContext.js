import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { api, loadSession, saveSession, clearSession, setAuthToken, setCachedUser } from './api';

// Estado global de sesión: token + perfil del estudiante.
// Al montar intenta auto-login con el token persistido en AsyncStorage.
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadSession()
      .then((session) => {
        if (session) {
          setAuthToken(session.token);
          setUser(session.user);
          // Refresca el perfil por si cambió en otro dispositivo.
          api
            .getMe()
            .then((me) => {
              setUser(me);
              return setCachedUser(me);
            })
            .catch(() => {}); // offline o token revocado: se mantiene el caché
        }
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const value = useMemo(
    () => ({
      user,
      loading,
      async login(identifier, password) {
        const auth = await api.login(identifier, password);
        setAuthToken(auth.accessToken);
        setUser(auth.user);
        await saveSession(auth);
        return auth.user;
      },
      async register(data) {
        const auth = await api.register(data);
        setAuthToken(auth.accessToken);
        setUser(auth.user);
        await saveSession(auth);
        return auth.user;
      },
      async updateProfile(patch) {
        const me = await api.updateMe(patch);
        setUser(me);
        await setCachedUser(me);
        return me;
      },
      async logout() {
        setAuthToken(null);
        setUser(null);
        await clearSession();
      },
    }),
    [user, loading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de <AuthProvider>');
  return ctx;
}
