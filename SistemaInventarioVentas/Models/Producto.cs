using System.ComponentModel.DataAnnotations;

namespace SistemaInventarioVentas.Models;

public class Producto
{
    public const int StockBajo = 10;

    [Key]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(0, 9999999, ErrorMessage = "El precio de compra no puede ser negativo.")]
    [Display(Name = "Precio de compra")]
    public decimal PrecioCompra { get; set; }

    [Range(0.01, 9999999, ErrorMessage = "El precio de venta debe ser mayor que cero.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, 1000000, ErrorMessage = "La existencia no puede ser negativa.")]
    public int Existencia { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30)]
    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "Pieza";

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(60)]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = string.Empty;

    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    public bool TieneStockBajo => Existencia <= StockBajo;

    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
}
