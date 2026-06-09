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

## Flujos end-to-end críticos

Las pruebas de integración con `WebApplicationFactory` recorren la API HTTP y las capas de
aplicación y dominio con dependencias externas controladas:

- ciclo CRUD completo: crear, buscar, editar, consultar y eliminar un registro;
- análisis de IA: crear, solicitar resumen y verificar que el resultado queda persistido.

El adaptador de IA se sustituye solamente dentro del host de pruebas. Producción sigue usando
la integración real configurada en `Infrastructure`.

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
