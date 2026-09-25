# Integración Azure Blob Storage — ReportU

> **Autor:** Polo  
> **Rama:** `report-u/azure`  
> **Fecha:** 2026-09-24  
> **Área:** Azure / Documentación

---

## 1. Resumen

ReportU almacena las imágenes de publicaciones estudiantiles en **Azure Blob Storage**.  
El servicio está diseñado como un módulo **aislado** (`IBlobStorageService`) que encapsula
todas las operaciones de blobs sin exponer credenciales ni acoplarse a la lógica de posts
o autenticación.

### Operaciones implementadas

| # | Operación | Método | Descripción |
|---|-----------|--------|-------------|
| 1 | **Upload** | `UploadAsync(blobName, stream, contentType)` | Sube un binario al container |
| 2 | **List** | `ListAsync(prefix?)` | Lista blobs por prefijo (o todos) |
| 3 | **Obtener URL/lectura** | `GetReadUrlAsync(blobName, expiry?)` | Genera SAS URL temporal de solo lectura |
| 4 | **Download/stream** | `OpenReadAsync(blobName)` | Descarga el binario como stream |
| 5 | **DeleteIfExists** | `DeleteAsync(blobName)` | Elimina el blob si existe |

### Implementaciones duales

- **`AzureBlobStorageService`** — Producción (Azure Blob Storage real).
- **`LocalFileStorageService`** — Desarrollo sin credenciales (carpeta `./uploads`).

La selección es automática: si `AzureBlob__ConnectionString` tiene valor → Azure; si está vacío → local.

---

## 2. Variables de entorno requeridas

> ⚠️ **NUNCA** subir connection strings o secretos reales al repositorio.

Configura estas variables en tu archivo `.env` (ver `.env.example`):

```env
# --- Azure Blob Storage ---
AzureBlob__ConnectionString=DefaultEndpointsProtocol=https;AccountName=TU_CUENTA;AccountKey=TU_LLAVE_BASE64;EndpointSuffix=core.windows.net
AzureBlob__Container=reportu-images
```

### Valores de ejemplo (falsos, para referencia)

```env
AzureBlob__ConnectionString=DefaultEndpointsProtocol=https;AccountName=reportudev2026;AccountKey=aBcDeFgHiJkLmNoPqRsTuVwXyZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdef==;EndpointSuffix=core.windows.net
AzureBlob__Container=reportu-images
```

### Modo desarrollo (sin Azure)

Deja `AzureBlob__ConnectionString` vacío y la API usa almacenamiento local:

```env
AzureBlob__ConnectionString=
AzureBlob__Container=reportu-media
```

Las imágenes se guardan en `./uploads/{postId}/{imageId}.ext`.

---

## 3. Cómo crear/configurar el container en Azure

### Desde Azure Portal

1. Ir a **Storage Accounts** → Crear o seleccionar cuenta.
2. En la cuenta → **Containers** → **+ Container**.
3. Nombre: `reportu-images` (o el valor de `AzureBlob__Container`).
4. Nivel de acceso: **Private** (sin acceso público anónimo).
5. Copiar la **Connection String** desde **Access keys** de la cuenta.

### Desde Azure CLI

```bash
# Crear grupo de recursos (si no existe)
az group create --name rg-reportu --location mexicocentral

# Crear cuenta de almacenamiento
az storage account create \
  --name reportudev2026 \
  --resource-group rg-reportu \
  --location mexicocentral \
  --sku Standard_LRS

# Crear container privado
az storage container create \
  --name reportu-images \
  --account-name reportudev2026 \
  --auth-mode login

# Obtener connection string
az storage account show-connection-string \
  --name reportudev2026 \
  --resource-group rg-reportu \
  --output tsv
```

### Automático desde la API

Al iniciar la API, si la connection string está configurada, se ejecuta
`EnsureContainerAsync()` que crea el container automáticamente si no existe
(`CreateIfNotExistsAsync` con acceso `None`).

