using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class InscripcionMonitoriaConfig : IEntityTypeConfiguration<InscripcionMonitoria>
{
    public void Configure(EntityTypeBuilder<InscripcionMonitoria> e)
    {
        e.ToTable("InscripcionMonitoria", "adso");
        e.HasKey(x => x.IdInscripcion);
        e.Property(x => x.IdInscripcion)
            .HasColumnName("id_inscripcion");
        e.Property(x => x.IdSesionMonitoria)
            .HasColumnName("id_sesion_monitoria");
        e.Property(x => x.IdAprendiz)
            .HasColumnName("id_aprendiz");
        e.Property(x => x.FechaInscripcion)
            .HasColumnName("fecha_inscripcion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => new { x.IdSesionMonitoria, x.IdAprendiz })
            .IsUnique()
            .HasDatabaseName("UX_InscripcionMonitoria_Sesion_Aprendiz");

        e.HasOne(x => x.SesionMonitoria)
            .WithMany(s => s.Inscripciones)
            .HasForeignKey(x => x.IdSesionMonitoria)
            .HasConstraintName("FK_InscripcionMonitoria_Sesion")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_InscripcionMonitoria_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
