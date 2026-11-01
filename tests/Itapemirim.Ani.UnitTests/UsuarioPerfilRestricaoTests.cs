namespace Itapemirim.Ani.UnitTests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.DTOs.Usuarios;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Application.Services;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;
using Xunit;

public class UsuarioPerfilRestricaoTests
{
    private class MockUsuarioRepository : IUsuarioRepository
    {
        public Usuario? UsuarioRetornado { get; set; }
        public Task<Usuario?> GetByIdAsync(long id, CancellationToken ct = default) => Task.FromResult(UsuarioRetornado);
        public Task<Usuario?> GetByGuidAsync(Guid guid, CancellationToken ct = default) => Task.FromResult(UsuarioRetornado);
        public Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default) => Task.FromResult(UsuarioRetornado);
        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(UsuarioRetornado);
        public Task<Usuario?> GetByLoginOrEmailAsync(string identificador, CancellationToken ct = default) => Task.FromResult(UsuarioRetornado);
        public Task<bool> ExistsLoginAsync(string login, long? ignorarUsuarioId = null, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExistsEmailAsync(string email, long? ignorarUsuarioId = null, CancellationToken ct = default) => Task.FromResult(false);
        public Task<(IEnumerable<Usuario> Itens, int Total)> GetPagedAsync(string? busca, UsuarioStatus? status, long? perfilId, int pageNumber, int pageSize, CancellationToken ct = default) =>
            Task.FromResult(((IEnumerable<Usuario>)new List<Usuario>(), 0));
        public Task AddAsync(Usuario usuario, CancellationToken ct = default) => Task.CompletedTask;
        public void Update(Usuario usuario) { }
    }

