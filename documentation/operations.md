# Documento operativo

## Flujo de negocio

1. Consultar el dashboard.
2. Crear un registro empresarial con título, contenido y fuente.
3. Localizar registros mediante búsqueda, filtros y paginación.
4. Editar o eliminar información con confirmación.
5. Solicitar resumen, clasificación o recomendaciones.
6. Formular preguntas sobre un registro.
7. Consultar el estado de análisis automatizados.

## Pantallas implementadas

- Dashboard.
- Listado de registros.
- Alta y edición.
- Edición con panel de análisis de IA.
- Chat global contextual.

Las capturas funcionales del dashboard, listado, alta, edición y panel de IA están disponibles
en [`evidence/README.md`](evidence/README.md).

## Datos de demostración

Cuando `SeedData:Enabled=true`, la aplicación crea tres registros empresariales:

- Política de atención a clientes.
- Notas de reunión comercial.
- Registro de mejora operativa.

Las semillas no incluyen respuestas de IA. El enriquecimiento deberá generarse mediante la
integración real con Mistral.

## Checklist de seguridad

- Configurar `Mistral__ApiKey` mediante variable de entorno o almacén local de secretos.
- Usar una URL HTTPS absoluta en `Mistral__BaseUrl`.
- Mantener `Mistral__TimeoutSeconds` entre 1 y 120 segundos.
- Definir en `Cors__AllowedOrigins` únicamente orígenes frontend conocidos.
- No registrar prompts, respuestas del proveedor, llaves ni cadenas de conexión.
- Reemplazar las contraseñas de ejemplo antes de iniciar Docker Compose.

Las preguntas de IA se normalizan y se limitan a 1000 caracteres. Entradas vacías o demasiado
largas reciben `ValidationProblemDetails` y no se envían al proveedor.
