# Backend

## Proyectos

- `KnowledgeAssistant.Domain`: modelo e invariantes.
- `KnowledgeAssistant.Application`: casos de uso y contratos.
- `KnowledgeAssistant.Infrastructure`: EF Core, SQL Server, Mistral y automatización.
- `KnowledgeAssistant.Api`: API REST versionada.
- `KnowledgeAssistant.UnitTests`: pruebas aisladas.
- `KnowledgeAssistant.IntegrationTests`: pruebas HTTP y persistencia.

## Ejecución

```powershell
dotnet restore ../../KnowledgeAssistant.sln
dotnet run --project src/KnowledgeAssistant.Api
```

La configuración local usa SQL Server LocalDB. Docker Compose reemplaza la cadena de conexión
para utilizar el contenedor de SQL Server.
