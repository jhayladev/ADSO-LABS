using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class HistorialEstadoAprendizConfig : IEntityTypeConfiguration<HistorialEstadoAprendiz>
{
    public void Configure(EntityTypeBuilder<HistorialEstadoAprendiz> e)
    {
        e.ToTable("HistorialEstadoAprendiz", "adso");
        e.HasKey(x => x.IdHistorial);
        e.Property(x => x.IdHistorial).HasColumnName("id_historial");
        e.Property(x => x.IdFicha).HasColumnName("id_ficha");
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.EstadoAnterior).HasColumnName("estado_anterior").HasMaxLength(20).IsRequired();
        e.Property(x => x.EstadoNuevo).HasColumnName("estado_nuevo").HasMaxLength(20).IsRequired();
        e.Property(x => x.IdUsuarioCambio).HasColumnName("id_usuario_cambio");
        e.Property(x => x.FechaCambio).HasColumnName("fecha_cambio")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => new { x.IdFicha, x.IdAprendiz }).HasDatabaseName("IX_HistorialEstado_FichaAprendiz");

        e.HasOne(x => x.FichaAprendiz)
            .WithMany()
            .HasForeignKey(x => new { x.IdFicha, x.IdAprendiz })
            .HasConstraintName("FK_HistorialEstado_FichaAprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
