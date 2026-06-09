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
| TC-031 | Aplicacion | Crear registro valido | Persiste el agregado y devuelve estado inicial |
| TC-032 | Aplicacion | Crear registro invalido | No persiste cambios |
| TC-033 | API | Crear registro valido | Responde 201 Created con ubicacion y cuerpo |
| TC-034 | API | Crear registro invalido | Responde 400 con ValidationProblemDetails |
| TC-035 | Aplicacion | Consultar registro existente | Devuelve el detalle del agregado |
| TC-036 | Aplicacion | Consultar registro inexistente | Devuelve resultado nulo |
| TC-037 | Aplicacion | Listar registros paginados | Devuelve items y metadatos de paginacion |
| TC-038 | Aplicacion | Paginacion invalida | Se rechaza por validacion |
| TC-039 | API | Consultar detalle existente | Responde 200 con el registro |
| TC-040 | API | Consultar detalle inexistente | Responde 404 con ProblemDetails |
| TC-041 | API | Listar registros paginados | Responde 200 con items y totales |
| TC-042 | API | Listar con paginacion invalida | Responde 400 con ValidationProblemDetails |
| TC-043 | Aplicacion | Editar registro existente | Actualiza campos y guarda cambios |
| TC-044 | Aplicacion | Editar registro inexistente | Devuelve resultado nulo sin guardar |
| TC-045 | Aplicacion | Editar con datos invalidos | Rechaza cambios sin guardar |
| TC-046 | Aplicacion | Editar contenido analizado | Invalida resultados previos de IA |
| TC-047 | API | Editar registro valido | Responde 200 con el registro actualizado |
| TC-048 | API | Editar registro inexistente | Responde 404 con ProblemDetails |
| TC-049 | API | Editar registro invalido | Responde 400 con ValidationProblemDetails |
| TC-050 | Aplicacion | Eliminar registro existente | Remueve el agregado y guarda cambios |
| TC-051 | Aplicacion | Eliminar registro inexistente | Devuelve falso sin guardar |
| TC-052 | API | Eliminar registro existente | Responde 204 y ya no aparece en detalle |
| TC-053 | API | Eliminar registro inexistente | Responde 404 con ProblemDetails |
| TC-054 | Frontend | Cancelar confirmacion de eliminacion | No llama al servicio de eliminacion |
| TC-055 | Frontend | Confirmar eliminacion | Llama al servicio y refresca registros |
| TC-056 | Aplicacion | Listar con filtros dinamicos | Normaliza filtros y los pasa al repositorio |
| TC-057 | Aplicacion | Filtro de texto demasiado largo | Responde con validacion |
| TC-058 | API | Buscar registros | Devuelve solo coincidencias por texto |
| TC-059 | API | Filtrar por estado y tipo | Devuelve solo registros coincidentes |
| TC-060 | Frontend | Listar con filtros | Serializa filtros como query params |
| TC-061 | API | Error de validacion | Responde application/problem+json con tipo y traceId |
| TC-062 | API | Recurso no encontrado | Responde ProblemDetails con tipo not-found y traceId |
| TC-063 | E2E API | Ciclo CRUD completo | Crea, busca, edita, consulta y elimina el registro por HTTP |
| TC-064 | E2E IA | Analizar y persistir resumen | Completa el analisis y conserva los resultados al consultar |
| TC-065 | Aplicacion | Chat con contexto | Limita registros, clasifica contexto y trunca contenido largo |
| TC-066 | Aplicacion | Metricas del dashboard | Mapea conteos y valores vacios para todos los estados |
| TC-067 | Frontend IA | Acciones de analisis | Ejecuta resumen, clasificacion, recomendaciones y errores |
| TC-068 | Frontend IA | Chat global | Maneja respuesta, respuesta vacia, error y solicitudes bloqueadas |
| TC-069 | Seguridad | Pregunta IA invalida | Rechaza texto vacio o mayor a 1000 caracteres con ProblemDetails |
| TC-070 | Configuracion | Mistral inseguro | Rechaza URL no HTTPS, modelo vacio y timeout fuera de rango |
| TC-071 | Secretos | Credencial Mistral ausente | No envia la solicitud ni registra contenido sensible |
| TC-072 | CI | Pipeline completo | Compila backend/frontend y ejecuta pruebas con SQL Server |
