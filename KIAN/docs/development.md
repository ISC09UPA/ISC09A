# Desarrollo local

## Requisitos

| Herramienta    | Versión        | Verificar          | Notas                                         |
| -------------- | -------------- | ------------------ | --------------------------------------------- |
| Git            | 2.40+          | `git --version`    |                                               |
| .NET SDK       | 10.0.x         | `dotnet --version` | Fijado en `global.json`                       |
| Node.js        | 22.13+ LTS     | `node --version`   | Requerido por Expo SDK 57 y RNTL 14           |
| npm            | 10+            | `npm --version`    |                                               |
| SQL Server     | LocalDB o Docker | `sqllocaldb info` | LocalDB viene con Visual Studio en Windows   |
| Docker Desktop | opcional       | `docker --version` | Para SQL Server + Azurite en contenedores     |
| Expo Go        | SDK 57         | —                  | En el teléfono, o un emulador Android/iOS     |

## 1. Clonar y crear tu rama

```bash
git clone <repo-url> imagecards
cd imagecards
git checkout develop
git pull origin develop
git checkout -b feature/mi-feature
```

## 2. Backend

### Opción A: todo en Docker (SQL Server + Azurite + API)

```bash
cp .env.example .env          # completar MSSQL_SA_PASSWORD y AZURITE_ACCOUNT_KEY
docker compose up -d --build
```

- Swagger: <http://localhost:5080/swagger> · Health: <http://localhost:5080/health>
- Las migrations se aplican al arrancar (`Database__ApplyMigrationsOnStartup=true`, solo en compose).

### Opción B: API con `dotnet run` (más cómodo para depurar)

1. Base de datos: LocalDB (Windows) o solo el contenedor de SQL (`docker compose up -d sqlserver`).
2. Blob Storage: Azurite (`docker compose up -d azurite` o `npx --package azurite azurite-blob --location .azurite`).
3. Secretos locales (una vez):

   ```bash
   dotnet user-secrets --project backend/ImageCards.Api set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=ImageCards;Integrated Security=true;TrustServerCertificate=true"
   dotnet user-secrets --project backend/ImageCards.Api set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
   ```

4. Migrations y arranque:

   ```bash
   dotnet tool restore
   dotnet restore
   dotnet build
   dotnet ef database update --project backend/ImageCards.Api --connection "<la misma connection string>"
   dotnet run --project backend/ImageCards.Api
   ```

   API en <http://localhost:5080> (Swagger en `/swagger`). En `Development` se crea automáticamente
   el usuario de desarrollo.

Para acceder desde un **teléfono físico**: `dotnet run --project backend/ImageCards.Api --urls http://0.0.0.0:5080`.

### Crear una tarjeta de prueba

