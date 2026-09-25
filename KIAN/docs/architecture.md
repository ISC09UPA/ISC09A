# Arquitectura

## Visión general

```
┌───────────────────────────────┐
│ Mobile: Expo SDK 57 / RN / TS │  Review · Cards · Settings (bottom tabs)
└──────────────┬────────────────┘
               │ HTTPS + JSON (REST) · EXPO_PUBLIC_API_URL
┌──────────────▼────────────────┐
│ ImageCards.Api (.NET 10)      │
│  Controllers (delgados)       │
│      │                        │
│  Services                     │  CardService · ReviewService · ImageValidator
│      │         │              │  ISpacedRepetitionScheduler · ICurrentUserService
│  DbContext   BlobStorageService│
└──────┬─────────────┬──────────┘
       │             │
┌──────▼──────┐ ┌────▼────────────────────────┐
│ Azure SQL / │ │ Azure Blob Storage          │
│ SQL Server  │ │ container privado · SAS     │
│ (metadatos) │ │ de lectura y corta duración │
└─────────────┘ └─────────────────────────────┘
```

## Estructura del repositorio

```
.
├── ImageCards.slnx               Solución .NET (raíz: dotnet build/test sin argumentos)
├── global.json                   SDK .NET fijado (10.0.x)
├── dotnet-tools.json             Herramientas locales (dotnet-ef)
├── docker-compose.yml            SQL Server + Azurite + API para desarrollo
├── backend/
│   ├── Dockerfile
│   └── ImageCards.Api/
│       ├── Configuration/        Opciones tipadas y registro de DI por área
│       ├── Controllers/          CardsController, ReviewsController
│       ├── Data/                 DbContext, configuraciones Fluent API, Migrations, inicializador
│       ├── DTOs/                 Contratos de la API + validación
│       ├── Exceptions/           Errores esperados → códigos HTTP
│       ├── HealthChecks/         /health y /health/live
│       ├── Middleware/           GlobalExceptionHandler (ProblemDetails)
│       ├── Models/               Entidades EF, ReviewRating, SupportedLanguages
│       └── Services/
│           ├── Cards/            ICardService, CardService
│           ├── CurrentUser/      ICurrentUserService + implementaciones
│           ├── Images/           ImageFormat, IImageValidator
│           ├── Reviews/          IReviewService, ReviewService
│           ├── SpacedRepetition/ ISpacedRepetitionScheduler, Sm2SpacedRepetitionScheduler
│           └── Storage/          IBlobStorageService, BlobStorageService, BlobNames
├── tests/ImageCards.Api.Tests/   xUnit: scheduler, validación, servicios, API
├── mobile/ImageCards.Mobile/     App Expo (tests junto al código en __tests__)
├── infrastructure/               IaC de Azure (fase futura)
└── docs/
```

Decisiones:

- **Solución en la raíz**: `dotnet restore/build/test` funcionan sin argumentos, igual que en un pipeline.
- **Un único proyecto de API** organizado por carpetas. Para un MVP es suficiente; si crece, se pueden
  extraer proyectos (Domain/Infrastructure) sin cambiar los contratos.
- **Carpetas extra** respecto a la propuesta inicial: `Exceptions/` y `HealthChecks/` para no mezclar
  responsabilidades en `Middleware/` o `Services/`.
- **Tests de mobile junto al código** (`__tests__`), convención de Jest/Expo.

## Backend

### Capas

```
HTTP → Controller → Service → ImageCardsDbContext / IBlobStorageService
```

- **Controllers**: enlazan la petición (con validación automática de `[ApiController]`), llaman a un
  servicio y devuelven DTOs. No tienen lógica de negocio.
- **Services**: lógica de negocio. Usan el `DbContext` directamente; **no hay Repository genérico**,
  porque EF Core ya implementa Unit of Work/Repository y una capa más solo duplicaría su API.
- **Errores**: los servicios lanzan excepciones de `Exceptions/` (`NotFoundException`,
  `RequestValidationException`, `StorageUnavailableException`, `UnauthenticatedException`).
  `GlobalExceptionHandler` las traduce a `ProblemDetails` con el código HTTP adecuado. Los errores
  inesperados devuelven `500` sin detalles fuera de `Development`.
