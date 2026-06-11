-- ============================================================
-- CommandCenter.SQL / Tables / 02_Celulas.sql
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Celulas')
BEGIN
	CREATE TABLE [dbo].[Celulas] (
		[Id]              INT IDENTITY(1,1)   NOT NULL,
		[Nombre]          NVARCHAR(100)        NOT NULL,
		[Descripcion]     NVARCHAR(500)        NULL,
		[Color]           NVARCHAR(20)         NULL DEFAULT '#28a745',
		[ImagenUrl]       NVARCHAR(500)        NULL,
		[Activo]          BIT                  NOT NULL DEFAULT 1,
		[FechaCreacion]   DATETIME2            NOT NULL DEFAULT GETUTCDATE(),
		[FechaModificacion] DATETIME2          NULL,
		[CreadoPor]       NVARCHAR(450)        NULL,
		[ModificadoPor]   NVARCHAR(450)        NULL,
		CONSTRAINT [PK_Celulas] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	CREATE INDEX [IX_Celulas_Activo] ON [dbo].[Celulas] ([Activo]);

	PRINT '✅ Tabla Celulas creada.';
END
ELSE
	PRINT '⚠️ Tabla Celulas ya existe.';
GO

-- ============================================================
-- CelulaMiembros
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CelulaMiembros')
BEGIN
	CREATE TABLE [dbo].[CelulaMiembros] (
		[Id]               INT IDENTITY(1,1) NOT NULL,
		[CelulaId]         INT               NOT NULL,
		[ConsultorId]      INT               NOT NULL,
		[FechaAsignacion]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
		CONSTRAINT [PK_CelulaMiembros] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_CelulaMiembros_Celula]    FOREIGN KEY ([CelulaId])    REFERENCES [dbo].[Celulas]([Id])     ON DELETE CASCADE,
		CONSTRAINT [FK_CelulaMiembros_Consultor] FOREIGN KEY ([ConsultorId]) REFERENCES [dbo].[Consultores]([Id]) ON DELETE NO ACTION
	);

	CREATE UNIQUE INDEX [UX_CelulaMiembros_CelulaConsultor]
		ON [dbo].[CelulaMiembros] ([CelulaId], [ConsultorId]);

	PRINT '✅ Tabla CelulaMiembros creada.';
END
GO

-- ============================================================
-- CelulaLideres
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CelulaLideres')
BEGIN
	CREATE TABLE [dbo].[CelulaLideres] (
		[Id]               INT IDENTITY(1,1) NOT NULL,
		[CelulaId]         INT               NOT NULL,
		[ConsultorId]      INT               NOT NULL,
		[FechaAsignacion]  DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
		CONSTRAINT [PK_CelulaLideres] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_CelulaLideres_Celula]    FOREIGN KEY ([CelulaId])    REFERENCES [dbo].[Celulas]([Id])     ON DELETE CASCADE,
		CONSTRAINT [FK_CelulaLideres_Consultor] FOREIGN KEY ([ConsultorId]) REFERENCES [dbo].[Consultores]([Id]) ON DELETE NO ACTION
	);

	CREATE UNIQUE INDEX [UX_CelulaLideres_CelulaConsultor]
		ON [dbo].[CelulaLideres] ([CelulaId], [ConsultorId]);

	PRINT '✅ Tabla CelulaLideres creada.';
END
GO
