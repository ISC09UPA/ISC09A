# ImageCards

ImageCards es una aplicación móvil para aprender vocabulario con **flashcards visuales**. El frente de
cada tarjeta es una imagen (guardada en Azure Blob Storage). Al tocarla aparece la traducción en el idioma
elegido y, si existe, una oración de ejemplo. El usuario indica qué tan bien la recordó
(**Again / Hard / Good / Easy**) y un algoritmo de **repetición espaciada** (SM-2, sustituible por FSRS)
decide cuándo volverá a aparecer.

> Inspirado conceptualmente en sistemas como Anki. No reutiliza código, diseño visual ni elementos propietarios de Anki.

## Estado

| Área                                        | Estado |
| ------------------------------------------- | ------ |
| Estructura, documentación, flujo de Git     | ✅     |
| Backend: API, EF Core + migrations, Swagger, health, errores, logging | ✅ |
| Blob Storage: subida validada, URLs SAS, borrado | ✅ |
| Cards API (CRUD) y Reviews API              | ✅     |
| Repetición espaciada SM-2 (`ISpacedRepetitionScheduler`) | ✅ |
| App móvil: repaso, tarjetas, ajustes, selector de idioma | ✅ |
| Tests backend (118) y mobile (54)           | ✅     |
| Docker (API) y docker-compose (SQL + Azurite + API) | ✅ |
| Autenticación real (JWT / Entra ID)         | ⏳ Abstracción lista (`ICurrentUserService`) |
| Crear tarjetas desde la app                 | ⏳ Hoy vía API/Swagger |
| Infraestructura Azure, CI/CD, despliegue    | ⏳ Fases futuras (no configurado a propósito) |

## Arquitectura

```
 React Native (Expo SDK 57, TypeScript)
            │ HTTPS · JSON (REST)
            ▼
 ASP.NET Core Web API (.NET 10)
   Controllers → Services → DbContext / BlobStorageService
            │                       │
            ▼                       ▼
   Azure SQL / SQL Server     Azure Blob Storage (container privado + SAS)
```

Detalle en [docs/architecture.md](docs/architecture.md).

## Stack

| Capa     | Tecnología                                                                 |
| -------- | -------------------------------------------------------------------------- |
| Backend  | C#, ASP.NET Core Web API (.NET 10 LTS), EF Core 10, Swashbuckle (Swagger)  |
| Datos    | Azure SQL Database · SQL Server LocalDB o SQL Server 2022 (Docker) en local |
| Imágenes | Azure Blob Storage (`Azure.Storage.Blobs`) · Azurite en local              |
| Mobile   | Expo SDK 57, React Native 0.86, TypeScript 6, React Navigation 7           |
| Tests    | xUnit + WebApplicationFactory + SQLite in-memory · Jest + React Native Testing Library 14 |

## Estructura

```
.
├── ImageCards.slnx            # Solución .NET (backend + tests)
├── global.json                # Versión del SDK .NET
├── dotnet-tools.json          # dotnet-ef
├── docker-compose.yml         # SQL Server + Azurite + API (desarrollo)
├── .env.example               # Variables de docker-compose
├── backend/
│   ├── Dockerfile
│   └── ImageCards.Api/        # Web API
├── tests/
│   └── ImageCards.Api.Tests/  # Tests del backend
├── mobile/
│   └── ImageCards.Mobile/     # App Expo
├── infrastructure/            # IaC de Azure (fase futura)
└── docs/
```

## Inicio rápido

Requisitos: .NET SDK 10, Node.js 22.13+, SQL Server (LocalDB o Docker), Azurite o una cuenta de Storage.

### Backend

```bash
git clone <repo-url> imagecards && cd imagecards
git checkout develop

# Opción A: todo en Docker
cp .env.example .env                  # completar los valores
docker compose up -d --build          # http://localhost:5080/swagger

# Opción B: dotnet run (ver docs/development.md para configurar user-secrets)
dotnet tool restore
dotnet restore
dotnet build
dotnet ef database update --project backend/ImageCards.Api --connection "<connection string>"
dotnet run --project backend/ImageCards.Api          # http://localhost:5080/swagger
```

### Mobile

```bash
cd mobile/ImageCards.Mobile
cp .env.example .env                  # EXPO_PUBLIC_API_URL
npm install
npx expo start
```

## Variables de entorno

| Componente | Variables principales                                                        | Dónde                         |
| ---------- | ---------------------------------------------------------------------------- | ----------------------------- |
| Backend    | `ConnectionStrings__DefaultConnection`, `AzureStorage__ConnectionString`, `AzureStorage__ContainerName` | user-secrets / env / App Settings |
| Compose    | `MSSQL_SA_PASSWORD`, `AZURITE_ACCOUNT_KEY`, `BLOB_PUBLIC_HOST`               | `.env` (raíz)                 |
| Mobile     | `EXPO_PUBLIC_API_URL`                                                        | `mobile/ImageCards.Mobile/.env` |

No hay secretos en el repositorio. Lista completa en [docs/environment.md](docs/environment.md).

## Tests y lint

```bash
dotnet build && dotnet test                     # backend, desde la raíz

cd mobile/ImageCards.Mobile
npm run lint && npm run typecheck && npm test   # mobile
```

## Migrations

```bash
dotnet ef migrations add <Nombre> --project backend/ImageCards.Api --output-dir Data/Migrations
dotnet ef database update --project backend/ImageCards.Api --connection "<connection string>"
```

Ver [docs/development.md](docs/development.md#3-migrations).

## API

| Método | Ruta                                  |
| ------ | ------------------------------------- |
| GET    | `/api/cards?language=en&page=1`       |
| GET    | `/api/cards/{id}`                     |
| POST   | `/api/cards` (multipart: imagen + traducciones) |
| PUT    | `/api/cards/{id}`                     |
| DELETE | `/api/cards/{id}`                     |
| GET    | `/api/reviews/due?language=en`        |
| POST   | `/api/reviews` `{ cardId, rating }`   |
| GET    | `/health`                             |

Contrato completo en [docs/api.md](docs/api.md).

## Flujo de Git

- `main` es **estable**: nadie hace commits, merges ni push directos a `main`.
- `develop` es la rama de **integración**; tampoco se desarrollan features directamente en ella.
- Todo el trabajo se hace en ramas `feature/*`, `fix/*`, `refactor/*`, `test/*`, `docs/*` o `chore/*` creadas desde `develop`.
- Commits con [Conventional Commits](https://www.conventionalcommits.org/).

```bash
git checkout develop
git pull origin develop
git checkout -b feature/mi-feature
git status
git add <archivos>
git commit -m "feat: descripción breve"
git push -u origin feature/mi-feature
```

Guía completa en [docs/git-workflow.md](docs/git-workflow.md).

## Preparación para CI/CD

No hay pipelines, `.github/` ni despliegues automáticos **a propósito**. Un pipeline futuro solo tendrá
que ejecutar los mismos comandos documentados:

```
dotnet tool restore → dotnet restore → dotnet build → dotnet test → dotnet publish / docker build
npm ci → npm run lint → npm run typecheck → npm run test:ci
```

Ver [docs/development.md](docs/development.md#preparación-para-cicd).

## Documentación

- [Arquitectura](docs/architecture.md)
- [Desarrollo local](docs/development.md)
- [Variables de entorno](docs/environment.md)
- [API](docs/api.md)
- [Flujo de Git](docs/git-workflow.md)
