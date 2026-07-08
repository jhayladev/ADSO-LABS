using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class SesionConfig : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> e)
    {
        e.ToTable("Sesion", "adso", t =>
        {
            t.HasCheckConstraint("CK_Sesion_estado",
                "estado IN ('Abierta','Finalizada','Cancelada')");
        });
        e.HasKey(x => x.IdSesion);
        e.Property(x => x.IdSesion).HasColumnName("id_sesion");
        e.Property(x => x.IdFichaCompetencia).HasColumnName("id_ficha_competencia");
        e.Property(x => x.IdFichaCompetenciaResultado).HasColumnName("id_ficha_competencia_resultado");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.Fecha).HasColumnName("fecha");
        e.Property(x => x.HoraInicio).HasColumnName("hora_inicio").HasPrecision(0);
        e.Property(x => x.HoraFin).HasColumnName("hora_fin").HasPrecision(0);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(15).HasDefaultValue("Abierta");

        e.HasOne(x => x.FichaCompetencia)
            .WithMany(fc => fc.Sesiones)
            .HasForeignKey(x => x.IdFichaCompetencia)
            .HasConstraintName("FK_Sesion_FC")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.FichaCompetenciaResultado)
            .WithMany()
            .HasForeignKey(x => x.IdFichaCompetenciaResultado)
            .HasConstraintName("FK_Sesion_FCR")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Instructor)
            .WithMany(i => i.Sesiones)
            .HasForeignKey(x => x.IdInstructor)
            .HasConstraintName("FK_Sesion_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AsistenciaConfig : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> e)
    {
        e.ToTable("Asistencia", "adso", t =>
        {
            t.HasCheckConstraint("CK_Asistencia_estado",
                "estado IN ('Presente','Ausente','Justificado','Tarde')");
        });
        e.HasKey(x => x.IdAsistencia);
        e.Property(x => x.IdAsistencia).HasColumnName("id_asistencia");
        e.Property(x => x.IdSesion).HasColumnName("id_sesion");
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(15);
        e.Property(x => x.MotivoInasistencia).HasColumnName("motivo_inasistencia");

        e.HasIndex(x => x.IdAprendiz).HasDatabaseName("IX_Asistencia_Aprendiz");
        e.HasIndex(x => new { x.IdSesion, x.IdAprendiz }).IsUnique()
            .HasDatabaseName("UQ_Asistencia");

        e.HasOne(x => x.Sesion)
            .WithMany(s => s.Asistencias)
            .HasForeignKey(x => x.IdSesion)
            .HasConstraintName("FK_Asistencia_Sesion")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Aprendiz)
            .WithMany(a => a.Asistencias)
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_Asistencia_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
