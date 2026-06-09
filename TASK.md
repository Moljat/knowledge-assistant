# Plan de implementación

Las tareas están ordenadas para producir incrementos pequeños y demostrables.

## Fase 0: Base del repositorio

- [x] `T0.1` Crear monorepo, solución .NET y aplicación Angular.
- [x] `T0.2` Separar capas Domain, Application, Infrastructure y Api.
- [x] `T0.3` Preparar Docker Compose, variables de entorno y documentación inicial.
- [x] `T0.4` Incorporar proyectos de pruebas unitarias e integración.

## Fase 1: Modelo y persistencia

- [x] `T1.1` Cerrar el modelo de `KnowledgeRecord` y sus invariantes.
- [x] `T1.2` Crear configuración EF Core y primera migración.
- [x] `T1.3` Implementar repositorio y unidad de trabajo.
- [x] `T1.4` Agregar datos semilla para demostración.
- [x] `T1.5` Probar reglas de dominio y persistencia.

## Fase 2: CRUD REST

- [x] `T2.1` Crear registro con validaciones.
- [x] `T2.2` Consultar detalle y listado paginado.
- [x] `T2.3` Editar registro preservando integridad.
- [x] `T2.4` Eliminar con confirmación desde el frontend.
- [x] `T2.5` Buscar y aplicar filtros dinámicos.
- [x] `T2.6` Estandarizar errores, logging y códigos HTTP.

## Fase 3: Frontend de gestión

- [x] `T3.0` Corregir entorno frontend para ejecutar `ng build` y `ng test` sin errores de resolución.
- [x] `T3.1` Crear layout responsive y navegación.
- [x] `T3.2` Implementar dashboard con indicadores.
- [x] `T3.3` Implementar tabla paginada, búsqueda y filtros.
- [x] `T3.4` Implementar formulario reactivo de alta y edición.
- [x] `T3.5` Agregar estados de carga, vacíos y errores.
- [x] `T3.6` Probar componentes, servicios e interacciones.

### ✅ Fase 3 completada

## Fase 4: Mistral e IA

- [x] `T4.1` Configurar cliente Mistral y manejo seguro de credenciales.
- [x] `T4.2` Diseñar prompts reutilizables y respuestas JSON estructuradas.
- [x] `T4.3` Implementar resumen inteligente.
- [x] `T4.4` Implementar clasificación automática.
- [x] `T4.5` Implementar recomendaciones.
- [x] `T4.6` Implementar preguntas sobre un registro o conjunto filtrado.
- [x] `T4.7` Manejar timeouts, rate limits, respuestas inválidas y reintentos acotados.
- [x] `T4.8` Probar el adaptador con HTTP simulado y una prueba real opcional.
- [x] `T4.9` Exponer campos de IA (summary, category, recommendations) en API y frontend.
- [x] `T4.10` Agregar panel de análisis IA en formulario de edición de registros.
- [x] `T4.11` Crear chat global con Mistral accesible desde cualquier página.
- [x] `T4.12` Inyectar registros de la base de datos como contexto en el chat global.

### ✅ Fase 4 completada


## Fase 5: Automatización

- [x] `T5.1` Crear un `BackgroundService` para procesar registros pendientes.
- [x] `T5.2` Añadir estados, reintentos e idempotencia.
- [x] `T5.3` Exponer métricas operativas básicas del procesamiento.
- [x] `T5.4` Probar ejecución, cancelación y recuperación de errores.

## Fase 6: Calidad y entrega

- [x] `T6.1` Completar pruebas end-to-end de flujos críticos.
- [ ] `T6.2` Medir cobertura y cerrar huecos de riesgo.
- [ ] `T6.3` Validar seguridad, sanitización y configuración.
- [ ] `T6.4` Crear workflow de CI para build y pruebas.
- [ ] `T6.5` Capturar evidencias funcionales y de pruebas.
- [ ] `T6.6` Completar documentación obligatoria y entrega final.

## Primera tarea propuesta

`T1.1`: definir definitivamente el agregado `KnowledgeRecord`, sus campos, estados y reglas.
Es el contrato que condiciona base de datos, CRUD, prompts y pantallas.
