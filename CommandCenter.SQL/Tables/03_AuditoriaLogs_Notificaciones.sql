-- ============================================================
-- CommandCenter.SQL / Tables / 03_AuditoriaLogs.sql
-- Transversal a TODOS los módulos del Command Center
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditoriaLogs')
BEGIN
    CREATE TABLE [dbo].[AuditoriaLogs] (
        [Id]             INT IDENTITY(1,1)  NOT NULL,
        [Modulo]         NVARCHAR(100)       NOT NULL,
        [Accion]         NVARCHAR(50)        NOT NULL,
        [Entidad]        NVARCHAR(100)       NOT NULL,
        [EntidadId]      NVARCHAR(100)       NULL,
        [ValorAnterior]  NVARCHAR(MAX)       NULL,
        [ValorNuevo]     NVARCHAR(MAX)       NULL,
        [UsuarioId]      NVARCHAR(450)       NULL,
        [UsuarioEmail]   NVARCHAR(200)       NULL,
        [IpAddress]      NVARCHAR(50)        NULL,
        [UserAgent]      NVARCHAR(500)       NULL,
        [Exitoso]        BIT                 NOT NULL DEFAULT 1,
        [MensajeError]   NVARCHAR(MAX)       NULL,
        [Activo]         BIT                 NOT NULL DEFAULT 1,
        [FechaCreacion]  DATETIME2           NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2        NULL,
        [CreadoPor]      NVARCHAR(450)       NULL,
        [ModificadoPor]  NVARCHAR(450)       NULL,
        CONSTRAINT [PK_AuditoriaLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE INDEX [IX_AuditoriaLogs_Modulo]        ON [dbo].[AuditoriaLogs] ([Modulo]);
    CREATE INDEX [IX_AuditoriaLogs_FechaCreacion] ON [dbo].[AuditoriaLogs] ([FechaCreacion] DESC);
    CREATE INDEX [IX_AuditoriaLogs_UsuarioId]     ON [dbo].[AuditoriaLogs] ([UsuarioId]);
    PRINT '✅ Tabla AuditoriaLogs creada.';
END
GO
-- ============================================================
-- CommandCenter.SQL / Tables / 04_Notificaciones.sql
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notificaciones')
BEGIN
    CREATE TABLE [dbo].[Notificaciones] (
        [Id]               INT IDENTITY(1,1) NOT NULL,
        [Modulo]           NVARCHAR(100)      NOT NULL,
        [Tipo]             NVARCHAR(50)       NOT NULL,
        [Destinatario]     NVARCHAR(200)      NOT NULL,
        [Asunto]           NVARCHAR(300)      NOT NULL,
        [Cuerpo]           NVARCHAR(MAX)      NOT NULL,
        [Enviado]          BIT                NOT NULL DEFAULT 0,
        [FechaEnvio]       DATETIME2          NULL,
        [FechaProgramada]  DATETIME2          NULL,
        [Intentos]         INT                NOT NULL DEFAULT 0,
        [ErrorMensaje]     NVARCHAR(MAX)      NULL,
        [AdjuntoUrl]       NVARCHAR(500)      NULL,
        [Activo]           BIT                NOT NULL DEFAULT 1,
        [FechaCreacion]    DATETIME2          NOT NULL DEFAULT GETUTCDATE(),
        [FechaModificacion] DATETIME2         NULL,
        [CreadoPor]        NVARCHAR(450)      NULL,
        [ModificadoPor]    NVARCHAR(450)      NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE INDEX [IX_Notificaciones_Enviado]          ON [dbo].[Notificaciones] ([Enviado]);
    CREATE INDEX [IX_Notificaciones_FechaProgramada]  ON [dbo].[Notificaciones] ([FechaProgramada]);
    PRINT '✅ Tabla Notificaciones creada.';
END
GO