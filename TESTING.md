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
