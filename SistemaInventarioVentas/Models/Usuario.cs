using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.Models;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 40 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Solo letras, números, punto, guion y guion bajo.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    // Nunca se guarda la contraseña: solo el hash generado por PasswordHasher.
    public string PasswordHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; } = RolUsuario.Empleado;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [StringLength(120)]
    public string Correo { get; set; } = string.Empty;

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
