# Guion de exposición

Presentación: `Presentacion_Proyecto.pptx` (15 diapositivas, 12 a 15 minutos). Cada diapositiva trae sus notas del presentador; este guion las amplía.
No se memorizan párrafos: se usan las **ideas clave** y se explican con palabras propias.

| Integrante | Diapositivas | Tema | Tiempo |
|---|---|---|---|
| 1 | 1 a 5 | Introducción y arquitectura | 4 a 4:30 min |
| 2 | 6 a 8 | Base de datos, autenticación y seguridad | 3:30 a 4 min |
| 3 | 9 a 15 | Funcionamiento, demostración y cierre | 5 a 6 min (con demo de ~3 min) |

Antes de empezar: base con datos de ejemplo cargados (F5 en Development), dos ventanas de navegador (una con `admin`, otra en incógnito con `empleado`), SQL Server abierto en la tabla `Usuarios`, y el zoom del navegador al 110 %.

---

## Integrante 1 · Introducción y arquitectura

**Idea fundamental:** el sistema reemplaza libretas y hojas sueltas por una aplicación web con una base de datos central.

**Diapositiva 1 · Portada (20 s)**
> "Buenos días, maestra, compañeros. Somos [nombres]. Les presentamos nuestro Sistema de Control de Inventario y Ventas, hecho con ASP.NET Core MVC, Entity Framework Core y SQL Server. Yo explico el problema y la arquitectura, [Integrante 2] la base de datos y el login, y [Integrante 3] muestra el sistema funcionando."

**Diapositiva 2 · Problema (45 s)**
> "En muchos negocios pequeños el inventario se lleva en libretas o en Excel. Eso trae errores de captura, información perdida y consultas lentas. Por ejemplo: se vende la última pieza, nadie la descuenta, y al día siguiente se le promete a otro cliente algo que ya no existe."

**Diapositiva 3 · Objetivo (30 s)**
> "Nuestro objetivo es digitalizar, centralizar y consultar: registrar todo en el sistema, guardarlo en una sola base de datos y poder ver reportes al momento."

**Diapositiva 4 · Tecnologías (40 s)**
> "Usamos C# con .NET 8 y ASP.NET Core MVC. Para la base de datos, SQL Server, y para hablar con ella Entity Framework Core, que nos deja trabajar con clases en lugar de escribir SQL a mano. El diseño es HTML, Tailwind CSS y JavaScript; la única librería extra es Chart.js para las gráficas."

**Diapositiva 5 · MVC (1:15 min)**
> "MVC separa la aplicación en tres partes. El **Modelo** son las clases con los datos, como Producto o Venta. La **Vista** es lo que ve el usuario, archivos Razor con HTML. El **Controlador** recibe la petición, pide los datos a la base y decide qué vista mostrar. Por ejemplo, cuando abres Productos, el ProductosController consulta SQL Server, recibe una lista de objetos Producto y se la manda a la vista Index."

**Mostrar la aplicación:** no es necesario; si se desea, abrir en Visual Studio la carpeta `Controllers` y `Views`.

**Transición:** "Ya vimos cómo está organizada la aplicación. [Integrante 2] les explica dónde y cómo se guarda la información."

**Preguntas probables**
- *¿Por qué MVC?* Porque separa responsabilidades: cada parte se modifica sin romper las otras, y es el patrón que pide el documento técnico.
- *¿Qué versión de .NET usan?* .NET 8, versión de soporte a largo plazo.
- *¿Por qué Tailwind?* Es un requisito del documento; permite un diseño consistente y responsive con clases en el HTML.

---

## Integrante 2 · Base de datos, autenticación y seguridad

**Idea fundamental:** los datos están en SQL Server, se acceden con EF Core, y nadie entra sin usuario y contraseña; lo que puede hacer depende de su rol.

**Diapositiva 6 · Modelo entidad-relación (1:15 min)**
> "La base tiene las cinco entidades del documento: Producto, Cliente, Usuario, Venta y DetalleVenta, más una tabla de apoyo, MovimientoInventario. Un cliente tiene muchas ventas, un usuario registra muchas ventas, cada venta tiene varios detalles y cada detalle apunta a un producto. El detalle guarda el precio con el que se vendió, así que si el producto cambia de precio la venta vieja no cambia. Las relaciones son claves foráneas y se definen en la clase AppDbContext."

**Diapositiva 7 · Autenticación (1:15 min)**
> "El usuario escribe su nombre y contraseña. El AccountController busca ese usuario en SQL Server con Entity Framework y trae el hash guardado. Con el PasswordHasher de ASP.NET Core se verifica la contraseña. Si es correcta y la cuenta está activa, se crea una cookie de sesión con el id y el rol. Si algo falla mostramos 'usuario o contraseña incorrectos' sin decir cuál falló."

**Mostrar la aplicación (opcional, 20 s):** login con una contraseña incorrecta.

**Diapositiva 8 · Seguridad (1 min)**
> "Las contraseñas se guardan como hash, no en texto; lo pueden ver en la tabla Usuarios. Los permisos se revisan en el servidor con el atributo Authorize por rol: si un empleado escribe /Usuarios a mano, lo manda a acceso denegado. Los formularios llevan token antifalsificación, validamos en el navegador y en el servidor, y la base tiene restricciones, por ejemplo que la existencia no pueda ser negativa."

