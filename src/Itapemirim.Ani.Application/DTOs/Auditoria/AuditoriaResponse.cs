namespace Itapemirim.Ani.Application.DTOs.Auditoria;

public record AuditoriaResponse(
    Guid Id,
    Guid? AtorUsuarioId,
    string? AtorNome,
    string? AtorLogin,
    string Acao,
    string Entidade,
    string EntidadeId,
    string Resultado,
    string? Origem,
    Guid? CorrelationId,
    DateTime OcorridoEmUtc
);