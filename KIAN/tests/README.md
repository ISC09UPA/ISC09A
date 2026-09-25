# Tests del backend

`ImageCards.Api.Tests` (xUnit). Desde la raíz del repositorio:

```bash
dotnet test
```

| Carpeta              | Qué prueba                                                                  |
| -------------------- | --------------------------------------------------------------------------- |
| `SpacedRepetition/`  | `Sm2SpacedRepetitionScheduler`: todos los ratings, intervalos, ease, lapses, fechas |
| `Images/`, `Storage/`| Validación de imágenes y nombres de blob seguros                            |
| `Services/`          | `CardService` y `ReviewService` reales sobre SQLite in-memory               |
| `Api/`               | Endpoints a través del pipeline HTTP completo (`WebApplicationFactory`)     |
| `TestSupport/`       | Blob Storage falso, reloj falso, usuarios de prueba, base SQLite            |

Los tests son deterministas y no dependen de Azure, Docker ni de la red. Se usa SQLite (y no el
proveedor InMemory de EF) porque aplica claves foráneas e índices únicos, como SQL Server.

Los tests de la app móvil están en `mobile/ImageCards.Mobile/src/**/__tests__`.