    private class MockPerfilRepository : IPerfilRepository
    {
        public Dictionary<Guid, Perfil> PerfisPorGuid { get; } = new();
        public Task<Perfil?> GetByIdAsync(long id, CancellationToken ct = default) => Task.FromResult<Perfil?>(null);
        public Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
        {
            PerfisPorGuid.TryGetValue(guid, out var perfil);
            return Task.FromResult(perfil);
        }
        public Task<bool> ExistsNomeNoSistemaAsync(string nome, long sistemaId, long? ignorarPerfilId = null, CancellationToken ct = default) => Task.FromResult(false);
        public Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, long? sistemaId = null, CancellationToken ct = default) => Task.FromResult<IEnumerable<Perfil>>(PerfisPorGuid.Values);
        public Task AddAsync(Perfil perfil, CancellationToken ct = default) => Task.CompletedTask;
        public void Update(Perfil perfil) { }
    }

    private class MockUnitOfWork : IUnitOfWork
    {
        public Task<int> CommitAsync(CancellationToken ct = default) => Task.FromResult(1);
    }

    private class MockPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => "hash";
        public bool VerifyPassword(string password, string hash) => true;
    }

    private class MockAuditoriaService : IAuditoriaService
    {
        public Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default) =>
            Task.FromResult(new PagedResult<AuditoriaResponse>(new List<AuditoriaResponse>(), 0, 1, 10));
        public Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default) => Task.FromResult<AuditoriaResponse?>(null);
        public Task RegistrarAsync(TipoAcaoAuditoria acao, string entidade, string entidadeId, ResultadoAuditoria resultado, CancellationToken ct = default) => Task.CompletedTask;
    }

    private class MockUserContext : IUserContext
    {
        public long? CurrentUserId => 1;
        public Guid? CurrentUserGuid => Guid.NewGuid();
        public string? IpAddress => "127.0.0.1";
        public Guid? CorrelationId => Guid.NewGuid();
    }

    [Fact]
    public async Task NaoDevePermitir_SegundoPerfil_NoMesmoSistema()
    {
        // Arrange
        var sistemaFinanceiro = new Sistema("PORTAL_FINANCEIRO", "Portal Financeiro", null);
        var perfilAdminFin = new Perfil(1, "Administrador", "Admin Fin");
        var perfilAnalistaFin = new Perfil(1, "Analista", "Analista Fin");

        // Simula reflexão/propriedades de navegação
        typeof(Perfil).GetProperty(nameof(Perfil.Sistema))?.SetValue(perfilAdminFin, sistemaFinanceiro);
        typeof(Perfil).GetProperty(nameof(Perfil.Sistema))?.SetValue(perfilAnalistaFin, sistemaFinanceiro);

        var usuario = new Usuario("João Silva", "joao", "joao@email.com", "hash");
        var vinculoExistente = new UsuarioPerfil(1, 1, 1, 1);
        typeof(UsuarioPerfil).GetProperty(nameof(UsuarioPerfil.Perfil))?.SetValue(vinculoExistente, perfilAdminFin);
        typeof(UsuarioPerfil).GetProperty(nameof(UsuarioPerfil.Sistema))?.SetValue(vinculoExistente, sistemaFinanceiro);
        usuario.PerfisAssociados.Add(vinculoExistente);

        var usuarioRepo = new MockUsuarioRepository { UsuarioRetornado = usuario };
        var perfilRepo = new MockPerfilRepository();
        perfilRepo.PerfisPorGuid[perfilAnalistaFin.PerfilGuid] = perfilAnalistaFin;

        var service = new UsuarioService(
            usuarioRepo,
            perfilRepo,
            new MockUnitOfWork(),
            new MockPasswordHasher(),
            new MockAuditoriaService(),
            new MockUserContext()
        );

        // Act & Assert: deve lançar ConflitoException informando a violação da regra de perfil único por sistema
        var ex = await Assert.ThrowsAsync<ConflitoException>(() =>
            service.AtribuirPerfilAsync(usuario.UsuarioGuid, perfilAnalistaFin.PerfilGuid));

        Assert.Contains("Não é permitido atribuir mais de um perfil no mesmo sistema", ex.Message);
    }

    [Fact]
    public async Task DevePermitir_PerfisEmSistemasDistintos_ParaMesmoUsuario()
    {
        // Arrange
        var sistemaFinanceiro = new Sistema("PORTAL_FINANCEIRO", "Portal Financeiro", null);
        var sistemaPassagens = new Sistema("PORTAL_PASSAGENS", "Portal Passagens", null);

        var perfilAdminFin = new Perfil(1, "Administrador", "Admin Fin");
        var perfilAdminPass = new Perfil(2, "Administrador", "Admin Pass");

        typeof(Perfil).GetProperty(nameof(Perfil.Sistema))?.SetValue(perfilAdminFin, sistemaFinanceiro);
        typeof(Perfil).GetProperty(nameof(Perfil.Sistema))?.SetValue(perfilAdminPass, sistemaPassagens);

        var usuario = new Usuario("João Silva", "joao", "joao@email.com", "hash");
        var vinculoFin = new UsuarioPerfil(1, 1, 1, 1);
        typeof(UsuarioPerfil).GetProperty(nameof(UsuarioPerfil.Perfil))?.SetValue(vinculoFin, perfilAdminFin);
        typeof(UsuarioPerfil).GetProperty(nameof(UsuarioPerfil.Sistema))?.SetValue(vinculoFin, sistemaFinanceiro);
        usuario.PerfisAssociados.Add(vinculoFin);

        var usuarioRepo = new MockUsuarioRepository { UsuarioRetornado = usuario };
        var perfilRepo = new MockPerfilRepository();
        perfilRepo.PerfisPorGuid[perfilAdminPass.PerfilGuid] = perfilAdminPass;

        var service = new UsuarioService(
            usuarioRepo,
            perfilRepo,
            new MockUnitOfWork(),
            new MockPasswordHasher(),
            new MockAuditoriaService(),
            new MockUserContext()
        );

        // Act
        await service.AtribuirPerfilAsync(usuario.UsuarioGuid, perfilAdminPass.PerfilGuid);

        // Assert: agora o usuário possui 2 perfis, pois são de sistemas diferentes
        Assert.Equal(2, usuario.PerfisAssociados.Count);
    }
}
