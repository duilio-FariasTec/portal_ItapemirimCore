namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Usuario?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<bool> ExistsLoginAsync(string login, long? ignorarUsuarioId = null, CancellationToken ct = default);
    Task<bool> ExistsEmailAsync(string email, long? ignorarUsuarioId = null, CancellationToken ct = default);
    Task<(IEnumerable<Usuario> Itens, int Total)> GetPagedAsync(
        string? busca,
        UsuarioStatus? status,
        long? perfilId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Usuario usuario, CancellationToken ct = default);
    void Update(Usuario usuario);
}