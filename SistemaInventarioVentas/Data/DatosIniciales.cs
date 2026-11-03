using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.Data;

// Solo se ejecuta en Development. Puede correr varias veces sin duplicar registros.
public static class DatosIniciales
{
    public static async Task EjecutarAsync(AppDbContext db, IConfiguration config)
    {
        if (config.GetValue<bool>("DatosIniciales:CrearUsuarios"))
        {
            await CrearUsuarioAsync(db, "admin", "Administrador del sistema", "admin@inventario.local",
                RolUsuario.Administrador, config["DatosIniciales:PasswordAdmin"]);
            await CrearUsuarioAsync(db, "empleado", "Empleado de mostrador", "empleado@inventario.local",
                RolUsuario.Empleado, config["DatosIniciales:PasswordEmpleado"]);
        }

        if (config.GetValue<bool>("DatosIniciales:CargarDatosDemo") && !await db.Productos.AnyAsync())
            await CargarDatosDemoAsync(db);
    }

    private static async Task CrearUsuarioAsync(AppDbContext db, string nombreUsuario, string nombreCompleto,
        string correo, RolUsuario rol, string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || await db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario))
            return;

        var usuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            Correo = correo,
            Rol = rol
        };
        usuario.PasswordHash = new PasswordHasher<Usuario>().HashPassword(usuario, password);
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
    }

    private static async Task CargarDatosDemoAsync(AppDbContext db)
    {
        var admin = await db.Usuarios.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Administrador);
        var empleado = await db.Usuarios.FirstOrDefaultAsync(u => u.Rol == RolUsuario.Empleado);
        if (admin == null || empleado == null)
            return;

        var hoy = DateTime.Today;

        var productos = new List<Producto>
        {
            Nuevo("Laptop 14\" Core i5", "Equipo portátil 8 GB RAM, 512 GB SSD", 9800, 12500, 14, "Pieza", "Computo", hoy.AddDays(-60)),
            Nuevo("Mouse inalámbrico", "Mouse óptico 2.4 GHz", 120, 249, 60, "Pieza", "Accesorios", hoy.AddDays(-60)),
            Nuevo("Teclado mecánico", "Teclado USB retroiluminado", 450, 799, 25, "Pieza", "Accesorios", hoy.AddDays(-58)),
            Nuevo("Monitor 24\" Full HD", "Monitor IPS HDMI", 1900, 2699, 30, "Pieza", "Computo", hoy.AddDays(-58)),
            Nuevo("Memoria USB 64 GB", "Unidad flash USB 3.0", 90, 159, 80, "Pieza", "Almacenamiento", hoy.AddDays(-55)),
            Nuevo("Disco duro externo 1 TB", "Disco portátil USB 3.0", 850, 1299, 18, "Pieza", "Almacenamiento", hoy.AddDays(-55)),
            Nuevo("Cable HDMI 2 m", "Cable HDMI 4K", 35, 89, 100, "Pieza", "Cables", hoy.AddDays(-50)),
            Nuevo("Impresora multifuncional", "Inyección de tinta con WiFi", 1700, 2399, 20, "Pieza", "Computo", hoy.AddDays(-50)),
            Nuevo("Papel bond carta", "Paquete de 500 hojas", 70, 119, 120, "Paquete", "Papelería", hoy.AddDays(-45)),
            Nuevo("Tinta negra", "Cartucho de tinta negra", 180, 289, 30, "Pieza", "Papelería", hoy.AddDays(-45)),
            Nuevo("Audífonos con micrófono", "Diadema con cable USB", 210, 399, 40, "Pieza", "Accesorios", hoy.AddDays(-40)),
            Nuevo("Router WiFi 6", "Router de doble banda", 780, 1190, 15, "Pieza", "Redes", hoy.AddDays(-40))
        };
        db.Productos.AddRange(productos);

        var clientes = new List<Cliente>
        {
            NuevoCliente("Público", "en general", null, "6421000000", "mostrador@inventario.local", "Mostrador", null),
            NuevoCliente("María", "López", "Hernández", "6441112233", "maria.lopez@correo.com", "Calle Sonora 120, Cd. Obregón", "LOHM900512AB3"),
            NuevoCliente("Carlos", "Ramírez", "Soto", "6442223344", "carlos.ramirez@correo.com", "Av. Náinari 455, Cd. Obregón", null),
            NuevoCliente("Fernanda", "Gómez", "Ruiz", "6443334455", "fernanda.gomez@correo.com", "Calle Yucatán 890, Cd. Obregón", "GORF950320XY1"),
            NuevoCliente("Luis", "Mendoza", null, "6444445566", "luis.mendoza@correo.com", "Blvd. Rodolfo Elías Calles 2100", null),
            NuevoCliente("Ana", "Torres", "Valdez", "6445556677", "ana.torres@correo.com", "Calle Hidalgo 33, Esperanza", null)
        };
        db.Clientes.AddRange(clientes);
        await db.SaveChangesAsync();

        foreach (var p in productos)
        {
            db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = p.IdProducto,
                IdUsuario = admin.IdUsuario,
                Fecha = p.FechaRegistro,
                Tipo = TipoMovimiento.Entrada,
                Cantidad = p.Existencia,
                Motivo = "Inventario inicial"
            });
        }

        // Ventas de los últimos 30 días con un generador fijo, para que los datos sean repetibles.
        var azar = new Random(2026);
        var metodos = Enum.GetValues<MetodoPago>();
        for (var i = 0; i < 28; i++)
        {
            var venta = new Venta
            {
                Fecha = hoy.AddDays(-azar.Next(0, 30)).AddHours(9 + azar.Next(0, 9)).AddMinutes(azar.Next(0, 60)),
                IdCliente = clientes[azar.Next(clientes.Count)].IdCliente,
                IdUsuario = azar.Next(2) == 0 ? admin.IdUsuario : empleado.IdUsuario,
                MetodoPago = metodos[azar.Next(metodos.Length)]
            };

            var elegidos = productos.OrderBy(_ => azar.Next()).Take(azar.Next(1, 4));
            foreach (var p in elegidos)
            {
                var cantidad = Math.Min(azar.Next(1, 5), p.Existencia);
                if (cantidad == 0)
                    continue;

                p.Existencia -= cantidad;
                venta.Detalles.Add(new DetalleVenta
                {
                    Producto = p,
                    Cantidad = cantidad,
                    PrecioUnitario = p.PrecioVenta,
                    Subtotal = p.PrecioVenta * cantidad
                });
            }

            if (venta.Detalles.Count == 0)
                continue;

            venta.Total = venta.Detalles.Sum(d => d.Subtotal);
            db.Ventas.Add(venta);
        }
        await db.SaveChangesAsync();

        // Movimientos de venta para que el historial de cada producto cuadre con sus existencias.
        var ventas = await db.Ventas.Include(v => v.Detalles).ToListAsync();
        foreach (var venta in ventas)
        {
            foreach (var d in venta.Detalles)
            {
                db.MovimientosInventario.Add(new MovimientoInventario
                {
                    IdProducto = d.IdProducto,
                    IdUsuario = venta.IdUsuario,
                    IdVenta = venta.IdVenta,
                    Fecha = venta.Fecha,
                    Tipo = TipoMovimiento.Venta,
                    Cantidad = d.Cantidad,
                    Motivo = $"Venta #{venta.IdVenta}"
                });
            }
        }

        // Algunos productos quedan con existencia baja para ver las alertas.
        foreach (var p in productos.Where(p => p.Nombre.StartsWith("Impresora") || p.Nombre.StartsWith("Monitor")))
        {
            var baja = Math.Min(p.Existencia, 4);
            var retiro = p.Existencia - baja;
            if (retiro <= 0)
                continue;

            p.Existencia = baja;
            db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = p.IdProducto,
                IdUsuario = admin.IdUsuario,
                Tipo = TipoMovimiento.Salida,
                Cantidad = retiro,
                Motivo = "Traslado a otra sucursal"
            });
        }
        await db.SaveChangesAsync();
    }

    private static Producto Nuevo(string nombre, string descripcion, decimal compra, decimal venta, int existencia,
        string unidad, string categoria, DateTime fecha) => new()
    {
        Nombre = nombre,
        Descripcion = descripcion,
        PrecioCompra = compra,
        PrecioVenta = venta,
        Existencia = existencia,
        UnidadMedida = unidad,
        Categoria = categoria,
        FechaRegistro = fecha
    };

    private static Cliente NuevoCliente(string nombre, string paterno, string? materno, string telefono,
        string correo, string direccion, string? rfc) => new()
    {
        Nombre = nombre,
        ApellidoPaterno = paterno,
        ApellidoMaterno = materno,
        Telefono = telefono,
        Correo = correo,
        Direccion = direccion,
        RFC = rfc
    };
}
