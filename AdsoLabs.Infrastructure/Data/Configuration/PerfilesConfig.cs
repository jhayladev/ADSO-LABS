using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class AprendizPerfilConfig : IEntityTypeConfiguration<AprendizPerfil>
{
    public void Configure(EntityTypeBuilder<AprendizPerfil> e)
    {
        e.ToTable("AprendizPerfil", "adso", t =>
        {
            t.HasCheckConstraint("CK_Aprendiz_estrato", "estrato BETWEEN 0 AND 6");
        });
        e.HasKey(x => x.IdAprendiz);
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.IdPersona).HasColumnName("id_persona");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");  // nullable
        e.Property(x => x.Estrato).HasColumnName("estrato");
        e.Property(x => x.CondicionEspecial).HasColumnName("condicion_especial").HasMaxLength(150);
        e.Property(x => x.TipoPoblacion).HasColumnName("tipo_poblacion").HasMaxLength(150);
        e.Property(x => x.ContactoEmergenciaNombre).HasColumnName("contacto_emergencia_nombre").HasMaxLength(150);
        e.Property(x => x.ContactoEmergenciaTelefono).HasColumnName("contacto_emergencia_telefono").HasMaxLength(40);
        e.Property(x => x.FechaRegistro).HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        // IdPersona: único (1 a 1 con Persona)
        e.HasIndex(x => x.IdPersona).IsUnique();
        // IdUsuario: único cuando no es null
        e.HasIndex(x => x.IdUsuario).IsUnique().HasFilter("[id_usuario] IS NOT NULL");

        // FK a Persona (directo, sin pasar por Usuario)
        e.HasOne(x => x.Persona)
            .WithOne(p => p.AprendizPerfil)
            .HasForeignKey<AprendizPerfil>(x => x.IdPersona)
            .HasConstraintName("FK_AprendizPerfil_Persona")
            .OnDelete(DeleteBehavior.Restrict);

        // FK a Usuario (opcional)
        e.HasOne(x => x.Usuario)
            .WithOne(u => u.AprendizPerfil)
            .HasForeignKey<AprendizPerfil>(x => x.IdUsuario)
            .HasConstraintName("FK_AprendizPerfil_Usuario")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-many con Ficha → tabla FichaAprendiz (explícita para tener Estado)
        e.HasMany(x => x.Fichas)
            .WithMany(f => f.Aprendices)
            .UsingEntity<FichaAprendiz>(
                j => j.HasOne(x => x.Ficha).WithMany(f => f.FichaAprendices)
                    .HasForeignKey(x => x.IdFicha)
                    .HasConstraintName("FK_FichaAprendiz_Ficha"),
                j => j.HasOne(x => x.Aprendiz).WithMany(a => a.FichaAprendices)
                    .HasForeignKey(x => x.IdAprendiz)
                    .HasConstraintName("FK_FichaAprendiz_Aprendiz"),
                j =>
                {
                    j.HasKey(x => new { x.IdFicha, x.IdAprendiz });
                    j.ToTable("FichaAprendiz", "adso", t =>
                        t.HasCheckConstraint("CK_FichaAprendiz_estado",
                            "estado IN ('En Formacion','Trasladado','Cancelado','Retiro Voluntario')"));
                    j.Property(x => x.IdFicha).HasColumnName("id_ficha");
                    j.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
                    j.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("En Formacion");
                });
    }
}

public class InstructorPerfilConfig : IEntityTypeConfiguration<InstructorPerfil>
{
    public void Configure(EntityTypeBuilder<InstructorPerfil> e)
    {
        e.ToTable("InstructorPerfil", "adso", t =>
            t.HasCheckConstraint("CK_InstrPerfil_vinculacion", "tipo_vinculacion IN ('Planta','Contratista','Provisional')")
        );
        e.HasKey(x => x.IdInstructor);
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.NumeroContrato).HasColumnName("numero_contrato").HasMaxLength(50);
        e.Property(x => x.Especialidad).HasColumnName("especialidad").HasMaxLength(150);
        e.Property(x => x.TipoVinculacion).HasColumnName("tipo_vinculacion").HasMaxLength(20);
        e.Property(x => x.FechaVinculacion).HasColumnName("fecha_vinculacion");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => x.IdUsuario).IsUnique();
        

        e.HasOne(x => x.Usuario)
            .WithOne(u => u.InstructorPerfil)
            .HasForeignKey<InstructorPerfil>(x => x.IdUsuario)
            .HasConstraintName("FK_InstrPerfil_Usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
