# Casos de prueba iniciales

| ID | Área | Caso | Resultado esperado |
| --- | --- | --- | --- |
| TC-001 | Dominio | Crear registro válido | Se crea con identificador y fechas UTC |
| TC-002 | Dominio | Crear sin título | Se rechaza por validación |
| TC-003 | API | Consultar health | Responde 200 con versión `v1` |
| TC-004 | CRUD | Listar paginado | Devuelve elementos y metadatos |
| TC-005 | CRUD | Filtrar por categoría | Devuelve solo coincidencias |
| TC-006 | IA | Resumir registro | Persiste una respuesta real de Mistral |
| TC-007 | IA | Mistral responde 429 | Devuelve error controlado o reintenta según política |
| TC-008 | IA | Respuesta JSON inválida | No persiste análisis inconsistente |
| TC-009 | UI | Formulario inválido | Bloquea envío y muestra mensajes |
| TC-010 | UI | Error HTTP | Muestra estado recuperable al usuario |
| TC-011 | Dominio | Archivar registro | Queda inmutable hasta restaurarlo |
| TC-012 | Dominio | Modificar contenido analizado | Invalida resultados anteriores de IA |
| TC-013 | Dominio | Completar análisis sin resultados | Se rechaza por regla de negocio |
| TC-014 | Dominio | Flujo de análisis válido | Avanza de pendiente a completado |
| TC-015 | Dominio | Actualización con tipo inválido | Falla sin modificar parcialmente |
| TC-016 | Dominio | Archivar durante análisis | Se rechaza hasta finalizar el proceso |
| TC-017 | Persistencia | Inspeccionar modelo EF Core | Tabla, columnas e índices coinciden con el dominio |
| TC-018 | Persistencia | Generar script idempotente | Incluye historial y migración inicial |
