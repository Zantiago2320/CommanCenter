-- ============================================================
-- CommandCenter.SQL / Views / VW_OrganigramaResumen.sql
-- Vista de resumen del organigrama para reportes y dashboards
-- ============================================================
CREATE OR ALTER VIEW [dbo].[VW_OrganigramaResumen] AS
SELECT
    c.Id           AS CelulaId,
    c.Nombre       AS CelulaNombre,
    c.Color        AS Color,
    c.Activo       AS Activa,
    COUNT(DISTINCT cm.ConsultorId)  AS TotalMiembros,
    COUNT(DISTINCT cl.ConsultorId)  AS TotalLideres,
    STRING_AGG(
        con_l.Nombre + ' ' + con_l.Apellido, ', '
    ) AS Lideres
FROM [dbo].[Celulas] c
LEFT JOIN [dbo].[CelulaMiembros] cm   ON cm.CelulaId = c.Id
LEFT JOIN [dbo].[CelulaLideres]  cl   ON cl.CelulaId = c.Id
LEFT JOIN [dbo].[Consultores]    con_l ON con_l.Id    = cl.ConsultorId AND con_l.Activo = 1
WHERE c.Activo = 1
GROUP BY c.Id, c.Nombre, c.Color, c.Activo;
GO
-- ============================================================
-- CommandCenter.SQL / Views / VW_ConsultoresActivos.sql
-- ============================================================
CREATE OR ALTER VIEW [dbo].[VW_ConsultoresActivos] AS
SELECT
    con.Id,
    con.Nombre,
    con.Apellido,
    con.Nombre + ' ' + con.Apellido AS NombreCompleto,
    con.Email,
    con.Cargo,
    con.Tecnologia,
    con.NivelSeniority,
    con.FechaIngreso,
    con.FechaNacimiento,
    c.Nombre  AS CelulaNombre,
    c.Color   AS CelulaColor
FROM [dbo].[Consultores] con
LEFT JOIN [dbo].[CelulaMiembros] cm ON cm.ConsultorId = con.Id
LEFT JOIN [dbo].[Celulas]        c  ON c.Id           = cm.CelulaId AND c.Activo = 1
WHERE con.Activo = 1 AND con.Habilitado = 1;
GO