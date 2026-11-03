-- Sistema de Control de Inventario y Ventas
-- Crea la base InventarioVentasDB (estructura + datos de demostración).
-- Ejecutar en SSMS o en el Explorador de objetos de SQL Server de Visual Studio.
IF DB_ID(N'InventarioVentasDB') IS NULL CREATE DATABASE [InventarioVentasDB];
GO
USE [InventarioVentasDB];
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [Clientes] (
        [IdCliente] int NOT NULL IDENTITY,
        [Nombre] nvarchar(80) NOT NULL,
        [ApellidoPaterno] nvarchar(80) NOT NULL,
        [ApellidoMaterno] nvarchar(80) NULL,
        [Telefono] nvarchar(max) NOT NULL,
        [Correo] nvarchar(120) NOT NULL,
        [Direccion] nvarchar(200) NOT NULL,
        [RFC] nvarchar(13) NULL,
        [Estado] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([IdCliente])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [Productos] (
        [IdProducto] int NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        [Descripcion] nvarchar(500) NULL,
        [PrecioCompra] decimal(18,2) NOT NULL,
        [PrecioVenta] decimal(18,2) NOT NULL,
        [Existencia] int NOT NULL,
        [UnidadMedida] nvarchar(30) NOT NULL,
        [Categoria] nvarchar(60) NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [Estado] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([IdProducto]),
        CONSTRAINT [CK_Producto_Existencia] CHECK ([Existencia] >= 0),
        CONSTRAINT [CK_Producto_Precios] CHECK ([PrecioCompra] >= 0 AND [PrecioVenta] > 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [IdUsuario] int NOT NULL IDENTITY,
        [NombreUsuario] nvarchar(40) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Rol] nvarchar(20) NOT NULL,
        [NombreCompleto] nvarchar(120) NOT NULL,
        [Correo] nvarchar(120) NOT NULL,
        [Estado] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([IdUsuario])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [MovimientosInventario] (
        [IdMovimiento] int NOT NULL IDENTITY,
        [IdProducto] int NOT NULL,
        [IdUsuario] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        [Tipo] nvarchar(15) NOT NULL,
        [Cantidad] int NOT NULL,
        [Motivo] nvarchar(200) NOT NULL,
        [IdVenta] int NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([IdMovimiento]),
        CONSTRAINT [CK_Movimiento_Cantidad] CHECK ([Cantidad] > 0),
        CONSTRAINT [FK_MovimientosInventario_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE CASCADE,
        CONSTRAINT [FK_MovimientosInventario_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [Ventas] (
        [IdVenta] int NOT NULL IDENTITY,
        [Fecha] datetime2 NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [IdCliente] int NOT NULL,
        [IdUsuario] int NOT NULL,
        [MetodoPago] nvarchar(20) NOT NULL,
        [Observaciones] nvarchar(500) NULL,
        [Estado] nvarchar(15) NOT NULL,
        [FechaCancelacion] datetime2 NULL,
        CONSTRAINT [PK_Ventas] PRIMARY KEY ([IdVenta]),
        CONSTRAINT [CK_Venta_Total] CHECK ([Total] >= 0),
        CONSTRAINT [FK_Ventas_Clientes_IdCliente] FOREIGN KEY ([IdCliente]) REFERENCES [Clientes] ([IdCliente]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_Usuarios_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [Usuarios] ([IdUsuario]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE TABLE [DetallesVenta] (
        [IdDetalle] int NOT NULL IDENTITY,
        [IdVenta] int NOT NULL,
        [IdProducto] int NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetallesVenta] PRIMARY KEY ([IdDetalle]),
        CONSTRAINT [CK_Detalle_Cantidad] CHECK ([Cantidad] > 0),
        CONSTRAINT [FK_DetallesVenta_Productos_IdProducto] FOREIGN KEY ([IdProducto]) REFERENCES [Productos] ([IdProducto]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesVenta_Ventas_IdVenta] FOREIGN KEY ([IdVenta]) REFERENCES [Ventas] ([IdVenta]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_IdProducto] ON [DetallesVenta] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_IdVenta] ON [DetallesVenta] ([IdVenta]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_IdProducto] ON [MovimientosInventario] ([IdProducto]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_MovimientosInventario_IdUsuario] ON [MovimientosInventario] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_Productos_Nombre] ON [Productos] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [Usuarios] ([NombreUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_Fecha] ON [Ventas] ([Fecha]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdCliente] ON [Ventas] ([IdCliente]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_IdUsuario] ON [Ventas] ([IdUsuario]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261103203226_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261103203226_Inicial', N'8.0.33');
END;
GO

COMMIT;
GO


-- Datos de demostración
GO

SET IDENTITY_INSERT [Usuarios] ON;
INSERT INTO [Usuarios] ([IdUsuario], [NombreUsuario], [PasswordHash], [Rol], [NombreCompleto], [Correo], [Estado]) VALUES (1, N'admin', N'AQAAAAIAAYagAAAAEGu7NxgEbsv1lbUOFoBhHq8Z63gP9vej2/HQOmPKyL9+zYNpCvjZMUGpTAEOVJsAYA==', N'Administrador', N'Administrador del sistema', N'admin@inventario.local', N'Activo');
INSERT INTO [Usuarios] ([IdUsuario], [NombreUsuario], [PasswordHash], [Rol], [NombreCompleto], [Correo], [Estado]) VALUES (2, N'empleado', N'AQAAAAIAAYagAAAAEKMOhv/miCcNBWwvBCElBvZbKePiX9q8vEiZnfOjE1VsMXyl1fgrIMtlO0gHWo9rXA==', N'Empleado', N'Empleado de mostrador', N'empleado@inventario.local', N'Activo');
SET IDENTITY_INSERT [Usuarios] OFF;
GO

SET IDENTITY_INSERT [Clientes] ON;
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (1, N'Público', N'en general', NULL, N'6421000000', N'mostrador@inventario.local', N'Mostrador', NULL, N'Activo');
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (2, N'María', N'López', N'Hernández', N'6441112233', N'maria.lopez@correo.com', N'Calle Sonora 120, Cd. Obregón', N'LOHM900512AB3', N'Activo');
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (3, N'Carlos', N'Ramírez', N'Soto', N'6442223344', N'carlos.ramirez@correo.com', N'Av. Náinari 455, Cd. Obregón', NULL, N'Activo');
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (4, N'Fernanda', N'Gómez', N'Ruiz', N'6443334455', N'fernanda.gomez@correo.com', N'Calle Yucatán 890, Cd. Obregón', N'GORF950320XY1', N'Activo');
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (5, N'Luis', N'Mendoza', NULL, N'6444445566', N'luis.mendoza@correo.com', N'Blvd. Rodolfo Elías Calles 2100', NULL, N'Activo');
INSERT INTO [Clientes] ([IdCliente], [Nombre], [ApellidoPaterno], [ApellidoMaterno], [Telefono], [Correo], [Direccion], [RFC], [Estado]) VALUES (6, N'Ana', N'Torres', N'Valdez', N'6445556677', N'ana.torres@correo.com', N'Calle Hidalgo 33, Esperanza', NULL, N'Activo');
SET IDENTITY_INSERT [Clientes] OFF;
GO

SET IDENTITY_INSERT [Productos] ON;
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (1, N'Laptop 14" Core i5', N'Equipo portátil 8 GB RAM, 512 GB SSD', 9800.00, 12500.00, 7, N'Pieza', N'Computo', '2026-09-04T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (2, N'Mouse inalámbrico', N'Mouse óptico 2.4 GHz', 120.00, 249.00, 41, N'Pieza', N'Accesorios', '2026-09-04T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (3, N'Teclado mecánico', N'Teclado USB retroiluminado', 450.00, 799.00, 18, N'Pieza', N'Accesorios', '2026-09-06T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (4, N'Monitor 24" Full HD', N'Monitor IPS HDMI', 1900.00, 2699.00, 4, N'Pieza', N'Computo', '2026-09-06T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (5, N'Memoria USB 64 GB', N'Unidad flash USB 3.0', 90.00, 159.00, 49, N'Pieza', N'Almacenamiento', '2026-09-09T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (6, N'Disco duro externo 1 TB', N'Disco portátil USB 3.0', 850.00, 1299.00, 8, N'Pieza', N'Almacenamiento', '2026-09-09T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (7, N'Cable HDMI 2 m', N'Cable HDMI 4K', 35.00, 89.00, 96, N'Pieza', N'Cables', '2026-09-14T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (8, N'Impresora multifuncional', N'Inyección de tinta con WiFi', 1700.00, 2399.00, 4, N'Pieza', N'Computo', '2026-09-14T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (9, N'Papel bond carta', N'Paquete de 500 hojas', 70.00, 119.00, 117, N'Paquete', N'Papelería', '2026-09-19T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (10, N'Tinta negra', N'Cartucho de tinta negra', 180.00, 289.00, 18, N'Pieza', N'Papelería', '2026-09-19T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (11, N'Audífonos con micrófono', N'Diadema con cable USB', 210.00, 399.00, 33, N'Pieza', N'Accesorios', '2026-09-24T00:00:00.000', N'Activo');
INSERT INTO [Productos] ([IdProducto], [Nombre], [Descripcion], [PrecioCompra], [PrecioVenta], [Existencia], [UnidadMedida], [Categoria], [FechaRegistro], [Estado]) VALUES (12, N'Router WiFi 6', N'Router de doble banda', 780.00, 1190.00, 15, N'Pieza', N'Redes', '2026-09-24T00:00:00.000', N'Activo');
SET IDENTITY_INSERT [Productos] OFF;
GO

SET IDENTITY_INSERT [Ventas] ON;
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (1, '2026-10-30T11:55:00.000', 4631.00, 6, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (2, '2026-10-23T16:15:00.000', 8097.00, 4, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (3, '2026-10-26T13:05:00.000', 8097.00, 6, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (4, '2026-10-13T16:13:00.000', 1383.00, 5, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (5, '2026-10-30T17:18:00.000', 6579.00, 1, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (6, '2026-10-23T09:50:00.000', 3196.00, 2, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (7, '2026-10-08T09:48:00.000', 89.00, 1, 2, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (8, '2026-10-15T15:55:00.000', 12194.00, 5, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (9, '2026-10-19T17:56:00.000', 3266.00, 2, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (10, '2026-10-31T16:10:00.000', 249.00, 5, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (11, '2026-11-01T15:07:00.000', 8896.00, 3, 1, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (12, '2026-10-31T14:54:00.000', 5398.00, 1, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (13, '2026-10-29T09:59:00.000', 119.00, 4, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (14, '2026-10-26T14:03:00.000', 7674.00, 2, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (15, '2026-10-24T14:55:00.000', 427.00, 1, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (16, '2026-10-12T11:14:00.000', 5893.00, 5, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (17, '2026-10-07T13:49:00.000', 1156.00, 1, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (18, '2026-10-18T15:45:00.000', 2234.00, 3, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (19, '2026-10-12T17:31:00.000', 3695.00, 3, 2, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (20, '2026-10-19T15:50:00.000', 289.00, 1, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (21, '2026-10-21T12:38:00.000', 38247.00, 2, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (22, '2026-10-18T14:41:00.000', 755.00, 6, 1, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (23, '2026-10-11T16:02:00.000', 8256.00, 4, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (24, '2026-10-14T11:00:00.000', 996.00, 4, 2, N'Transferencia', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (25, '2026-10-30T16:21:00.000', 10774.00, 4, 1, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (26, '2026-10-21T09:18:00.000', 58733.00, 3, 2, N'Efectivo', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (27, '2026-10-15T11:38:00.000', 9685.00, 1, 2, N'Tarjeta', NULL, N'Completada', NULL);
INSERT INTO [Ventas] ([IdVenta], [Fecha], [Total], [IdCliente], [IdUsuario], [MetodoPago], [Observaciones], [Estado], [FechaCancelacion]) VALUES (28, '2026-10-17T12:47:00.000', 1674.00, 1, 1, N'Tarjeta', NULL, N'Completada', NULL);
SET IDENTITY_INSERT [Ventas] OFF;
GO

SET IDENTITY_INSERT [DetallesVenta] ON;
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (1, 1, 11, 4, 399.00, 1596.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (2, 1, 8, 1, 2399.00, 2399.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (3, 1, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (4, 2, 4, 3, 2699.00, 8097.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (5, 3, 4, 3, 2699.00, 8097.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (6, 4, 2, 3, 249.00, 747.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (7, 4, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (8, 5, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (9, 5, 2, 3, 249.00, 747.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (10, 5, 6, 4, 1299.00, 5196.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (11, 6, 3, 4, 799.00, 3196.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (12, 7, 7, 1, 89.00, 89.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (13, 8, 8, 4, 2399.00, 9596.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (14, 8, 6, 2, 1299.00, 2598.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (15, 9, 8, 1, 2399.00, 2399.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (16, 9, 10, 3, 289.00, 867.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (17, 10, 2, 1, 249.00, 249.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (18, 11, 4, 3, 2699.00, 8097.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (19, 11, 3, 1, 799.00, 799.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (20, 12, 4, 2, 2699.00, 5398.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (21, 13, 9, 1, 119.00, 119.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (22, 14, 8, 3, 2399.00, 7197.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (23, 14, 5, 3, 159.00, 477.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (24, 15, 7, 2, 89.00, 178.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (25, 15, 2, 1, 249.00, 249.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (26, 16, 9, 1, 119.00, 119.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (27, 16, 10, 2, 289.00, 578.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (28, 16, 6, 4, 1299.00, 5196.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (29, 17, 10, 4, 289.00, 1156.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (30, 18, 3, 2, 799.00, 1598.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (31, 18, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (32, 19, 2, 4, 249.00, 996.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (33, 19, 4, 1, 2699.00, 2699.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (34, 20, 10, 1, 289.00, 289.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (35, 21, 1, 3, 12500.00, 37500.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (36, 21, 2, 3, 249.00, 747.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (37, 22, 9, 1, 119.00, 119.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (38, 22, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (39, 23, 5, 1, 159.00, 159.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (40, 23, 4, 3, 2699.00, 8097.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (41, 24, 2, 4, 249.00, 996.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (42, 25, 10, 2, 289.00, 578.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (43, 25, 8, 2, 2399.00, 4798.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (44, 25, 4, 2, 2699.00, 5398.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (45, 26, 4, 3, 2699.00, 8097.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (46, 26, 5, 4, 159.00, 636.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (47, 26, 1, 4, 12500.00, 50000.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (48, 27, 8, 4, 2399.00, 9596.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (49, 27, 7, 1, 89.00, 89.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (50, 28, 5, 3, 159.00, 477.00);
INSERT INTO [DetallesVenta] ([IdDetalle], [IdVenta], [IdProducto], [Cantidad], [PrecioUnitario], [Subtotal]) VALUES (51, 28, 11, 3, 399.00, 1197.00);
SET IDENTITY_INSERT [DetallesVenta] OFF;
GO

SET IDENTITY_INSERT [MovimientosInventario] ON;
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (1, 1, 1, '2026-09-04T00:00:00.000', N'Entrada', 14, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (2, 2, 1, '2026-09-04T00:00:00.000', N'Entrada', 60, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (3, 3, 1, '2026-09-06T00:00:00.000', N'Entrada', 25, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (4, 4, 1, '2026-09-06T00:00:00.000', N'Entrada', 30, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (5, 5, 1, '2026-09-09T00:00:00.000', N'Entrada', 80, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (6, 6, 1, '2026-09-09T00:00:00.000', N'Entrada', 18, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (7, 7, 1, '2026-09-14T00:00:00.000', N'Entrada', 100, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (8, 8, 1, '2026-09-14T00:00:00.000', N'Entrada', 20, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (9, 9, 1, '2026-09-19T00:00:00.000', N'Entrada', 120, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (10, 10, 1, '2026-09-19T00:00:00.000', N'Entrada', 30, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (11, 11, 1, '2026-09-24T00:00:00.000', N'Entrada', 40, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (12, 12, 1, '2026-09-24T00:00:00.000', N'Entrada', 15, N'Inventario inicial', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (13, 11, 2, '2026-10-30T11:55:00.000', N'Venta', 4, N'Venta #1', 1);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (14, 8, 2, '2026-10-30T11:55:00.000', N'Venta', 1, N'Venta #1', 1);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (15, 5, 2, '2026-10-30T11:55:00.000', N'Venta', 4, N'Venta #1', 1);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (16, 4, 2, '2026-10-23T16:15:00.000', N'Venta', 3, N'Venta #2', 2);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (17, 4, 2, '2026-10-26T13:05:00.000', N'Venta', 3, N'Venta #3', 3);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (18, 2, 1, '2026-10-13T16:13:00.000', N'Venta', 3, N'Venta #4', 4);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (19, 5, 1, '2026-10-13T16:13:00.000', N'Venta', 4, N'Venta #4', 4);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (20, 5, 2, '2026-10-30T17:18:00.000', N'Venta', 4, N'Venta #5', 5);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (21, 2, 2, '2026-10-30T17:18:00.000', N'Venta', 3, N'Venta #5', 5);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (22, 6, 2, '2026-10-30T17:18:00.000', N'Venta', 4, N'Venta #5', 5);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (23, 3, 1, '2026-10-23T09:50:00.000', N'Venta', 4, N'Venta #6', 6);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (24, 7, 2, '2026-10-08T09:48:00.000', N'Venta', 1, N'Venta #7', 7);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (25, 8, 1, '2026-10-15T15:55:00.000', N'Venta', 4, N'Venta #8', 8);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (26, 6, 1, '2026-10-15T15:55:00.000', N'Venta', 2, N'Venta #8', 8);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (27, 8, 2, '2026-10-19T17:56:00.000', N'Venta', 1, N'Venta #9', 9);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (28, 10, 2, '2026-10-19T17:56:00.000', N'Venta', 3, N'Venta #9', 9);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (29, 2, 2, '2026-10-31T16:10:00.000', N'Venta', 1, N'Venta #10', 10);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (30, 4, 1, '2026-11-01T15:07:00.000', N'Venta', 3, N'Venta #11', 11);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (31, 3, 1, '2026-11-01T15:07:00.000', N'Venta', 1, N'Venta #11', 11);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (32, 4, 2, '2026-10-31T14:54:00.000', N'Venta', 2, N'Venta #12', 12);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (33, 9, 2, '2026-10-29T09:59:00.000', N'Venta', 1, N'Venta #13', 13);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (34, 8, 2, '2026-10-26T14:03:00.000', N'Venta', 3, N'Venta #14', 14);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (35, 5, 2, '2026-10-26T14:03:00.000', N'Venta', 3, N'Venta #14', 14);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (36, 7, 1, '2026-10-24T14:55:00.000', N'Venta', 2, N'Venta #15', 15);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (37, 2, 1, '2026-10-24T14:55:00.000', N'Venta', 1, N'Venta #15', 15);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (38, 9, 1, '2026-10-12T11:14:00.000', N'Venta', 1, N'Venta #16', 16);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (39, 10, 1, '2026-10-12T11:14:00.000', N'Venta', 2, N'Venta #16', 16);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (40, 6, 1, '2026-10-12T11:14:00.000', N'Venta', 4, N'Venta #16', 16);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (41, 10, 1, '2026-10-07T13:49:00.000', N'Venta', 4, N'Venta #17', 17);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (42, 3, 2, '2026-10-18T15:45:00.000', N'Venta', 2, N'Venta #18', 18);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (43, 5, 2, '2026-10-18T15:45:00.000', N'Venta', 4, N'Venta #18', 18);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (44, 2, 2, '2026-10-12T17:31:00.000', N'Venta', 4, N'Venta #19', 19);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (45, 4, 2, '2026-10-12T17:31:00.000', N'Venta', 1, N'Venta #19', 19);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (46, 10, 2, '2026-10-19T15:50:00.000', N'Venta', 1, N'Venta #20', 20);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (47, 1, 2, '2026-10-21T12:38:00.000', N'Venta', 3, N'Venta #21', 21);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (48, 2, 2, '2026-10-21T12:38:00.000', N'Venta', 3, N'Venta #21', 21);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (49, 9, 1, '2026-10-18T14:41:00.000', N'Venta', 1, N'Venta #22', 22);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (50, 5, 1, '2026-10-18T14:41:00.000', N'Venta', 4, N'Venta #22', 22);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (51, 5, 2, '2026-10-11T16:02:00.000', N'Venta', 1, N'Venta #23', 23);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (52, 4, 2, '2026-10-11T16:02:00.000', N'Venta', 3, N'Venta #23', 23);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (53, 2, 2, '2026-10-14T11:00:00.000', N'Venta', 4, N'Venta #24', 24);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (54, 10, 1, '2026-10-30T16:21:00.000', N'Venta', 2, N'Venta #25', 25);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (55, 8, 1, '2026-10-30T16:21:00.000', N'Venta', 2, N'Venta #25', 25);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (56, 4, 1, '2026-10-30T16:21:00.000', N'Venta', 2, N'Venta #25', 25);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (57, 4, 2, '2026-10-21T09:18:00.000', N'Venta', 3, N'Venta #26', 26);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (58, 5, 2, '2026-10-21T09:18:00.000', N'Venta', 4, N'Venta #26', 26);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (59, 1, 2, '2026-10-21T09:18:00.000', N'Venta', 4, N'Venta #26', 26);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (60, 8, 2, '2026-10-15T11:38:00.000', N'Venta', 4, N'Venta #27', 27);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (61, 7, 2, '2026-10-15T11:38:00.000', N'Venta', 1, N'Venta #27', 27);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (62, 5, 1, '2026-10-17T12:47:00.000', N'Venta', 3, N'Venta #28', 28);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (63, 11, 1, '2026-10-17T12:47:00.000', N'Venta', 3, N'Venta #28', 28);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (64, 4, 1, '2026-11-03T21:07:18.858', N'Salida', 6, N'Traslado a otra sucursal', NULL);
INSERT INTO [MovimientosInventario] ([IdMovimiento], [IdProducto], [IdUsuario], [Fecha], [Tipo], [Cantidad], [Motivo], [IdVenta]) VALUES (65, 8, 1, '2026-11-03T21:07:18.858', N'Salida', 1, N'Traslado a otra sucursal', NULL);
SET IDENTITY_INSERT [MovimientosInventario] OFF;
GO
