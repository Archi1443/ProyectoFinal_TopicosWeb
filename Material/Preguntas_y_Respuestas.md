# Preguntas y respuestas

Todas las respuestas corresponden al código del proyecto (entre paréntesis, el archivo donde comprobarlo).

## Cuestionario técnico

1. **¿Qué es ASP.NET Core MVC?**
   Un framework de Microsoft para hacer aplicaciones web separando Modelo (datos), Vista (pantalla) y Controlador (lógica de cada petición).
2. **¿Por qué utilizamos Entity Framework Core?**
   Porque permite trabajar con clases de C# en lugar de escribir SQL a mano, genera consultas parametrizadas (evita inyección SQL) y crea la base con migraciones. Es lo que pide el documento técnico.
3. **¿Qué hace DbContext?**
   `AppDbContext` es el puente entre el código y SQL Server: tiene un `DbSet` por tabla, define relaciones y restricciones, y guarda los cambios con `SaveChangesAsync` (`Data/AppDbContext.cs`).
4. **¿Cuál es la diferencia entre modelo y controlador?**
   El modelo describe los datos y sus validaciones (`Models/Producto.cs`); el controlador recibe la petición, usa el modelo y decide qué vista devolver (`Controllers/ProductosController.cs`).
5. **¿Cuál es la diferencia entre Model y ViewModel?**
   Model = una tabla de la base. ViewModel = los datos que necesita una pantalla (login, dashboard, reportes), que no siempre son una tabla (`ViewModels/`).
6. **¿Cómo sabe el sistema quién inició sesión?**
   Al iniciar sesión se crea una cookie de autenticación con claims (id, nombre, rol). En cada petición ASP.NET la lee y `User` contiene esos datos; `User.ObtenerIdUsuario()` devuelve el id (`Controllers/AccountController.cs`, `Services/ClaimsExtensions.cs`).
7. **¿Dónde se almacenan los usuarios?**
   En la tabla `Usuarios` de SQL Server (`Models/Usuario.cs`).
8. **¿Cómo se verifica la contraseña?**
   Con `PasswordHasher.VerifyHashedPassword`, que compara lo escrito contra el `PasswordHash` guardado. Nunca se guarda ni se compara texto plano.
9. **¿Qué es un hash?**
   Un resultado irreversible de aplicar un algoritmo a la contraseña (con sal aleatoria). No se puede "descifrar"; solo comparar.
10. **¿Qué sucede cuando se vende un producto?**
    `VentaService.RegistrarAsync` valida, calcula precios y total, inicia una transacción, guarda Venta y DetalleVenta, descuenta la existencia de cada producto, registra movimientos de inventario y confirma la transacción (`Services/VentaService.cs`).
11. **¿Cómo se descuenta el inventario?**
    Con `UPDATE Productos SET Existencia = Existencia - n WHERE IdProducto = x AND Existencia >= n` (ExecuteUpdateAsync). Si no se afecta ninguna fila, no había suficientes y se revierte todo.
12. **¿Por qué usamos SQL Server?**
    Es la base de datos relacional que exige el documento técnico; se integra directamente con EF Core y trae LocalDB para desarrollo.
13. **¿Qué es una clave primaria y una foránea?**
    Primaria: identifica de forma única cada fila (IdProducto). Foránea: apunta a la primaria de otra tabla (Venta.IdCliente → Cliente.IdCliente).
14. **¿Qué ocurre si un empleado intenta entrar al módulo de usuarios?**
    El `UsuariosController` tiene `[Authorize(Roles = Roles.Administrador)]`; el servidor lo redirige a `/Account/AccessDenied`, aunque escriba la URL manualmente.
15. **¿Qué pasa si un usuario solicita más productos de los disponibles?**
    En pantalla el carrito no deja agregar más que la existencia; si aun así llegara al servidor, la venta se rechaza con "No hay existencias suficientes" y no se guarda nada (transacción + CHECK `Existencia >= 0`).
16. **¿Cómo se generan los reportes?**
    `ReportesController` consulta Ventas y DetalleVenta con LINQ (filtro por fechas, estado Completada, GroupBy, Sum y Count). Los datos pasan a la vista como JSON y `reportes.js` dibuja las gráficas con Chart.js.
17. **¿Qué es una migración?**
    Un cambio versionado de la estructura de la base. `Add-Migration` genera el código a partir de las clases y `Update-Database` lo aplica en SQL Server (`Migrations/`).
18. **¿Para qué sirve Program.cs?**
    Configura la aplicación: conexión a SQL Server, servicios, autenticación por cookies, autorización, localización y rutas; es lo primero que se ejecuta.
19. **¿Qué es una transacción y por qué la usamos al vender?**
    Un conjunto de operaciones de todo o nada. Si falla el descuento de un producto, no queda una venta guardada a medias.
20. **¿Qué pasa al cancelar una venta?**
    Solo el administrador puede. Se suman de vuelta las cantidades a las existencias, se registra un movimiento de cancelación y la venta queda con estado Cancelada; no se elimina.
