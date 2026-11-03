using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.Services;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Ambos)]
public class VentasController : Controller
{
    private readonly AppDbContext _db;
    private readonly VentaService _ventaService;

    public VentasController(AppDbContext db, VentaService ventaService)
    {
        _db = db;
        _ventaService = ventaService;
    }

    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, EstadoVenta? estado)
    {
        var consulta = _db.Ventas.AsNoTracking().Include(v => v.Cliente).Include(v => v.Usuario).AsQueryable();

        // El empleado solo consulta las ventas que él registró.
        if (!User.IsInRole(Roles.Administrador))
            consulta = consulta.Where(v => v.IdUsuario == User.ObtenerIdUsuario());

        if (desde.HasValue)
            consulta = consulta.Where(v => v.Fecha >= desde.Value.Date);
        if (hasta.HasValue)
            consulta = consulta.Where(v => v.Fecha < hasta.Value.Date.AddDays(1));
        if (estado.HasValue)
            consulta = consulta.Where(v => v.Estado == estado.Value);

        ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
        ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");
        ViewBag.Estado = estado;

        return View(await consulta.OrderByDescending(v => v.Fecha).Take(200).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var venta = await _db.Ventas.AsNoTracking()
            .Include(v => v.Cliente).Include(v => v.Usuario)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.IdVenta == id);
        if (venta == null)
            return NotFound();

        if (!User.IsInRole(Roles.Administrador) && venta.IdUsuario != User.ObtenerIdUsuario())
            return Forbid();

        return View(venta);
    }

    public async Task<IActionResult> Create(int? idCliente)
    {
        var modelo = new VentaCreateViewModel { IdCliente = idCliente ?? 0 };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [HttpPost]
    public async Task<IActionResult> Create(VentaCreateViewModel modelo)
    {
        List<LineaVenta>? lineas = null;
        try
        {
            lineas = JsonSerializer.Deserialize<List<LineaVenta>>(modelo.DetallesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
        }

        if (lineas == null)
            ModelState.AddModelError(string.Empty, "No se pudo leer el detalle de la venta.");

        if (ModelState.IsValid)
        {
            var resultado = await _ventaService.RegistrarAsync(modelo.IdCliente, User.ObtenerIdUsuario(),
                modelo.MetodoPago, modelo.Observaciones, lineas!);

            if (resultado.Exitoso)
            {
                TempData["Exito"] = $"Venta #{resultado.IdVenta} registrada correctamente.";
                return RedirectToAction(nameof(Details), new { id = resultado.IdVenta });
            }

            ModelState.AddModelError(string.Empty, resultado.Error!);
        }

        await CargarListasAsync(modelo);
        return View(modelo);
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var venta = await _db.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.IdVenta == id);
        if (venta == null)
            return NotFound();

        if (venta.Estado == EstadoVenta.Cancelada)
        {
            TempData["Error"] = "Una venta cancelada ya no se puede modificar.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var modelo = new VentaEditViewModel
        {
            IdVenta = id,
            IdCliente = venta.IdCliente,
            MetodoPago = venta.MetodoPago,
            Observaciones = venta.Observaciones
        };
        modelo.Clientes = await ObtenerClientesAsync(venta.IdCliente);
        return View(modelo);
    }

    // Solo se modifican datos que no afectan el inventario ni el total.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Edit(int id, VentaEditViewModel modelo)
    {
        var venta = await _db.Ventas.FindAsync(id);
        if (venta == null)
            return NotFound();

        if (venta.Estado == EstadoVenta.Cancelada)
            return RedirectToAction(nameof(Details), new { id });

        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == modelo.IdCliente))
            ModelState.AddModelError(nameof(modelo.IdCliente), "Selecciona un cliente válido.");

        if (!ModelState.IsValid)
        {
            modelo.IdVenta = id;
            modelo.Clientes = await ObtenerClientesAsync(modelo.IdCliente);
            return View(modelo);
        }

        venta.IdCliente = modelo.IdCliente;
        venta.MetodoPago = modelo.MetodoPago;
        venta.Observaciones = string.IsNullOrWhiteSpace(modelo.Observaciones) ? null : modelo.Observaciones.Trim();
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Venta actualizada correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Cancelar(int id)
    {
        var error = await _ventaService.CancelarAsync(id, User.ObtenerIdUsuario());
        if (error == null)
            TempData["Exito"] = "Venta cancelada. Las existencias se devolvieron al inventario.";
        else
            TempData["Error"] = error;

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task CargarListasAsync(VentaCreateViewModel modelo)
    {
        modelo.Clientes = await ObtenerClientesAsync(modelo.IdCliente);
        modelo.Productos = await _db.Productos.AsNoTracking()
            .Where(p => p.Estado == EstadoRegistro.Activo && p.Existencia > 0)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> ObtenerClientesAsync(int idSeleccionado)
    {
        var clientes = await _db.Clientes.AsNoTracking()
            .Where(c => c.Estado == EstadoRegistro.Activo || c.IdCliente == idSeleccionado)
            .OrderBy(c => c.Nombre).ThenBy(c => c.ApellidoPaterno)
            .ToListAsync();

        return clientes.Select(c => new SelectListItem(c.NombreCompleto, c.IdCliente.ToString())).ToList();
    }
}
