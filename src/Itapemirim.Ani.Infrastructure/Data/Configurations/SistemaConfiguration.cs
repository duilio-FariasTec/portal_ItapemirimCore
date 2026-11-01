namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class SistemaConfiguration : IEntityTypeConfiguration<Sistema>
{
    public void Configure(EntityTypeBuilder<Sistema> builder)
    {
        builder.ToTable("Sistema", "ani");

        builder.HasKey(s => s.SistemaId).HasName("PK_Sistema");
        builder.Property(s => s.SistemaId).UseIdentityColumn();

        builder.Property(s => s.SistemaGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(s => s.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Nome).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Descricao).HasMaxLength(500).IsRequired(false);
        builder.Property(s => s.Ativo).HasDefaultValue(true).IsRequired();

        builder.Property(s => s.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(s => s.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(s => s.SistemaGuid, "UX_Sistema_SistemaGuid").IsUnique();
        builder.HasIndex(s => s.Codigo, "UX_Sistema_Codigo").IsUnique();
    }
}
