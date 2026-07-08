using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdsoLabs.Infrastructure.Data.Configurations;

public class InformeMonitoriaAprendizConfig : IEntityTypeConfiguration<InformeMonitoriaAprendiz>
{
    public void Configure(EntityTypeBuilder<InformeMonitoriaAprendiz> e)
    {
        e.ToTable("InformeMonitoriaAprendiz", "adso");
        e.HasKey(x => new { x.IdInforme, x.IdAprendiz });
        e.Property(x => x.IdInforme)
            .HasColumnName("id_informe");
        e.Property(x => x.IdAprendiz)
            .HasColumnName("id_aprendiz");

        e.HasOne(x => x.Informe)
            .WithMany(i => i.Aprendices)
            .HasForeignKey(x => x.IdInforme)
            .HasConstraintName("FK_InformeMonitoriaAprendiz_Informe")
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.Aprendiz)
            .WithMany()
            .HasForeignKey(x => x.IdAprendiz)
            .HasConstraintName("FK_InformeMonitoriaAprendiz_Aprendiz")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
