using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ReportesController : Controller
{
    private readonly AppDbContext _db;

    public ReportesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        var hoy = DateTime.Today;
        var inicio = (desde ?? new DateTime(hoy.Year, hoy.Month, 1)).Date;
        var fin = (hasta ?? hoy).Date;
        if (fin < inicio)
            (inicio, fin) = (fin, inicio);

        var limiteFin = fin.AddDays(1);
        var todas = _db.Ventas.AsNoTracking().Where(v => v.Fecha >= inicio && v.Fecha < limiteFin);
        var completadas = todas.Where(v => v.Estado == EstadoVenta.Completada);

        var porDia = await completadas
            .GroupBy(v => v.Fecha.Date)
            .Select(g => new { Dia = g.Key, Operaciones = g.Count(), Total = g.Sum(v => v.Total) })
            .OrderBy(g => g.Dia).ToListAsync();

        var inicioMeses = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-11);
        var porMes = await _db.Ventas.AsNoTracking()
            .Where(v => v.Estado == EstadoVenta.Completada && v.Fecha >= inicioMeses)
            .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Operaciones = g.Count(), Total = g.Sum(v => v.Total) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month).ToListAsync();

        var vendidos = await _db.DetallesVenta.AsNoTracking()
            .Where(d => d.Venta!.Estado == EstadoVenta.Completada && d.Venta.Fecha >= inicio && d.Venta.Fecha < limiteFin)
            .GroupBy(d => d.Producto!.Nombre)
            .Select(g => new { Nombre = g.Key, Unidades = g.Sum(d => d.Cantidad), Importe = g.Sum(d => d.Subtotal) })
            .OrderByDescending(p => p.Unidades).ToListAsync();

        var modelo = new ReporteViewModel
        {
            Desde = inicio,
            Hasta = fin,
            TotalIngresos = await completadas.SumAsync(v => (decimal?)v.Total) ?? 0,
            TotalOperaciones = await completadas.CountAsync(),
            VentasCanceladas = await todas.CountAsync(v => v.Estado == EstadoVenta.Cancelada),
            UnidadesVendidas = vendidos.Sum(p => p.Unidades),
            VentasDiarias = porDia.Select(d => new VentaPorPeriodo(d.Dia.ToString("dd/MM/yyyy"), d.Operaciones, d.Total)).ToList(),
            VentasMensuales = porMes.Select(m => new VentaPorPeriodo($"{m.Month:00}/{m.Year}", m.Operaciones, m.Total)).ToList(),
            MasVendidos = vendidos.Take(10).Select(p => new ProductoVendido(p.Nombre, p.Unidades, p.Importe)).ToList(),
            Inventario = await _db.Productos.AsNoTracking()
                .Where(p => p.Estado == EstadoRegistro.Activo)
                .OrderBy(p => p.Existencia).ThenBy(p => p.Nombre).ToListAsync()
        };

        return View(modelo);
    }
}
