using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class CompetenciaConfig : IEntityTypeConfiguration<Competencia>
{
    public void Configure(EntityTypeBuilder<Competencia> e)
    {
        e.ToTable("Competencia", "adso", t =>
        {
            t.HasCheckConstraint("CK_Competencia_tipo", "tipo IN ('Tecnica','Transversal','Clave','Induccion','Practica')");
            t.HasCheckConstraint("CK_Competencia_horas", "horas_asignadas > 0");
            t.HasCheckConstraint("CK_Competencia_estado", "estado IN ('Activa','Clausurada')");
            t.HasCheckConstraint("CK_Competencia_fase_formativa",
                "fase_formativa IS NULL OR fase_formativa IN ('Induccion','Analisis','Planeacion','Ejecucion','Evaluacion','Transversal')");
        });
        e.HasKey(x => x.IdCompetencia);
        e.Property(x => x.IdCompetencia).HasColumnName("id_competencia");
        e.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.HorasAsignadas).HasColumnName("horas_asignadas");
        e.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(12).HasDefaultValue("Tecnica");
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Activa");
        e.Property(x => x.FaseFormativa).HasColumnName("fase_formativa").HasMaxLength(12);

        e.HasIndex(x => x.Codigo).IsUnique();
        
        
    }
}

public class FichaConfig : IEntityTypeConfiguration<Ficha>
{
    public void Configure(EntityTypeBuilder<Ficha> e)
    {
        e.ToTable("Ficha", "adso", t =>
            t.HasCheckConstraint("CK_Ficha_estado", "estado IN ('Activa','Finalizada', 'Suspendida')")
        );
        e.HasKey(x => x.IdFicha);
        e.Property(x => x.IdFicha).HasColumnName("id_ficha");
        e.Property(x => x.NumeroFicha).HasColumnName("numero_ficha").HasMaxLength(30).IsRequired();
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        e.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20);
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.NumeroFicha).IsUnique();
        
    }
}

public class FichaCompetenciaConfig : IEntityTypeConfiguration<FichaCompetencia>
{
    public void Configure(EntityTypeBuilder<FichaCompetencia> e)
    {
        e.ToTable("FichaCompetencia", "adso", t =>
            t.HasCheckConstraint("CK_FC_estado", "estado IN ('Pendiente','En Curso','Programada','Vista')")
        );
        e.HasKey(x => x.IdFichaCompetencia);
        e.Property(x => x.IdFichaCompetencia).HasColumnName("id_ficha_competencia");
        e.Property(x => x.IdFicha).HasColumnName("id_ficha");
        e.Property(x => x.IdCompetencia).HasColumnName("id_competencia");
        e.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired(false);
        e.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        e.Property(x => x.TotalHoras).HasColumnName("total_horas");
        e.Property(x => x.HorasEjecutadas).HasColumnName("horas_ejecutadas").HasDefaultValue(0);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12);

        e.HasIndex(x => new { x.IdFicha, x.IdCompetencia }).IsUnique()
            .HasDatabaseName("UX_FC_Ficha_Competencia");

        e.HasOne(x => x.Ficha)
            .WithMany(f => f.FichasCompetencias)
            .HasForeignKey(x => x.IdFicha)
            .HasConstraintName("FK_FC_Ficha")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Competencia)
            .WithMany(c => c.FichasCompetencias)
            .HasForeignKey(x => x.IdCompetencia)
            .HasConstraintName("FK_FC_Competencia")
            .OnDelete(DeleteBehavior.Restrict);

    }
}