---

## 4. Endpoints de imágenes

Todos requieren autenticación JWT (`Authorization: Bearer {token}`).

### 4.1 Subir imagen a un post

```http
POST /api/posts/{postId}/images
Content-Type: multipart/form-data

file: <archivo imagen>
```

**Restricciones:**
- Solo el autor del post puede subir.
- Tipos permitidos: `image/jpeg`, `image/png`, `image/webp`, `image/gif`.
- Tamaño máximo: 5 MB por imagen.
- Máximo 4 imágenes por publicación.

**Respuesta 201:**
```json
{
  "id": "a1b2c3d4-...",
  "url": "/api/media/a1b2c3d4-...",
  "downloadUrl": "/api/media/a1b2c3d4-.../download",
  "contentType": "image/jpeg",
  "sizeBytes": 245760,
  "createdAt": "2026-09-24T19:00:00Z"
}
```

### 4.2 Listar imágenes de un post

```http
GET /api/posts/{postId}/images
```

**Respuesta 200:** Array de `PostImageResponse`.

### 4.3 Mostrar imagen (inline)

```http
GET /api/media/{imageId}
```

Devuelve el binario con el `Content-Type` original (para mostrar en `<img>` o visor).

### 4.4 Descargar imagen

```http
GET /api/media/{imageId}/download
```

Devuelve el binario con `Content-Disposition: attachment` para descarga directa.

### 4.5 Eliminar imagen

```http
DELETE /api/media/{imageId}
```

**Seguridad:** Solo el propietario del post puede eliminar sus imágenes.
Elimina el metadato en BD y el blob en el storage de forma coherente.

---

## 5. Pasos reproducibles para probar (Postman/Bruno)

### Prerequisitos
1. API corriendo (`dotnet run` o `docker compose up`).
2. PostgreSQL levantado con migraciones aplicadas.
3. Variables `.env` configuradas.

### Flujo de prueba completo

