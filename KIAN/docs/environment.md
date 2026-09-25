# Variables de entorno

**Nunca** se versionan secretos: connection strings reales, contraseñas de SQL, secretos JWT, API keys,
claves de Storage ni credenciales de Azure. Solo se versionan archivos `*.example` sin valores reales.

## Backend (`backend/ImageCards.Api`)

ASP.NET Core combina, en este orden (el último gana): `appsettings.json` → `appsettings.{Environment}.json`
→ user-secrets (solo `Development`) → variables de entorno. En variables de entorno, las claves anidadas
usan `__` (doble guion bajo).

| Variable de entorno                      | Obligatoria | Por defecto   | Descripción                                                                 |
| ---------------------------------------- | ----------- | ------------- | --------------------------------------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                 | Sí          | `Production`  | `Development` habilita Swagger, el usuario de desarrollo y detalles de error |
| `ConnectionStrings__DefaultConnection`   | Sí          | —             | SQL Server local o Azure SQL                                                |
| `AzureStorage__ConnectionString`         | Sí          | —             | Cuenta de Storage **con account key** (necesaria para firmar URLs SAS)      |
| `AzureStorage__ContainerName`            | No          | `card-images` | Container privado de imágenes (se crea en la primera subida)                |
| `AzureStorage__SasExpiryMinutes`         | No          | `60`          | Validez de las URLs de imagen (1–1440)                                      |
| `ImageUpload__MaxBytes`                  | No          | `5242880`     | Tamaño máximo de imagen en bytes (máx. 20 MB)                               |
| `Database__ApplyMigrationsOnStartup`     | No          | `false`       | Aplica migrations al arrancar. Usar solo en desarrollo local                |
| `DevelopmentUser__Id` / `__Name` / `__Email` | No      | ver `appsettings.Development.json` | Usuario fijo del entorno `Development`               |

La API **no arranca** si falta `AzureStorage__ConnectionString` o si una opción es inválida
(validación al inicio). Si falta la connection string de SQL, la API arranca pero las peticiones a
datos fallan con un mensaje claro en el log.

### Desarrollo: user-secrets

Guardados en tu perfil de usuario, fuera del repositorio:

```bash
dotnet user-secrets --project backend/ImageCards.Api set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=ImageCards;Integrated Security=true;TrustServerCertificate=true"
dotnet user-secrets --project backend/ImageCards.Api set "AzureStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets --project backend/ImageCards.Api list
```

`UseDevelopmentStorage=true` apunta a Azurite en `127.0.0.1:10000`.

## Docker Compose (`.env` en la raíz)

Crear a partir de `.env.example`.

| Variable              | Obligatoria | Descripción                                                                            |
| --------------------- | ----------- | -------------------------------------------------------------------------------------- |
| `MSSQL_SA_PASSWORD`   | Sí          | Contraseña de `sa` del SQL Server local (reglas de complejidad de SQL Server)          |
| `AZURITE_ACCOUNT_KEY` | Sí          | Clave fija y pública de Azurite ([documentación oficial](https://learn.microsoft.com/azure/storage/common/storage-use-azurite#http-connection-strings)). Solo sirve contra el emulador |
| `BLOB_PUBLIC_HOST`    | No          | Host de las URLs de imagen. `host.docker.internal` (por defecto) o la IP LAN del PC para un teléfono físico |

## Mobile (`mobile/ImageCards.Mobile/.env`)

Crear a partir de `mobile/ImageCards.Mobile/.env.example`.

| Variable              | Obligatoria | Descripción                              |
| --------------------- | ----------- | ---------------------------------------- |
| `EXPO_PUBLIC_API_URL` | Sí          | URL base de la API, sin `/` final        |

| Dónde corre la app      | Valor típico                    |
| ----------------------- | ------------------------------- |
| Emulador Android        | `http://10.0.2.2:5080`          |
| Simulador iOS           | `http://localhost:5080`         |
| Teléfono físico         | `http://<IP-LAN-del-PC>:5080`   |

> Las variables `EXPO_PUBLIC_*` se **incrustan en el bundle** y cualquiera puede leerlas.
> Nunca pongas secretos en ellas. Tras cambiar `.env`, reinicia `npx expo start`.

## Producción (Azure, fase futura)

- Configuración en **App Settings** del App Service, con los mismos nombres (`ConnectionStrings__DefaultConnection`, `AzureStorage__ConnectionString`, ...).
- Secretos en **Azure Key Vault** mediante Key Vault references.
- `ASPNETCORE_ENVIRONMENT=Production`: sin Swagger, sin usuario de desarrollo, sin detalles de error.
- Fuera de `Development` la API exige un usuario autenticado; hasta que se configure JWT / Entra ID
  responde `401` en los endpoints de datos (comportamiento intencional, sin atajos inseguros).
