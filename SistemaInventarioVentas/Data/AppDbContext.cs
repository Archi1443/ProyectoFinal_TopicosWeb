using Microsoft.EntityFrameworkCore;
using SistemaInventarioVentas.Models;

namespace SistemaInventarioVentas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(e =>
        {
            e.Property(p => p.PrecioCompra).HasPrecision(18, 2);
            e.Property(p => p.PrecioVenta).HasPrecision(18, 2);
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(10);
            e.Ignore(p => p.TieneStockBajo);
            e.HasIndex(p => p.Nombre);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Producto_Existencia", "[Existencia] >= 0");
                t.HasCheckConstraint("CK_Producto_Precios", "[PrecioCompra] >= 0 AND [PrecioVenta] > 0");
            });
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Estado).HasConversion<string>().HasMaxLength(10);
            e.Ignore(c => c.NombreCompleto);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.Estado).HasConversion<string>().HasMaxLength(10);
            e.HasIndex(u => u.NombreUsuario).IsUnique();
        });

        modelBuilder.Entity<Venta>(e =>
        {
            e.Property(v => v.Total).HasPrecision(18, 2);
            e.Property(v => v.MetodoPago).HasConversion<string>().HasMaxLength(20);
            e.Property(v => v.Estado).HasConversion<string>().HasMaxLength(15);
            e.HasIndex(v => v.Fecha);

            e.HasOne(v => v.Cliente).WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCliente).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Usuario).WithMany(u => u.Ventas)
                .HasForeignKey(v => v.IdUsuario).OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint("CK_Venta_Total", "[Total] >= 0"));
        });

        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.Property(d => d.PrecioUnitario).HasPrecision(18, 2);
            e.Property(d => d.Subtotal).HasPrecision(18, 2);

            e.HasOne(d => d.Venta).WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Producto).WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdProducto).OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint("CK_Detalle_Cantidad", "[Cantidad] > 0"));
        });

        modelBuilder.Entity<MovimientoInventario>(e =>
        {
            e.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(15);

            e.HasOne(m => m.Producto).WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.IdProducto).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Usuario).WithMany()
                .HasForeignKey(m => m.IdUsuario).OnDelete(DeleteBehavior.Restrict);

            e.ToTable(t => t.HasCheckConstraint("CK_Movimiento_Cantidad", "[Cantidad] > 0"));
        });
    }
}
