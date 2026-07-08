using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class FichaCompetenciaResultadoConfig
    : IEntityTypeConfiguration<FichaCompetenciaResultado>
{
    public void Configure(EntityTypeBuilder<FichaCompetenciaResultado> e)
    {
        e.ToTable("FichaCompetenciaResultado", "adso", t =>
            t.HasCheckConstraint("CK_FCR_estado", "estado IN ('Pendiente','Programado','En Curso','Completado')")
        );
        e.HasKey(x => x.IdFichaCompetenciaResultado);
        e.Property(x => x.IdFichaCompetenciaResultado)
            .HasColumnName("id_fcresultado");
        e.Property(x => x.IdFichaCompetencia)
            .HasColumnName("id_ficha_competencia");
        e.Property(x => x.IdResultado)
            .HasColumnName("id_resultado");
        e.Property(x => x.IdInstructor)
            .HasColumnName("id_instructor");
        e.Property(x => x.FechaInicio)
            .HasColumnName("fecha_inicio");
        e.Property(x => x.FechaFin)
            .HasColumnName("fecha_fin");
        e.Property(x => x.HorasProgramadas)
            .HasColumnName("horas_programadas");
        e.Property(x => x.HoraInicio)
            .HasColumnName("hora_inicio").HasPrecision(0);
        e.Property(x => x.HoraFin)
            .HasColumnName("hora_fin").HasPrecision(0);
        e.Property(x => x.Estado)
            .HasColumnName("estado").HasMaxLength(12)
            .HasDefaultValue("Pendiente");

        e.HasIndex(x => x.IdFichaCompetencia)
            .HasDatabaseName("IX_FCR_FichaCompetencia");
        e.HasIndex(x => x.IdResultado)
            .HasDatabaseName("IX_FCR_Resultado");
        e.HasIndex(x => new { x.IdInstructor, x.FechaInicio, x.FechaFin })
            .HasDatabaseName("IX_FCR_Instructor_Fecha");
        e.HasIndex(x => new { x.IdFichaCompetencia, x.IdResultado })
            .IsUnique().HasDatabaseName("UX_FCR_FC_Resultado");

        e.HasOne(x => x.FichaCompetencia)
            .WithMany(fc => fc.ResultadosProgramados)
            .HasForeignKey(x => x.IdFichaCompetencia)
            .HasConstraintName("FK_FCR_FichaCompetencia")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Resultado)
            .WithMany(r => r.ProgramacionesFicha)
            .HasForeignKey(x => x.IdResultado)
            .HasConstraintName("FK_FCR_Resultado")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor)
            .WithMany(i => i.ResultadosProgramados)
            .HasForeignKey(x => x.IdInstructor)
            .HasConstraintName("FK_FCR_Instructor")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}