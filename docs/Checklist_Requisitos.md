# Checklist de requisitos y evidencias

**Cómo se verificó:** la aplicación se ejecutó con `dotnet run` (.NET 8, entorno Development) contra **SQL Server 2025 Developer** real en Linux, partiendo de una base nueva (migración `Inicial` + datos demo). Las pruebas funcionales se ejecutaron con peticiones HTTP reales (con token antifalsificación) y consultas directas a SQL Server para comprobar el resultado (78 comprobaciones, todas correctas), y la interfaz se probó con un navegador Chromium (carrito, menú móvil, validación nativa, diálogos de confirmación; 16 comprobaciones correctas).

**No se pudo verificar en el entorno de desarrollo** (hay que comprobarlo en tu computadora): abrir el `.slnx` en Visual Studio, F5 con los perfiles `https`/`IIS Express`, **LocalDB** como servidor, y `Update-Database` desde la Consola del Administrador de paquetes. La compilación de la solución `.slnx` con `dotnet build` sí se verificó (0 errores, 0 advertencias).

> Nota: los archivos `Documento_Tecnico_Etapa1_Empresarial 2.pdf` y la captura de la rúbrica no estaban en el entorno; los requisitos se tomaron de la descripción del encargo. Compara esta lista con el PDF antes de entregar.

## Requisitos generales

| Requisito | Estado | Dónde / cómo se comprobó |
|---|---|---|
| ASP.NET Core MVC (.NET 8) | Verificado | `dotnet build` sin errores; app en ejecución |
| Solución `.slnx` + `.csproj` + `launchSettings.json` | Compila (verificado); F5 en Visual Studio pendiente en tu PC | `SistemaInventarioVentas.slnx` |
| EF Core + SQL Server + migraciones | Verificado | Migración `Inicial` aplicada desde cero con `MigrateAsync` |
| Tailwind CSS | Verificado | `wwwroot/css/site.css` compilado con el CLI de Tailwind 3.4.17 |
| Diagrama E-R, MVC y flujo de autenticación | Entregado | `docs/Diagramas.md` y diapositivas |
| README de instalación | Entregado | `README.md` |

## Rúbrica del Login

| Criterio (peso) | Estado | Evidencia |
|---|---|---|
| Paquetes NuGet de EF Core (10 %) | Verificado | `SistemaInventarioVentas.csproj`: `Microsoft.EntityFrameworkCore.SqlServer`, `.Tools`, `.Design` (8.0.33) |
| Conexión del Login con SQL Server (50 %) | Verificado | Cadena `DefaultConnection` en `appsettings.json`; `AppDbContext` registrado en `Program.cs`; `AccountController` consulta `Usuarios` con EF Core; tabla `Usuarios` creada por la migración |
| Formulario de Login (40 %) | Verificado | `Views/Account/Login.cshtml`: campos obligatorios, mostrar/ocultar contraseña, mensajes de error, estado de envío, responsive, foco visible |

Evidencias concretas comprobadas:
- Contraseñas almacenadas como hash (`AQAAAA...`, nunca texto plano): `SELECT NombreUsuario, LEFT(PasswordHash,20) FROM Usuarios`.
- Login exitoso (admin y empleado) y rechazado (contraseña incorrecta, usuario inexistente, campos vacíos, usuario inactivo, POST sin token antifalsificación).
- Cierre de sesión; sesión inválida redirige al Login; usuario desactivado pierde la sesión abierta.
- Restricciones por rol: el empleado es redirigido a "Acceso denegado" en `/Usuarios`, `/Reportes`, `/Productos/Create`, `/Clientes/Create` y en POST directos.

## Módulos

| Requisito | Estado | Comprobación |
|---|---|---|
| CRUD Productos (buscar, categoría, desactivar/eliminar, entradas y salidas, sin existencia negativa) | Verificado | Crear, editar, buscar, filtrar, movimiento +10 / −3, salida excesiva rechazada, desactivar, eliminar sin ventas, no eliminar con ventas |
| CRUD Clientes (RFC opcional, validaciones, historial) | Verificado | Validación de teléfono/correo/RFC, alta sin RFC, edición, búsqueda, historial; con ventas se desactiva |
| CRUD Usuarios | Verificado | Alta, duplicado rechazado, contraseña mínima, edición, inactivo no inicia sesión, no se quita su propio rol |
| Ventas: registrar, total en servidor, descuento de inventario, transacción | Verificado | Total = Σ precio × cantidad; existencias −2 y −3; movimientos de venta; falla en una línea revierte todo |
| Rechazo por inventario insuficiente, cantidades 0, negativas, vacías, JSON inválido | Verificado | Sin cambios en `Ventas` ni `Productos` |
| Venta simultánea de las últimas unidades | Verificado | Dos ventas concurrentes de 2 con existencia 3: una procede, otra se rechaza, existencia final 1 |
| Modificar venta (cliente, pago, observaciones) y cancelar con restauración | Verificado | Existencias vuelven al valor inicial; doble cancelación no duplica; historial conservado |
| Empleado ve solo sus ventas | Verificado | Detalle de venta ajena → 403 |
| Dashboard con datos reales | Verificado | Indicadores, recientes, más vendidos, stock bajo; estados vacíos implementados |
| Reportes (diario, mensual, rango, ingresos, más vendidos, inventario, bajo stock, operaciones) | Verificado | Total de ingresos coincide con `SUM(Total)` en SQL; rango vacío muestra estado vacío |
| Datos de demostración idempotentes (solo Development) | Verificado | 2 usuarios, 12 productos, 6 clientes, 28 ventas; re-ejecutar no duplica |
| Responsive | Verificado | Chromium 375 px: sin scroll horizontal en 10 pantallas; menú móvil funciona |
| Validaciones y mensajes de confirmación | Verificado | HTML5 + servidor; diálogo de confirmación para acciones destructivas |

## Limitaciones conocidas

- El portal público para clientes no se implementó (el documento lo marca como condicional); el historial de compras se consulta internamente.
- Las tablas no tienen paginación (la lista de ventas se limita a las 200 más recientes).
- Sin recuperación de contraseña ni bloqueo por intentos fallidos.
- Las pruebas automatizadas usadas en la verificación no forman parte del repositorio.

## Material académico

`Material/Presentacion_Proyecto.pptx` (15 diapositivas con notas), `Material/Capacitacion_Equipo.pptx` (61 diapositivas con notas), `Material/Guion_Exposicion.md` y `Material/Preguntas_y_Respuestas.md` (26 preguntas, 5 ejercicios). Los `.pptx` son editables; los nombres de los integrantes y la fecha están como `[nombre]` / `[fecha]` en la portada. Las capturas son de la aplicación real con los datos demo.
