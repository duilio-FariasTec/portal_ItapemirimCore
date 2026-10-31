namespace Itapemirim.Ani.Domain.Entities;

using Itapemirim.Ani.Domain.Enums;

public class Auditoria
{
    public long AuditoriaId { get; private set; }
    public Guid AuditoriaGuid { get; private set; }
    public long? AtorUsuarioId { get; private set; }
    public TipoAcaoAuditoria Acao { get; private set; }
    public string Entidade { get; private set; } = null!;
    public string EntidadeId { get; private set; } = null!;
    public ResultadoAuditoria Resultado { get; private set; }
    public string? Origem { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTime OcorridoEmUtc { get; private set; }

    public Usuario? Ator { get; private set; }

    protected Auditoria() { }

    public Auditoria(
        long? atorUsuarioId,
        TipoAcaoAuditoria acao,
        string entidade,
        string entidadeId,
        ResultadoAuditoria resultado,
        string? origem = null,
        Guid? correlationId = null)
    {
        AuditoriaGuid = Guid.NewGuid();
        AtorUsuarioId = atorUsuarioId;
        Acao = acao;
        Entidade = entidade;
        EntidadeId = entidadeId;
        Resultado = resultado;
        Origem = origem;
        CorrelationId = correlationId;
        OcorridoEmUtc = DateTime.UtcNow;
    }
}