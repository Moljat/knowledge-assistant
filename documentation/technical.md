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

Las migraciones viven en
`backend/src/KnowledgeAssistant.Infrastructure/Persistence/Migrations`.

La versión de `dotnet-ef` está fijada en `.config/dotnet-tools.json`. Después de clonar:

```powershell
dotnet tool restore
dotnet ef database update --project backend/src/KnowledgeAssistant.Infrastructure
```

Para crear una migración posterior:

```powershell
dotnet ef migrations add MigrationName --project backend/src/KnowledgeAssistant.Infrastructure --output-dir Persistence/Migrations
```

Para regenerar el script SQL idempotente:

```powershell
dotnet ef migrations script --idempotent --project backend/src/KnowledgeAssistant.Infrastructure --output backend/database/initial-schema.sql
```

La fábrica de diseño usa `ConnectionStrings__DefaultConnection` cuando está definida y
LocalDB como alternativa para generar migraciones. También acepta
`-- --connection=<connection-string>`.

## Pruebas físicas de persistencia

Las pruebas de repositorio requieren una instancia SQL Server migrada. Configure:

```powershell
$env:KNOWLEDGE_ASSISTANT_TEST_CONNECTION="Server=localhost,1433;Database=KnowledgeAssistantValidation;User Id=sa;Password=<password>;Encrypt=False;TrustServerCertificate=True"
dotnet test backend/tests/KnowledgeAssistant.IntegrationTests
```

Sin la variable, las pruebas físicas se omiten. Cada caso usa una transacción que se revierte.

## Inicialización y semillas

La inicialización automática está desactivada por defecto:

```text
DatabaseInitialization__Enabled=false
SeedData__Enabled=false
```

Docker Compose activa ambas variables. Al iniciar la API, primero aplica migraciones pendientes
y después inserta tres registros demo. El proceso de semillas es idempotente y no duplica
registros ya existentes con la fuente técnica `Knowledge Assistant Demo Seed v1`.
