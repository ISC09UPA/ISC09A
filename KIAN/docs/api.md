# API

Referencia interactiva en **Swagger** (`/swagger`, solo en `Development`). Este documento resume el contrato.

- Base: `/api`. JSON en `camelCase`. Enums como texto (`"Good"`, no `3`).
- Fechas ISO 8601 en **UTC** (terminan en `Z`).
- Errores en `application/problem+json` (RFC 9457) con `traceId`. En producción no incluyen stack traces.
- Todas las operaciones son del **usuario actual** (`ICurrentUserService`). Las tarjetas de otros usuarios responden `404`.

## Health

| Método | Ruta           | Descripción                                                            |
| ------ | -------------- | ---------------------------------------------------------------------- |
| GET    | `/health`      | SQL + Blob Storage. `200 Healthy` o `503 Unhealthy`. Path para App Service |
| GET    | `/health/live` | Solo el proceso, sin dependencias                                      |

```json
{ "status": "Healthy", "totalDurationMs": 5, "checks": { "sql": "Healthy", "blob-storage": "Healthy" } }
```

## Cards

| Método | Ruta                                        | Cuerpo / query                                   | Respuesta                        |
| ------ | ------------------------------------------- | ------------------------------------------------ | -------------------------------- |
| GET    | `/api/cards`                                | `language?`, `page` (≥1, def. 1), `pageSize` (1–100, def. 20) | `200 PagedResponse<Card>` |
| GET    | `/api/cards/{id}`                           | `language?`                                      | `200 Card` · `404`               |
| GET    | `/api/cards/{id}/translations/{language}`   | —                                                | `200 Translation` · `404`        |
| POST   | `/api/cards`                                | `multipart/form-data` (ver abajo)                | `201 Card` + `Location` · `400`  |
| PUT    | `/api/cards/{id}`                           | `multipart/form-data`, `image` opcional          | `200 Card` · `400` · `404`       |
| DELETE | `/api/cards/{id}`                           | —                                                | `204` · `404`                    |

Con `language`, solo se devuelven tarjetas que tienen esa traducción, y solo esa traducción.

### Crear una tarjeta

Campos del formulario:

| Campo                              | Obligatorio | Reglas                                                     |
| ---------------------------------- | ----------- | ---------------------------------------------------------- |
| `image`                            | Sí (POST)   | `image/jpeg`, `image/png` o `image/webp`; ≤ 5 MB (configurable); extensión y contenido deben coincidir con el tipo |
| `translations[i].language`         | Sí          | `en`, `es`, `fr`, `de`, `it`, `pt`; sin repetir            |
| `translations[i].translatedText`   | Sí          | ≤ 200 caracteres                                           |
| `translations[i].exampleSentence`  | No          | ≤ 500 caracteres                                           |

Entre 1 y 20 traducciones. En `PUT`, las traducciones enviadas **reemplazan** a las existentes.

```bash
curl -F "image=@apple.png;type=image/png" \
     -F "translations[0].language=en" -F "translations[0].translatedText=Apple" \
     -F "translations[0].exampleSentence=I eat an apple every day." \
     -F "translations[1].language=es" -F "translations[1].translatedText=Manzana" \
     http://localhost:5080/api/cards
```

```json
{
  "id": "8b6da2bc-7f84-408d-88f4-08df18e7d812",
  "imageUrl": "https://<cuenta>.blob.core.windows.net/card-images/253c...a14.png?sv=...&sp=r&sig=...",
  "createdAt": "2026-09-22T20:26:55.138Z",
  "translations": [
    { "language": "en", "translatedText": "Apple", "exampleSentence": "I eat an apple every day." },
    { "language": "es", "translatedText": "Manzana", "exampleSentence": null }
  ]
}
```

`imageUrl` es una URL SAS **de solo lectura y temporal** (60 min por defecto). El container es privado:
sin SAS, la imagen responde `403`. El nombre del blob lo genera el servidor; el nombre del archivo del cliente se ignora.

## Reviews

| Método | Ruta                    | Cuerpo / query                                    | Respuesta                  |
| ------ | ----------------------- | ------------------------------------------------- | -------------------------- |
| GET    | `/api/reviews/due`      | `language` (def. `en`), `limit` (1–200, def. 50)  | `200 DueCards`             |
| POST   | `/api/reviews`          | `{ "cardId": "...", "rating": "Good" }`           | `200 ReviewState` · `400` · `404` |
| GET    | `/api/reviews/{cardId}` | —                                                 | `200 ReviewState` · `404`  |

`GET /api/reviews/due` devuelve las tarjetas con `NextReviewAt <= UtcNow` que tienen traducción en
`language`, de la más atrasada a la más reciente. Una tarjeta nueva está pendiente desde su creación.

```json
{
  "totalDue": 1,
  "cards": [
    {
      "id": "8b6da2bc-...",
      "imageUrl": "https://...sig=...",
      "translation": "Manzana",
      "language": "es",
      "exampleSentence": null,
      "nextReviewAt": "2026-09-22T20:26:55.138Z"
    }
  ]
}
```

`rating` ∈ `Again | Hard | Good | Easy`. Respuesta:

```json
{
  "cardId": "8b6da2bc-...",
  "nextReviewAt": "2026-09-23T20:26:55.983Z",
  "lastReviewedAt": "2026-09-22T20:26:55.983Z",
  "intervalDays": 1,
  "easeFactor": 2.5,
  "repetitions": 1,
  "lapses": 0
}
```

El algoritmo se describe en [architecture.md](architecture.md#repetición-espaciada).

## Errores

| Estado | Cuándo                                                            |
| ------ | ----------------------------------------------------------------- |
| 400    | Validación (`errors` con los campos), JSON inválido, rating desconocido |
| 401    | Sin usuario autenticado (fuera de `Development`)                  |
| 404    | Recurso inexistente o de otro usuario; id que no es GUID          |
| 503    | Blob Storage no disponible                                        |
| 500    | Error inesperado (sin detalles fuera de `Development`)            |

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "image": ["The file content is not a valid image of the declared type."] },
  "traceId": "0HNOOSV330EID:00000001"
}
```
