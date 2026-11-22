# Inventario Claro

Frontend Angular para iniciar sesión y administrar el catálogo de productos y sus lotes por proveedor.

## Desarrollo

1. Inicia el backend en `https://localhost:7013` (o ejecuta el perfil HTTPS del proyecto Backend).
2. Desde esta carpeta, ejecuta `npm install` si aún no están instaladas las dependencias y luego `npm start`.
3. Abre `http://127.0.0.1:62175`. El proxy de desarrollo reenvía `/api` al backend para evitar una configuración de CORS local.

El inicio de sesión usa `POST /api/auth/login` y guarda el token durante la sesión del navegador. Los productos, proveedores y lotes se consultan y actualizan a través de los endpoints protegidos del backend.

El modelo actual identifica cada lote por producto y proveedor; por eso solo permite un precio y un stock por cada combinación. El número de lote no forma parte del contrato de API existente.

## Compilación y pruebas

```bash
npm run build
npm test
```
