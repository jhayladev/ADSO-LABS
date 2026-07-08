using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class PersonaConfig : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> e)
    {
        e.ToTable("Persona", "adso");
        e.HasKey(x => x.IdPersona);
        e.Property(x => x.IdPersona).HasColumnName("id_persona");
        e.Property(x => x.IdTipoDocumento).HasColumnName("id_tipo_documento");
        e.Property(x => x.NumeroDocumento).HasColumnName("numero_documento").HasMaxLength(40).IsRequired();
        e.Property(x => x.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
        e.Property(x => x.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
        e.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(40);
        e.Property(x => x.FechaNacimiento).HasColumnName("fecha_nacimiento");
        e.Property(x => x.Direccion).HasColumnName("direccion").HasMaxLength(200);
        e.Property(x => x.Municipio).HasColumnName("municipio").HasMaxLength(100);
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.NumeroDocumento).IsUnique().HasDatabaseName("UX_Persona_Documento");

        e.HasOne(x => x.TipoDocumento)
            .WithMany(t => t.Personas)
            .HasForeignKey(x => x.IdTipoDocumento)
            .HasConstraintName("FK_Persona_TipoDoc")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> e)
    {
        e.ToTable("Usuario", "adso");
        e.HasKey(x => x.IdUsuario);
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.IdPersona).HasColumnName("id_persona");
        e.Property(x => x.IdRol).HasColumnName("id_rol");
        e.Property(x => x.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
        e.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(256);
        e.Property(x => x.PasswordSalt).HasColumnName("password_salt").HasMaxLength(128);
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        e.Property(x => x.PrimerLogin).HasColumnName("primer_login").HasDefaultValue(true);
        e.Property(x => x.IntentosFallidos).HasColumnName("intentos_fallidos").HasDefaultValue(0);
        e.Property(x => x.BloqueadoHasta).HasColumnName("bloqueado_hasta").HasPrecision(0);
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.FechaActualizacion).HasColumnName("fecha_actualizacion").HasPrecision(0);
        e.Property(x => x.FechaInicioSesion).HasColumnName("fecha_inicio_sesion").HasPrecision(0);
        e.Property(x => x.ConsentimientoFecha).HasColumnName("consentimiento_fecha").HasPrecision(0);

        e.HasIndex(x => x.IdPersona).IsUnique().HasDatabaseName("UX_Usuario_Persona");
        e.HasIndex(x => x.Correo).IsUnique().HasDatabaseName("UX_Usuario_Correo");

        e.HasOne(x => x.Persona)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Usuario>(x => x.IdPersona)
            .HasConstraintName("FK_Usuario_Persona")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(x => x.IdRol)
            .HasConstraintName("FK_Usuario_Rol")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PasswordResetTokenConfig : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> e)
    {
        e.ToTable("PasswordResetToken", "adso");
        e.HasKey(x => x.IdToken);
        e.Property(x => x.IdToken).HasColumnName("id_token");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(256).IsRequired();
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.FechaExpiracion).HasColumnName("fecha_expiracion").HasPrecision(0);
        e.Property(x => x.FechaUso).HasColumnName("fecha_uso").HasPrecision(0);

        e.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("UX_PRT_TokenHash");
        e.HasIndex(x => x.IdUsuario).HasDatabaseName("IX_PRT_Usuario");

        e.HasOne(x => x.Usuario)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(x => x.IdUsuario)
            .HasConstraintName("FK_PRT_Usuario")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
