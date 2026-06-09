# Knowledge Assistant

Plataforma interna para registrar información empresarial y enriquecerla con Inteligencia
Artificial. Permitirá administrar registros, resumirlos, clasificarlos, generar recomendaciones
y responder preguntas sobre la información almacenada mediante la API real de Mistral.

## Estado

El repositorio contiene el scaffold inicial y la planeación técnica. La implementación funcional
se realizará por tareas pequeñas documentadas en [TASK.md](TASK.md).

## Stack

- Backend: .NET 9, ASP.NET Core Web API y Entity Framework Core.
- Frontend: Angular 20, TypeScript y Angular Material.
- Datos: SQL Server 2022.
- IA: Mistral API mediante `HttpClient`.
- Pruebas: xUnit, `WebApplicationFactory`, Jasmine y Karma.
- Contenedores: Docker Compose para SQL Server, API y frontend.

## Arquitectura

El backend sigue Clean Architecture:

```text
Api -> Application <- Infrastructure
          |
        Domain
```

`Domain` no depende de ningún otro proyecto. `Application` define casos de uso y contratos.
`Infrastructure` implementa persistencia e integraciones externas. `Api` expone la aplicación.

Consulta [documentation/architecture.md](documentation/architecture.md) para el detalle.

## Inicio rápido

1. Copia `.env.example` como `.env` y configura `SQL_SA_PASSWORD`.
2. Agrega `MISTRAL_API_KEY` cuando iniciemos la integración de IA.
3. Restaura herramientas .NET con `dotnet tool restore`.
4. Instala dependencias con `npm install`.
5. Ejecuta el backend con `npm run start:backend`.
6. Ejecuta el frontend con `npm run start:frontend`.

Para levantar la solución completa:

```powershell
docker-compose up --build
```

El frontend quedará en `http://localhost:4200` y la API en
`http://localhost:8080/api/v1/system/health`.

## Variables sensibles

Las llaves y cadenas de conexión no se versionan. En desarrollo se usarán variables de entorno
o secretos de usuario de .NET; Docker Compose leerá el archivo `.env`.

## Documentación

- [Plan de trabajo](TASK.md)
- [Estrategia de pruebas](TESTING.md)
- [Arquitectura](documentation/architecture.md)
- [API prevista](documentation/api.md)
- [Decisiones técnicas](documentation/decisions.md)
- [Integración de IA](documentation/ai.md)
- [Instalación técnica](documentation/technical.md)
- [Uso operativo](documentation/operations.md)
