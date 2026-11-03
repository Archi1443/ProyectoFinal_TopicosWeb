using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.Services;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Ambos)]
public class ProductosController : Controller
{
    private readonly AppDbContext _db;

    public ProductosController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? buscar, string? categoria)
    {
        var consulta = _db.Productos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(p => p.Nombre.Contains(buscar.Trim()));
        if (!string.IsNullOrWhiteSpace(categoria))
            consulta = consulta.Where(p => p.Categoria == categoria);

        ViewBag.Buscar = buscar;
        ViewBag.Categoria = categoria;
        ViewBag.Categorias = await _db.Productos.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToListAsync();

        return View(await consulta.OrderBy(p => p.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
        if (producto == null)
            return NotFound();

        ViewBag.Movimientos = await _db.MovimientosInventario.AsNoTracking()
            .Include(m => m.Usuario)
            .Where(m => m.IdProducto == id)
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.IdMovimiento)
            .Take(30)
            .ToListAsync();

        return View(producto);
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Create()
    {
        await CargarCategoriasAsync();
        return View(new Producto());
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [Bind("Nombre,Descripcion,PrecioCompra,PrecioVenta,Existencia,UnidadMedida,Categoria")] Producto producto)
    {
        if (!ModelState.IsValid)
        {
            await CargarCategoriasAsync();
            return View(producto);
        }

        producto.FechaRegistro = DateTime.Now;
        producto.Estado = EstadoRegistro.Activo;
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        if (producto.Existencia > 0)
        {
            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                IdProducto = producto.IdProducto,
                IdUsuario = User.ObtenerIdUsuario(),
                Tipo = TipoMovimiento.Entrada,
                Cantidad = producto.Existencia,
                Motivo = "Inventario inicial"
            });
            await _db.SaveChangesAsync();
        }

        TempData["Exito"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        await CargarCategoriasAsync();
        return View(producto);
    }

    // La existencia no se edita aquí: solo cambia con ventas o con movimientos de inventario.
    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        var actualizado = await TryUpdateModelAsync(producto, "", p => p.Nombre, p => p.Descripcion,
            p => p.PrecioCompra, p => p.PrecioVenta, p => p.UnidadMedida, p => p.Categoria, p => p.Estado);
        if (!actualizado)
        {
            await CargarCategoriasAsync();
            return View(producto);
        }

        await _db.SaveChangesAsync();
        TempData["Exito"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Movimiento(int id)
    {
        var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == id);
        if (producto == null)
            return NotFound();

        return View(new MovimientoViewModel
        {
            IdProducto = id,
            NombreProducto = producto.Nombre,
            ExistenciaActual = producto.Existencia
        });
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Movimiento(MovimientoViewModel modelo)
    {
        var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == modelo.IdProducto);
        if (producto == null)
            return NotFound();

        modelo.NombreProducto = producto.Nombre;
        modelo.ExistenciaActual = producto.Existencia;

        if (modelo.Tipo != TipoMovimiento.Entrada && modelo.Tipo != TipoMovimiento.Salida)
            ModelState.AddModelError(nameof(modelo.Tipo), "Elige entrada o salida.");
        if (!ModelState.IsValid)
            return View(modelo);

        var cantidad = modelo.Cantidad;
        var filas = modelo.Tipo == TipoMovimiento.Entrada
            ? await _db.Productos.Where(p => p.IdProducto == producto.IdProducto)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia + cantidad))
            : await _db.Productos.Where(p => p.IdProducto == producto.IdProducto && p.Existencia >= cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Existencia, p => p.Existencia - cantidad));

        if (filas == 0)
        {
            ModelState.AddModelError(nameof(modelo.Cantidad), "No puedes sacar más piezas de las que hay en existencia.");
            return View(modelo);
        }

        _db.MovimientosInventario.Add(new MovimientoInventario
        {
            IdProducto = producto.IdProducto,
            IdUsuario = User.ObtenerIdUsuario(),
            Tipo = modelo.Tipo,
            Cantidad = cantidad,
            Motivo = modelo.Motivo.Trim()
        });
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Movimiento de inventario registrado.";
        return RedirectToAction(nameof(Details), new { id = producto.IdProducto });
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        producto.Estado = producto.Estado == EstadoRegistro.Activo ? EstadoRegistro.Inactivo : EstadoRegistro.Activo;
        await _db.SaveChangesAsync();

        TempData["Exito"] = producto.Estado == EstadoRegistro.Activo ? "Producto activado." : "Producto desactivado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        if (await _db.DetallesVenta.AnyAsync(d => d.IdProducto == id))
        {
            TempData["Error"] = "Este producto tiene ventas registradas y no se puede eliminar. Puedes desactivarlo.";
            return RedirectToAction(nameof(Index));
        }

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Producto eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync()
    {
        ViewBag.Categorias = await _db.Productos.Select(p => p.Categoria).Distinct().OrderBy(c => c).ToListAsync();
    }
}