- **Tiempo**: todo usa `TimeProvider` (UTC). Los tests lo sustituyen por un reloj falso para ser deterministas.
- **DI**: `Configuration/ServiceCollectionExtensions.cs` agrupa los registros por área; `Program.cs` queda en ~40 líneas
  y es raro que dos ramas lo modifiquen a la vez.

### Modelo de datos

```
User 1───* Card 1───* CardTranslation
  │          │
  │          └───* CardReview   (único por UserId + CardId)
  └───* CardReview
```

| Entidad           | Campos                                                                                     |
| ----------------- | ------------------------------------------------------------------------------------------ |
| `User`            | Id, Name (100), Email (256, único), CreatedAt                                              |
| `Card`            | Id, UserId, ImageBlobName (200, único), CreatedAt                                          |
| `CardTranslation` | Id, CardId, LanguageCode (2), TranslatedText (200), ExampleSentence? (500)                 |
| `CardReview`      | Id, CardId, UserId, NextReviewAt, LastReviewedAt?, IntervalDays, EaseFactor, Repetitions, Lapses |

- Índices: `UserId`, `CardId`, `NextReviewAt`, `(UserId, NextReviewAt)`, `(CardId, LanguageCode)` **único**,
  `(UserId, CardId)` **único**.
- Borrado en cascada: `User → Card → {CardTranslation, CardReview}`. `User → CardReview` es `NO ACTION`
  porque SQL Server no permite múltiples rutas de cascada; los reviews se borran igualmente a través de `Card`.
- `LastReviewedAt` no estaba en el modelo inicial: se agregó porque FSRS necesita el tiempo transcurrido.
- Todas las fechas en UTC. Un `ValueConverter` marca las fechas leídas como `DateTimeKind.Utc`.
- Las imágenes **nunca** se guardan en SQL; solo `ImageBlobName`.
- Cada tarjeta crea su `CardReview` inicial (pendiente de inmediato) en la misma transacción.

### Imágenes

1. `ImageValidator` comprueba: tamaño > 0 y ≤ `ImageUpload:MaxBytes`, MIME permitido
   (`image/jpeg`, `image/png`, `image/webp`), extensión coherente con el MIME y **firma binaria**
   (magic bytes) coherente con el MIME. Un archivo `.png` que no es PNG se rechaza.
2. `BlobNames.Create` genera `<guid>.<ext>` con la extensión canónica. El nombre del archivo del
   cliente no se usa nunca, así que no hay path traversal posible.
3. `BlobStorageService` sube con `If-None-Match: *` (nunca sobrescribe) y `Content-Type` correcto.
   El container se crea privado (`PublicAccessType.None`) en la primera subida.
4. Las respuestas incluyen URLs SAS de **solo lectura**, con expiración configurable y tolerancia de
   5 minutos por desfase de reloj.
5. Fallos de Azure → `StorageUnavailableException` → `503`. Los logs registran solo estado y código
   de error, nunca URLs ni tokens.
6. Consistencia: si falla el guardado en SQL tras subir la imagen, el blob se borra. Al eliminar una
   tarjeta, primero se borra de SQL (fuente de verdad) y después el blob; si esto último falla, se
   registra un warning (blob huérfano, inofensivo).

### Repetición espaciada

```
ReviewService ──► ISpacedRepetitionScheduler
                          ▲
            ┌─────────────┴─────────────┐
 Sm2SpacedRepetitionScheduler    FsrsSpacedRepetitionScheduler (futuro)
```

El scheduler es **puro**: recibe `SchedulingState`, el rating y la hora, y devuelve el nuevo estado.
No accede a la base de datos ni al reloj. Cambiar de algoritmo = cambiar una línea de DI en
`AddImageCardsServices`.

Variante de SM-2 implementada (`Sm2SpacedRepetitionScheduler`):

| Rating  | Ease        | Intervalo                                             | Repeticiones | Lapses                        |
| ------- | ----------- | ----------------------------------------------------- | ------------ | ----------------------------- |
| Again   | −0.20       | 0 días; vuelve a estar pendiente en **10 minutos**    | → 0          | +1 si la tarjeta ya estaba aprendida |
| Hard    | −0.15       | 1ª: 1 día · luego anterior × 1.2                      | +1           | —                             |
| Good    | =           | 1ª: 1 · 2ª: 6 · luego anterior × ease                 | +1           | —                             |
| Easy    | +0.15       | 1ª: 4 · 2ª: 8 · luego anterior × ease × 1.3           | +1           | —                             |

