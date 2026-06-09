# Documento de Inteligencia Artificial

## Proveedor

Mistral API. El modelo inicial configurable será `mistral-small-latest`; no se codificará un
modelo fijo en los casos de uso.

## Estrategia de prompts

- Separar instrucciones del sistema, contexto empresarial y solicitud del usuario.
- Solicitar JSON estructurado cuando la salida alimente lógica de la aplicación.
- Incluir identificadores y contenido mínimo necesario.
- No enviar secretos ni datos ajenos al registro seleccionado.
- Versionar plantillas de prompt y probar su contrato.

## Controles

- Validación del tamaño y contenido antes de enviar.
- Timeout y cancelación.
- Reintentos limitados solo para errores transitorios.
- Manejo explícito de `429` y errores del proveedor.
- Validación de JSON y rechazo de respuestas incompletas.
- Registro de metadatos técnicos sin almacenar la llave.

## Limitaciones conocidas

Los modelos pueden alucinar, clasificar incorrectamente o producir formatos inesperados. Las
salidas se mostrarán como asistencia, conservarán trazabilidad y no reemplazarán el contenido
fuente.
