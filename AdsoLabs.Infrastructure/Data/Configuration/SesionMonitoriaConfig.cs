using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class SesionMonitoriaConfig : IEntityTypeConfiguration<SesionMonitoria>
{
    public void Configure(EntityTypeBuilder<SesionMonitoria> e)
    {
        e.ToTable("SesionMonitoria", "adso", t =>
        {
            t.HasCheckConstraint("CK_SesionMonitoria_jornada",
                "jornada IN ('Mañana','Tarde')");
            t.HasCheckConstraint("CK_SesionMonitoria_modalidad",
                "modalidad IN ('Presencial','Virtual')");
            t.HasCheckConstraint("CK_SesionMonitoria_estado",
                "estado IN ('Activa','Finalizada','Cancelada')");
        });
        e.HasKey(x => x.IdSesionMonitoria);
        e.Property(x => x.IdSesionMonitoria)
            .HasColumnName("id_sesion_monitoria");
        e.Property(x => x.IdMonitor)
            .HasColumnName("id_monitor");
        e.Property(x => x.Nombre)
            .HasColumnName("nombre").HasMaxLength(150);
        e.Property(x => x.Descripcion)
            .HasColumnName("descripcion").HasMaxLength(1000);
        e.Property(x => x.Jornada)
            .HasColumnName("jornada").HasMaxLength(10);
        e.Property(x => x.Modalidad)
            .HasColumnName("modalidad").HasMaxLength(12);
        e.Property(x => x.HoraInicio)
            .HasColumnName("hora_inicio").HasPrecision(0);
        e.Property(x => x.HoraFin)
            .HasColumnName("hora_fin").HasPrecision(0);
        e.Property(x => x.FechaInicio)
            .HasColumnName("fecha_inicio");
        e.Property(x => x.FechaFin)
            .HasColumnName("fecha_fin");
        e.Property(x => x.Estado)
            .HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Activa");
        e.Property(x => x.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.IdMonitor)
            .HasDatabaseName("IX_SesionMonitoria_Monitor");

        e.HasOne(x => x.Monitor)
            .WithMany(m => m.Sesiones)
            .HasForeignKey(x => x.IdMonitor)
            .HasConstraintName("FK_SesionMonitoria_Monitor")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
