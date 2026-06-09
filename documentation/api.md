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

## Inteligencia Artificial

- `POST /records/{id}/ai/summary`: generar resumen.
- `POST /records/{id}/ai/classification`: clasificar.
- `POST /records/{id}/ai/recommendations`: generar recomendaciones.
- `POST /records/{id}/ai/questions`: responder una pregunta sobre el registro.

Las respuestas de error usarán `application/problem+json`. El contrato final se publicará
mediante OpenAPI y se actualizará junto con cada caso de uso.
