namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("Permissao", "ani");

        builder.HasKey(p => p.PermissaoId).HasName("PK_Permissao");
        builder.Property(p => p.PermissaoId).UseIdentityColumn();

        builder.Property(p => p.PermissaoGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(p => p.Codigo).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.Recurso).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Acao).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Ativo).HasDefaultValue(true).IsRequired();

        builder.Property(p => p.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(p => p.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(p => p.PermissaoGuid, "UX_Permissao_PermissaoGuid").IsUnique();
        builder.HasIndex(p => p.Codigo, "UX_Permissao_Codigo").IsUnique();
    }
}