using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.Services;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Ambos)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        // Administrador ve todas las ventas; empleado solo las suyas.
        var ventas = _db.Ventas.AsNoTracking().Where(v => v.Estado == EstadoVenta.Completada);
        if (!User.IsInRole(Roles.Administrador))
        {
            var idUsuario = User.ObtenerIdUsuario();
            ventas = ventas.Where(v => v.IdUsuario == idUsuario);
        }

        var delMes = ventas.Where(v => v.Fecha >= inicioMes);
        var deHoy = ventas.Where(v => v.Fecha >= hoy);
        var hace30Dias = hoy.AddDays(-30);

        var masVendidos = await ventas.Where(v => v.Fecha >= hace30Dias)
            .SelectMany(v => v.Detalles)
            .GroupBy(d => d.Producto!.Nombre)
            .Select(g => new { Nombre = g.Key, Unidades = g.Sum(d => d.Cantidad), Importe = g.Sum(d => d.Subtotal) })
            .OrderByDescending(p => p.Unidades).Take(5).ToListAsync();

        var modelo = new DashboardViewModel
        {
            TotalProductos = await _db.Productos.CountAsync(p => p.Estado == EstadoRegistro.Activo),
            TotalClientes = await _db.Clientes.CountAsync(c => c.Estado == EstadoRegistro.Activo),
            VentasHoy = await deHoy.SumAsync(v => (decimal?)v.Total) ?? 0,
            OperacionesHoy = await deHoy.CountAsync(),
            VentasMes = await delMes.SumAsync(v => (decimal?)v.Total) ?? 0,
            OperacionesMes = await delMes.CountAsync(),
            StockBajo = await _db.Productos.AsNoTracking()
                .Where(p => p.Estado == EstadoRegistro.Activo && p.Existencia <= Producto.StockBajo)
                .OrderBy(p => p.Existencia).Take(6).ToListAsync(),
            VentasRecientes = await ventas.Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha).Take(6).ToListAsync(),
            MasVendidos = masVendidos.Select(p => new ProductoVendido(p.Nombre, p.Unidades, p.Importe)).ToList()
        };

        return View(modelo);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
