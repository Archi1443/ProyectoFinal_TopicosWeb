using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.Models;

public class Venta
{
    [Key]
    public int IdVenta { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public decimal Total { get; set; }

    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }

    public int IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public EstadoVenta Estado { get; set; } = EstadoVenta.Completada;

    public DateTime? FechaCancelacion { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
