using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.Services;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public UsuariosController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _db.Usuarios.AsNoTracking().OrderBy(u => u.NombreUsuario).ToListAsync());
    }

    public IActionResult Create()
    {
        return View(new UsuarioViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioViewModel modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.Password))
            ModelState.AddModelError(nameof(modelo.Password), "La contraseña es obligatoria.");

        var nombre = modelo.NombreUsuario?.Trim() ?? string.Empty;
        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            ModelState.AddModelError(nameof(modelo.NombreUsuario), "Ese nombre de usuario ya existe.");

        if (!ModelState.IsValid)
            return View(modelo);

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            NombreCompleto = modelo.NombreCompleto.Trim(),
            Correo = modelo.Correo.Trim(),
            Rol = modelo.Rol,
            Estado = modelo.Estado
        };
        usuario.PasswordHash = _hasher.HashPassword(usuario, modelo.Password!);
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        TempData["Exito"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
        if (usuario == null)
            return NotFound();

        return View(new UsuarioViewModel
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Correo,
            Rol = usuario.Rol,
            Estado = usuario.Estado
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UsuarioViewModel modelo)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound();

        var nombre = modelo.NombreUsuario?.Trim() ?? string.Empty;
        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre && u.IdUsuario != id))
            ModelState.AddModelError(nameof(modelo.NombreUsuario), "Ese nombre de usuario ya existe.");

        // El administrador no puede quitarse su propio acceso por error.
        if (id == User.ObtenerIdUsuario() && (modelo.Rol != RolUsuario.Administrador || modelo.Estado != EstadoRegistro.Activo))
            ModelState.AddModelError(string.Empty, "No puedes cambiar tu propio rol ni desactivar tu propia cuenta.");

        if (!ModelState.IsValid)
        {
            modelo.IdUsuario = id;
            return View(modelo);
        }

        usuario.NombreUsuario = nombre;
        usuario.NombreCompleto = modelo.NombreCompleto.Trim();
        usuario.Correo = modelo.Correo.Trim();
        usuario.Rol = modelo.Rol;
        usuario.Estado = modelo.Estado;
        if (!string.IsNullOrWhiteSpace(modelo.Password))
            usuario.PasswordHash = _hasher.HashPassword(usuario, modelo.Password);

        await _db.SaveChangesAsync();
        TempData["Exito"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == User.ObtenerIdUsuario())
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null)
            return NotFound();

        var tieneHistorial = await _db.Ventas.AnyAsync(v => v.IdUsuario == id) ||
                             await _db.MovimientosInventario.AnyAsync(m => m.IdUsuario == id);
        if (tieneHistorial)
        {
            usuario.Estado = EstadoRegistro.Inactivo;
            await _db.SaveChangesAsync();
            TempData["Exito"] = "El usuario tiene historial registrado, por eso se desactivó en lugar de eliminarse.";
            return RedirectToAction(nameof(Index));
        }

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
        TempData["Exito"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
