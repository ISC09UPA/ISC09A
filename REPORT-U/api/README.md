# ReportU — API base (ASP.NET Core + PostgreSQL + Azure Blob Storage)

Base técnica del MVP: autenticación JWT, publicaciones, imágenes, apoyos,
comentarios y guardados. Rol único `Student`. Sin panel admin, sin moderación.

> Alcance de esta base: define entidades, contratos, errores y autenticación
> para que el equipo no invente contratos distintos. Las features del Nivel 2/3
> se construyen sobre estos mismos endpoints.

## Requisitos

```text
.NET 10 SDK
Podman + podman-compose
Cuenta de Azure (solo para probar Blob real; sin ella se usa carpeta local)
```

## Arranque rápido

```bash
cp .env.example .env          # 1. variables locales (no commitear el .env)

podman compose up -d          # 2. Postgres 18 en Podman

dotnet tool install -g dotnet-ef --version 10.0.0   # 3. solo una vez por máquina

dotnet ef database update     # 4. aplica la migración inicial

dotnet run                    # 5. API en http://localhost:5034
```

Swagger (documentado por endpoint, con botón **Authorize** para el JWT):

```text
http://localhost:5034/swagger
```

## Migración reproducible

```bash
dotnet ef migrations add NombreCambio   # genera migración desde las entidades
dotnet ef database update               # aplica pendientes (usa ConnectionStrings__DefaultConnection)
```

Los comandos `dotnet ef` leen `ConnectionStrings__DefaultConnection` del entorno
(o del `.env`, que `Program.cs` importa solo en local) y caen a la BD del compose.

## Arquitectura MVC

API sin vistas server-side: la "V" son las respuestas JSON + Swagger UI.

```text
Controllers/   → C: Auth, Users, Posts, Comments, Supports, Me, Media
Models/        → M: User, Post, PostImage, Comment, PostSupport, Bookmark + Enums
Dtos/          →    contratos request/response (la "vista" que ve la app móvil)
Data/          →    ReportUDbContext + factoría de tiempo de diseño
Services/      →    JWT, BCrypt, usuario actual, Blob Storage, identidad pública
Middleware/    →    errores RFC 7807 + códigos propios
Configuration/ →    JwtOptions, BlobOptions (todo por variables de entorno)
```

Reglas puestas en código (no renegociar por integrante):

- Timestamps UTC (`CreatedAt`/`UpdatedAt`) asignados en `SaveChangesAsync`.
- `Post.SupportCount` / `Post.CommentCount` desnormalizados para el feed Populares.
- Borrar un post borra sus blobs, comentarios, apoyos y guardados (cascada).
- El email **nunca** sale en autores ni comentarios (solo `GET /api/users/me`).

## Configuración por variables de entorno

| Variable | Ejemplo | Notas |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;...` | Igual que el compose |
| `Jwt__Key` | mínimo 32 caracteres | Obligatoria; en dev hay una en `appsettings.Development.json` |
| `Jwt__Issuer` / `Jwt__Audience` | `ReportU` | Claims `iss`/`aud` |
| `Jwt__ExpiresMinutes` | `720` | Vida del accessToken |
| `AzureBlob__ConnectionString` | `DefaultEndpointsProtocol=https;...` | **Vacía = carpeta local `./uploads`** (mismo contrato, ideal sin Azure) |
| `AzureBlob__Container` | `reportu-media` | Se crea sola al arrancar si hay conexión |

## Autenticación

```text
POST /api/auth/register  → 201 + { accessToken, tokenType: "Bearer", expiresAt, user }
POST /api/auth/login     → 200 + lo mismo (identifier = email o username)
```

- Hash BCrypt (`IPasswordService`); el `passwordHash` jamás sale en respuestas.
- Claims: `sub` (userId), `email`, `username`, `jti`.
- Todo exige `Authorization: Bearer {token}` excepto register/login.
- No hay refresh ni recuperación de contraseña en el MVP (el "logout" es borrar el token en el cliente).

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/register` | Registro |
| POST | `/api/auth/login` | Login |
| GET / PATCH | `/api/users/me` | Perfil propio (único con email) |
| GET | `/api/posts?sort=recent\|popular&page=&pageSize=` | Feed Recientes (`createdAt` DESC) / Populares (`supportCount` DESC) |
| GET | `/api/posts/{id}` | Detalle + imágenes |
| POST / PATCH / DELETE | `/api/posts`, `/api/posts/{id}` | CRUD propio (editar/borrar: solo autor) |
| POST | `/api/posts/{id}/images` | Subir imagen (multipart, máx. 4 por post, 5 MB, jpg/png/webp/gif) |
| GET | `/api/posts/{id}/images` | Listar imágenes |
| GET | `/api/media/{id}` | Mostrar imagen (binario inline) |
| GET | `/api/media/{id}/download` | Descargar imagen (attachment) |
| DELETE | `/api/media/{id}` | Eliminar imagen (BD + blob, solo autor) |
| GET / POST | `/api/posts/{id}/comments` | Comentarios solo texto, sin anidación |
| PUT / DELETE | `/api/posts/{id}/support` | Apoyar / retirar (máx. 1 por usuario) |
| GET | `/api/me/posts` | Mis publicaciones |
| GET | `/api/me/bookmarks` | Guardados |
| PUT / DELETE | `/api/posts/{id}/bookmark` | Guardar / quitar |

Paginación: `page` base 1, `pageSize` 1–50 → `{ items, page, pageSize, totalCount, totalPages }`.
Enums viajan como strings (`"Incidencia"`, `"Infraestructura"`, ...).
URLs de imagen relativas: `url: /api/media/{id}`, `downloadUrl: /api/media/{id}/download`
(la app antepone la base del API; a futuro se pueden cambiar a SAS sin romper el contrato).

## Errores

Todo error es `application/problem+json` (RFC 7807) con extensiones `code` y `traceId`:

```json
{ "title": "Ya apoyaste esta publicación", "status": 409,
  "detail": "Un usuario solo puede apoyar una vez cada publicación.",
  "code": "already_supported", "traceId": "0HN..." }
```

Códigos propios (`Middleware/ErrorCodes`): `validation_error`, `unauthorized`,
`forbidden_not_owner`, `not_found`, `email_taken`, `username_taken`,
`invalid_credentials`, `already_supported`, `not_supported`, `already_bookmarked`,
`not_bookmarked`, `image_limit_reached`, `unsupported_media_type`,
`payload_too_large`, `blob_not_found`, `blob_error`.
La app móvil debe ramificar por `code`, no por mensajes.

## Identidad pública

- Perfil: `displayNamePreference` (`RealName`/`Username`/`Anonymous`) + `showEnrollmentNumber`.
- Post: `identityMode` (`Default` = respeta el perfil, o fuerza una opción).
- `Anonymous` → `"Anónimo"` sin matrícula. Ver `Services/AuthorDisplay.cs`.

## Criterio de terminado (flujo demo verificado end-to-end)

```text
Registro → login → publicar → subir foto → ver en feed → abrir detalle →
mostrar/descargar imagen → otro usuario comenta y apoya → el autor edita →
elimina (la imagen desaparece del storage)
```

## Trabajo futuro (fuera del MVP)

Administradores, reportes/bloqueos, estados de incidencias, notificaciones/push,
buscador, filtros, GPS/mapas, seguidores, mensajería, grupos.
