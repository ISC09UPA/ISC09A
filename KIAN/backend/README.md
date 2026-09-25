# Backend: ImageCards.Api

Web API ASP.NET Core (.NET 10). Arquitectura en [docs/architecture.md](../docs/architecture.md),
contrato en [docs/api.md](../docs/api.md).

```bash
# Desde la raíz del repositorio
dotnet run --project backend/ImageCards.Api      # http://localhost:5080/swagger
docker build -f backend/Dockerfile -t imagecards-api .
```

| Carpeta          | Contenido                                                          |
| ---------------- | ------------------------------------------------------------------ |
| `Configuration/` | Opciones tipadas y `ServiceCollectionExtensions` (registro de DI)  |
| `Controllers/`   | Controllers delgados                                               |
| `Data/`          | `ImageCardsDbContext`, configuraciones Fluent API, `Migrations/`   |
| `DTOs/`          | Contratos de entrada/salida y validación                           |
| `Exceptions/`    | Errores esperados que se traducen a códigos HTTP                   |
| `HealthChecks/`  | `/health` y `/health/live`                                         |
| `Middleware/`    | `GlobalExceptionHandler`                                           |
| `Models/`        | Entidades EF Core, `ReviewRating`, `SupportedLanguages`            |
| `Services/`      | Cards, Reviews, SpacedRepetition, Storage, Images, CurrentUser     |

Los tests están en [`/tests/ImageCards.Api.Tests`](../tests/README.md).
