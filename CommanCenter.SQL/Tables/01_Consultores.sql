-- ============================================================
-- CommanCenter.SQL / Tables / 01_Consultores.sql
-- Módulo: DataTeam (reutilizable para RRHH, DevSecOps, etc.)
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Consultores')
BEGIN
	CREATE TABLE [dbo].[Consultores] (
		[Id]              INT IDENTITY(1,1)   NOT NULL,
		[Cedula]          NVARCHAR(30)         NULL,
		[Nombre]          NVARCHAR(100)        NOT NULL,
		[Apellido]        NVARCHAR(100)        NOT NULL,
		[Email]           NVARCHAR(200)        NOT NULL,
		[Telefono]        NVARCHAR(20)         NULL,
		[Celular]         NVARCHAR(20)         NULL,
		[Cargo]           NVARCHAR(150)        NULL,
		[Rol]             NVARCHAR(100)        NULL,
		[Tecnologia]      NVARCHAR(100)        NULL,
		[NivelSeniority]  NVARCHAR(50)         NULL,
		[Capacidad]       NVARCHAR(50)         NULL,
		[Empresa]         NVARCHAR(150)        NULL,
		[Direccion]       NVARCHAR(250)        NULL,
		[Barrio]          NVARCHAR(100)        NULL,
		[ContactoEmergenciaNombre]    NVARCHAR(150) NULL,
		[ContactoEmergenciaTelefono]  NVARCHAR(20)  NULL,
		[Estado]          NVARCHAR(20)         NOT NULL DEFAULT 'Activo',
		[FechaIngreso]    DATE                 NULL,
		[FechaNacimiento] DATE                 NULL,
		[Habilitado]      BIT                  NOT NULL DEFAULT 1,
		[FotoUrl]         NVARCHAR(500)        NULL,
		[Observaciones]   NVARCHAR(MAX)        NULL,
		[Activo]          BIT                  NOT NULL DEFAULT 1,
		[FechaCreacion]   DATETIME2            NOT NULL DEFAULT GETUTCDATE(),
		[FechaModificacion] DATETIME2          NULL,
		[CreadoPor]       NVARCHAR(450)        NULL,
		[ModificadoPor]   NVARCHAR(450)        NULL,
		CONSTRAINT [PK_Consultores] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	CREATE UNIQUE INDEX [UX_Consultores_Email]
		ON [dbo].[Consultores] ([Email])
		WHERE [Activo] = 1;

	CREATE INDEX [IX_Consultores_Habilitado]
		ON [dbo].[Consultores] ([Habilitado], [Activo]);

	PRINT '✅ Tabla Consultores creada.';
END
ELSE
	PRINT '⚠️ Tabla Consultores ya existe.';
GO

-- ============================================================
-- Columnas de deshabilitación (motivo + fecha)
-- Se agregan de forma idempotente para tablas ya existentes.
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns
	WHERE object_id = OBJECT_ID('dbo.Consultores') AND name = 'MotivoDeshabilitacion')
BEGIN
	ALTER TABLE [dbo].[Consultores] ADD [MotivoDeshabilitacion] NVARCHAR(500) NULL;
	PRINT '✅ Columna MotivoDeshabilitacion agregada.';
END
ELSE
	PRINT '⚠️ Columna MotivoDeshabilitacion ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
	WHERE object_id = OBJECT_ID('dbo.Consultores') AND name = 'FechaDeshabilitacion')
BEGIN
	ALTER TABLE [dbo].[Consultores] ADD [FechaDeshabilitacion] DATETIME2 NULL;
	PRINT '✅ Columna FechaDeshabilitacion agregada.';
END
ELSE
	PRINT '⚠️ Columna FechaDeshabilitacion ya existe.';
GO

-- ============================================================
-- Columnas del directorio de personal (campos extendidos del consultor).
-- Se agregan de forma idempotente para tablas ya existentes.
-- ============================================================
DECLARE @cols TABLE (Nombre SYSNAME, Tipo NVARCHAR(100));
INSERT INTO @cols (Nombre, Tipo) VALUES
	('Cedula',                     'NVARCHAR(30) NULL'),
	('Celular',                    'NVARCHAR(20) NULL'),
	('Rol',                        'NVARCHAR(100) NULL'),
	('Capacidad',                  'NVARCHAR(50) NULL'),
	('Empresa',                    'NVARCHAR(150) NULL'),
	('Direccion',                  'NVARCHAR(250) NULL'),
	('Barrio',                     'NVARCHAR(100) NULL'),
	('ContactoEmergenciaNombre',   'NVARCHAR(150) NULL'),
	('ContactoEmergenciaTelefono', 'NVARCHAR(20) NULL');

DECLARE @nombre SYSNAME, @tipo NVARCHAR(100), @sql NVARCHAR(MAX);
DECLARE col_cursor CURSOR FOR SELECT Nombre, Tipo FROM @cols;
OPEN col_cursor;
FETCH NEXT FROM col_cursor INTO @nombre, @tipo;
WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT 1 FROM sys.columns
		WHERE object_id = OBJECT_ID('dbo.Consultores') AND name = @nombre)
	BEGIN
		SET @sql = 'ALTER TABLE [dbo].[Consultores] ADD [' + @nombre + '] ' + @tipo + ';';
		EXEC sp_executesql @sql;
		PRINT '✅ Columna ' + @nombre + ' agregada.';
	END
	ELSE
		PRINT '⚠️ Columna ' + @nombre + ' ya existe.';
	FETCH NEXT FROM col_cursor INTO @nombre, @tipo;
END
CLOSE col_cursor;
DEALLOCATE col_cursor;
GO

-- Estado laboral (Activo/Retirado) con valor por defecto 'Activo'.
IF NOT EXISTS (SELECT 1 FROM sys.columns
	WHERE object_id = OBJECT_ID('dbo.Consultores') AND name = 'Estado')
BEGIN
	ALTER TABLE [dbo].[Consultores] ADD [Estado] NVARCHAR(20) NOT NULL DEFAULT 'Activo';
	PRINT '✅ Columna Estado agregada.';
END
ELSE
	PRINT '⚠️ Columna Estado ya existe.';
GO
