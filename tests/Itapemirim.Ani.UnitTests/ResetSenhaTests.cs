namespace Itapemirim.Ani.UnitTests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Application.Services;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Xunit;

public class ResetSenhaTests
{
    private class FakeUsuarioRepository : IUsuarioRepository
    {
        public Usuario? Usuario { get; set; }
        public bool Atualizado { get; private set; }

        public Task<Usuario?> GetByIdAsync(long id, CancellationToken ct = default) => Task.FromResult(Usuario);
        public Task<Usuario?> GetByGuidAsync(Guid guid, CancellationToken ct = default) => Task.FromResult(Usuario);
        public Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default) => Task.FromResult(Usuario);
        public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult(Usuario);
        public Task<Usuario?> GetByLoginOrEmailAsync(string identificador, CancellationToken ct = default) => Task.FromResult(Usuario);
        public Task<bool> ExistsLoginAsync(string login, long? ignorarUsuarioId = null, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> ExistsEmailAsync(string email, long? ignorarUsuarioId = null, CancellationToken ct = default) => Task.FromResult(false);
        public Task<(IEnumerable<Usuario> Itens, int Total)> GetPagedAsync(string? busca, UsuarioStatus? status, long? perfilId, int pageNumber, int pageSize, CancellationToken ct = default) =>
            Task.FromResult(((IEnumerable<Usuario>)Array.Empty<Usuario>(), 0));
        public Task AddAsync(Usuario usuario, CancellationToken ct = default) => Task.CompletedTask;
        public void Update(Usuario usuario) => Atualizado = true;
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public int Commits { get; private set; }
        public Task<int> CommitAsync(CancellationToken ct = default)
        {
            Commits++;
            return Task.FromResult(1);
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        public string? UltimaSenhaHasheada { get; private set; }
        public string HashPassword(string password)
        {
            UltimaSenhaHasheada = password;
            return $"BCRYPT_HASH({password})";
        }
        public bool VerifyPassword(string password, string hash) => hash == $"BCRYPT_HASH({password})";
    }

    private class FakeEmailService : IEmailService
    {
        public bool Enviado { get; private set; }
        public string? Destinatario { get; private set; }
        public string? Assunto { get; private set; }
        public string? CorpoHtml { get; private set; }

        public Task EnviarAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default)
        {
            Enviado = true;
            Destinatario = destinatario;
            Assunto = assunto;
            CorpoHtml = corpoHtml;
            return Task.CompletedTask;
        }
    }

