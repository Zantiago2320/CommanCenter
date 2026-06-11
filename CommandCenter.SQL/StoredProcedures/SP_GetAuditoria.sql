-- ============================================================
-- CommandCenter.SQL / StoredProcedures / SP_GetAuditoria.sql
-- Consulta de auditoría con filtros para todos los módulos
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[SP_GetAuditoria]
	@Modulo      NVARCHAR(100) = NULL,
	@Accion      NVARCHAR(50)  = NULL,
	@UsuarioId   NVARCHAR(450) = NULL,
	@FechaDesde  DATETIME2     = NULL,
	@FechaHasta  DATETIME2     = NULL,
	@PageNumber  INT           = 1,
	@PageSize    INT           = 50
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		a.Id,
		a.Modulo,
		a.Accion,
		a.Entidad,
		a.EntidadId,
		a.ValorAnterior,
		a.ValorNuevo,
		a.UsuarioEmail,
		a.IpAddress,
		a.Exitoso,
		a.MensajeError,
		a.FechaCreacion,
		COUNT(*) OVER() AS TotalRegistros
	FROM [dbo].[AuditoriaLogs] a
	WHERE (@Modulo    IS NULL OR a.Modulo    = @Modulo)
	  AND (@Accion    IS NULL OR a.Accion    = @Accion)
	  AND (@UsuarioId IS NULL OR a.UsuarioId = @UsuarioId)
	  AND (@FechaDesde IS NULL OR a.FechaCreacion >= @FechaDesde)
	  AND (@FechaHasta IS NULL OR a.FechaCreacion <= @FechaHasta)
	ORDER BY a.FechaCreacion DESC
	OFFSET (@PageNumber - 1) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY;
END
GO