#### Paso 1 — Registrar usuario
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "polo@upa.edu.mx",
  "username": "polo_dev",
  "password": "Password123!",
  "firstName": "Marco",
  "lastName": "Polo"
}
```
→ Guardar el `accessToken` de la respuesta.

#### Paso 2 — Crear un post
```http
POST /api/posts
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Prueba de imágenes Azure",
  "description": "Post para probar upload/list/download/delete de blobs.",
  "type": "Incidencia",
  "category": "Infraestructura",
  "location": "Edificio C"
}
```
→ Guardar el `id` del post.

#### Paso 3 — Upload: subir imagen al post
```http
POST /api/posts/{postId}/images
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: <seleccionar imagen.jpg>
```
→ Verificar respuesta 201 con `url` y `downloadUrl`.  
→ Guardar el `id` de la imagen.

#### Paso 4 — List: listar imágenes del post
```http
GET /api/posts/{postId}/images
Authorization: Bearer {token}
```
→ Verificar que aparece la imagen subida en el array.

#### Paso 5 — Read/Download: obtener la imagen
```http
GET /api/media/{imageId}
Authorization: Bearer {token}
```
→ Verificar que regresa el binario con Content-Type `image/jpeg`.

```http
GET /api/media/{imageId}/download
Authorization: Bearer {token}
```
→ Verificar descarga del archivo.

#### Paso 6 — Delete: eliminar imagen
```http
DELETE /api/media/{imageId}
Authorization: Bearer {token}
```
→ Verificar respuesta 204.  
→ Repetir GET para confirmar 404.

#### Paso 7 — Verificar seguridad (usuario ajeno)
- Registrar un segundo usuario.
- Intentar `DELETE /api/media/{imageId}` con el token del segundo usuario.
- → Verificar respuesta 403 (`forbidden_not_owner`).

---

## 6. Flujo React Native → ASP.NET → Azure

```
┌──────────────┐     multipart/form-data      ┌─────────────────┐       Azure SDK        ┌───────────────────┐
│              │  POST /api/posts/{id}/images  │                 │   BlobClient.Upload    │                   │
│  React       │ ────────────────────────────► │  ASP.NET API    │ ────────────────────► │  Azure Blob       │
│  Native      │      + JWT Bearer            │  (MediaCtrl →   │                       │  Storage          │
│  App         │                               │   PostImageSvc  │                       │  Container:       │
│              │ ◄──────────────────────────── │   → BlobSvc)    │ ◄──────────────────── │  reportu-images   │
│              │     201 { id, url, ... }      │                 │   stream + metadata   │                   │
└──────────────┘                               └─────────────────┘                       └───────────────────┘
```

### Detalle del flujo de upload

1. **React Native** selecciona imagen con `expo-image-picker` o `react-native-image-picker`.
2. Construye un `FormData` con el archivo y lo envía como `multipart/form-data` al endpoint
   `POST /api/posts/{postId}/images` con el header `Authorization: Bearer {token}`.
3. **MediaController** recibe el `IFormFile`, valida que no sea null y delega a `IPostImageService`.
4. **PostImageService** valida:
   - El post existe y el `requesterId` es el autor.
   - El archivo no está vacío, es de tipo permitido, no excede 5 MB, y el post tiene < 4 imágenes.
   - Genera un `blobName` único: `{postId}/{imageId}.{ext}`.
5. **IBlobStorageService** (Azure o Local) sube el binario al storage.
6. Se guarda el metadato (`PostImage`) en PostgreSQL.
7. Se retorna el `PostImageResponse` con URLs para mostrar y descargar.

### Detalle del flujo de lectura

1. **React Native** usa la `url` del `PostImageResponse` para mostrar la imagen
   (ej. `<Image source={{ uri: baseUrl + image.url, headers: { Authorization: ... } }} />`).
2. **MediaController.ShowImage** busca el metadato en BD, obtiene el stream del
   `IBlobStorageService` y lo retorna como `FileResult` con el `Content-Type` original.

### Detalle del flujo de eliminación

1. **React Native** llama `DELETE /api/media/{imageId}` con JWT.
2. **PostImageService** verifica propiedad → elimina metadato en BD → elimina blob del storage.
3. Si falla el borrado del blob después de borrar el metadato, retorna 502 con código
   `blob_error` para que el cliente reintente.

---

## 7. Arquitectura de archivos

```
api/
├── Configuration/
│   └── Options.cs                  # BlobOptions (ConnectionString, Container, LocalPath)
├── Controllers/
│   └── MediaController.cs          # Endpoints: upload, list, show, download, delete
├── Dtos/
│   └── MediaDtos.cs                # PostImageResponse, PagedResult<T>
├── Middleware/
│   └── ExceptionHandlingMiddleware # Captura PostImageException → ProblemDetails
├── Models/
│   └── PostImage.cs                # Entidad: Id, PostId, BlobName, ContentType, SizeBytes
├── Services/
│   ├── BlobStorageService.cs       # IBlobStorageService + Azure + Local (5 operaciones)
│   └── PostImageService.cs         # IPostImageService (orquestación EF + Blob)
└── Program.cs                      # DI: AzureBlobStorageService | LocalFileStorageService
```

---

## 8. Códigos de error relevantes

| HTTP | Código | Cuándo |
|------|--------|--------|
| 400 | `validation_error` | Archivo vacío o datos inválidos |
| 401 | `unauthorized` | Token ausente o expirado |
| 403 | `forbidden_not_owner` | Intento de modificar recurso ajeno |
| 404 | `not_found` | Post o imagen inexistente |
| 409 | `image_limit_reached` | Post ya tiene 4 imágenes |
| 413 | `payload_too_large` | Imagen > 5 MB |
| 415 | `unsupported_media_type` | Tipo de archivo no permitido |
| 502 | `blob_error` | Fallo al eliminar blob (reintentar) |
