# Documento técnico

## Requisitos

- .NET SDK 9.0.305.
- Node.js 20.19 a 24.x y npm.
- Docker Desktop o SQL Server LocalDB/Express.
- Llave de Mistral para las funciones de IA.

## Configuración

Las variables se describen en `.env.example`. En ASP.NET Core se mapean con doble guion bajo,
por ejemplo `Mistral__ApiKey`.

## Ejecución local

```powershell
dotnet restore KnowledgeAssistant.sln
dotnet run --project backend/src/KnowledgeAssistant.Api
npm install
npm --workspace frontend start
```

## Migraciones

Las migraciones vivirán en `KnowledgeAssistant.Infrastructure/Migrations`.

```powershell
dotnet ef migrations add InitialCreate --project backend/src/KnowledgeAssistant.Infrastructure --startup-project backend/src/KnowledgeAssistant.Api
dotnet ef database update --project backend/src/KnowledgeAssistant.Infrastructure --startup-project backend/src/KnowledgeAssistant.Api
```

Estos comandos se habilitarán en la fase de persistencia.
