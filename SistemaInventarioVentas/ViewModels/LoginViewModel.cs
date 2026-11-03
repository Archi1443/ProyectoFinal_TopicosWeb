using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre de usuario.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
