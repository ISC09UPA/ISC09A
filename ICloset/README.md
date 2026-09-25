# ICloset

Aplicación móvil para organizar prendas por zona del cuerpo y crear outfits.

## Estructura del proyecto

```text
ICloset/
├── backend/
│   └── Closet.Api/       # API .NET, SQLite y conexión con Azure Blob
└── mobile/
    └── closet-mobile/    # Aplicación React Native con Expo
```

## Tecnologías

- API: .NET 8
- Base de datos: SQLite, archivo local `closet.db`
- Fotos: Azure Blob Storage, contenedor `garments`
- App móvil: React Native y Expo

## Datos de una prenda

La API usa el modelo `Garment` con estos campos:

- `Id`
- `Name`
- `BodyPart`: `Head`, `Torso`, `Legs` o `Feet`
- `FileName`: nombre del archivo de imagen almacenado en Blob
- `CreatedAtUtc`: fecha de creación en UTC

SQLite guarda los datos de la prenda y el nombre de su imagen. Azure Blob almacena el archivo de imagen.

## Requisitos

- SDK de .NET 8
- Node.js y npm
- Expo Go para probar la app en un celular
- Una cuenta de Azure Storage con un contenedor privado llamado `garments`

## Ejecutar el backend

Desde la carpeta del proyecto de la API:

```bash
cd backend/Closet.Api
dotnet restore
dotnet run
```

La primera ejecución crea el archivo SQLite y un registro de prueba si la tabla está vacía. La dirección y el puerto aparecen en la terminal. Abre `/swagger` en esa dirección para consultar la documentación de la API.

Por ejemplo, si la terminal indica el puerto `5005`:

```text
http://localhost:5005/swagger
```

### Configurar Azure Blob localmente

El backend lee la cadena de conexión de Azure desde .NET User Secrets. Desde `backend/Closet.Api`, ejecuta una vez:

```bash
dotnet user-secrets init
```

Luego guarda la cadena de conexión:

```bash
dotnet user-secrets set "ConnectionStrings:BlobStorage" "PEGA_AQUI_LA_CADENA_DE_CONEXION"
```

Reemplaza el texto de ejemplo por la cadena de conexión de Azure. No la agregues a `appsettings.json`, al código ni al repositorio.

La configuración de SQLite está en `appsettings.json`. No compartas el archivo `closet.db` como sustituto de Azure Blob: contiene datos locales de desarrollo.

## Ejecutar la app móvil

Desde la carpeta del proyecto Expo:

```bash
cd mobile/closet-mobile
npm install
npx expo start
```

Escanea el código QR con Expo Go. La app y el backend se ejecutan por separado.

Para que un celular pueda comunicarse con el backend, ambos deben estar en la misma red Wi-Fi. En la app se debe usar la IP local de la computadora que ejecuta la API, no `localhost`.

## Estado actual

- El backend crea SQLite y un registro de prueba.
- Existe `GET /api/test` para comprobar la lectura de SQLite.
- Al iniciar, el backend se conecta al contenedor `garments` de Azure Blob y lo crea si no existe.
- La pantalla móvil inicial muestra “Mi clóset”.
- Falta implementar los endpoints reales para subir, listar y borrar prendas y conectar esos flujos con la app móvil.

## Trabajo en equipo

- Ejecuta una sola instancia del backend cuando el equipo quiera probar con la misma base SQLite.
- Los demás dispositivos se conectan a la IP local de esa computadora.
- No subas `node_modules`, `.expo`, `bin`, `obj`, bases de datos locales ni secretos de Azure. El `.gitignore` de la raíz debe excluirlos.
