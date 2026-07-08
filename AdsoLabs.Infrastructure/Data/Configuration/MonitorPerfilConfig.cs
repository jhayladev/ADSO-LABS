using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class MonitorPerfilConfig : IEntityTypeConfiguration<MonitorPerfil>
{
    public void Configure(EntityTypeBuilder<MonitorPerfil> e)
    {
        e.ToTable("MonitorPerfil", "adso");
        e.HasKey(x => x.IdMonitor);
        e.Property(x => x.IdMonitor)
            .HasColumnName("id_monitor");
        e.Property(x => x.IdAprendiz)
            .HasColumnName("id_aprendiz");
        e.Property(x => x.Activo)
            .HasColumnName("activo").HasDefaultValue(true);
        e.Property(x => x.FechaAsignacion)
            .HasColumnName("fecha_asignacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.IdAprendiz)
            .HasFilter("[activo] = 1")
            .IsUnique()
            .HasDatabaseName("UX_MonitorPerfil_Aprendiz_Activo");

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_MonitorPerfil_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
