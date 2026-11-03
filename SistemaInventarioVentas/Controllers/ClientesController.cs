using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Ambos)]
public class ClientesController : Controller
{
    private readonly AppDbContext _db;

    public ClientesController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        var consulta = _db.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            consulta = consulta.Where(c => (c.Nombre + " " + c.ApellidoPaterno + " " + c.ApellidoMaterno).Contains(texto));
        }

        ViewBag.Buscar = buscar;
        return View(await consulta.OrderBy(c => c.Nombre).ThenBy(c => c.ApellidoPaterno).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente == null)
            return NotFound();

        ViewBag.Ventas = await _db.Ventas.AsNoTracking()
            .Include(v => v.Usuario)
            .Where(v => v.IdCliente == id)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return View(cliente);
    }

    [Authorize(Roles = Roles.Administrador)]
    public IActionResult Create()
    {
        return View(new Cliente());
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [Bind("Nombre,ApellidoPaterno,ApellidoMaterno,Telefono,Correo,Direccion,RFC")] Cliente cliente)
    {
        LimpiarOpcionales(cliente);
        if (!ModelState.IsValid)
            return View(cliente);

        cliente.Estado = EstadoRegistro.Activo;
        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Cliente registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        return View(cliente);
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Edit(int id,
        [Bind("Nombre,ApellidoPaterno,ApellidoMaterno,Telefono,Correo,Direccion,RFC,Estado")] Cliente datos)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        LimpiarOpcionales(datos);
        if (!ModelState.IsValid)
        {
            datos.IdCliente = id;
            return View(datos);
        }

        cliente.Nombre = datos.Nombre;
        cliente.ApellidoPaterno = datos.ApellidoPaterno;
        cliente.ApellidoMaterno = datos.ApellidoMaterno;
        cliente.Telefono = datos.Telefono;
        cliente.Correo = datos.Correo;
        cliente.Direccion = datos.Direccion;
        cliente.RFC = datos.RFC;
        cliente.Estado = datos.Estado;
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        // Con ventas registradas no se borra: se desactiva para conservar el historial.
        if (await _db.Ventas.AnyAsync(v => v.IdCliente == id))
        {
            cliente.Estado = EstadoRegistro.Inactivo;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "El cliente tiene ventas registradas, por eso se desactivó en lugar de eliminarse.";
            return RedirectToAction(nameof(Index));
        }

        _db.Clientes.Remove(cliente);
        await _db.SaveChangesAsync();
        TempData["Exito"] = "Cliente eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private static void LimpiarOpcionales(Cliente cliente)
    {
        cliente.RFC = string.IsNullOrWhiteSpace(cliente.RFC) ? null : cliente.RFC.Trim().ToUpper();
        cliente.ApellidoMaterno = string.IsNullOrWhiteSpace(cliente.ApellidoMaterno) ? null : cliente.ApellidoMaterno.Trim();
    }
}
