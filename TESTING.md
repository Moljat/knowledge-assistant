# Estrategia de pruebas

## Backend

- Unitarias: entidades, reglas de negocio, validadores y servicios de aplicación.
- Integración: endpoints con `WebApplicationFactory`, EF Core y SQL Server de pruebas.
- Adaptador Mistral: `HttpMessageHandler` controlado para verificar solicitudes, respuestas,
  timeouts, rate limits y JSON inválido.
- Prueba real de IA: opcional y separada, habilitada solamente cuando exista
  `MISTRAL_API_KEY`.

Comandos:

```powershell
dotnet test KnowledgeAssistant.sln
dotnet test KnowledgeAssistant.sln --collect:"XPlat Code Coverage"
```

Para la métrica objetivo de lógica se combinan por archivo y número de línea los reportes
Cobertura de pruebas unitarias e integración, tomando una línea como cubierta si cualquiera
de las suites la ejecuta. El alcance medido es `KnowledgeAssistant.Domain` y
`KnowledgeAssistant.Application`.

Las pruebas que requieren SQL Server usan `KNOWLEDGE_ASSISTANT_TEST_CONNECTION`. Cuando la
variable no existe se reportan como omitidas; en CI deberán ejecutarse contra un servicio SQL.

## Frontend

- Componentes: renderizado, validaciones, estados y eventos.
- Servicios: URL, parámetros, serialización y errores HTTP.
- Integración de UI: navegación y flujos principales con dependencias sustituidas.

Comando:

```powershell
npm --workspace frontend run test:ci
```

## Criterios iniciales

- Cobertura objetivo: al menos 80% en lógica de dominio y aplicación.
- Todo defecto corregido debe incluir una prueba de regresión.
- Las pruebas no dependen de una llave real de Mistral salvo la suite marcada como externa.
- Las evidencias de entrega se guardarán en `documentation/evidence/`.

El índice versionado de capturas funcionales, comandos y resultados se encuentra en
[`documentation/evidence/README.md`](documentation/evidence/README.md).

## Cobertura actual

Medición del 9 de junio de 2026:

- Domain + Application: 95.4% de líneas (503/527).
- Frontend: 86.2% de statements, 59.25% de branches, 80.35% de functions y 88.07% de lines.

Los huecos de mayor riesgo cerrados fueron el chat con contexto, las métricas del dashboard,
el panel de análisis IA y el chat global. Las ramas residuales pertenecen principalmente a
variaciones de filtros y estados visuales ya cubiertos por recorridos representativos.

## Validaciones de seguridad

```powershell
dotnet list KnowledgeAssistant.sln package --vulnerable --include-transitive
git grep -n -I -E "(api[_-]?key|password|secret)"
npm audit --omit=dev
```

La revisión incluye secretos rastreados, límites de entradas de IA, errores `ProblemDetails`,
configuración HTTPS de Mistral y ausencia de contenido de proveedor en logs. `npm audit` debe
ejecutarse desde una instalación que use un lockfile compatible con el workspace.

## Flujos end-to-end críticos

Las pruebas de integración con `WebApplicationFactory` recorren la API HTTP y las capas de
aplicación y dominio con dependencias externas controladas:

- ciclo CRUD completo: crear, buscar, editar, consultar y eliminar un registro;
- análisis de IA: crear, solicitar resumen y verificar que el resultado queda persistido.

El adaptador de IA se sustituye solamente dentro del host de pruebas. Producción sigue usando
la integración real configurada en `Infrastructure`.

## Integración continua

El workflow `.github/workflows/ci.yml` se ejecuta en pushes y pull requests hacia `dev`.

- Backend: restaura, compila en Release y ejecuta todas las pruebas con SQL Server y cobertura.
- Frontend: instala con `npm ci --workspaces=false`, compila y ejecuta pruebas headless con cobertura.
- Los resultados backend y la cobertura frontend se publican como artefactos durante 14 días.

## Frontend

`ng build` y `ng test` requieren un navegador Chromium disponible. El script
`frontend/scripts/resolve-browser.mjs` detecta automáticamente Chrome o Edge
y establece `CHROME_BIN`. Si no se encuentra ninguno, los tests fallarán al
lanzar el navegador.

Validación adicional de TypeScript (útil si `ng test` no puede ejecutarse):

```powershell
cd frontend
.\node_modules\.bin\tsc.cmd -p tsconfig.app.json --noEmit
.\node_modules\.bin\tsc.cmd -p tsconfig.spec.json --noEmit
```
