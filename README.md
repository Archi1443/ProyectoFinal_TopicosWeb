# Sistema de Control de Inventario y Ventas

Proyecto de **Tópicos Avanzados de Programación Web** · Instituto Tecnológico Superior de Cajeme (ITESCA).
Aplicación web con ASP.NET Core MVC (.NET 8), Entity Framework Core y SQL Server para administrar productos, clientes, usuarios, ventas e inventario, con login por roles, dashboard y reportes.

## Integrantes

- Lilian Itzel García Bojorquez · 24130552
- Beyra Carolina Morales Leyva · 21130168
- Jesus Arturo Rodriguez Sanchez · 21130376

Entrega: `Entrega/` (lista de integrantes y enlace, `script.sql` de la base de datos y cómo generar `Backup.bak`).

## Requisitos

| Herramienta | Versión | Notas |
|---|---|---|
| Visual Studio | 2022 (17.13 o posterior) o 2026 | Carga de trabajo **ASP.NET y desarrollo web** y componente **Almacenamiento y procesamiento de datos** (incluye SQL Server LocalDB). La versión 17.13+ es necesaria para abrir archivos `.slnx`. |
| .NET SDK | 8.0 | Se instala con Visual Studio. |
| SQL Server | LocalDB (incluido) o SQL Server Express | No se necesita Docker, Node.js ni Visual Studio Code. |

## Ejecutar desde Visual Studio

1. Abre `SistemaInventarioVentas.slnx` (doble clic o *Archivo > Abrir > Proyecto o solución*).
2. Visual Studio restaura los paquetes NuGet automáticamente. Si no, clic derecho en la solución > **Restaurar paquetes NuGet**.
3. **Configura la conexión** (ver la siguiente sección). Con LocalDB no hay que cambiar nada.
4. **Aplica las migraciones**: *Herramientas > Administrador de paquetes NuGet > Consola del Administrador de paquetes* y ejecuta:
   ```powershell
   Update-Database
   ```
   (Con el proyecto `SistemaInventarioVentas` como proyecto predeterminado.) En modo *Development* la aplicación también aplica las migraciones sola al arrancar, así que este paso es opcional.
5. Presiona **F5**. Se abre el navegador en el Login.

Alternativa por línea de comandos, desde la carpeta de la solución:

```bash
dotnet tool restore
dotnet ef database update --project SistemaInventarioVentas
dotnet run --project SistemaInventarioVentas --launch-profile https
```

## Cadena de conexión

Está en `SistemaInventarioVentas/appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=InventarioVentasDB;Trusted_Connection=True;TrustServerCertificate=True"
```

- **LocalDB** (predeterminado): no requiere cambios.
- **SQL Server Express**: cambia `Server` por `.\\SQLEXPRESS`.
- **SQL Server con usuario y contraseña**: no escribas la contraseña en el repositorio. Usa *clic derecho en el proyecto > Administrar secretos de usuario* y pega:
  ```json
  { "ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=InventarioVentasDB;User Id=mi_usuario;Password=mi_contraseña;TrustServerCertificate=True" } }
  ```

Para ver la base: *Vista > Explorador de objetos de SQL Server > (localdb)\MSSQLLocalDB > Bases de datos > InventarioVentasDB*.

## Usuarios y datos de demostración

En modo *Development* (F5) la aplicación crea, **solo si no existen**, estos usuarios y datos de ejemplo (12 productos, 6 clientes y 28 ventas). Se puede reiniciar sin duplicar nada.

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin#2026` | Administrador |
| `empleado` | `Empleado#2026` | Empleado |

Son contraseñas **solo de demostración**, definidas en `appsettings.Development.json`, que no se usa en producción. En la base solo se guarda su hash.

Para empezar con la base vacía: pon `"CargarDatosDemo": false` en `appsettings.Development.json` antes del primer arranque (o elimina la base con `Drop-Database` y vuelve a ejecutar). Para no crear usuarios, `"CrearUsuarios": false`.

## Roles

| Función | Administrador | Empleado |
|---|:---:|:---:|
| Dashboard | Todo el negocio | Solo sus ventas |
| Consultar productos e inventario | Sí | Sí |
| Crear, editar, desactivar y eliminar productos; movimientos de inventario | Sí | No |
| Consultar clientes e historial | Sí | Sí |
| Crear, editar y eliminar clientes | Sí | No |
| Registrar ventas | Sí | Sí |
| Consultar ventas | Todas | Solo las que registró |
| Modificar y cancelar ventas | Sí | No |
| Reportes | Sí | No |
| Administrar usuarios | Sí | No |

Los permisos se validan en el servidor con `[Authorize(Roles = ...)]`; ocultar botones es solo estética.

## Tailwind CSS

El CSS ya compilado está en `wwwroot/css/site.css`, así que **para ejecutar el proyecto no hace falta compilar nada**. Solo si cambias clases de Tailwind en las vistas:

