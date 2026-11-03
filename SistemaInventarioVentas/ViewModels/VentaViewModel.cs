using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.ViewModels;

public class VentaCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente.")]
    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;

    [StringLength(500)]
    public string? Observaciones { get; set; }

    // Las líneas del carrito viajan como JSON: [{"idProducto":1,"cantidad":2}]
    public string DetallesJson { get; set; } = "[]";

    public List<SelectListItem> Clientes { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
}

public class VentaEditViewModel
{
    public int IdVenta { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente.")]
    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Display(Name = "Método de pago")]
    public MetodoPago MetodoPago { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public List<SelectListItem> Clientes { get; set; } = new();
}