21. **¿Por qué algunos registros se desactivan en vez de borrarse?**
    Porque tienen historial (ventas, movimientos). Las claves foráneas están en Restrict y la aplicación desactiva en lugar de borrar.
22. **¿Qué es el token antifalsificación?**
    Un valor secreto que va en cada formulario POST y se valida en el servidor (`AutoValidateAntiforgeryTokenAttribute` en Program.cs); evita que otro sitio envíe formularios en nombre del usuario.
23. **¿Por qué se valida en el navegador y en el servidor?**
    El navegador da respuesta rápida (atributos `required`, `min`, `type=email`), pero se puede saltar; la validación del servidor (DataAnnotations) y las restricciones de SQL Server son la protección real.
24. **¿Dónde está la cadena de conexión?**
    En `appsettings.json` (`DefaultConnection`, LocalDB por defecto). Con usuario y contraseña se usan los secretos de usuario para no subirla al repositorio.
25. **¿Qué diferencia hay entre los roles?**
    Administrador: todo. Empleado: consulta productos, inventario y clientes, registra ventas y ve solo las suyas (tabla en el README).
26. **¿Qué es Razor?**
    La sintaxis que mezcla HTML con C# en los archivos `.cshtml` (`@model`, `@foreach`, `@p.Nombre`).

## Ejercicios prácticos y respuestas esperadas

1. **Localiza el controlador del Login y explica sus métodos.**
   `Controllers/AccountController.cs`. `Login` (GET) muestra el formulario; `Login` (POST) valida campos, consulta el usuario con EF Core, verifica el hash, comprueba que esté activo, crea la sesión y redirige; `Logout` (POST) cierra la sesión; `AccessDenied` muestra el aviso de permisos.
   *Criterio:* menciona los 4 métodos y el orden de validaciones del POST.
2. **Crea un producto y encuéntralo en SQL Server.**
   Productos > Nuevo producto, guardar. En el Explorador de objetos de SQL Server: `SELECT * FROM Productos WHERE Nombre = '...'`. Además existe un movimiento "Inventario inicial" en `MovimientosInventario`.
   *Criterio:* el registro aparece con el IdProducto asignado.
3. **Modifica un cliente y verifica el cambio.**
   Clientes > Editar, cambiar teléfono, guardar. `SELECT Telefono FROM Clientes WHERE IdCliente = ...` muestra el nuevo valor.
   *Criterio:* el cambio se ve en la página y en la tabla.
4. **Registra una venta y comprueba el descuento.**
   Anotar existencia, vender 2 unidades, volver a consultar: baja en 2. En `MovimientosInventario` hay un movimiento tipo `Venta`. Al cancelar la venta, la existencia regresa.
   *Criterio:* existencia final = inicial − 2; total = suma de subtotales.
5. **¿Qué archivos participan al abrir Reportes?**
   `ReportesController.cs` (consultas), `ViewModels/ReporteViewModel.cs` (datos), `Views/Reportes/Index.cshtml` (pantalla), `Views/Shared/_Layout.cshtml` (marco), `wwwroot/js/reportes.js` y `chart.umd.js` (gráficas), `Data/AppDbContext.cs` y `Models/Venta.cs`, `DetalleVenta.cs`, `Producto.cs` (datos), `Program.cs` (autorización).
   *Criterio:* controlador, vista, ViewModel, JS y contexto.

## Preguntas de la profesora (simulación)

| Pregunta | Respuesta corta |
|---|---|
| ¿Por qué no guardan la contraseña tal cual? | Si alguien accede a la base vería todas las contraseñas; con hash no se pueden recuperar. |
| ¿Cómo evitan que alguien vea una página sin iniciar sesión? | Política de autorización global: toda página exige sesión salvo el Login (`FallbackPolicy` en Program.cs). |
| ¿Y si desactivan al usuario mientras está conectado? | En cada petición se comprueba que siga Activo; si no, se cierra la sesión (`OnValidatePrincipal`). |
| ¿Qué pasa si dos personas venden la última pieza a la vez? | El `UPDATE ... WHERE Existencia >= cantidad` solo lo logra una; la otra recibe un error y su transacción se revierte. |
| ¿Por qué guardan el precio en DetalleVenta? | Para que el historial no cambie si el precio del producto cambia después. |
| ¿Por qué decimal y no float para dinero? | `float` tiene errores de redondeo; `decimal` es exacto para importes. |
| ¿Cómo evitan la inyección SQL? | EF Core genera consultas parametrizadas; no se concatena texto del usuario. |
| ¿Qué mejoraría del sistema? | Tickets, exportar a Excel/PDF, paginación, portal de clientes, recuperación de contraseña. |
| ¿Cómo se ejecuta en otra computadora? | Abrir el `.slnx` en Visual Studio, configurar la conexión, `Update-Database` y F5 (README). |

## Errores frecuentes al exponer

- Llamar "cifrado" al hash: no se descifra, se compara.
- Decir que ocultar botones da seguridad: la seguridad real es `[Authorize]` en el servidor.
- Confundir Model con ViewModel.
- Decir que "los datos son de ejemplo en la vista": todos los datos salen de SQL Server.
- No saber dónde está la cadena de conexión (`appsettings.json`).
