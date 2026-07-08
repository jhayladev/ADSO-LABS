using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class SolicitudAsignacionConfig : IEntityTypeConfiguration<SolicitudAsignacion>
{
    public void Configure(EntityTypeBuilder<SolicitudAsignacion> e)
    {
        e.ToTable("SolicitudAsignacion", "adso", t =>
            t.HasCheckConstraint("CK_SA_estado", "estado IN ('Pendiente','Aprobada','Rechazada')")
        );
        e.HasKey(x => x.IdSolicitud);
        e.Property(x => x.IdSolicitud)
            .HasColumnName("id_solicitud");
        e.Property(x => x.IdInstructor)
            .HasColumnName("id_instructor");
        e.Property(x => x.IdFichaCompetencia)
            .HasColumnName("id_ficha_competencia");
        e.Property(x => x.Estado)
            .HasColumnName("estado").HasMaxLength(12)
            .HasDefaultValue("Pendiente");
        e.Property(x => x.RazonamientoIA)
            .HasColumnName("razonamiento_ia").HasMaxLength(2000);
        e.Property(x => x.PorcentajeCompatibilidad)
            .HasColumnName("porcentaje_compatibilidad");
        e.Property(x => x.FechaSolicitud)
            .HasColumnName("fecha_solicitud");
        e.Property(x => x.FechaRespuesta)
            .HasColumnName("fecha_respuesta");
        e.Property(x => x.ObservacionAdmin)
            .HasColumnName("observacion_admin").HasMaxLength(500);

        e.HasIndex(x => x.IdInstructor)
            .HasDatabaseName("IX_SA_Instructor");
        e.HasIndex(x => x.IdFichaCompetencia)
            .HasDatabaseName("IX_SA_FichaCompetencia");

        e.HasOne(x => x.Instructor)
            .WithMany()
            .HasForeignKey(x => x.IdInstructor)
            .HasConstraintName("FK_SA_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.FichaCompetencia)
            .WithMany()
            .HasForeignKey(x => x.IdFichaCompetencia)
            .HasConstraintName("FK_SA_FichaCompetencia")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
