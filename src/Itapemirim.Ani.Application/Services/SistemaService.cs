namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Sistemas;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class SistemaService : ISistemaService
{
    private readonly ISistemaRepository _sistemaRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public SistemaService(
        ISistemaRepository sistemaRepo,
        IUnitOfWork uow,
        IAuditoriaService auditoria)
    {
        _sistemaRepo = sistemaRepo;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task<IEnumerable<SistemaResponse>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var sistemas = await _sistemaRepo.GetAllAsync(apenasAtivos, ct);
        return sistemas.Select(s => new SistemaResponse(s.SistemaGuid, s.Codigo, s.Nome, s.Descricao, s.Ativo, s.CriadoEmUtc, s.AtualizadoEmUtc));
    }

    public async Task<SistemaResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var sistema = await _sistemaRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Sistema", guid);

        return new SistemaResponse(sistema.SistemaGuid, sistema.Codigo, sistema.Nome, sistema.Descricao, sistema.Ativo, sistema.CriadoEmUtc, sistema.AtualizadoEmUtc);
    }

    public async Task<SistemaResponse> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var sistema = await _sistemaRepo.GetByCodigoAsync(codigo, ct)
            ?? throw new EntidadeNaoEncontradaException("Sistema", codigo);

        return new SistemaResponse(sistema.SistemaGuid, sistema.Codigo, sistema.Nome, sistema.Descricao, sistema.Ativo, sistema.CriadoEmUtc, sistema.AtualizadoEmUtc);
    }

    public async Task<SistemaResponse> CriarAsync(SistemaCreateRequest request, CancellationToken ct = default)
    {
        if (await _sistemaRepo.ExistsCodigoAsync(request.Codigo, null, ct))
            throw new ConflitoException($"Já existe um sistema cadastrado com o código '{request.Codigo.Trim().ToUpperInvariant()}'.");

        var sistema = new Sistema(request.Codigo, request.Nome, request.Descricao, request.Ativo);
        await _sistemaRepo.AddAsync(sistema, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Sistema", sistema.SistemaGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new SistemaResponse(sistema.SistemaGuid, sistema.Codigo, sistema.Nome, sistema.Descricao, sistema.Ativo, sistema.CriadoEmUtc, sistema.AtualizadoEmUtc);
    }

    public async Task<SistemaResponse> AtualizarAsync(Guid guid, SistemaUpdateRequest request, CancellationToken ct = default)
    {
        var sistema = await _sistemaRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Sistema", guid);

        if (await _sistemaRepo.ExistsCodigoAsync(request.Codigo, sistema.SistemaId, ct))
            throw new ConflitoException($"Já existe outro sistema com o código '{request.Codigo.Trim().ToUpperInvariant()}'.");

        sistema.Atualizar(request.Codigo, request.Nome, request.Descricao, request.Ativo);
        _sistemaRepo.Update(sistema);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Sistema", sistema.SistemaGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new SistemaResponse(sistema.SistemaGuid, sistema.Codigo, sistema.Nome, sistema.Descricao, sistema.Ativo, sistema.CriadoEmUtc, sistema.AtualizadoEmUtc);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var sistema = await _sistemaRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Sistema", guid);

        sistema.Desativar();
        _sistemaRepo.Update(sistema);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Sistema", sistema.SistemaGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }
}
