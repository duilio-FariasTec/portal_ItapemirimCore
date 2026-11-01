namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface ISistemaRepository
{
    Task<Sistema?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Sistema?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<Sistema?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<bool> ExistsCodigoAsync(string codigo, long? ignorarSistemaId = null, CancellationToken ct = default);
    Task<IEnumerable<Sistema>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task AddAsync(Sistema sistema, CancellationToken ct = default);
    void Update(Sistema sistema);
}
