# AGENTS.md

## Propósito

Este repositorio implementa la prueba técnica Knowledge Assistant. Todo cambio debe respetar
los requisitos del documento fuente y mantener coherencia entre código, pruebas y documentación.

## Reglas de arquitectura

- `Domain` no depende de infraestructura, ASP.NET Core ni Entity Framework Core.
- `Application` depende solamente de `Domain`.
- `Infrastructure` implementa contratos definidos por `Application`.
- `Api` contiene transporte HTTP, configuración y composición, no lógica de negocio.
- Los componentes Angular consumen servicios y no construyen peticiones HTTP directamente.
- Ninguna respuesta de IA puede ser simulada en producción.

## Convenciones

- C# con nullable habilitado, métodos asíncronos y `CancellationToken`.
- TypeScript en modo estricto y componentes Angular standalone.
- Rutas HTTP bajo `/api/v1`.
- Errores HTTP con `ProblemDetails`.
- Fechas persistidas en UTC.
- Secretos únicamente mediante variables de entorno o almacenes locales excluidos de Git.

## Flujo de trabajo

1. Seleccionar una tarea de `TASK.md`.
2. Implementar el cambio mínimo completo.
3. Agregar o actualizar pruebas automatizadas.
4. Ejecutar las comprobaciones indicadas en `TESTING.md`.
5. Actualizar documentación y estado de la tarea.

## Definition of Done

- Compila sin errores.
- Pruebas relacionadas en verde.
- No expone secretos.
- Incluye validaciones y manejo de errores.
- Documentación alineada con el comportamiento real.
