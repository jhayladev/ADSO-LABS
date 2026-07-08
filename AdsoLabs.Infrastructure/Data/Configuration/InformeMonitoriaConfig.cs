using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class InformeMonitoriaConfig : IEntityTypeConfiguration<InformeMonitoria>
{
    public void Configure(EntityTypeBuilder<InformeMonitoria> e)
    {
        e.ToTable("InformeMonitoria", "adso");
        e.HasKey(x => x.IdInforme);
        e.Property(x => x.IdInforme)
            .HasColumnName("id_informe");
        e.Property(x => x.IdSesionMonitoria)
            .HasColumnName("id_sesion_monitoria");
        e.Property(x => x.IdInstructorDestinatario)
            .HasColumnName("id_instructor_destinatario");
        e.Property(x => x.Texto)
            .HasColumnName("texto").HasMaxLength(2000);
        e.Property(x => x.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasPrecision(0).HasDefaultValueSql("sysdatetime()");

        e.HasIndex(x => x.IdSesionMonitoria)
            .HasDatabaseName("IX_InformeMonitoria_Sesion");

        e.HasOne(x => x.SesionMonitoria)
            .WithMany(s => s.Informes)
            .HasForeignKey(x => x.IdSesionMonitoria)
            .HasConstraintName("FK_InformeMonitoria_Sesion")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.InstructorDestinatario)
            .WithMany()
            .HasForeignKey(x => x.IdInstructorDestinatario)
            .HasConstraintName("FK_InformeMonitoria_Instructor")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