1. Descarga el ejecutable independiente de Tailwind v3.4.17 (no requiere Node.js) desde <https://github.com/tailwindlabs/tailwindcss/releases/tag/v3.4.17> (`tailwindcss-windows-x64.exe`) y renómbralo a `tailwindcss.exe` dentro de `SistemaInventarioVentas/`.
2. Desde esa carpeta:
   ```powershell
   .\tailwindcss.exe -c tailwind.config.js -i Styles/tailwind.css -o wwwroot/css/site.css --minify
   ```
   (agrega `--watch` mientras desarrollas).

`tailwindcss.exe` está en `.gitignore`; no se sube al repositorio.

## Paquetes NuGet

Se instalan al restaurar; para instalarlos a mano: *Administrar paquetes NuGet* o

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.33
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.33
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.33
```

| Paquete | Para qué se usa |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | Proveedor de EF Core para SQL Server. |
| `Microsoft.EntityFrameworkCore.Tools` | Comandos `Add-Migration` y `Update-Database` en la Consola del Administrador de paquetes. |
| `Microsoft.EntityFrameworkCore.Design` | Generación de migraciones en tiempo de diseño. |

La autenticación por cookies, el hash de contraseñas (`PasswordHasher`) y la protección antifalsificación ya vienen con ASP.NET Core. Además, la herramienta local `dotnet-ef` (`.config/dotnet-tools.json`) permite usar `dotnet ef` sin instalarla globalmente.

Frontend: Tailwind (compilado a un solo CSS) y **Chart.js 4.4.7** (`wwwroot/js/chart.umd.js`, copia local, solo para las gráficas de reportes). No hay jQuery ni CDN.

## Estructura

```
SistemaInventarioVentas.slnx
SistemaInventarioVentas/
├── Controllers/    Reciben la petición, consultan la BD con EF Core y devuelven una vista
├── Models/         Entidades que EF Core convierte en tablas (Producto, Cliente, Usuario, Venta, DetalleVenta, MovimientoInventario)
├── ViewModels/     Datos que necesita una pantalla (login, dashboard, reportes, venta)
├── Views/          Pantallas Razor (.cshtml) agrupadas por controlador; Shared = layout y partials
├── Data/           AppDbContext (puente con SQL Server) y DatosIniciales (usuarios y datos demo)
├── Services/       VentaService (registrar y cancelar ventas con transacción)
├── Migrations/     Historial de cambios de la base de datos
├── Styles/         Entrada de Tailwind
├── wwwroot/        css/site.css, js/ (site.js, venta.js, reportes.js, chart.umd.js)
├── Properties/     launchSettings.json (perfiles de F5)
└── Program.cs      Configuración: EF Core, cookies, autorización, rutas
```

Diagramas (entidad-relación, MVC y flujo del login): `docs/Diagramas.md`.
Requisitos cumplidos y evidencias: `docs/Checklist_Requisitos.md`.

## Reglas de negocio importantes

- El **servidor** calcula subtotales y total con el precio vigente del producto; el navegador solo muestra una vista previa.
- Una venta se guarda en una **transacción**: Venta, DetalleVenta, descuento de existencias y movimientos de inventario se guardan juntos o no se guarda nada.
- El descuento usa `UPDATE ... WHERE Existencia >= cantidad`, así que dos ventas simultáneas de las últimas unidades no pueden dejar el inventario en negativo. Además hay una restricción `CHECK (Existencia >= 0)` en la tabla.
- **Cancelar** una venta devuelve las existencias y conserva el historial. Las ventas canceladas no suman a ingresos ni a "más vendidos".
- Productos, clientes o usuarios con historial no se borran: se **desactivan**.
- Existencia baja = 10 unidades o menos (`Producto.StockBajo`).

## Errores frecuentes

| Problema | Solución |
|---|---|
| Visual Studio no abre el `.slnx` | Actualiza a Visual Studio 2022 17.13 o posterior (o 2026). También puedes abrir el `.csproj` directamente. |
| `Cannot open database` / `A network-related error` | Verifica que LocalDB esté instalado (`sqllocaldb info` en una terminal). Si usas Express, revisa `Server` en la cadena de conexión. |
| `Login failed for user` | Credenciales SQL incorrectas: configúralas en *Administrar secretos de usuario*. |
| `Update-Database` no se reconoce | Selecciona la **Consola del Administrador de paquetes** (no la terminal) y verifica que el paquete `EntityFrameworkCore.Tools` esté restaurado. |
| El login dice "Usuario o contraseña incorrectos" con los usuarios demo | La base ya existía sin esos usuarios o con otro entorno: ejecuta en *Development* (F5) y revisa que `CrearUsuarios` sea `true`. |
| Los estilos se ven sin formato | Falta `wwwroot/css/site.css`: recompílalo con el comando de Tailwind. |
| Advertencia de certificado HTTPS | Acepta el certificado de desarrollo de Visual Studio, o usa el perfil `http`. |
