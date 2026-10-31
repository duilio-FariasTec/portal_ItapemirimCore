namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Permissoes;

public interface IPerfilService
{
    Task<IEnumerable<PerfilResponse>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task<PerfilResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<PerfilResponse> CriarAsync(PerfilCreateRequest request, CancellationToken ct = default);
    Task<PerfilResponse> AtualizarAsync(Guid guid, PerfilUpdateRequest request, CancellationToken ct = default);
    Task InativarAsync(Guid guid, CancellationToken ct = default);
    Task<IEnumerable<PermissaoResponse>> ObterPermissoesAsync(Guid perfilGuid, CancellationToken ct = default);
    Task AssociarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default);
    Task RevogarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default);
}