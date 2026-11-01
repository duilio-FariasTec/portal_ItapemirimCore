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

        builder.Property(up => up.UsuarioId).IsRequired();
        builder.Property(up => up.PerfilId).IsRequired();
        builder.Property(up => up.SistemaId).IsRequired();

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

        builder.HasOne(up => up.Sistema)
            .WithMany(s => s.Usuarios)
            .HasForeignKey(up => up.SistemaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_Sistema");

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

        // Regra mandatória a nível de banco: Usuário NÃO pode possuir mais de um perfil no mesmo sistema
        builder.HasIndex(up => new { up.UsuarioId, up.SistemaId }, "UX_UsuarioPerfil_UsuarioSistema_Ativo")
            .IsUnique()
            .HasFilter("[RevogadoEmUtc] IS NULL");

        builder.HasIndex(up => new { up.PerfilId, up.UsuarioId }, "IX_UsuarioPerfil_PerfilId");
        builder.HasIndex(up => up.SistemaId, "IX_UsuarioPerfil_SistemaId");
    }
}