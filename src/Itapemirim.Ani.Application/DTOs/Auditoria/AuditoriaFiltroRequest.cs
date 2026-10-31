namespace Itapemirim.Ani.Application.DTOs.Auditoria;

public record AuditoriaFiltroRequest(
    Guid? AtorUsuarioId = null,
    string? Acao = null,
    string? Entidade = null,
    string? Resultado = null,
    DateTime? DataInicioUtc = null,
    DateTime? DataFimUtc = null,
    Guid? CorrelationId = null,
    int PageNumber = 1,
    int PageSize = 20
);