**Mostrar la aplicación:** tabla `Usuarios` en SQL Server (columna `PasswordHash`).

**Transición:** "Con esto ya sabemos cómo se protege el sistema. [Integrante 3] nos lo muestra funcionando."

**Preguntas probables**
- *¿Qué es un hash?* Un resultado irreversible de la contraseña; no se descifra, se compara el hash de lo que se escribe con el guardado.
- *¿Cómo sabe el sistema quién inició sesión?* Por la cookie de sesión: contiene el id, el nombre y el rol del usuario, y ASP.NET la lee en cada petición.
- *¿Dónde se guardan los usuarios?* En la tabla Usuarios de SQL Server.
- *¿Qué es una clave foránea?* Una columna que apunta a la clave primaria de otra tabla, por ejemplo Venta.IdCliente.

---

## Integrante 3 · Funcionamiento, demostración y cierre

**Idea fundamental:** todo lo que se ve sale de la base de datos y todo lo que se captura se guarda en ella.

**Diapositiva 9 · Dashboard (45 s)**
> "Este es el dashboard. Arriba: ventas del día, ventas del mes, productos y clientes. Abajo: ventas recientes, los más vendidos de los últimos 30 días y los productos con existencias bajas, de 10 piezas o menos. Todo sale de consultas a la base; si una venta se cancela, los números cambian."

**Diapositiva 10 · Productos y clientes (1 min)**
> "Productos y clientes tienen CRUD completo, con búsqueda y filtros. Las validaciones evitan precios negativos o correos inválidos. Si un producto o cliente ya tiene ventas no se borra, se desactiva, para no perder el historial. La existencia no se edita a mano: cambia con ventas o con movimientos de entrada y salida que quedan registrados."

**Diapositiva 11 · Ventas (1:15 min)**
> "Se elige el cliente, se agregan productos y se ve el total. El usuario sale de la sesión. Al confirmar, el servidor vuelve a calcular todo y guarda Venta, Detalles y descuento de inventario en una transacción: si algo falla, no se guarda nada. Ejemplo: 2 mouse de $249 y 3 cables de $89 dan $765, y el stock baja 2 y 3. Si piden más de lo que hay, se rechaza. Si el administrador cancela, las piezas regresan y la venta queda Cancelada."

**Diapositiva 12 · Reportes (45 s)**
> "Los reportes son solo para el administrador. Se elige un rango de fechas, o los botones Hoy y Este mes, y se ven ingresos, operaciones, ventas por día y por mes, más vendidos e inventario. Las canceladas no cuentan."

**Diapositiva 13 · Demostración (≈3 min)** — Orden exacto:
1. Login con contraseña incorrecta → mensaje de error.
2. Login como `admin` → dashboard.
3. Productos → crear "Producto de prueba" con existencia 10 → aparece en la lista.
4. Ventas → nueva venta: cliente, 2 productos, total en pantalla → Confirmar.
5. Detalle de la venta → volver a Productos y mostrar que bajó la existencia.
6. Nueva venta pidiendo más cantidad de la que hay → se rechaza.
7. Cancelar la venta → confirmación → la existencia regresa.
8. Reportes → rango de fechas y gráficas.
9. Ventana de `empleado` → escribir /Usuarios → acceso denegado.
10. (Opcional) SQL Server → tabla Usuarios con hashes.

**Diapositiva 14 · Cumplimiento (45 s)**
> "Aquí relacionamos el documento técnico con lo que implementamos: las cinco entidades, el login con EF Core y SQL Server, el CRUD de productos, clientes, usuarios y ventas, el control de existencias, el dashboard y los reportes."

**Diapositiva 15 · Conclusiones (45 s)**
> "El sistema cumple el objetivo: el negocio controla inventario y ventas con información confiable. Aprendimos MVC, trabajar con una base de datos real con Entity Framework, autenticación con roles, y por qué importan las transacciones. A futuro: tickets, exportar reportes a Excel o PDF, un portal para clientes y paginación. Muchas gracias, quedamos atentos a sus preguntas."

**Preguntas probables**
- *¿Qué pasa si piden más productos de los que hay?* La venta se rechaza y no se guarda nada; el descuento usa `UPDATE ... WHERE Existencia >= cantidad` y además hay una restricción CHECK.
- *¿Cómo se descuenta el inventario?* Dentro de la transacción de la venta, con un UPDATE por cada producto.
- *¿Cómo se generan los reportes?* Con consultas LINQ (GroupBy, Sum, Count) sobre Ventas y DetalleVenta filtradas por fecha y por estado Completada; las gráficas se dibujan con Chart.js.
- *¿Qué pasa si cancelan una venta?* Se devuelven las existencias, se registra un movimiento de cancelación y la venta queda como Cancelada; no se borra.
- *¿Por qué el total se calcula en el servidor?* Porque no se debe confiar en datos que vienen del navegador.

Más preguntas con respuestas completas: `Preguntas_y_Respuestas.md`.