    private class FakeAuditoriaService : IAuditoriaService
    {
        public ResultadoAuditoria? UltimoResultado { get; private set; }
        public Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default) =>
            Task.FromResult(new PagedResult<AuditoriaResponse>(Array.Empty<AuditoriaResponse>(), 0, 1, 10));
        public Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default) => Task.FromResult<AuditoriaResponse?>(null);
        public Task RegistrarAsync(TipoAcaoAuditoria acao, string entidade, string entidadeId, ResultadoAuditoria resultado, CancellationToken ct = default)
        {
            UltimoResultado = resultado;
            return Task.CompletedTask;
        }
    }

    private class FakeConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string?> _values;
        public FakeConfiguration(Dictionary<string, string?>? values = null) => _values = values ?? new();
        public string? this[string key]
        {
            get => _values.TryGetValue(key, out var val) ? val : null;
            set => _values[key] = value;
        }
        public IEnumerable<IConfigurationSection> GetChildren() => Array.Empty<IConfigurationSection>();
        public Microsoft.Extensions.Primitives.IChangeToken GetReloadToken() => null!;
        public IConfigurationSection GetSection(string key) => null!;
    }

    [Fact]
    public async Task ResetarSenhaAsync_UsuarioAtivo_DeveGerarNovaSenhaHashEEnviarEmail()
    {
        var usuario = new Usuario("Admin Master", "admin.master", "admin@itapemirim.com.br", "HASH_ANTIGO", UsuarioStatus.Ativo);
        var repo = new FakeUsuarioRepository { Usuario = usuario };
        var uow = new FakeUnitOfWork();
        var hasher = new FakePasswordHasher();
        var email = new FakeEmailService();
        var auditoria = new FakeAuditoriaService();
        var config = new FakeConfiguration(new Dictionary<string, string?>
        {
            ["Email:AlsoWriteToFile"] = "true"
        });

        var service = new AuthService(repo, uow, hasher, auditoria, email, config);

        var response = await service.ResetarSenhaAsync(new ResetSenhaDto(Login: "admin.master"));

        Assert.NotNull(response);
        Assert.Equal(hasher.UltimaSenhaHasheada, response.SenhaTemporaria);
        Assert.True(repo.Atualizado);
        Assert.Equal(1, uow.Commits);
        Assert.True(usuario.ExigeTrocaSenhaNoProximoLogin);
        Assert.NotNull(hasher.UltimaSenhaHasheada);
        Assert.Equal(12, hasher.UltimaSenhaHasheada!.Length);
        Assert.Equal($"BCRYPT_HASH({hasher.UltimaSenhaHasheada})", usuario.SenhaHash);
        Assert.True(email.Enviado);
        Assert.Equal("admin@itapemirim.com.br", email.Destinatario);
        Assert.Contains(hasher.UltimaSenhaHasheada, email.CorpoHtml);
        Assert.Equal(ResultadoAuditoria.SUCESSO, auditoria.UltimoResultado);
    }

    [Fact]
    public async Task ResetarSenhaAsync_UsuarioInexistente_DeveRetornarSucessoSemEnviarEmail()
    {
        var repo = new FakeUsuarioRepository { Usuario = null };
        var uow = new FakeUnitOfWork();
        var hasher = new FakePasswordHasher();
        var email = new FakeEmailService();
        var auditoria = new FakeAuditoriaService();
        var config = new FakeConfiguration();

        var service = new AuthService(repo, uow, hasher, auditoria, email, config);

        var response = await service.ResetarSenhaAsync(new ResetSenhaDto(Email: "naoexiste@itapemirim.com.br"));

        Assert.NotNull(response);
        Assert.False(repo.Atualizado);
        Assert.Equal(0, uow.Commits);
        Assert.False(email.Enviado);
        Assert.Equal(ResultadoAuditoria.FALHA, auditoria.UltimoResultado);
    }

    [Fact]
    public async Task TrocarSenhaAsync_ComSenhaAtualCorreta_DeveAtualizarHashELimparFlag()
    {
        var usuario = new Usuario("Admin Master", "admin.master", "admin@itapemirim.com.br", "BCRYPT_HASH(Temp@1234)", UsuarioStatus.Ativo);
        usuario.AtualizarSenha("BCRYPT_HASH(Temp@1234)", exigeTrocaNoProximoLogin: true);

        var repo = new FakeUsuarioRepository { Usuario = usuario };
        var uow = new FakeUnitOfWork();
        var hasher = new FakePasswordHasher();
        var email = new FakeEmailService();
        var auditoria = new FakeAuditoriaService();
        var config = new FakeConfiguration();

        var service = new AuthService(repo, uow, hasher, auditoria, email, config);

        var response = await service.TrocarSenhaAsync(new TrocarSenhaDto(
            Usuario: "admin.master",
            SenhaAtual: "Temp@1234",
            NovaSenha: "NovaSenhaForte@2026"));

        Assert.NotNull(response);
        Assert.True(repo.Atualizado);
        Assert.Equal(1, uow.Commits);
        Assert.False(usuario.ExigeTrocaSenhaNoProximoLogin);
        Assert.Equal("BCRYPT_HASH(NovaSenhaForte@2026)", usuario.SenhaHash);
        Assert.Equal(ResultadoAuditoria.SUCESSO, auditoria.UltimoResultado);
    }

    [Fact]
    public void CriptografarEVoltarSenha_ComChaveConfigurada_DeveReverterParaSenhaOriginal()
    {
        var repo = new FakeUsuarioRepository();
        var uow = new FakeUnitOfWork();
        var hasher = new FakePasswordHasher();
        var email = new FakeEmailService();
        var auditoria = new FakeAuditoriaService();
        var config = new FakeConfiguration(new Dictionary<string, string?>
        {
            ["PasswordSecurity:SecretKey"] = "Itapemirim_Ani_PasswordSecretKey_2026!"
        });

        var service = new AuthService(repo, uow, hasher, auditoria, email, config);

        const string senhaOriginal = "Ita@2026#Forte";
        var criptografada = service.CriptografarSenha(senhaOriginal);
        var senhaRevertida = service.VoltarSenha(criptografada);

        Assert.NotEqual(senhaOriginal, criptografada);
        Assert.Equal(senhaOriginal, senhaRevertida);
    }

    [Fact]
    public void GerarEVoltarSenhaReversivelSimples_ComChaveConfigurada_DeveReverterParaTextoOriginal()
    {
        var repo = new FakeUsuarioRepository();
        var uow = new FakeUnitOfWork();
        var hasher = new FakePasswordHasher();
        var email = new FakeEmailService();
        var auditoria = new FakeAuditoriaService();
        var config = new FakeConfiguration(new Dictionary<string, string?>
        {
            ["PasswordSecurity:SecretKey"] = "Itapemirim_Ani_PasswordSecretKey_2026!"
        });

        var service = new AuthService(repo, uow, hasher, auditoria, email, config);

        const string textoBase = "admin.master@2026";
        var senhaGerada = service.GerarSenhaReversivelSimples(textoBase);
        var revertida = service.VoltarSenhaReversivelSimples(senhaGerada);

        Assert.NotEqual(textoBase, senhaGerada);
        Assert.Equal(textoBase, revertida);
    }
}
