# API prevista

Prefijo base: `/api/v1`.

## Sistema

- `GET /system/health`: disponibilidad de la API.

## Registros

- `POST /records`: crear.
- `GET /records/{id}`: consultar detalle.
- `GET /records`: listar con `page`, `pageSize`, `search`, `category` y `status`.
- `PUT /records/{id}`: editar.
- `DELETE /records/{id}`: eliminar.

### Crear registro

`POST /api/v1/records`

Solicitud:

```json
{
  "title": "Politica de credito",
  "content": "Registrar aprobaciones y excepciones.",
  "source": "Manual interno",
  "type": 1
}
```

`type` usa los valores del dominio: `1` documento, `2` nota y `3` registro de negocio.

Respuesta `201 Created`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "title": "Politica de credito",
  "content": "Registrar aprobaciones y excepciones.",
  "source": "Manual interno",
  "type": 1,
  "status": 1,
  "aiStatus": 0,
  "createdAtUtc": "2026-06-09T18:00:00+00:00",
  "updatedAtUtc": "2026-06-09T18:00:00+00:00"
}
```

Los payloads invalidos responden `400 Bad Request` con `ValidationProblemDetails`.

### Consultar detalle

`GET /api/v1/records/{id}`

Respuesta `200 OK`: usa el mismo contrato `KnowledgeRecordResponse` del alta. Cuando el
identificador no existe responde `404 Not Found` con `ProblemDetails`.

### Listar registros

`GET /api/v1/records?page=1&pageSize=20&search=credito&category=Operaciones&status=2&type=1&aiStatus=3`

`page` inicia en `1`. `pageSize` acepta valores de `1` a `100`. Los filtros son opcionales:

- `search`: busca en titulo, contenido, fuente y categoria.
- `category`: filtra por categoria exacta.
- `status`: filtra por estado del registro.
- `type`: filtra por tipo de registro.
- `aiStatus`: filtra por estado del analisis de IA.

Respuesta `200 OK`:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0,
  "totalPages": 0
}
```

La paginacion invalida responde `400 Bad Request` con `ValidationProblemDetails`.

### Editar registro

`PUT /api/v1/records/{id}`

Solicitud:

```json
{
  "title": "Politica de credito actualizada",
  "content": "Registrar aprobaciones, excepciones y responsables.",
  "source": "Manual interno",
  "type": 1
}
```

Respuesta `200 OK`: usa el contrato `KnowledgeRecordResponse`. Si el registro no existe
responde `404 Not Found` con `ProblemDetails`. Los datos invalidos responden `400 Bad Request`
con `ValidationProblemDetails`. Cuando cambia el contenido, los resultados previos de IA se
invalidan de acuerdo con las reglas del dominio.

### Eliminar registro

`DELETE /api/v1/records/{id}`

Respuesta `204 No Content` cuando el registro existe y se elimina. Si el identificador no
existe responde `404 Not Found` con `ProblemDetails`.

## Inteligencia Artificial

- `POST /records/{id}/ai/summary`: generar resumen.
- `POST /records/{id}/ai/classification`: clasificar.
- `POST /records/{id}/ai/recommendations`: generar recomendaciones.
- `POST /records/{id}/ai/questions`: responder una pregunta sobre el registro.

Las respuestas de error usarán `application/problem+json`. El contrato final se publicará
mediante OpenAPI y se actualizará junto con cada caso de uso.

## Errores estandarizados

Las respuestas de error usan `application/problem+json` e incluyen `traceId` para correlacion operativa. Tipos estandarizados:

- `/problems/validation-error`: parametros, filtros o payloads invalidos.
- `/problems/not-found`: recursos inexistentes.
- `/problems/business-rule-violation`: reglas de dominio que impiden la operacion.
- `/problems/internal-server-error`: errores no controlados.
