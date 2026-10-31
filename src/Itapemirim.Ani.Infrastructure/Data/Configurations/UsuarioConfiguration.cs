namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "ani", t =>
        {
            t.HasCheckConstraint("CK_Usuario_Status", "[Status] IN (N'Ativo', N'Inativo', N'Bloqueado')");
        });

        builder.HasKey(u => u.UsuarioId).HasName("PK_Usuario");
        builder.Property(u => u.UsuarioId).UseIdentityColumn();

        builder.Property(u => u.UsuarioGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(u => u.Nome).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Login).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired(false);
        builder.Property(u => u.SenhaHash).HasMaxLength(512).IsRequired();

        builder.Property(u => u.Status)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (UsuarioStatus)Enum.Parse(typeof(UsuarioStatus), v))
            .HasDefaultValue(UsuarioStatus.Ativo)
            .IsRequired();

        builder.Property(u => u.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(u => u.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(u => u.UsuarioGuid, "UX_Usuario_UsuarioGuid").IsUnique();
        builder.HasIndex(u => u.Login, "UX_Usuario_Login").IsUnique();
        builder.HasIndex(u => u.Email, "UX_Usuario_Email").IsUnique().HasFilter("[Email] IS NOT NULL");
    }
}