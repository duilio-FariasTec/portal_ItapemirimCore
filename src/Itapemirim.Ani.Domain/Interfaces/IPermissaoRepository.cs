namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface IPermissaoRepository
{
    Task<Permissao?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Permissao?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<bool> ExistsCodigoAsync(string codigo, long? ignorarPermissaoId = null, CancellationToken ct = default);
    Task<IEnumerable<Permissao>> GetAllAsync(bool apenasAtivas = false, CancellationToken ct = default);
    Task AddAsync(Permissao permissao, CancellationToken ct = default);
    void Update(Permissao permissao);
}