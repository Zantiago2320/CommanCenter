# CommandCenter.SQL

Repositorio de scripts T-SQL puros para la base de datos del Command Center.

## Estructura

```
CommandCenter.SQL/
├── Tables/                     ← CREATE TABLE scripts
│   ├── 01_Consultores.sql
│   ├── 02_Celulas.sql
│   └── 03_AuditoriaLogs_Notificaciones.sql
├── Views/
│   └── VW_Organigrama_Consultores.sql
├── StoredProcedures/
│   ├── SP_GetOrganigramaCompleto.sql
│   └── SP_GetAuditoria.sql
├── Seeds/
│   └── 01_Seeds.sql            ← ⚠️ Solo para entornos nuevos
└── README.md
```

## Orden de ejecución en servidor nuevo

```sql
-- 1. Tablas (EF Core lo hace automático con migrate, pero esto es el backup)
-- Tables/01_Consultores.sql
-- Tables/02_Celulas.sql
-- Tables/03_AuditoriaLogs_Notificaciones.sql

-- 2. Vistas
-- Views/VW_Organigrama_Consultores.sql

-- 3. Stored Procedures
-- StoredProcedures/SP_GetOrganigramaCompleto.sql
-- StoredProcedures/SP_GetAuditoria.sql

-- 4. Seeds (solo primera vez)
-- Seeds/01_Seeds.sql
```

## Reglas

- ❌ Nunca colocar código C# aquí
- ❌ Nunca colocar HTML aquí
- ✅ Solo T-SQL puro
- ✅ Todos los scripts deben ser idempotentes (IF NOT EXISTS)
