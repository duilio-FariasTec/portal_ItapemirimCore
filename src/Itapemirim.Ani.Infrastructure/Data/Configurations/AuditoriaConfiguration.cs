namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditoria", "ani", t =>
        {
            t.HasCheckConstraint("CK_Auditoria_Acao", "[Acao] IN (N'INCLUSAO', N'ALTERACAO', N'REMOCAO')");
            t.HasCheckConstraint("CK_Auditoria_Resultado", "[Resultado] IN (N'SUCESSO', N'FALHA')");
        });

        builder.HasKey(a => a.AuditoriaId).HasName("PK_Auditoria");
        builder.Property(a => a.AuditoriaId).UseIdentityColumn();

        builder.Property(a => a.AuditoriaGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(a => a.Acao)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (TipoAcaoAuditoria)Enum.Parse(typeof(TipoAcaoAuditoria), v))
            .IsRequired();

        builder.Property(a => a.Entidade).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntidadeId).HasMaxLength(128).IsRequired();

        builder.Property(a => a.Resultado)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (ResultadoAuditoria)Enum.Parse(typeof(ResultadoAuditoria), v))
            .IsRequired();

        builder.Property(a => a.Origem).HasMaxLength(100).IsRequired(false);
        builder.Property(a => a.CorrelationId).IsRequired(false);

        builder.Property(a => a.OcorridoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(a => a.Ator)
            .WithMany(u => u.AuditoriasRealizadas)
            .HasForeignKey(a => a.AtorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Auditoria_Ator");

        builder.HasIndex(a => a.AuditoriaGuid, "UX_Auditoria_AuditoriaGuid").IsUnique();
        builder.HasIndex(a => a.OcorridoEmUtc, "IX_Auditoria_OcorridoEmUtc").IsDescending();
        builder.HasIndex(a => new { a.AtorUsuarioId, a.OcorridoEmUtc }, "IX_Auditoria_Ator_OcorridoEmUtc");
    }
}