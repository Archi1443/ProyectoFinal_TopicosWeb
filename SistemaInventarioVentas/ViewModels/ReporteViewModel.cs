using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.ViewModels;

public record VentaPorPeriodo(string Etiqueta, int Operaciones, decimal Total);

public class ReporteViewModel
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }

    public decimal TotalIngresos { get; set; }
    public int TotalOperaciones { get; set; }
    public int VentasCanceladas { get; set; }
    public int UnidadesVendidas { get; set; }

    public List<VentaPorPeriodo> VentasDiarias { get; set; } = new();
    public List<VentaPorPeriodo> VentasMensuales { get; set; } = new();
    public List<ProductoVendido> MasVendidos { get; set; } = new();
    public List<Producto> Inventario { get; set; } = new();

    public decimal ValorInventario => Inventario.Sum(p => p.Existencia * p.PrecioCompra);
}
