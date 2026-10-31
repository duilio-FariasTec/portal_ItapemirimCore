namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.DTOs.Permissoes;

public interface IPermissaoService
{
    Task<IEnumerable<PermissaoResponse>> ListarAsync(bool apenasAtivas = false, CancellationToken ct = default);
    Task<PermissaoResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<PermissaoResponse> CadastrarAsync(PermissaoCreateRequest request, CancellationToken ct = default);
    Task<PermissaoResponse> AtualizarAsync(Guid guid, PermissaoUpdateRequest request, CancellationToken ct = default);
    Task InativarAsync(Guid guid, CancellationToken ct = default);
}