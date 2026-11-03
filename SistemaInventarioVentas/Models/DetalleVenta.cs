using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.Models;

public class DetalleVenta
{
    [Key]
    public int IdDetalle { get; set; }

    public int IdVenta { get; set; }
    public Venta? Venta { get; set; }

    public int IdProducto { get; set; }
    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }

    // Precio con el que se vendió; no cambia aunque el producto suba de precio después.
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }
}
