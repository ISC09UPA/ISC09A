# Mobile: ImageCards.Mobile

App Expo (SDK 57) + React Native + TypeScript.

```bash
cd mobile/ImageCards.Mobile
cp .env.example .env        # EXPO_PUBLIC_API_URL
npm install
npx expo start

npm run lint
npm run typecheck
npm test
```

Pantallas (bottom tabs): **Repasar** (flujo de flashcards), **Tarjetas** (listado por idioma) y
**Ajustes** (idioma de estudio, servidor configurado).

- Toda la comunicación HTTP pasa por `src/services/api/`.
- Los textos de la UI están en `src/constants/strings.ts`.
- Los tests viven junto al código, en carpetas `__tests__`.
- Instala dependencias con `npx expo install <paquete>` (versiones compatibles con el SDK).

Arquitectura en [docs/architecture.md](../docs/architecture.md#mobile).
