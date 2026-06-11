-- ============================================================
-- CommandCenter.SQL / StoredProcedures / SP_GetOrganigramaCompleto.sql
-- Retorna células con sus líderes y miembros para el organigrama
-- ============================================================

CREATE OR ALTER PROCEDURE [dbo].[SP_GetOrganigramaCompleto]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		c.Id           AS CelulaId,
		c.Nombre       AS CelulaNombre,
		c.Color        AS CelulaColor,
		c.ImagenUrl    AS CelulaImagenUrl,
		c.Descripcion  AS CelulaDescripcion,
		-- Líder
		cl.ConsultorId AS LiderId,
		con_l.Nombre + ' ' + con_l.Apellido AS LiderNombre,
		con_l.Cargo    AS LiderCargo,
		-- Miembro
		cm.ConsultorId AS MiembroId,
		con_m.Nombre + ' ' + con_m.Apellido AS MiembroNombre,
		con_m.Cargo    AS MiembroCargo,
		con_m.Tecnologia AS MiembroTecnologia,
		con_m.FotoUrl  AS MiembroFoto
	FROM [dbo].[Celulas] c
	LEFT JOIN [dbo].[CelulaLideres]  cl    ON cl.CelulaId    = c.Id
	LEFT JOIN [dbo].[Consultores]    con_l ON con_l.Id        = cl.ConsultorId AND con_l.Activo = 1
	LEFT JOIN [dbo].[CelulaMiembros] cm    ON cm.CelulaId    = c.Id
	LEFT JOIN [dbo].[Consultores]    con_m ON con_m.Id        = cm.ConsultorId AND con_m.Activo = 1
	WHERE c.Activo = 1
	ORDER BY c.Nombre, con_m.Apellido, con_m.Nombre;
END
GO
