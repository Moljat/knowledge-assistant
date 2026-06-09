# Documento de Inteligencia Artificial

## Proveedor

Mistral API. El modelo configurable predeterminado es `mistral-small-latest`; los casos de uso
no dependen de un modelo fijo ni de un SDK externo.

## Estrategia de prompts

- Separar instrucciones del sistema, contexto empresarial y solicitud del usuario.
- Solicitar JSON estructurado cuando la salida alimente lógica de la aplicación.
- Incluir identificadores y contenido mínimo necesario.
- No enviar secretos ni datos ajenos al registro seleccionado.
- Mantener plantillas de prompt reutilizables y probar su contrato.

## Controles

- Validación del tamaño y contenido antes de enviar.
- Timeout y cancelación.
- Reintentos limitados solo para errores transitorios.
- Manejo explícito de `429` y errores del proveedor.
- Validación de JSON y rechazo de respuestas incompletas.
- Registro de metadatos técnicos sin almacenar la llave.
- Hasta tres reintentos con espera incremental para fallos transitorios.
- Procesamiento en segundo plano de registros pendientes con idempotencia.

## Limitaciones conocidas

Los modelos pueden alucinar, clasificar incorrectamente o producir formatos inesperados. Las
salidas se muestran como asistencia y no reemplazan el contenido fuente. La aplicación requiere
una llave real para producción; las respuestas simuladas existen únicamente en pruebas.
