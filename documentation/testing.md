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

## T6.1 - Flujos end-to-end críticos

Validación ejecutada el 9 de junio de 2026:

- `dotnet test KnowledgeAssistant.sln`: 113 pruebas superadas y 8 omitidas por no estar
  configurada `KNOWLEDGE_ASSISTANT_TEST_CONNECTION`.
- `ng build`: correcto, con advertencia conocida de presupuesto inicial (685.21 kB).
- `ng test --watch=false --browsers=ChromeHeadless --code-coverage`: 31 pruebas superadas.
- `tsc -p tsconfig.app.json --noEmit`: correcto.
- `tsc -p tsconfig.spec.json --noEmit`: correcto.

Los nuevos recorridos cubren el ciclo CRUD completo por HTTP y la solicitud de análisis de IA
con persistencia del resultado. El flujo de IA detectó y corrigió la transición faltante de
`Pending` a `Processing` antes de completar el análisis.
