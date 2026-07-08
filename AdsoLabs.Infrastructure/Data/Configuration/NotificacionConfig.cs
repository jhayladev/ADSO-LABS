using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class NotificacionConfig : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> e)
    {
        e.ToTable("Notificacion", "adso");
        e.HasKey(x => x.IdNotificacion);
        e.Property(x => x.IdNotificacion).HasColumnName("id_notificacion").ValueGeneratedOnAdd();
        e.Property(x => x.IdUsuarioDestinatario).HasColumnName("id_usuario_destinatario");
        e.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(50);
        e.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(100);
        e.Property(x => x.Mensaje).HasColumnName("mensaje").HasMaxLength(500);
        e.Property(x => x.Leida).HasColumnName("leida").HasDefaultValue(false);
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.FechaLectura).HasColumnName("fecha_lectura").HasPrecision(0);
        e.Property(x => x.ReferenciaId).HasColumnName("referencia_id");
        e.Property(x => x.ReferenciaTipo).HasColumnName("referencia_tipo").HasMaxLength(50);

        e.HasIndex(x => new { x.IdUsuarioDestinatario, x.Leida, x.FechaCreacion });

        e.HasOne(x => x.UsuarioDestinatario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuarioDestinatario)
            .HasConstraintName("FK_Notificacion_Usuario")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
