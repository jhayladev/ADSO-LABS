using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class TipoDocumentoConfig : IEntityTypeConfiguration<TipoDocumento>
{
    public void Configure(EntityTypeBuilder<TipoDocumento> e)
    {
        e.ToTable("TipoDocumento", "adso");
        e.HasKey(x => x.IdTipoDocumento);
        e.Property(x => x.IdTipoDocumento).HasColumnName("id_tipo_documento");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(30).IsRequired();
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class RolConfig : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> e)
    {
        e.ToTable("Rol", "adso");
        e.HasKey(x => x.IdRol);
        e.Property(x => x.IdRol).HasColumnName("id_rol");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(250);
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class TipoArchivoConfig : IEntityTypeConfiguration<TipoArchivo>
{
    public void Configure(EntityTypeBuilder<TipoArchivo> e)
    {
        e.ToTable("TipoArchivo", "adso");
        e.HasKey(x => x.IdTipoArchivo);
        e.Property(x => x.IdTipoArchivo).HasColumnName("id_tipo_archivo");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Nombre).IsUnique();
    }
}