Desde Swagger (`POST /api/cards`) o con curl (ver [api.md](api.md#crear-una-tarjeta)).

## 3. Migrations

```bash
# Crear (PascalCase, describe el cambio)
dotnet ef migrations add AddCardTags --project backend/ImageCards.Api --output-dir Data/Migrations

# Aplicar a tu base local
dotnet ef database update --project backend/ImageCards.Api --connection "<connection string>"

# Ver el SQL generado / script idempotente para despliegues
dotnet ef migrations script --idempotent --project backend/ImageCards.Api -o artifacts/migrate.sql
```

- `DesignTimeDbContextFactory` permite crear migrations sin configurar nada. Para `database update`
  usa `--connection` o la variable `ConnectionStrings__DefaultConnection`.
- No edites migrations que ya estén en `develop`: crea una nueva.
- Si dos ramas agregan migrations en paralelo, la segunda en integrarse la regenera sobre `develop` actualizado
  (`ImageCardsDbContextModelSnapshot.cs` es el archivo que suele entrar en conflicto).
- Nunca apliques `database update` sin revisar antes contra una base con datos reales.

## 4. Mobile

```bash
cd mobile/ImageCards.Mobile
cp .env.example .env          # EXPO_PUBLIC_API_URL (ver tabla en environment.md)
npm ci                        # o npm install
npx expo start                # escanear el QR con Expo Go, o pulsar a (Android) / i (iOS)
```

Instala dependencias nuevas siempre con `npx expo install <paquete>` para obtener versiones compatibles con el SDK.

### Imágenes desde el teléfono

Las URLs de imagen apuntan al host del Blob Storage. Con Azurite en `127.0.0.1`, un teléfono físico
**no** puede cargarlas. Opciones:

- Emulador/simulador en el mismo PC: funciona con `host.docker.internal` (compose) o con `10.0.2.2`
  según el caso.
- Teléfono físico: en compose, `BLOB_PUBLIC_HOST=<IP-LAN-del-PC>`; con `dotnet run`, una connection string
  de Azurite con `BlobEndpoint=http://<IP-LAN>:10000/devstoreaccount1`; o una cuenta de Storage real.

## 5. Lint, tipos y tests

```bash
# Backend (desde la raíz)
dotnet build                 # 0 warnings esperados
dotnet test

# Mobile
cd mobile/ImageCards.Mobile
npm run lint                 # ESLint (eslint-config-expo), 0 warnings permitidos
npm run typecheck            # tsc --noEmit
npm test                     # Jest + React Native Testing Library
npm run test:ci              # igual, con cobertura y modo CI
```

Qué cubren:

| Suite                         | Qué prueba                                                                 |
| ----------------------------- | -------------------------------------------------------------------------- |
| `Sm2SpacedRepetitionSchedulerTests` | Tarjeta nueva, Again/Hard/Good/Easy, crecimiento de intervalo, ease, lapses, fechas |
| `ImageValidatorTests`, `BlobNamesTests` | Tamaño, MIME, extensión, magic bytes, nombres seguros            |
| `CardServiceTests`, `ReviewServiceTests` | Servicios reales sobre SQLite in-memory (FKs e índices únicos reales) |
| `CardsApiTests`, `ReviewsApiTests`, `HealthApiTests` | Pipeline HTTP completo con `WebApplicationFactory` |
| Mobile `components/__tests__` | Flashcard (revelar), ReviewButtons, LanguageSelector, Loading/Error/Progress |
| Mobile `screens/__tests__`    | Flujo de repaso completo, vacío, errores, reintento, cambio de idioma      |
| Mobile `services`, `utils`, `hooks` | Cliente HTTP, mapeo de errores, máquina de estados de la sesión      |

Los tests **no** usan Azure, Docker ni red: SQLite in-memory, Blob Storage falso y reloj falso.

## Preparación para CI/CD

No hay pipelines todavía. Un pipeline futuro solo tendría que ejecutar:

```bash
# Backend
dotnet tool restore
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
dotnet publish backend/ImageCards.Api -c Release -o artifacts/api        # o docker build -f backend/Dockerfile .
dotnet ef migrations script --idempotent --project backend/ImageCards.Api -o artifacts/migrate.sql

# Mobile
cd mobile/ImageCards.Mobile
npm ci
npm run lint
npm run typecheck
npm run test:ci
```

Garantías actuales:

- Versiones fijadas: `global.json`, `dotnet-tools.json`, `package-lock.json`.
- Sin rutas absolutas ni configuración manual; todo por variables de entorno.
- Sin secretos en el repositorio (`.env`, user-secrets y Key Vault fuera de Git).
- Comandos no interactivos.

## Definition of Done

- [ ] Implementado en una rama `feature/*` (o `fix/*`, ...) creada desde `develop`.
- [ ] Tests nuevos o actualizados, y pasan (`dotnet test`, `npm test`).
- [ ] `dotnet build` sin warnings; `npm run lint` y `npm run typecheck` sin errores.
- [ ] Documentación actualizada si cambia comportamiento, configuración o API.
- [ ] Sin secretos ni rutas locales en el diff.
- [ ] Commits con Conventional Commits.
