namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Usuarios;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponse>> ListarAsync(UsuarioFiltroRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task<UsuarioResponse> CadastrarAsync(UsuarioCreateRequest request, CancellationToken ct = default);
    Task<UsuarioResponse> AtualizarAsync(Guid guid, UsuarioUpdateRequest request, CancellationToken ct = default);
    Task InativarAsync(Guid guid, CancellationToken ct = default);
    Task<IEnumerable<PerfilResumidoResponse>> ObterPerfisAsync(Guid guid, CancellationToken ct = default);
    Task AtribuirPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default);
    Task RevogarPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default);
    Task<IEnumerable<PermissaoEfetivaResponse>> ObterPermissoesEfetivasAsync(Guid usuarioGuid, CancellationToken ct = default);
}