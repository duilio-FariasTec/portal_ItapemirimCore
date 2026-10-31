namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfil", "ani");

        builder.HasKey(p => p.PerfilId).HasName("PK_Perfil");
        builder.Property(p => p.PerfilId).UseIdentityColumn();

        builder.Property(p => p.PerfilGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(p => p.Nome).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.Ativo).HasDefaultValue(true).IsRequired();

        builder.Property(p => p.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(p => p.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(p => p.PerfilGuid, "UX_Perfil_PerfilGuid").IsUnique();
        builder.HasIndex(p => p.Nome, "UX_Perfil_Nome").IsUnique();
    }
}