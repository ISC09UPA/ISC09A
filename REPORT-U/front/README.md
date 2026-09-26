# ReportU — App móvil (React Native + Expo)

Frontend de ReportU conectado a la API de `../api` (ASP.NET Core + PostgreSQL + Azure Blob Storage).

## 1. Instalar dependencias

El cliente de API usa paquetes del ecosistema Expo que no vienen en el template
en blanco. Instálalos con `npx expo install` (elige las versiones correctas para
el SDK 57):

```bash
cd front
npm install
npx expo install expo-image-picker expo-file-system expo-sharing @react-native-async-storage/async-storage
```

| Paquete | Uso |
|---|---|
| `expo-image-picker` | Tomar foto / seleccionar de galería |
| `expo-file-system` | Descargar la imagen original (`expo-file-system/legacy`) |
| `expo-sharing` | Guardar la imagen descargada en el dispositivo |
| `@react-native-async-storage/async-storage` | Persistir token JWT y sesión |

## 2. Configurar la URL de la API

Crea `front/.env` (no se commitea) con la URL según dónde corras la app:

```text
# Emulador de Android Studio (la API corre en tu máquina)
EXPO_PUBLIC_API_URL=http://10.0.2.2:5034

# Expo Go en teléfono físico (misma red Wi-Fi; usa la IP LAN de tu PC)
# EXPO_PUBLIC_API_URL=http://192.168.1.10:5034

# Simulador de iOS
# EXPO_PUBLIC_API_URL=http://localhost:5034
```

Si no existe el `.env`, el default es `http://10.0.2.2:5034` (emulador Android).
Reinicia `npm start` después de cambiarlo.

## 3. Levantar todo

```bash
# Terminal 1 — base de datos + API (ver api/README.md)
cd api && podman compose up -d && dotnet run

# Terminal 2 — app
cd front && npm start
```

Swagger con el contrato completo: http://localhost:5034/swagger

## Arquitectura del frontend

```text
App.js               → Navegación (auth stack / tabs) + AuthProvider
src/api.js           → Cliente HTTP: JWT, ProblemDetails, FormData, imageUrl()
src/AuthContext.js   → Sesión global (login, registro, perfil, logout)
src/constants.js     → Enums de la API (valores exactos) + labels con acentos
src/labels.js        → value ↔ label para categorías, tipos e identidad
src/format.js        → mapPost() DTO→UI, timeAgo()
src/imagePicker.js   → Cámara/galería con permisos (expo-image-picker)
screens/             → Las 10 pantallas del mockup, ya conectadas
components/          → PostCard, PostBadges, Dropdown, RadioGroup, AppHeader
```

Decisiones de integración:

- **JWT**: se guarda en AsyncStorage y viaja como `Authorization: Bearer` en
  todas las llamadas autenticadas. Para `<Image>` y `downloadAsync` (que no
  aceptan headers) se anexa `?access_token=...` a la URL de `/api/media/{id}`.
- **Errores**: la API responde RFC 7807 con `code` propio; `ApiError` lo expone
  (`e.code === 'already_supported'`, `email_taken`, etc.) para que la UI
  ramifique por código y no por mensajes.
- **Imágenes**: máx. 4 por publicación (validado en UI y en la API). Al crear se
  suben después del `POST /api/posts`; al editar se pueden eliminar las
  existentes (`DELETE /api/media/{id}`) o agregar nuevas.
- **Apoyos/guardados**: actualización optimista con reconciliación con la
  respuesta del servidor (`supportCount` real).
- **Logout**: se borra el token del cliente (la API es JWT sin estado).

## Flujo de la demo (criterio de terminado)

Registrarse → iniciar sesión → publicar con foto → verla en el feed → abrir
detalle → descargar imagen → otro usuario comenta y apoya → el autor edita →
elimina la publicación (la imagen desaparece de Azure Blob Storage).
