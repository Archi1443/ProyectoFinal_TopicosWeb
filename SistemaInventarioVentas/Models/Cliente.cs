using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaInventarioVentas.Models;

public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(80)]
    [Display(Name = "Apellido paterno")]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Apellido materno")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9 \-+()]{7,20}$", ErrorMessage = "Escribe un teléfono válido (solo números, espacios, guiones o +).")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Escribe un correo electrónico válido.")]
    [StringLength(120)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    [RegularExpression(@"^[A-Za-zÑ&]{3,4}[0-9]{6}[A-Za-z0-9]{3}$", ErrorMessage = "El RFC debe tener 12 o 13 caracteres con formato válido.")]
    [StringLength(13)]
    public string? RFC { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    [NotMapped]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