- Ease inicial 2.5, mínimo 1.3 (redondeado a 2 decimales para evitar deriva de coma flotante).
- Tras el primer acierto el intervalo crece al menos 1 día; máximo 36 500 días.
- `NextReviewAt = hora de la revisión + intervalo`.
- Ejemplo con "Good" repetido: 1 → 6 → 15 → 38 → 95 días.

### Usuario actual

`ICurrentUserService.UserId` es el **único** lugar del que los servicios obtienen el usuario.

| Entorno       | Implementación                  | Comportamiento                                                   |
| ------------- | ------------------------------- | ---------------------------------------------------------------- |
| `Development` | `DevelopmentCurrentUserService` | Usuario fijo de `DevelopmentUser`; se crea en la BD al arrancar  |
| Resto         | `ClaimsCurrentUserService`      | Lee `oid` / `nameidentifier` / `sub` del usuario autenticado; si no hay → `401` |

Para añadir JWT / Entra ID: configurar `AddAuthentication().AddJwtBearer(...)` (o Microsoft.Identity.Web),
`UseAuthentication()`/`UseAuthorization()` y `[Authorize]`. `ClaimsCurrentUserService` ya lee los claims
estándar. Para eliminar el usuario de desarrollo basta con borrar `DevelopmentCurrentUserService`,
`DevelopmentUserOptions` y su registro.

### Observabilidad

- `ILogger<T>` con plantillas estructuradas (`{CardId}`, `{Status}`...), sin interpolar strings.
- No se registran connection strings, URLs SAS ni contenido de imágenes (verificado revisando los logs).
- `/health` (SQL + Blob, timeout 10 s) y `/health/live`.
- Application Insights: fase posterior. Bastará con `Azure.Monitor.OpenTelemetry.AspNetCore` y
  `APPLICATIONINSIGHTS_CONNECTION_STRING`; los logs estructurados ya son compatibles.

## Mobile

```
mobile/ImageCards.Mobile/
├── App.tsx                    Providers + NavigationContainer
└── src/
    ├── config/env.ts          EXPO_PUBLIC_API_URL
    ├── constants/             strings (textos de UI), theme
    ├── types/                 api.ts (contratos), language.ts (idiomas)
    ├── services/api/          apiClient, cardsApi, reviewsApi, errors
    ├── utils/                 errorMessages (error → mensaje para el usuario)
    ├── hooks/                 useLanguage (contexto), useReviewSession + reducer, useCards
    ├── components/            Flashcard, ReviewButtons, ProgressBar, LanguageSelector,
    │                          LoadingView, ErrorView, MessageView, PrimaryButton
    ├── screens/               ReviewScreen, CardsScreen, SettingsScreen
    ├── navigation/            RootNavigator (bottom tabs), types
    └── test-utils/            fixtures de tests
```

- **Sin `fetch` en componentes**: `apiClient.apiRequest` es el único punto de acceso HTTP. Añade
  timeout (15 s) y convierte errores en `ApiError` (con ProblemDetails), `NetworkError` o `ConfigurationError`.
- **Flujo de repaso**: `reviewSessionReducer` es una máquina de estados pura
  (`loading → reviewing ⇄ submitting → completed | empty | error`) y `useReviewSession` la conecta con la API.
  Ignora respuestas tardías si el idioma cambió mientras tanto.
- **Idioma**: `LanguageProvider` lo comparte entre pantallas; cambiarlo recarga las tarjetas.
- **Textos**: todos en `constants/strings.ts` (español), listos para i18n.
- **Navegación**: React Navigation (bottom tabs). Se eligió en lugar de Expo Router porque así se pidió;
  las rutas están tipadas en `navigation/types.ts`.

## Despliegue previsto (no automatizado)

| Componente | Servicio Azure                                           |
| ---------- | -------------------------------------------------------- |
| API        | Azure App Service (Linux, contenedor de `backend/Dockerfile` o `dotnet publish`) · health check `/health` |
| Datos      | Azure SQL Database (migrations con script idempotente)   |
| Imágenes   | Azure Blob Storage (container privado)                   |
| Secretos   | App Settings + Key Vault references                      |
| Mobile     | EAS Build (fase futura)                                  |
