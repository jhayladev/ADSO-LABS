using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class PatrocinioConfig : IEntityTypeConfiguration<Patrocinio>
{
    public void Configure(EntityTypeBuilder<Patrocinio> e)
    {
        e.ToTable("Patrocinio", "adso", t =>
            t.HasCheckConstraint("CK_Patrocinio_etapa",
                "etapa IN ('Lectiva','Productiva')")
        );
        e.HasKey(x => x.IdPatrocinio);
        e.Property(x => x.IdPatrocinio)
            .HasColumnName("id_patrocinio");
        e.Property(x => x.IdAprendiz)
            .HasColumnName("id_aprendiz");
        e.Property(x => x.IdFicha)
            .HasColumnName("id_ficha");
        e.Property(x => x.Activo)
            .HasColumnName("activo").HasDefaultValue(true);
        e.Property(x => x.Etapa)
            .HasColumnName("etapa").HasMaxLength(12);
        e.Property(x => x.FechaInicioEtapa)
            .HasColumnName("fecha_inicio_etapa");
        e.Property(x => x.FechaFinEtapa)
            .HasColumnName("fecha_fin_etapa");
        e.Property(x => x.HoraInicio)
            .HasColumnName("hora_inicio").HasPrecision(0);
        e.Property(x => x.HoraFin)
            .HasColumnName("hora_fin").HasPrecision(0);
        e.Property(x => x.NombreEmpresa)
            .HasColumnName("nombre_empresa").HasMaxLength(200);
        e.Property(x => x.ContactoEmpresa)
            .HasColumnName("contacto_empresa").HasMaxLength(200);
        e.Property(x => x.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => new { x.IdAprendiz, x.IdFicha })
            .HasFilter("[activo] = 1")
            .IsUnique()
            .HasDatabaseName("UX_Patrocinio_Aprendiz_Ficha_Activo");
        e.HasIndex(x => x.IdFicha)
            .HasDatabaseName("IX_Patrocinio_Ficha");

        e.HasOne(x => x.Aprendiz)
            .WithMany(a => a.Patrocinios)
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_Patrocinio_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Ficha)
            .WithMany(f => f.Patrocinios)
            .HasForeignKey(x => x.IdFicha)
            .HasConstraintName("FK_Patrocinio_Ficha")
            .OnDelete(DeleteBehavior.Restrict);
    }
}