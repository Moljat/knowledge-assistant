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
| TC-019 | Repositorio | Agregar y consultar registro | Persiste y devuelve una entidad sin tracking |
| TC-020 | Repositorio | Consultar para actualización | Mantiene tracking y guarda los cambios |
| TC-021 | Repositorio | Eliminar registro | La unidad de trabajo confirma la eliminación |
| TC-022 | Semillas | Primera ejecución | Inserta tres registros demo sin resultados de IA |
| TC-023 | Semillas | Segunda ejecución | No crea registros duplicados |
| TC-024 | Inicialización | Funcionalidad desactivada | No migra ni inserta datos automáticamente |
| TC-025 | Dominio | Activar registro archivado | Se rechaza hasta restaurarlo |
| TC-026 | Dominio | Restaurar registro no archivado | Se rechaza por transicion invalida |
| TC-027 | Dominio | Resultado de IA excede limites | Se rechaza por validacion |
| TC-028 | Persistencia | Limites de columnas EF Core | Coinciden con los limites del dominio |
| TC-029 | Persistencia | Fechas UTC | Se configuran con precision de segundos |
| TC-030 | Persistencia | Recargar registro analizado | Conserva estado, fechas y resultados de IA |
