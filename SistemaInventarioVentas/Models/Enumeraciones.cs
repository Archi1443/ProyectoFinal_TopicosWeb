namespace SistemaInventarioVentas.Models;

public enum EstadoRegistro
{
    Activo,
    Inactivo
}

public enum RolUsuario
{
    Administrador,
    Empleado
}

public enum MetodoPago
{
    Efectivo,
    Tarjeta,
    Transferencia
}

public enum EstadoVenta
{
    Completada,
    Cancelada
}

public enum TipoMovimiento
{
    Entrada,
    Salida,
    Venta,
    Cancelacion
}

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Ambos = "Administrador,Empleado";
}
