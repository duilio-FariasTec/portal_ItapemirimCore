namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.DTOs.Sistemas;

public interface ISistemaService
{
    Task<IEnumerable<SistemaResponse>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task<SistemaResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<SistemaResponse> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<SistemaResponse> CriarAsync(SistemaCreateRequest request, CancellationToken ct = default);
    Task<SistemaResponse> AtualizarAsync(Guid guid, SistemaUpdateRequest request, CancellationToken ct = default);
    Task InativarAsync(Guid guid, CancellationToken ct = default);
}
