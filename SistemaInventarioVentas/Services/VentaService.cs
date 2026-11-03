using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.Services;

public record LineaVenta(int IdProducto, int Cantidad);

public record ResultadoVenta(bool Exitoso, string? Error, int IdVenta);

public class VentaService
{
    private readonly AppDbContext _db;

    public VentaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResultadoVenta> RegistrarAsync(int idCliente, int idUsuario, MetodoPago metodoPago,
        string? observaciones, List<LineaVenta> lineas)
    {
        if (lineas.Count == 0)
            return new ResultadoVenta(false, "Agrega al menos un producto a la venta.", 0);

        if (lineas.Any(l => l.Cantidad <= 0))
            return new ResultadoVenta(false, "Las cantidades deben ser mayores que cero.", 0);

        var cliente = await _db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.Estado == EstadoRegistro.Activo);
        if (cliente == null)
            return new ResultadoVenta(false, "Selecciona un cliente válido.", 0);

        // Si el mismo producto viene repetido se suman las cantidades.
        var agrupadas = lineas
            .GroupBy(l => l.IdProducto)
            .Select(g => new LineaVenta(g.Key, g.Sum(l => l.Cantidad)))
            .ToList();

        var ids = agrupadas.Select(l => l.IdProducto).ToList();
        var productos = await _db.Productos.AsNoTracking()
            .Where(p => ids.Contains(p.IdProducto) && p.Estado == EstadoRegistro.Activo)
            .ToDictionaryAsync(p => p.IdProducto);

        if (productos.Count != ids.Count)
            return new ResultadoVenta(false, "Uno de los productos ya no está disponible.", 0);

        await using var transaccion = await _db.Database.BeginTransactionAsync();

        var venta = new Venta
        {
            Fecha = DateTime.Now,
            IdCliente = idCliente,
            IdUsuario = idUsuario,
            MetodoPago = metodoPago,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()
        };

        foreach (var linea in agrupadas)
        {
            var producto = productos[linea.IdProducto];
            venta.Detalles.Add(new DetalleVenta
            {
                IdProducto = producto.IdProducto,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.PrecioVenta,
                Subtotal = producto.PrecioVenta * linea.Cantidad
            });
        }
        venta.Total = venta.Detalles.Sum(d => d.Subtotal);

        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();

        foreach (var linea in agrupadas)
        {
            // El WHERE evita descontar más de lo que hay, incluso con dos ventas al mismo tiempo.
            var filas = await _db.Productos
                .Where(p => p.IdProducto == linea.IdProducto && p.Existencia >= linea.Cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia - linea.Cantidad));

            if (filas == 0)
            {
                var nombre = productos[linea.IdProducto].Nombre;
                return new ResultadoVenta(false, $"No hay existencias suficientes de \"{nombre}\".", 0);
            }

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = linea.IdProducto,
                IdUsuario = idUsuario,
                IdVenta = venta.IdVenta,
                Tipo = TipoMovimiento.Venta,
                Cantidad = linea.Cantidad,
                Motivo = $"Venta #{venta.IdVenta}"
            });
        }

        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return new ResultadoVenta(true, null, venta.IdVenta);
    }

    public async Task<string?> CancelarAsync(int idVenta, int idUsuario)
    {
        var venta = await _db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null)
            return "La venta no existe.";
        if (venta.Estado == EstadoVenta.Cancelada)
            return "La venta ya estaba cancelada.";

        await using var transaccion = await _db.Database.BeginTransactionAsync();

        foreach (var detalle in venta.Detalles)
        {
            await _db.Productos
                .Where(p => p.IdProducto == detalle.IdProducto)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia + detalle.Cantidad));

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = detalle.IdProducto,
                IdUsuario = idUsuario,
                IdVenta = venta.IdVenta,
                Tipo = TipoMovimiento.Cancelacion,
                Cantidad = detalle.Cantidad,
                Motivo = $"Cancelación de la venta #{venta.IdVenta}"
            });
        }

        venta.Estado = EstadoVenta.Cancelada;
        venta.FechaCancelacion = DateTime.Now;

        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        return null;
    }
}
