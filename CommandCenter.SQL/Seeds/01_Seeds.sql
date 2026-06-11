-- ============================================================
-- CommandCenter.SQL / Seeds / 01_RolesYAdmin.sql
-- Seed inicial: Roles y usuario SuperAdmin
-- ⚠️ SOLO ejecutar en entorno nuevo. NO en producción con datos.
-- ============================================================

-- Roles de Identity (si no existen)
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'SuperAdmin')
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
	VALUES (NEWID(), 'SuperAdmin', 'SUPERADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
	VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Lider')
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
	VALUES (NEWID(), 'Lider', 'LIDER', NEWID());

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'User')
	INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
	VALUES (NEWID(), 'User', 'USER', NEWID());

PRINT '✅ Roles insertados.';
GO

-- ============================================================
-- CommandCenter.SQL / Seeds / 02_CelulasBase.sql
-- Células iniciales del módulo DataTeam
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM [dbo].[Celulas] WHERE [Nombre] = 'Célula Backend')
BEGIN
	INSERT INTO [dbo].[Celulas] ([Nombre], [Descripcion], [Color], [Activo], [FechaCreacion])
	VALUES
		('Célula Backend',  'Equipo de desarrollo backend .NET',     '#0d6efd', 1, GETUTCDATE()),
		('Célula Frontend', 'Equipo de desarrollo frontend React',   '#198754', 1, GETUTCDATE()),
		('Célula DevOps',   'Equipo de infraestructura y pipelines', '#dc3545', 1, GETUTCDATE()),
		('Célula QA',       'Equipo de calidad y testing',           '#ffc107', 1, GETUTCDATE()),
		('Célula Data',     'Equipo de datos y analítica',           '#6f42c1', 1, GETUTCDATE());

	PRINT '✅ Células base insertadas.';
END
GO
