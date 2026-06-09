# Pruebas de frontend

Las pruebas unitarias y de integración de UI se mantienen junto a componentes y servicios con
archivos `*.spec.ts`. Cubren navegación, dashboard, CRUD, filtros, formularios, panel de IA,
chat global y cliente HTTP.

La ejecución headless se realiza con:

```powershell
cd frontend
node scripts/resolve-browser.mjs --watch=false --browsers=ChromeHeadless --code-coverage
```
