using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.Models;

public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    public int IdProducto { get; set; }
    public Producto? Producto { get; set; }

    public int IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public TipoMovimiento Tipo { get; set; }

    public int Cantidad { get; set; }

    [StringLength(200)]
    public string Motivo { get; set; } = string.Empty;

    public int? IdVenta { get; set; }
}
