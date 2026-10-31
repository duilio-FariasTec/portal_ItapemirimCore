namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuarioPerfil", "ani", t =>
        {
            t.HasCheckConstraint("CK_UsuarioPerfil_Revogacao", "[RevogadoEmUtc] IS NULL OR [RevogadoEmUtc] >= [ConcedidoEmUtc]");
        });

        builder.HasKey(up => up.UsuarioPerfilId).HasName("PK_UsuarioPerfil");
        builder.Property(up => up.UsuarioPerfilId).UseIdentityColumn();

        builder.Property(up => up.UsuarioPerfilGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(up => up.ConcedidoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(up => up.RevogadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasOne(up => up.Usuario)
            .WithMany(u => u.PerfisAssociados)
            .HasForeignKey(up => up.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_Usuario");

        builder.HasOne(up => up.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(up => up.PerfilId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_Perfil");

        builder.HasOne(up => up.ConcedidoPor)
            .WithMany(u => u.PerfisConcedidos)
            .HasForeignKey(up => up.ConcedidoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_ConcedidoPor");

        builder.HasOne(up => up.RevogadoPor)
            .WithMany(u => u.PerfisRevogados)
            .HasForeignKey(up => up.RevogadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_RevogadoPor");

        builder.HasIndex(up => up.UsuarioPerfilGuid, "UX_UsuarioPerfil_UsuarioPerfilGuid").IsUnique();
        builder.HasIndex(up => new { up.UsuarioId, up.PerfilId }, "UX_UsuarioPerfil_Ativo")
            .IsUnique()
            .HasFilter("[RevogadoEmUtc] IS NULL");
        builder.HasIndex(up => new { up.PerfilId, up.UsuarioId }, "IX_UsuarioPerfil_PerfilId");
    }
}