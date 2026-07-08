using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class ArchivoConfig : IEntityTypeConfiguration<Archivo>
{
    public void Configure(EntityTypeBuilder<Archivo> e)
    {
        e.ToTable("Archivo", "adso");
        e.HasKey(x => x.IdArchivo);
        e.Property(x => x.IdArchivo).HasColumnName("id_archivo");
        e.Property(x => x.NombreOriginal).HasColumnName("nombre_original").HasMaxLength(255).IsRequired();
        e.Property(x => x.NombreAlmacenado).HasColumnName("nombre_almacenado").HasMaxLength(255).IsRequired();
        e.Property(x => x.RutaStorage).HasColumnName("ruta_storage").HasMaxLength(500).IsRequired();
        e.Property(x => x.ExtensionArchivo).HasColumnName("extension_archivo").HasMaxLength(10);
        e.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(120);
        e.Property(x => x.IdTipoArchivo).HasColumnName("id_tipo_archivo");
        e.Property(x => x.IdUsuarioSubio).HasColumnName("id_usuario_subio");
        e.Property(x => x.TamanioBytes).HasColumnName("tamanio_bytes");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.SiteId).HasColumnName("site_id").HasMaxLength(200);
        e.Property(x => x.DriveId).HasColumnName("drive_id").HasMaxLength(200);
        e.Property(x => x.ItemId).HasColumnName("item_id").HasMaxLength(200);
        e.Property(x => x.WebUrl).HasColumnName("web_url").HasMaxLength(500);
        e.Property(x => x.FechaSubida).HasColumnName("fecha_subida")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => new { x.IdTipoArchivo, x.FechaSubida })
            .HasDatabaseName("IX_Archivo_Tipo_Fecha").IsDescending(false, true);

        e.HasOne(x => x.TipoArchivo)
            .WithMany(t => t.Archivos)
            .HasForeignKey(x => x.IdTipoArchivo)
            .HasConstraintName("FK_Archivo_Tipo")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.UsuarioSubio)
            .WithMany(u => u.Archivos)
            .HasForeignKey(x => x.IdUsuarioSubio)
            .HasConstraintName("FK_Archivo_SubidoPor")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GuiaAprendizajeConfig : IEntityTypeConfiguration<GuiaAprendizaje>
{
    public void Configure(EntityTypeBuilder<GuiaAprendizaje> e)
    {
        e.ToTable("GuiaAprendizaje", "adso", t =>
            t.HasCheckConstraint("CK_Guia_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdGuia);
        e.Property(x => x.IdGuia).HasColumnName("id_guia");
        e.Property(x => x.IdFichaCompetencia).HasColumnName("id_ficha_competencia");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(300).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => x.IdFichaCompetencia).HasDatabaseName("IX_GuiaAprendizaje_FC");

        e.HasOne(x => x.FichaCompetencia).WithMany(fc => fc.GuiasAprendizaje)
            .HasForeignKey(x => x.IdFichaCompetencia).HasConstraintName("FK_Guia_FC")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor).WithMany(i => i.GuiasAprendizaje)
            .HasForeignKey(x => x.IdInstructor).HasConstraintName("FK_Guia_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Documento).WithMany(a => a.GuiasAprendizaje)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Guia_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InstrumentoEvaluacionConfig : IEntityTypeConfiguration<InstrumentoEvaluacion>
{
    public void Configure(EntityTypeBuilder<InstrumentoEvaluacion> e)
    {
        e.ToTable("InstrumentoEvaluacion", "adso", t =>
            t.HasCheckConstraint("CK_Inst_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdInstrumento);
        e.Property(x => x.IdInstrumento).HasColumnName("id_instrumento");
        e.Property(x => x.IdCompetencia).HasColumnName("id_competencia");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(300).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => x.IdCompetencia).HasDatabaseName("IX_InstrumentoEvaluacion_Competencia");

        e.HasOne(x => x.Competencia).WithMany(c => c.InstrumentosEvaluacion)
            .HasForeignKey(x => x.IdCompetencia).HasConstraintName("FK_Inst_Competencia")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor).WithMany(i => i.InstrumentosEvaluacion)
            .HasForeignKey(x => x.IdInstructor).HasConstraintName("FK_Inst_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Documento).WithMany(a => a.InstrumentosEvaluacion)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Inst_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlaneacionPedagogicaConfig : IEntityTypeConfiguration<PlaneacionPedagogica>
{
    public void Configure(EntityTypeBuilder<PlaneacionPedagogica> e)
    {
        e.ToTable("PlaneacionPedagogica", "adso", t =>
            t.HasCheckConstraint("CK_Planea_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdPlaneacion);
        e.Property(x => x.IdPlaneacion).HasColumnName("id_planeacion");
        e.Property(x => x.IdFichaCompetencia).HasColumnName("id_ficha_competencia");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => x.IdFichaCompetencia).HasDatabaseName("IX_PlaneacionPedagogica_FC");
        

        e.HasOne(x => x.FichaCompetencia).WithMany(fc => fc.PlaneacionesPedagogicas)
            .HasForeignKey(x => x.IdFichaCompetencia).HasConstraintName("FK_Planea_FC")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor).WithMany(i => i.PlaneacionesPedagogicas)
            .HasForeignKey(x => x.IdInstructor).HasConstraintName("FK_Planea_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Documento).WithMany(a => a.PlaneacionesPedagogicas)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Planea_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProyectoFormativoConfig : IEntityTypeConfiguration<ProyectoFormativo>
{
    public void Configure(EntityTypeBuilder<ProyectoFormativo> e)
    {
        e.ToTable("ProyectoFormativo", "adso", t =>
            t.HasCheckConstraint("CK_Proy_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdProyecto);
        e.Property(x => x.IdProyecto).HasColumnName("id_proyecto");
        e.Property(x => x.IdFicha).HasColumnName("id_ficha");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(300).IsRequired();
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        

        e.HasOne(x => x.Ficha).WithMany(f => f.ProyectosFormativos)
            .HasForeignKey(x => x.IdFicha).HasConstraintName("FK_Proy_Ficha")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor).WithMany(i => i.ProyectosFormativos)
            .HasForeignKey(x => x.IdInstructor).HasConstraintName("FK_Proy_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Documento).WithMany(a => a.ProyectosFormativos)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Proy_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DesarrolloCurricularConfig : IEntityTypeConfiguration<DesarrolloCurricular>
{
    public void Configure(EntityTypeBuilder<DesarrolloCurricular> e)
    {
        e.ToTable("DesarrolloCurricular", "adso", t =>
            t.HasCheckConstraint("CK_DesaCurr_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdDesarrollo);
        e.Property(x => x.IdDesarrollo).HasColumnName("id_desarrollo");
        e.Property(x => x.IdCompetencia).HasColumnName("id_competencia");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.Descripcion).HasColumnName("descripcion");
        e.Property(x => x.Version).HasColumnName("version").HasDefaultValue(1);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);

        e.HasIndex(x => x.IdCompetencia).HasDatabaseName("IX_DesarrolloCurricular_Competencia");
        

        e.HasOne(x => x.Competencia).WithMany(c => c.DesarrollosCurriculares)
            .HasForeignKey(x => x.IdCompetencia).HasConstraintName("FK_DesaCurr_Competencia")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor).WithMany(i => i.DesarrollosCurriculares)
            .HasForeignKey(x => x.IdInstructor).HasConstraintName("FK_DesaCurr_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Documento).WithMany(a => a.DesarrollosCurriculares)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_DesaCurr_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
