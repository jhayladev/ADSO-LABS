using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class AsistenciaMonitoriaConfig : IEntityTypeConfiguration<AsistenciaMonitoria>
{
    public void Configure(EntityTypeBuilder<AsistenciaMonitoria> e)
    {
        e.ToTable("AsistenciaMonitoria", "adso", t =>
            t.HasCheckConstraint("CK_AsistenciaMonitoria_estado",
                "estado IN ('Presente','Justificado','Ausente')")
        );
        e.HasKey(x => x.IdAsistenciaMonitoria);
        e.Property(x => x.IdAsistenciaMonitoria)
            .HasColumnName("id_asistencia_monitoria");
        e.Property(x => x.IdSesionMonitoria)
            .HasColumnName("id_sesion_monitoria");
        e.Property(x => x.IdAprendiz)
            .HasColumnName("id_aprendiz");
        e.Property(x => x.Fecha)
            .HasColumnName("fecha");
        e.Property(x => x.Estado)
            .HasColumnName("estado").HasMaxLength(12);
        e.Property(x => x.IdInstructorDestinatario)
            .HasColumnName("id_instructor_destinatario");
        e.Property(x => x.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => new { x.IdSesionMonitoria, x.IdAprendiz, x.Fecha })
            .IsUnique()
            .HasDatabaseName("UX_AsistenciaMonitoria_Sesion_Aprendiz_Fecha");

        e.HasOne(x => x.SesionMonitoria)
            .WithMany(s => s.Asistencias)
            .HasForeignKey(x => x.IdSesionMonitoria)
            .HasConstraintName("FK_AsistenciaMonitoria_Sesion")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_AsistenciaMonitoria_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.InstructorDestinatario)
            .WithMany()
            .HasForeignKey(x => x.IdInstructorDestinatario)
            .HasConstraintName("FK_AsistenciaMonitoria_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
