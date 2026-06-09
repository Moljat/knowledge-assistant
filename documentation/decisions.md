# Decisiones técnicas

## ADR-001: .NET 9

Se utiliza .NET 9 porque está instalado en el entorno y cumple el requisito de .NET 8 o
superior. `global.json` fija el SDK para evitar diferencias locales.

## ADR-002: Angular 20 standalone

Angular 20 cumple el requisito de Angular 18 o superior. Los componentes standalone reducen
configuración y son el estilo recomendado por el generador actual.

## ADR-003: Monolito modular con Clean Architecture

Es suficiente para el alcance y las 24 horas de la prueba, sin renunciar a límites claros ni
capacidad de prueba. No se introducirán microservicios sin una necesidad demostrable.

## ADR-004: Mistral mediante HTTP

El adaptador usará la API HTTP oficial detrás de `IAiAnalysisService`. Esto evita acoplar
Application a un SDK y permite probar el protocolo con un manejador HTTP controlado.

## ADR-005: SQL Server en Docker y LocalDB

Docker ofrece reproducibilidad. La configuración local mantiene compatibilidad con LocalDB en
Windows. EF Core será la única fuente de cambios del esquema.

## ADR-006: BackgroundService

La automatización será interna para reducir componentes operativos durante la prueba. Procesará
registros pendientes con idempotencia y reintentos limitados.
