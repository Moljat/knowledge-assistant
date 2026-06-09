# Frontend

Aplicación Angular 20 con componentes standalone y Angular Material.

## Estructura prevista

- `core`: configuración transversal, interceptores y clientes.
- `shared`: componentes reutilizables.
- `features/dashboard`: indicadores principales.
- `features/records`: CRUD, búsqueda y filtros.
- `features/ai`: resumen, clasificación, recomendaciones y preguntas.

## Comandos

```powershell
npm install
npm start
npm run build
npm run test:ci
```

En desarrollo, `proxy.conf.json` redirige `/api` al backend local.
