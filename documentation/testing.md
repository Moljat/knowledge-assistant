# Documento de pruebas

La estrategia viva se encuentra en `TESTING.md`.

Durante el desarrollo este documento registrará:

- Casos ejecutados y resultado.
- Cobertura por proyecto.
- Evidencias de consola o CI.
- Defectos encontrados y pruebas de regresión.
- Pruebas reales de Mistral ejecutadas con credenciales privadas.

Las evidencias se almacenarán en `documentation/evidence/` sin incluir secretos.

## Línea base

Validación inicial ejecutada el 9 de junio de 2026:

- `dotnet build KnowledgeAssistant.sln`: correcto, sin advertencias.
- `dotnet test KnowledgeAssistant.sln`: 3 pruebas superadas.
- `npm run build --prefix frontend`: bundle de producción correcto.
- `npm run test:ci --prefix frontend`: 3 pruebas superadas.
- `docker-compose config --quiet`: configuración válida.
