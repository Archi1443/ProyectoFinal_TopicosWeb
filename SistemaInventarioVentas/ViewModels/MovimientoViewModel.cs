using System.ComponentModel.DataAnnotations;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.ViewModels;

public class MovimientoViewModel
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public int ExistenciaActual { get; set; }

    [Display(Name = "Tipo de movimiento")]
    public TipoMovimiento Tipo { get; set; } = TipoMovimiento.Entrada;

    [Range(1, 1000000, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "Escribe el motivo del movimiento.")]
    [StringLength(200)]
    public string Motivo { get; set; } = string.Empty;
}
