# Diagramas

Los diagramas usan Mermaid: se ven directamente en GitHub y en Visual Studio Code con la extensión Markdown Preview Mermaid.

## Modelo entidad-relación

Corresponde exactamente a las clases de `Models/` y a la migración `Inicial`. `MovimientoInventario` es la sexta tabla: guarda la trazabilidad de entradas, salidas, ventas y cancelaciones.

```mermaid
erDiagram
    Usuario ||--o{ Venta : registra
    Cliente ||--o{ Venta : compra
    Venta ||--|{ DetalleVenta : contiene
    Producto ||--o{ DetalleVenta : "se vende en"
    Producto ||--o{ MovimientoInventario : "tiene"
    Usuario ||--o{ MovimientoInventario : realiza

    Usuario {
        int IdUsuario PK
        string NombreUsuario UK
        string PasswordHash
        string Rol "Administrador | Empleado"
        string NombreCompleto
        string Correo
        string Estado "Activo | Inactivo"
    }
    Cliente {
        int IdCliente PK
        string Nombre
        string ApellidoPaterno
        string ApellidoMaterno
        string Telefono
        string Correo
        string Direccion
        string RFC "opcional"
        string Estado
    }
    Producto {
        int IdProducto PK
        string Nombre
        string Descripcion
        decimal PrecioCompra
        decimal PrecioVenta
        int Existencia "CHECK >= 0"
        string UnidadMedida
        string Categoria
        datetime FechaRegistro
        string Estado
    }
    Venta {
        int IdVenta PK
        datetime Fecha
        decimal Total
        int IdCliente FK
        int IdUsuario FK
        string MetodoPago "Efectivo | Tarjeta | Transferencia"
        string Observaciones
        string Estado "Completada | Cancelada"
        datetime FechaCancelacion
    }
    DetalleVenta {
        int IdDetalle PK
        int IdVenta FK
        int IdProducto FK
        int Cantidad "CHECK > 0"
        decimal PrecioUnitario
        decimal Subtotal
    }
    MovimientoInventario {
        int IdMovimiento PK
        int IdProducto FK
        int IdUsuario FK
        datetime Fecha
        string Tipo "Entrada | Salida | Venta | Cancelacion"
        int Cantidad
        string Motivo
        int IdVenta
    }
```

Reglas de borrado: `DetalleVenta` se elimina junto con su `Venta` (cascada); `Venta`→`Cliente`, `Venta`→`Usuario` y `DetalleVenta`→`Producto` son *Restrict*, por eso nada con historial se puede borrar (la aplicación lo desactiva).

## Arquitectura MVC

```mermaid
flowchart LR
    N[Navegador] -- "1. petición HTTP" --> C[Controlador<br/>Controllers/]
    C -- "2. consulta / guarda con LINQ" --> D[AppDbContext<br/>Entity Framework Core]
    D -- "3. SQL parametrizado" --> S[(SQL Server)]
    S -- "4. filas" --> D
    D -- "5. objetos del Modelo<br/>Models/" --> C
    C -- "6. Modelo o ViewModel" --> V[Vista Razor<br/>Views/]
    V -- "7. HTML + Tailwind" --> N
```

- **Modelo**: clases en `Models/` y `ViewModels/`; datos y reglas de validación.
- **Vista**: archivos `.cshtml`; solo presentan datos.
- **Controlador**: decide qué hacer con cada petición; las reglas de venta más delicadas están en `Services/VentaService.cs`.

## Flujo de autenticación

Archivos que participan: `Views/Account/Login.cshtml`, `ViewModels/LoginViewModel.cs`, `Controllers/AccountController.cs`, `Data/AppDbContext.cs`, `Models/Usuario.cs`, `Program.cs`.

```mermaid
sequenceDiagram
    actor U as Usuario
    participant V as Login.cshtml
    participant C as AccountController
    participant E as AppDbContext (EF Core)
    participant S as SQL Server
    U->>V: Abre /Account/Login
    U->>V: Escribe usuario y contraseña
    V->>C: POST (con token antifalsificación)
    C->>C: ¿Campos completos? (ModelState)
    C->>E: Usuarios.FirstOrDefault(NombreUsuario)
    E->>S: SELECT TOP(1) ... WHERE NombreUsuario = @p
    S-->>E: Fila del usuario (con PasswordHash)
    E-->>C: Objeto Usuario
    C->>C: PasswordHasher.VerifyHashedPassword
    C->>C: ¿Estado = Activo?
    C->>C: Crea claims (id, nombre, rol)
    C-->>U: SignInAsync: cookie de sesión HttpOnly
    C-->>U: Redirige al Dashboard
    Note over C,U: Logout: POST /Account/Logout borra la cookie.<br/>En cada petición Program.cs revisa que el usuario siga activo.
```

## Registro de una venta

```mermaid
flowchart TD
    A[Formulario: cliente + carrito + pago] --> B[VentasController.Create POST]
    B --> C[VentaService.RegistrarAsync]
    C --> D{¿Líneas válidas?<br/>cantidad > 0, cliente y productos activos}
    D -- No --> X[Mensaje de error, no se guarda nada]
    D -- Sí --> E[BEGIN TRANSACTION]
    E --> F[Calcula precios, subtotales y total en el servidor]
    F --> G[INSERT Venta + DetalleVenta]
    G --> H["UPDATE Productos SET Existencia = Existencia - n<br/>WHERE Existencia >= n"]
    H --> I{¿Se actualizó la fila?}
    I -- No --> R[ROLLBACK: sin venta ni cambios]
    I -- Sí --> J[INSERT MovimientoInventario]
    J --> K[COMMIT]
    K --> L[Redirige al detalle de la venta]
```
