using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.ViewModels;

public record ProductoVendido(string Nombre, int Unidades, decimal Importe);

public class DashboardViewModel
{
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public decimal VentasHoy { get; set; }
    public int OperacionesHoy { get; set; }
    public decimal VentasMes { get; set; }
    public int OperacionesMes { get; set; }
    public List<Producto> StockBajo { get; set; } = new();
    public List<Venta> VentasRecientes { get; set; } = new();
    public List<ProductoVendido> MasVendidos { get; set; } = new();
}
