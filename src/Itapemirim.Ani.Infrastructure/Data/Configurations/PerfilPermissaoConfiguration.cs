namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PerfilPermissaoConfiguration : IEntityTypeConfiguration<PerfilPermissao>
{
    public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
    {
        builder.ToTable("PerfilPermissao", "ani", t =>
        {
            t.HasCheckConstraint("CK_PerfilPermissao_Revogacao", "[RevogadoEmUtc] IS NULL OR [RevogadoEmUtc] >= [ConcedidoEmUtc]");
        });

        builder.HasKey(pp => pp.PerfilPermissaoId).HasName("PK_PerfilPermissao");
        builder.Property(pp => pp.PerfilPermissaoId).UseIdentityColumn();

        builder.Property(pp => pp.PerfilPermissaoGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(pp => pp.ConcedidoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(pp => pp.RevogadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasOne(pp => pp.Perfil)
            .WithMany(p => p.Permissoes)
            .HasForeignKey(pp => pp.PerfilId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_Perfil");

        builder.HasOne(pp => pp.Permissao)
            .WithMany(p => p.Perfis)
            .HasForeignKey(pp => pp.PermissaoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_Permissao");

        builder.HasOne(pp => pp.ConcedidoPor)
            .WithMany(u => u.PermissoesConcedidas)
            .HasForeignKey(pp => pp.ConcedidoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_ConcedidoPor");

        builder.HasOne(pp => pp.RevogadoPor)
            .WithMany(u => u.PermissoesRevogadas)
            .HasForeignKey(pp => pp.RevogadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_RevogadoPor");

        builder.HasIndex(pp => pp.PerfilPermissaoGuid, "UX_PerfilPermissao_PerfilPermissaoGuid").IsUnique();
        builder.HasIndex(pp => new { pp.PerfilId, pp.PermissaoId }, "UX_PerfilPermissao_Ativa")
            .IsUnique()
            .HasFilter("[RevogadoEmUtc] IS NULL");
        builder.HasIndex(pp => new { pp.PermissaoId, pp.PerfilId }, "IX_PerfilPermissao_PermissaoId");
    }
}