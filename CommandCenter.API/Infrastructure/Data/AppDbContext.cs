using CommandCenter.API.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CommandCenter.API.Infrastructure.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DataTeam
    public DbSet<Consultor> Consultores => Set<Consultor>();
    public DbSet<Celula> Celulas => Set<Celula>();
    public DbSet<CelulaMiembro> CelulaMiembros => Set<CelulaMiembro>();
    public DbSet<CelulaLider> CelulaLideres => Set<CelulaLider>();

    // Transversal a todos los módulos
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Consultor ──────────────────────────────────────────
        builder.Entity<Consultor>(e =>
        {
            e.ToTable("Consultores");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellido).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Cargo).HasMaxLength(150);
            e.Property(x => x.Tecnologia).HasMaxLength(100);
            e.Property(x => x.NivelSeniority).HasMaxLength(50);
            e.Property(x => x.MotivoDeshabilitacion).HasMaxLength(500);
            e.HasQueryFilter(x => x.Activo);
        });

        // ── Celula ─────────────────────────────────────────────
        builder.Entity<Celula>(e =>
        {
            e.ToTable("Celulas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Color).HasMaxLength(20);
            e.HasQueryFilter(x => x.Activo);
        });

        // ── CelulaMiembro ──────────────────────────────────────
        builder.Entity<CelulaMiembro>(e =>
        {
            e.ToTable("CelulaMiembros");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Celula).WithMany(c => c.Miembros).HasForeignKey(x => x.CelulaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Consultor).WithMany(c => c.Celulas).HasForeignKey(x => x.ConsultorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CelulaId, x.ConsultorId }).IsUnique();
        });

        // ── CelulaLider ────────────────────────────────────────
        builder.Entity<CelulaLider>(e =>
        {
            e.ToTable("CelulaLideres");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Celula).WithMany(c => c.Lideres).HasForeignKey(x => x.CelulaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Consultor).WithMany(c => c.CelulasLideradas).HasForeignKey(x => x.ConsultorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CelulaId, x.ConsultorId }).IsUnique();
        });

        // ── AuditoriaLog ───────────────────────────────────────
        builder.Entity<AuditoriaLog>(e =>
        {
            e.ToTable("AuditoriaLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Modulo).HasMaxLength(100).IsRequired();
            e.Property(x => x.Accion).HasMaxLength(50).IsRequired();
            e.Property(x => x.Entidad).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Modulo);
            e.HasIndex(x => x.FechaCreacion);
        });

        // ── Notificacion ───────────────────────────────────────
        builder.Entity<Notificacion>(e =>
        {
            e.ToTable("Notificaciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Destinatario).HasMaxLength(200).IsRequired();
            e.Property(x => x.Asunto).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.Enviado);
            e.HasIndex(x => x.FechaProgramada);
        });
    }
}
