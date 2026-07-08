using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class AuditoriaConfig : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> e)
    {
        e.ToTable("Auditoria", "adso");
        e.HasKey(x => x.IdAuditoria);
        e.Property(x => x.IdAuditoria).HasColumnName("id_auditoria");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.Accion).HasColumnName("accion").HasMaxLength(10).IsRequired();
        e.Property(x => x.TablaAfectada).HasColumnName("tabla_afectada").HasMaxLength(128).IsRequired();
        e.Property(x => x.IdRegistroAfectado).HasColumnName("id_registro_afectado").HasMaxLength(128).IsRequired();
        e.Property(x => x.DatosViejos).HasColumnName("datos_viejos");
        e.Property(x => x.DatosNuevos).HasColumnName("datos_nuevos");
        e.Property(x => x.Fecha).HasColumnName("fecha")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.DireccionIp).HasColumnName("direccion_ip").HasMaxLength(45);
        // Sin FK a Usuario: la auditoría debe sobrevivir aunque se elimine el usuario
    }
}
