using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Data;
using SistemaInventarioVentas.Models;
using SistemaInventarioVentas.ViewModels;

namespace SistemaInventarioVentas.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var nombre = modelo.NombreUsuario.Trim();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombre);

        // Mismo mensaje para usuario inexistente y contraseña incorrecta: no se revela cuál falló.
        var resultado = usuario == null
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, modelo.Password);

        if (usuario == null || resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            modelo.Password = string.Empty;
            return View(modelo);
        }

        if (usuario.Estado == EstadoRegistro.Inactivo)
        {
            ModelState.AddModelError(string.Empty, "Tu cuenta está inactiva. Consulta al administrador.");
            modelo.Password = string.Empty;
            return View(modelo);
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.PasswordHash = _hasher.HashPassword(usuario, modelo.Password);
            await _db.SaveChangesAsync();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new("NombreCompleto", usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));

        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
            return LocalRedirect(modelo.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
