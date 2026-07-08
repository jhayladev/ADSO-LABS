using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class PlanConcertadoConfig : IEntityTypeConfiguration<PlanConcertado>
{
    public void Configure(EntityTypeBuilder<PlanConcertado> e)
    {
        e.ToTable("PlanConcertado", "adso", t =>
            t.HasCheckConstraint("CK_Plan_estado", "estado IN ('Borrador','Publicado','Cerrado')")
        );
        e.HasKey(x => x.IdPlan);
        e.Property(x => x.IdPlan).HasColumnName("id_plan");
        e.Property(x => x.IdCompetencia).HasColumnName("id_competencia");
        e.Property(x => x.IdInstructor).HasColumnName("id_instructor");
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.DescripcionGeneral).HasColumnName("descripcion_general")
            .HasDefaultValue("Plan de evaluacion concertado con los aprendices de acuerdo con los resultados de aprendizaje de la competencia.");
        e.Property(x => x.FechaCreacion).HasColumnName("fecha_creacion")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(12).HasDefaultValue("Borrador");
        e.Property(x => x.FechaCierre).HasColumnName("fecha_cierre").HasPrecision(0);

        e.HasOne(x => x.Competencia).WithMany(c => c.PlanesConcertados)
            .HasForeignKey(x => x.IdCompetencia).HasConstraintName("FK_Plan_Competencia")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Instructor)
            .WithMany(i => i.PlanesConcertados)
            .HasForeignKey(x => x.IdInstructor)
            .HasConstraintName("FK_Plan_Instructor")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        e.HasOne(x => x.Documento).WithMany(a => a.PlanesConcertados)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Plan_Archivo")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ResultadoAprendizajeConfig : IEntityTypeConfiguration<ResultadoAprendizaje>
{
    public void Configure(EntityTypeBuilder<ResultadoAprendizaje> e)
    {
        e.ToTable("ResultadoAprendizaje", "adso");
        e.HasKey(x => x.IdResultado);
        e.Property(x => x.IdResultado).HasColumnName("id_resultado");
        e.Property(x => x.IdPlan).HasColumnName("id_plan");
        e.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        e.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();

        e.HasIndex(x => x.IdPlan).HasDatabaseName("IX_ResultadoAprendizaje_Plan");
        e.HasIndex(x => new { x.IdPlan, x.Codigo }).IsUnique().HasDatabaseName("UQ_Res_Plan_Codigo");

        e.HasOne(x => x.Plan).WithMany(p => p.ResultadosAprendizaje)
            .HasForeignKey(x => x.IdPlan).HasConstraintName("FK_Res_Plan")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JuicioResultadoConfig : IEntityTypeConfiguration<JuicioResultado>
{
    public void Configure(EntityTypeBuilder<JuicioResultado> e)
    {
        e.ToTable("JuicioResultado", "adso", t =>
            t.HasCheckConstraint("CK_Juicio_juicio", "juicio IN ('Aprobado','Por Evaluar')")
        );
        e.HasKey(x => x.IdJuicio);
        e.Property(x => x.IdJuicio).HasColumnName("id_juicio");
        e.Property(x => x.IdResultado).HasColumnName("id_resultado");
        e.Property(x => x.IdAprendiz).HasColumnName("id_aprendiz");
        e.Property(x => x.Juicio).HasColumnName("juicio").HasMaxLength(20).HasDefaultValue("Por Evaluar");
        e.Property(x => x.RegistradoPor).HasColumnName("registrado_por").HasMaxLength(200);
        e.Property(x => x.FechaJuicio).HasColumnName("fecha_juicio").HasPrecision(0);
        e.Property(x => x.FechaRegistro).HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.IdAprendiz).HasDatabaseName("IX_JuicioResultado_Aprendiz");
        e.HasIndex(x => x.IdResultado).HasDatabaseName("IX_JuicioResultado_Resultado");
        e.HasIndex(x => new { x.IdResultado, x.IdAprendiz }).IsUnique().HasDatabaseName("UQ_Juicio");

        e.HasOne(x => x.Resultado).WithMany(r => r.JuiciosResultado)
            .HasForeignKey(x => x.IdResultado).HasConstraintName("FK_Juicio_Resultado")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Aprendiz).WithMany(a => a.JuiciosResultado)
            .HasForeignKey(x => x.IdAprendiz).HasConstraintName("FK_Juicio_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ImportacionArchivoConfig : IEntityTypeConfiguration<ImportacionArchivo>
{
    public void Configure(EntityTypeBuilder<ImportacionArchivo> e)
    {
        e.ToTable("ImportacionArchivo", "adso", t =>
        {
            t.HasCheckConstraint("CK_Import_tipo", "tipo_entidad IN ('Aprendiz','Instructor','Ficha','Competencia','FichaCompetencia','Asistencia')");
            t.HasCheckConstraint("CK_Import_estado", "estado_procesamiento IN ('Pendiente','Procesado','Error')");
        });
        e.HasKey(x => x.IdImportacion);
        e.Property(x => x.IdImportacion).HasColumnName("id_importacion");
        e.Property(x => x.TipoEntidad).HasColumnName("tipo_entidad").HasMaxLength(20).IsRequired();
        e.Property(x => x.IdDocumento).HasColumnName("id_documento");
        e.Property(x => x.IdUsuario).HasColumnName("id_usuario");
        e.Property(x => x.EstadoProcesamiento).HasColumnName("estado_procesamiento")
            .HasMaxLength(12).HasDefaultValue("Pendiente");
        e.Property(x => x.TotalFilas).HasColumnName("total_filas");
        e.Property(x => x.FilasOk).HasColumnName("filas_ok");
        e.Property(x => x.FilasError).HasColumnName("filas_error");
        e.Property(x => x.Observacion).HasColumnName("observacion").HasMaxLength(500);
        e.Property(x => x.FechaSubida).HasColumnName("fecha_subida")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");
        e.Property(x => x.FechaProcesado).HasColumnName("fecha_procesado").HasPrecision(0);

        e.HasIndex(x => new { x.TipoEntidad, x.FechaSubida })
            .HasDatabaseName("IX_Import_Tipo_Fecha").IsDescending(false, true);
        e.HasIndex(x => x.IdUsuario).HasDatabaseName("IX_Import_Usuario");

        

        e.HasOne(x => x.Documento).WithMany(a => a.Importaciones)
            .HasForeignKey(x => x.IdDocumento).HasConstraintName("FK_Import_Archivo")
            .OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Usuario).WithMany(u => u.Importaciones)
            .HasForeignKey(x => x.IdUsuario).HasConstraintName("FK_Import_Usuario")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ImportacionErrorConfig : IEntityTypeConfiguration<ImportacionError>
{
    public void Configure(EntityTypeBuilder<ImportacionError> e)
    {
        e.ToTable("ImportacionError", "adso");
        e.HasKey(x => x.IdError);
        e.Property(x => x.IdError).HasColumnName("id_error");
        e.Property(x => x.IdImportacion).HasColumnName("id_importacion");
        e.Property(x => x.NumeroFila).HasColumnName("numero_fila");
        e.Property(x => x.DatosFila).HasColumnName("datos_fila");
        e.Property(x => x.MotivoError).HasColumnName("motivo_error").HasMaxLength(500).IsRequired();

        e.HasIndex(x => x.IdImportacion).HasDatabaseName("IX_ImportErr_Importacion");

        e.HasOne(x => x.Importacion).WithMany(i => i.Errores)
            .HasForeignKey(x => x.IdImportacion).HasConstraintName("FK_ImportErr_Import")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
