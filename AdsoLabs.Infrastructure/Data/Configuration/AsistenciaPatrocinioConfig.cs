using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class AsistenciaPatrocinioConfig : IEntityTypeConfiguration<AsistenciaPatrocinio>
{
    public void Configure(EntityTypeBuilder<AsistenciaPatrocinio> e)
    {
        e.ToTable("AsistenciaPatrocinio", "adso", t =>
            t.HasCheckConstraint("CK_AsistenciaPatrocinio_estado",
                "estado IN ('Presente','Ausente','Justificado')")
        );
        e.HasKey(x => x.IdAsistenciaPatrocinio);
        e.Property(x => x.IdAsistenciaPatrocinio).HasColumnName("id_asistencia_patrocinio");
        e.Property(x => x.IdPatrocinio).HasColumnName("id_patrocinio");
        e.Property(x => x.Fecha).HasColumnName("fecha");
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(15);
        e.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        e.Property(x => x.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasPrecision(0)
            .HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => new { x.IdPatrocinio, x.Fecha })
            .IsUnique()
            .HasDatabaseName("UX_AsistenciaPatrocinio_Patrocinio_Fecha");
        e.HasIndex(x => x.Fecha)
            .HasDatabaseName("IX_AsistenciaPatrocinio_Fecha");

        e.HasOne(x => x.Patrocinio)
            .WithMany(p => p.Asistencias)
            .HasForeignKey(x => x.IdPatrocinio)
            .HasConstraintName("FK_AsistenciaPatrocinio_Patrocinio")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
