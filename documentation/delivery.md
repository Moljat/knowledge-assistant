# Checklist de entrega

Estado verificado el 9 de junio de 2026.

## Funcionalidad

- CRUD de registros con búsqueda, filtros y paginación.
- Dashboard con métricas de registros y procesamiento de IA.
- Resumen, clasificación, recomendaciones y preguntas mediante Mistral.
- Chat global con registros almacenados como contexto.
- Procesamiento en segundo plano con reintentos e idempotencia.
- SQL Server con migraciones y datos semilla.

## Calidad

- Arquitectura por capas respetada.
- Errores HTTP estandarizados con `ProblemDetails`.
- Entradas de IA validadas y credenciales fuera de Git.
- 91 pruebas unitarias y 48 de integración superadas.
- 38 pruebas frontend superadas.
- Cobertura de Domain + Application de 95.4% de líneas.
- Cobertura frontend de 88.07% de líneas.
- Workflow de CI para backend, frontend, SQL Server y cobertura.

## Revisión

1. Configurar `.env` a partir de `.env.example`.
2. Ejecutar `docker compose up --build`.
3. Abrir `http://localhost:4200`.
4. Verificar la API en `http://localhost:8080/api/v1/system/health`.
5. Consultar las capturas y resultados en [`evidence/README.md`](evidence/README.md).
6. Consultar comandos de pruebas y cobertura en [`../TESTING.md`](../TESTING.md).

Las funciones de IA requieren `MISTRAL_API_KEY`. Sin esa variable, el CRUD, dashboard y
persistencia siguen disponibles, pero las llamadas al proveedor devuelven un error controlado.
