import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';

import { DEFAULT_LANGUAGE, type LanguageCode } from '../types/language';

interface LanguageContextValue {
  language: LanguageCode;
  setLanguage: (language: LanguageCode) => void;
}

const LanguageContext = createContext<LanguageContextValue | null>(null);

interface LanguageProviderProps {
  children: ReactNode;
  initialLanguage?: LanguageCode;
}

/** Study language shared by every screen. Kept in memory; persistence can be added here later. */
export function LanguageProvider({ children, initialLanguage = DEFAULT_LANGUAGE }: LanguageProviderProps) {
  const [language, setLanguage] = useState<LanguageCode>(initialLanguage);
  const value = useMemo(() => ({ language, setLanguage }), [language]);
  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useLanguage(): LanguageContextValue {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used inside a LanguageProvider.');
  }
  return context;
}
