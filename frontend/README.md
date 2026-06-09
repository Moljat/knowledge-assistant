# Frontend

Aplicación Angular 20 con componentes standalone y Angular Material.

## Estructura

- `app.config.ts`: proveedores y configuración transversal.
- `features/dashboard`: indicadores y chat global.
- `features/records`: CRUD, búsqueda, filtros y panel de IA.
- `knowledge-record.service.ts`: único cliente HTTP usado por los componentes.

## Comandos

```powershell
npm ci --workspaces=false
npm start
npm run build
npm run test:ci
```

En desarrollo, `proxy.conf.json` redirige `/api` al backend local.
