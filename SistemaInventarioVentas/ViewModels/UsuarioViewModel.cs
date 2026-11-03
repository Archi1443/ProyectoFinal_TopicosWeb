using System.ComponentModel.DataAnnotations;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.ViewModels;

public class UsuarioViewModel
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 40 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Solo letras, números, punto, guion y guion bajo.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [StringLength(120)]
    public string Correo { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    // Obligatoria al crear; al editar solo se escribe si se quiere cambiar.
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmarPassword { get; set; }
}
