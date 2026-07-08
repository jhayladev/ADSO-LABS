using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class AcudienteConfig : IEntityTypeConfiguration<Acudiente>
{
    public void Configure(EntityTypeBuilder<Acudiente> e)
    {
        e.ToTable("Acudiente", "adso");
        e.HasKey(x => x.IdAcudiente);
        e.Property(x => x.IdAcudiente).HasColumnName("id_acudiente").ValueGeneratedOnAdd();
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200);
        e.Property(x => x.Parentesco).HasColumnName("parentesco").HasMaxLength(50);
        e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(200);
        e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(20);
        e.Property(x => x.Genero).HasColumnName("genero").HasMaxLength(20);
        e.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(50);
        e.Property(x => x.NumeroDocumento).HasColumnName("numero_documento").HasMaxLength(30);
        e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(300);
        e.Property(x => x.Estrato).HasColumnName("estrato");
        e.Property(x => x.FechaRegistro).HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        // Un aprendiz → un acudiente
        e.HasIndex(x => x.IdAprendiz).IsUnique();

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_Acudiente_AprendizPerfil")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ObservacionAprendizConfig : IEntityTypeConfiguration<ObservacionAprendiz>
{
    public void Configure(EntityTypeBuilder<ObservacionAprendiz> e)
    {
        e.ToTable("ObservacionAprendiz", "adso");
        e.HasKey(x => x.IdObservacion);
        e.Property(x => x.IdObservacion).HasColumnName("id_observacion").ValueGeneratedOnAdd();
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
        e.Property(x => x.Fecha).HasColumnName("fecha")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");

        e.HasIndex(x => new { x.IdAprendiz, x.Fecha });

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_ObservacionAprendiz_AprendizPerfil")
            .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.IdUsuario)
            .HasConstraintName("FK_ObservacionAprendiz_Usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
