# ==============================================================================
# Script de Criação das Classes da Solução Itapemirim.Ani
# ==============================================================================
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path "$root/Itapemirim.Ani.sln")) {
    $root = Get-Location
}

Write-Host "Criando estrutura de classes em: $root" -ForegroundColor Cyan

function Write-Utf8File($path, $content) {
    $dir = Split-Path -Parent $path
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    [System.IO.File]::WriteAllText($path, $content, (New-Object System.Text.UTF8Encoding $false))
    Write-Host "  -> Criado: $path" -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# 0. Limpeza de arquivos padrão de template
# ------------------------------------------------------------------------------
@("$root/src/Itapemirim.Ani.Domain/Class1.cs",
  "$root/src/Itapemirim.Ani.Application/Class1.cs",
  "$root/src/Itapemirim.Ani.Infrastructure/Class1.cs",
  "$root/src/Itapemirim.Ani.Api/WeatherForecast.cs",
  "$root/src/Itapemirim.Ani.Api/Controllers/WeatherForecastController.cs") | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item $_ -Force
        Write-Host "  - Removido template default: $_" -ForegroundColor DarkGray
    }
}

# ==============================================================================
# 1. Itapemirim.Ani.Domain
# ==============================================================================
Write-Host "`n1. Gerando arquivos do projeto Domain..." -ForegroundColor Yellow

# Enums
Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Enums/UsuarioStatus.cs" @'
namespace Itapemirim.Ani.Domain.Enums;

public enum UsuarioStatus
{
    Ativo,
    Inativo,
    Bloqueado
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Enums/TipoAcaoAuditoria.cs" @'
namespace Itapemirim.Ani.Domain.Enums;

public enum TipoAcaoAuditoria
{
    INCLUSAO,
    ALTERACAO,
    REMOCAO
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Enums/ResultadoAuditoria.cs" @'
namespace Itapemirim.Ani.Domain.Enums;

public enum ResultadoAuditoria
{
    SUCESSO,
    FALHA
}
'@

# Exceptions
Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Exceptions/DomainException.cs" @'
namespace Itapemirim.Ani.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Exceptions/EntidadeNaoEncontradaException.cs" @'
namespace Itapemirim.Ani.Domain.Exceptions;

public class EntidadeNaoEncontradaException : DomainException
{
    public EntidadeNaoEncontradaException(string entidade, object id)
        : base($"Entidade '{entidade}' com identificador '{id}' não foi encontrada.") { }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Exceptions/RegraNegocioException.cs" @'
namespace Itapemirim.Ani.Domain.Exceptions;

public class RegraNegocioException : DomainException
{
    public RegraNegocioException(string message) : base(message) { }
}
'@

# Entities
Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/Usuario.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

using Itapemirim.Ani.Domain.Enums;

public class Usuario
{
    public long UsuarioId { get; private set; }
    public Guid UsuarioGuid { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Login { get; private set; } = null!;
    public string? Email { get; private set; }
    public string SenhaHash { get; private set; } = null!;
    public UsuarioStatus Status { get; private set; } = UsuarioStatus.Ativo;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<UsuarioPerfil> PerfisAssociados { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<UsuarioPerfil> PerfisConcedidos { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<UsuarioPerfil> PerfisRevogados { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<PerfilPermissao> PermissoesConcedidas { get; private set; } = new List<PerfilPermissao>();
    public ICollection<PerfilPermissao> PermissoesRevogadas { get; private set; } = new List<PerfilPermissao>();
    public ICollection<Auditoria> AuditoriasRealizadas { get; private set; } = new List<Auditoria>();

    protected Usuario() { }

    public Usuario(string nome, string login, string? email, string senhaHash, UsuarioStatus status = UsuarioStatus.Ativo)
    {
        UsuarioGuid = Guid.NewGuid();
        Nome = nome;
        Login = login;
        Email = email;
        SenhaHash = senhaHash;
        Status = status;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void AtualizarDados(string nome, string? email, UsuarioStatus status)
    {
        Nome = nome;
        Email = email;
        Status = status;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void Inativar()
    {
        Status = UsuarioStatus.Inativo;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void AtualizarSenha(string novoHash)
    {
        SenhaHash = novoHash;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/Perfil.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

public class Perfil
{
    public long PerfilId { get; private set; }
    public Guid PerfilGuid { get; private set; }
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<UsuarioPerfil> Usuarios { get; private set; } = new List<UsuarioPerfil>();
    public ICollection<PerfilPermissao> Permissoes { get; private set; } = new List<PerfilPermissao>();

    protected Perfil() { }

    public Perfil(string nome, string? descricao, bool ativo = true)
    {
        PerfilGuid = Guid.NewGuid();
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? descricao, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        Ativo = ativo;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/Permissao.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

public class Permissao
{
    public long PermissaoId { get; private set; }
    public Guid PermissaoGuid { get; private set; }
    public string Codigo { get; private set; } = null!;
    public string Nome { get; private set; } = null!;
    public string? Descricao { get; private set; }
    public string Recurso { get; private set; } = null!;
    public string Acao { get; private set; } = null!;
    public string? DscUrl { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEmUtc { get; private set; }
    public DateTime? AtualizadoEmUtc { get; private set; }

    public ICollection<PerfilPermissao> Perfis { get; private set; } = new List<PerfilPermissao>();

    protected Permissao() { }

    public Permissao(string codigo, string nome, string? descricao, string recurso, string acao, string? dscUrl = null, bool ativo = true)
    {
        PermissaoGuid = Guid.NewGuid();
        Codigo = codigo;
        Nome = nome;
        Descricao = descricao;
        Recurso = recurso;
        Acao = acao;
        DscUrl = dscUrl;
        Ativo = ativo;
        CriadoEmUtc = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string? descricao, string recurso, string acao, string? dscUrl, bool ativo)
    {
        Nome = nome;
        Descricao = descricao;
        Recurso = recurso;
        Acao = acao;
        DscUrl = dscUrl;
        Ativo = ativo;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/UsuarioPerfil.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

public class UsuarioPerfil
{
    public long UsuarioPerfilId { get; private set; }
    public Guid UsuarioPerfilGuid { get; private set; }
    public long UsuarioId { get; private set; }
    public long PerfilId { get; private set; }
    public long? ConcedidoPorUsuarioId { get; private set; }
    public DateTime ConcedidoEmUtc { get; private set; }
    public long? RevogadoPorUsuarioId { get; private set; }
    public DateTime? RevogadoEmUtc { get; private set; }

    public Usuario Usuario { get; private set; } = null!;
    public Perfil Perfil { get; private set; } = null!;
    public Usuario? ConcedidoPor { get; private set; }
    public Usuario? RevogadoPor { get; private set; }

    public bool EstaAtivo => RevogadoEmUtc == null;

    protected UsuarioPerfil() { }

    public UsuarioPerfil(long usuarioId, long perfilId, long? concedidoPorUsuarioId)
    {
        UsuarioPerfilGuid = Guid.NewGuid();
        UsuarioId = usuarioId;
        PerfilId = perfilId;
        ConcedidoPorUsuarioId = concedidoPorUsuarioId;
        ConcedidoEmUtc = DateTime.UtcNow;
    }

    public void Revogar(long? revogadoPorUsuarioId)
    {
        RevogadoPorUsuarioId = revogadoPorUsuarioId;
        RevogadoEmUtc = DateTime.UtcNow;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/PerfilPermissao.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

public class PerfilPermissao
{
    public long PerfilPermissaoId { get; private set; }
    public Guid PerfilPermissaoGuid { get; private set; }
    public long PerfilId { get; private set; }
    public long PermissaoId { get; private set; }
    public long? ConcedidoPorUsuarioId { get; private set; }
    public DateTime ConcedidoEmUtc { get; private set; }
    public long? RevogadoPorUsuarioId { get; private set; }
    public DateTime? RevogadoEmUtc { get; private set; }

    public Perfil Perfil { get; private set; } = null!;
    public Permissao Permissao { get; private set; } = null!;
    public Usuario? ConcedidoPor { get; private set; }
    public Usuario? RevogadoPor { get; private set; }

    public bool EstaAtivo => RevogadoEmUtc == null;

    protected PerfilPermissao() { }

    public PerfilPermissao(long perfilId, long permissaoId, long? concedidoPorUsuarioId)
    {
        PerfilPermissaoGuid = Guid.NewGuid();
        PerfilId = perfilId;
        PermissaoId = permissaoId;
        ConcedidoPorUsuarioId = concedidoPorUsuarioId;
        ConcedidoEmUtc = DateTime.UtcNow;
    }

    public void Revogar(long? revogadoPorUsuarioId)
    {
        RevogadoPorUsuarioId = revogadoPorUsuarioId;
        RevogadoEmUtc = DateTime.UtcNow;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Entities/Auditoria.cs" @'
namespace Itapemirim.Ani.Domain.Entities;

using Itapemirim.Ani.Domain.Enums;

public class Auditoria
{
    public long AuditoriaId { get; private set; }
    public Guid AuditoriaGuid { get; private set; }
    public long? AtorUsuarioId { get; private set; }
    public TipoAcaoAuditoria Acao { get; private set; }
    public string Entidade { get; private set; } = null!;
    public string EntidadeId { get; private set; } = null!;
    public ResultadoAuditoria Resultado { get; private set; }
    public string? Origem { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public DateTime OcorridoEmUtc { get; private set; }

    public Usuario? Ator { get; private set; }

    protected Auditoria() { }

    public Auditoria(
        long? atorUsuarioId,
        TipoAcaoAuditoria acao,
        string entidade,
        string entidadeId,
        ResultadoAuditoria resultado,
        string? origem = null,
        Guid? correlationId = null)
    {
        AuditoriaGuid = Guid.NewGuid();
        AtorUsuarioId = atorUsuarioId;
        Acao = acao;
        Entidade = entidade;
        EntidadeId = entidadeId;
        Resultado = resultado;
        Origem = origem;
        CorrelationId = correlationId;
        OcorridoEmUtc = DateTime.UtcNow;
    }
}
'@

# Interfaces de Repositório
Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Interfaces/IUnitOfWork.cs" @'
namespace Itapemirim.Ani.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Interfaces/IUsuarioRepository.cs" @'
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
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Interfaces/IPerfilRepository.cs" @'
namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface IPerfilRepository
{
    Task<Perfil?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<bool> ExistsNomeAsync(string nome, long? ignorarPerfilId = null, CancellationToken ct = default);
    Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default);
    Task AddAsync(Perfil perfil, CancellationToken ct = default);
    void Update(Perfil perfil);
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Interfaces/IPermissaoRepository.cs" @'
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
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Domain/Interfaces/IAuditoriaRepository.cs" @'
namespace Itapemirim.Ani.Domain.Interfaces;

using Itapemirim.Ani.Domain.Entities;

public interface IAuditoriaRepository
{
    Task<Auditoria?> GetByGuidAsync(Guid guid, CancellationToken ct = default);
    Task<(IEnumerable<Auditoria> Itens, int Total)> GetPagedAsync(
        Guid? atorGuid,
        string? acao,
        string? entidade,
        string? resultado,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? correlationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Auditoria auditoria, CancellationToken ct = default);
}
'@

# ==============================================================================
# 2. Itapemirim.Ani.Application
# ==============================================================================
Write-Host "`n2. Gerando arquivos do projeto Application..." -ForegroundColor Yellow

# Common
Write-Utf8File "$root/src/Itapemirim.Ani.Application/Common/PagedResult.cs" @'
namespace Itapemirim.Ani.Application.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalItems { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));

    public PagedResult() { }

    public PagedResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Common/PagedRequest.cs" @'
namespace Itapemirim.Ani.Application.Common;

public record PagedRequest(int PageNumber = 1, int PageSize = 20);
'@

# DTOs
Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioCreateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioCreateRequest(
    string Nome,
    string Login,
    string? Email,
    string Senha,
    string? Status = "Ativo"
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioUpdateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioUpdateRequest(
    string Nome,
    string? Email,
    string Status
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilResumidoResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResumidoResponse(
    Guid Id,
    string Nome,
    DateTime ConcedidoEmUtc
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

using Itapemirim.Ani.Application.DTOs.Perfis;

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Login,
    string? Email,
    string Status,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc,
    List<PerfilResumidoResponse> PerfisAtivos
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioFiltroRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record UsuarioFiltroRequest(
    string? Busca = null,
    string? Status = null,
    Guid? PerfilGuid = null,
    int PageNumber = 1,
    int PageSize = 20
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/AtribuirPerfilRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record AtribuirPerfilRequest(
    Guid PerfilId
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Usuarios/PermissaoEfetivaResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Usuarios;

public record PermissaoEfetivaResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string Recurso,
    string Acao,
    string? DscUrl
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilCreateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilCreateRequest(
    string Nome,
    string? Descricao,
    bool Ativo = true
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilUpdateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilUpdateRequest(
    string Nome,
    string? Descricao,
    bool Ativo
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record PerfilResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Perfis/AssociarPermissaoRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Perfis;

public record AssociarPermissaoRequest(
    Guid PermissaoId
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoCreateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoCreateRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl = null,
    bool Ativo = true
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoUpdateRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoUpdateRequest(
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl,
    bool Ativo
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Permissoes;

public record PermissaoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl,
    bool Ativo,
    DateTime CriadoEmUtc,
    DateTime? AtualizadoEmUtc
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auth/ValidarCredenciaisRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auth;

public record ValidarCredenciaisRequest(
    string Login,
    string Senha
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auth/PermissaoUsuarioAutenticadoResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auth;

public record PermissaoUsuarioAutenticadoResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Recurso,
    string Acao,
    string? DscUrl
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auth/PerfilUsuarioAutenticadoResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auth;

public record PerfilUsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    List<PermissaoUsuarioAutenticadoResponse> Permissoes
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auth/UsuarioAutenticadoResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auth;

public record UsuarioAutenticadoResponse(
    Guid Id,
    string Nome,
    string Login,
    string? Email,
    string Status,
    List<PerfilUsuarioAutenticadoResponse> Perfis,
    List<PermissaoUsuarioAutenticadoResponse> Permissoes,
    List<string> Roles,
    List<string> PermissoesCodigos
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auditoria/AuditoriaFiltroRequest.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auditoria;

public record AuditoriaFiltroRequest(
    Guid? AtorUsuarioId = null,
    string? Acao = null,
    string? Entidade = null,
    string? Resultado = null,
    DateTime? DataInicioUtc = null,
    DateTime? DataFimUtc = null,
    Guid? CorrelationId = null,
    int PageNumber = 1,
    int PageSize = 20
);
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/DTOs/Auditoria/AuditoriaResponse.cs" @'
namespace Itapemirim.Ani.Application.DTOs.Auditoria;

public record AuditoriaResponse(
    Guid Id,
    Guid? AtorUsuarioId,
    string? AtorNome,
    string? AtorLogin,
    string Acao,
    string Entidade,
    string EntidadeId,
    string Resultado,
    string? Origem,
    Guid? CorrelationId,
    DateTime OcorridoEmUtc
);
'@

# Abstrações de Serviços
Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs" @'
namespace Itapemirim.Ani.Application.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IUserContext.cs" @'
namespace Itapemirim.Ani.Application.Interfaces;

public interface IUserContext
{
    long? CurrentUserId { get; }
    Guid? CurrentUserGuid { get; }
    string? IpAddress { get; }
    Guid? CorrelationId { get; }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IUsuarioService.cs" @'
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
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IPerfilService.cs" @'
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
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IPermissaoService.cs" @'
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
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IAuthInternoService.cs" @'
namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.DTOs.Auth;

public interface IAuthInternoService
{
    Task<UsuarioAutenticadoResponse> ValidarAsync(ValidarCredenciaisRequest request, CancellationToken ct = default);
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Interfaces/IAuditoriaService.cs" @'
namespace Itapemirim.Ani.Application.Interfaces;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Domain.Enums;

public interface IAuditoriaService
{
    Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default);
    Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default);
    Task RegistrarAsync(TipoAcaoAuditoria acao, string entidade, string entidadeId, ResultadoAuditoria resultado, CancellationToken ct = default);
}
'@

# Validators
Write-Utf8File "$root/src/Itapemirim.Ani.Application/Validators/UsuarioCreateValidator.cs" @'
namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;

public class UsuarioCreateValidator : AbstractValidator<UsuarioCreateRequest>
{
    public UsuarioCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O login é obrigatório.")
            .MaximumLength(100).WithMessage("O login deve ter no máximo 100 caracteres.")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("O login contém caracteres inválidos.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail em formato inválido.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha deve conter ao menos 8 caracteres.");
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Validators/UsuarioUpdateValidator.cs" @'
namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;

public class UsuarioUpdateValidator : AbstractValidator<UsuarioUpdateRequest>
{
    public UsuarioUpdateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail em formato inválido.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Status)
            .Must(s => s is "Ativo" or "Inativo" or "Bloqueado")
            .WithMessage("Status inválido. Use Ativo, Inativo ou Bloqueado.");
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Validators/PerfilCreateValidator.cs" @'
namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Perfis;

public class PerfilCreateValidator : AbstractValidator<PerfilCreateRequest>
{
    public PerfilCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do perfil é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Validators/PermissaoCreateValidator.cs" @'
namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Permissoes;

public class PermissaoCreateValidator : AbstractValidator<PermissaoCreateRequest>
{
    public PermissaoCreateValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O código é obrigatório.")
            .MaximumLength(150).WithMessage("O código deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Recurso)
            .NotEmpty().WithMessage("O recurso é obrigatório.")
            .MaximumLength(100).WithMessage("O recurso deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Acao)
            .NotEmpty().WithMessage("A ação é obrigatória.")
            .MaximumLength(50).WithMessage("A ação deve ter no máximo 50 caracteres.");

        RuleFor(x => x.DscUrl)
            .MaximumLength(250).WithMessage("A rota/caminho da tela (DscUrl) deve ter no máximo 250 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DscUrl));
    }
}
'@

# Implementações dos Services
Write-Utf8File "$root/src/Itapemirim.Ani.Application/Services/UsuarioService.cs" @'
namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Usuarios;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPerfilRepository _perfilRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;
    private readonly IUserContext _userContext;

    public UsuarioService(
        IUsuarioRepository usuarioRepo,
        IPerfilRepository perfilRepo,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IUserContext userContext)
    {
        _usuarioRepo = usuarioRepo;
        _perfilRepo = perfilRepo;
        _uow = uow;
        _hasher = hasher;
        _auditoria = auditoria;
        _userContext = userContext;
    }

    public async Task<PagedResult<UsuarioResponse>> ListarAsync(UsuarioFiltroRequest request, CancellationToken ct = default)
    {
        UsuarioStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UsuarioStatus>(request.Status, true, out var parsedStatus))
            status = parsedStatus;

        long? perfilId = null;
        if (request.PerfilGuid.HasValue)
        {
            var perfil = await _perfilRepo.GetByGuidAsync(request.PerfilGuid.Value, ct);
            perfilId = perfil?.PerfilId;
        }

        var (itens, total) = await _usuarioRepo.GetPagedAsync(request.Busca, status, perfilId, request.PageNumber, request.PageSize, ct);

        var dtos = itens.Select(MapearParaResponse).ToList();
        return new PagedResult<UsuarioResponse>(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<UsuarioResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", guid);

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> CadastrarAsync(UsuarioCreateRequest request, CancellationToken ct = default)
    {
        if (await _usuarioRepo.ExistsLoginAsync(request.Login, null, ct))
            throw new RegraNegocioException($"O login '{request.Login}' já está em uso.");

        if (!string.IsNullOrWhiteSpace(request.Email) && await _usuarioRepo.ExistsEmailAsync(request.Email, null, ct))
            throw new RegraNegocioException($"O e-mail '{request.Email}' já está em uso.");

        var status = UsuarioStatus.Ativo;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UsuarioStatus>(request.Status, true, out var parsed))
            status = parsed;

        var senhaHash = _hasher.HashPassword(request.Senha);
        var usuario = new Usuario(request.Nome, request.Login, request.Email, senhaHash, status);

        await _usuarioRepo.AddAsync(usuario, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid guid, UsuarioUpdateRequest request, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", guid);

        if (!string.IsNullOrWhiteSpace(request.Email) && await _usuarioRepo.ExistsEmailAsync(request.Email, usuario.UsuarioId, ct))
            throw new RegraNegocioException($"O e-mail '{request.Email}' já está cadastrado para outro usuário.");

        if (!Enum.TryParse<UsuarioStatus>(request.Status, true, out var status))
            throw new RegraNegocioException($"Status '{request.Status}' inválido.");

        usuario.AtualizarDados(request.Nome, request.Email, status);
        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(usuario);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", guid);

        usuario.Inativar();
        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Usuario", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PerfilResumidoResponse>> ObterPerfisAsync(Guid guid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", guid);

        return usuario.PerfisAssociados
            .Where(p => p.EstaAtivo && p.Perfil.Ativo)
            .Select(p => new PerfilResumidoResponse(p.Perfil.PerfilGuid, p.Perfil.Nome, p.ConcedidoEmUtc))
            .ToList();
    }

    public async Task AtribuirPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", usuarioGuid);

        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var jaAtribuido = usuario.PerfisAssociados.Any(p => p.PerfilId == perfil.PerfilId && p.EstaAtivo);
        if (jaAtribuido)
            throw new RegraNegocioException("O usuário já possui este perfil ativo.");

        var vinculo = new UsuarioPerfil(usuario.UsuarioId, perfil.PerfilId, _userContext.CurrentUserId);
        usuario.PerfisAssociados.Add(vinculo);

        await _uow.CommitAsync(ct);
        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "UsuarioPerfil", vinculo.UsuarioPerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task RevogarPerfilAsync(Guid usuarioGuid, Guid perfilGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", usuarioGuid);

        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var vinculo = usuario.PerfisAssociados.FirstOrDefault(p => p.PerfilId == perfil.PerfilId && p.EstaAtivo)
            ?? throw new RegraNegocioException("O usuário não possui este perfil ativo para revogar.");

        vinculo.Revogar(_userContext.CurrentUserId);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "UsuarioPerfil", vinculo.UsuarioPerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PermissaoEfetivaResponse>> ObterPermissoesEfetivasAsync(Guid usuarioGuid, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByGuidAsync(usuarioGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Usuário", usuarioGuid);

        var permissoes = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .SelectMany(up => up.Perfil.Permissoes)
            .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao)
            .DistinctBy(p => p.PermissaoId)
            .Select(p => new PermissaoEfetivaResponse(p.PermissaoGuid, p.Codigo, p.Nome, p.Recurso, p.Acao, p.DscUrl))
            .ToList();

        return permissoes;
    }

    private static UsuarioResponse MapearParaResponse(Usuario u) =>
        new(
            u.UsuarioGuid,
            u.Nome,
            u.Login,
            u.Email,
            u.Status.ToString(),
            u.CriadoEmUtc,
            u.AtualizadoEmUtc,
            u.PerfisAssociados
                .Where(p => p.EstaAtivo && p.Perfil.Ativo)
                .Select(p => new PerfilResumidoResponse(p.Perfil.PerfilGuid, p.Perfil.Nome, p.ConcedidoEmUtc))
                .ToList()
        );
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Services/PerfilService.cs" @'
namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfilRepo;
    private readonly IPermissaoRepository _permissaoRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;
    private readonly IUserContext _userContext;

    public PerfilService(
        IPerfilRepository perfilRepo,
        IPermissaoRepository permissaoRepo,
        IUnitOfWork uow,
        IAuditoriaService auditoria,
        IUserContext userContext)
    {
        _perfilRepo = perfilRepo;
        _permissaoRepo = permissaoRepo;
        _uow = uow;
        _auditoria = auditoria;
        _userContext = userContext;
    }

    public async Task<IEnumerable<PerfilResponse>> ListarAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var perfis = await _perfilRepo.GetAllAsync(apenasAtivos, ct);
        return perfis.Select(p => new PerfilResponse(p.PerfilGuid, p.Nome, p.Descricao, p.Ativo, p.CriadoEmUtc, p.AtualizadoEmUtc));
    }

    public async Task<PerfilResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        return new PerfilResponse(perfil.PerfilGuid, perfil.Nome, perfil.Descricao, perfil.Ativo, perfil.CriadoEmUtc, perfil.AtualizadoEmUtc);
    }

    public async Task<PerfilResponse> CriarAsync(PerfilCreateRequest request, CancellationToken ct = default)
    {
        if (await _perfilRepo.ExistsNomeAsync(request.Nome, null, ct))
            throw new RegraNegocioException($"Já existe um perfil com o nome '{request.Nome}'.");

        var perfil = new Perfil(request.Nome, request.Descricao, request.Ativo);
        await _perfilRepo.AddAsync(perfil, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new PerfilResponse(perfil.PerfilGuid, perfil.Nome, perfil.Descricao, perfil.Ativo, perfil.CriadoEmUtc, perfil.AtualizadoEmUtc);
    }

    public async Task<PerfilResponse> AtualizarAsync(Guid guid, PerfilUpdateRequest request, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        if (await _perfilRepo.ExistsNomeAsync(request.Nome, perfil.PerfilId, ct))
            throw new RegraNegocioException($"Já existe outro perfil com o nome '{request.Nome}'.");

        perfil.Atualizar(request.Nome, request.Descricao, request.Ativo);
        _perfilRepo.Update(perfil);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new PerfilResponse(perfil.PerfilGuid, perfil.Nome, perfil.Descricao, perfil.Ativo, perfil.CriadoEmUtc, perfil.AtualizadoEmUtc);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", guid);

        perfil.Desativar();
        _perfilRepo.Update(perfil);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Perfil", perfil.PerfilGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task<IEnumerable<PermissaoResponse>> ObterPermissoesAsync(Guid perfilGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        return perfil.Permissoes
            .Where(p => p.EstaAtivo && p.Permissao.Ativo)
            .Select(p => new PermissaoResponse(
                p.Permissao.PermissaoGuid,
                p.Permissao.Codigo,
                p.Permissao.Nome,
                p.Permissao.Descricao,
                p.Permissao.Recurso,
                p.Permissao.Acao,
                p.Permissao.DscUrl,
                p.Permissao.Ativo,
                p.Permissao.CriadoEmUtc,
                p.Permissao.AtualizadoEmUtc
            ));
    }

    public async Task AssociarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var permissao = await _permissaoRepo.GetByGuidAsync(permissaoGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", permissaoGuid);

        var jaAssociada = perfil.Permissoes.Any(p => p.PermissaoId == permissao.PermissaoId && p.EstaAtivo);
        if (jaAssociada)
            throw new RegraNegocioException("A permissão já está vinculada a este perfil.");

        var vinculo = new PerfilPermissao(perfil.PerfilId, permissao.PermissaoId, _userContext.CurrentUserId);
        perfil.Permissoes.Add(vinculo);

        await _uow.CommitAsync(ct);
        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "PerfilPermissao", vinculo.PerfilPermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    public async Task RevogarPermissaoAsync(Guid perfilGuid, Guid permissaoGuid, CancellationToken ct = default)
    {
        var perfil = await _perfilRepo.GetByGuidAsync(perfilGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Perfil", perfilGuid);

        var permissao = await _permissaoRepo.GetByGuidAsync(permissaoGuid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", permissaoGuid);

        var vinculo = perfil.Permissoes.FirstOrDefault(p => p.PermissaoId == permissao.PermissaoId && p.EstaAtivo)
            ?? throw new RegraNegocioException("A permissão não está vinculada ativamente a este perfil.");

        vinculo.Revogar(_userContext.CurrentUserId);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "PerfilPermissao", vinculo.PerfilPermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Services/PermissaoService.cs" @'
namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class PermissaoService : IPermissaoService
{
    private readonly IPermissaoRepository _permissaoRepo;
    private readonly IUnitOfWork _uow;
    private readonly IAuditoriaService _auditoria;

    public PermissaoService(IPermissaoRepository permissaoRepo, IUnitOfWork uow, IAuditoriaService auditoria)
    {
        _permissaoRepo = permissaoRepo;
        _uow = uow;
        _auditoria = auditoria;
    }

    public async Task<IEnumerable<PermissaoResponse>> ListarAsync(bool apenasAtivas = false, CancellationToken ct = default)
    {
        var permissoes = await _permissaoRepo.GetAllAsync(apenasAtivas, ct);
        return permissoes.Select(MapearParaResponse);
    }

    public async Task<PermissaoResponse> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", guid);

        return MapearParaResponse(permissao);
    }

    public async Task<PermissaoResponse> CadastrarAsync(PermissaoCreateRequest request, CancellationToken ct = default)
    {
        if (await _permissaoRepo.ExistsCodigoAsync(request.Codigo, null, ct))
            throw new RegraNegocioException($"O código de permissão '{request.Codigo}' já existe.");

        var permissao = new Permissao(request.Codigo, request.Nome, request.Descricao, request.Recurso, request.Acao, request.DscUrl, request.Ativo);
        await _permissaoRepo.AddAsync(permissao, ct);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.INCLUSAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(permissao);
    }

    public async Task<PermissaoResponse> AtualizarAsync(Guid guid, PermissaoUpdateRequest request, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", guid);

        permissao.Atualizar(request.Nome, request.Descricao, request.Recurso, request.Acao, request.DscUrl, request.Ativo);
        _permissaoRepo.Update(permissao);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return MapearParaResponse(permissao);
    }

    public async Task InativarAsync(Guid guid, CancellationToken ct = default)
    {
        var permissao = await _permissaoRepo.GetByGuidAsync(guid, ct)
            ?? throw new EntidadeNaoEncontradaException("Permissão", guid);

        permissao.Desativar();
        _permissaoRepo.Update(permissao);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.REMOCAO, "Permissao", permissao.PermissaoGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);
    }

    private static PermissaoResponse MapearParaResponse(Permissao p) =>
        new(p.PermissaoGuid, p.Codigo, p.Nome, p.Descricao, p.Recurso, p.Acao, p.DscUrl, p.Ativo, p.CriadoEmUtc, p.AtualizadoEmUtc);
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs" @'
namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;

public class AuthInternoService : IAuthInternoService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;

    public AuthInternoService(IUsuarioRepository usuarioRepo, IPasswordHasher hasher, IAuditoriaService auditoria)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _auditoria = auditoria;
    }

    public async Task<UsuarioAutenticadoResponse> ValidarAsync(ValidarCredenciaisRequest request, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByLoginAsync(request.Login, ct);

        if (usuario == null || !_hasher.VerifyPassword(request.Senha, usuario.SenhaHash))
        {
            await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", request.Login, ResultadoAuditoria.FALHA, ct);
            throw new RegraNegocioException("Credenciais inválidas.");
        }

        if (usuario.Status != UsuarioStatus.Ativo)
        {
            await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", request.Login, ResultadoAuditoria.FALHA, ct);
            throw new RegraNegocioException($"Usuário não pode se autenticar pois está com status '{usuario.Status}'.");
        }

        var perfisAtivos = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .Select(up => up.Perfil)
            .DistinctBy(p => p.PerfilId)
            .ToList();

        var perfisDto = perfisAtivos.Select(p =>
        {
            var permissoesDoPerfil = p.Permissoes
                .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
                .Select(pp => pp.Permissao)
                .DistinctBy(perm => perm.PermissaoId)
                .Select(perm => new PermissaoUsuarioAutenticadoResponse(
                    perm.PermissaoGuid,
                    perm.Codigo,
                    perm.Nome,
                    perm.Descricao,
                    perm.Recurso,
                    perm.Acao,
                    perm.DscUrl
                ))
                .ToList();

            return new PerfilUsuarioAutenticadoResponse(
                p.PerfilGuid,
                p.Nome,
                p.Descricao,
                permissoesDoPerfil
            );
        }).ToList();

        var permissoesConsolidadasDto = perfisAtivos
            .SelectMany(p => p.Permissoes)
            .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao)
            .DistinctBy(perm => perm.PermissaoId)
            .Select(perm => new PermissaoUsuarioAutenticadoResponse(
                perm.PermissaoGuid,
                perm.Codigo,
                perm.Nome,
                perm.Descricao,
                perm.Recurso,
                perm.Acao,
                perm.DscUrl
            ))
            .ToList();

        var roles = perfisAtivos.Select(p => p.Nome).Distinct().ToList();
        var permissoesCodigos = permissoesConsolidadasDto.Select(p => p.Codigo).Distinct().ToList();

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new UsuarioAutenticadoResponse(
            usuario.UsuarioGuid,
            usuario.Nome,
            usuario.Login,
            usuario.Email,
            usuario.Status.ToString(),
            perfisDto,
            permissoesConsolidadasDto,
            roles,
            permissoesCodigos
        );
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Application/Services/AuditoriaService.cs" @'
namespace Itapemirim.Ani.Application.Services;

using Itapemirim.Ani.Application.Common;
using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Interfaces;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepo;
    private readonly IUnitOfWork _uow;
    private readonly IUserContext _userContext;

    public AuditoriaService(IAuditoriaRepository auditoriaRepo, IUnitOfWork uow, IUserContext userContext)
    {
        _auditoriaRepo = auditoriaRepo;
        _uow = uow;
        _userContext = userContext;
    }

    public async Task<PagedResult<AuditoriaResponse>> ListarAsync(AuditoriaFiltroRequest request, CancellationToken ct = default)
    {
        var (itens, total) = await _auditoriaRepo.GetPagedAsync(
            request.AtorUsuarioId,
            request.Acao,
            request.Entidade,
            request.Resultado,
            request.DataInicioUtc,
            request.DataFimUtc,
            request.CorrelationId,
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = itens.Select(a => new AuditoriaResponse(
            a.AuditoriaGuid,
            a.Ator?.UsuarioGuid,
            a.Ator?.Nome,
            a.Ator?.Login,
            a.Acao.ToString(),
            a.Entidade,
            a.EntidadeId,
            a.Resultado.ToString(),
            a.Origem,
            a.CorrelationId,
            a.OcorridoEmUtc
        )).ToList();

        return new PagedResult<AuditoriaResponse>(dtos, total, request.PageNumber, request.PageSize);
    }

    public async Task<AuditoriaResponse?> ObterPorGuidAsync(Guid guid, CancellationToken ct = default)
    {
        var a = await _auditoriaRepo.GetByGuidAsync(guid, ct);
        if (a == null) return null;

        return new AuditoriaResponse(
            a.AuditoriaGuid,
            a.Ator?.UsuarioGuid,
            a.Ator?.Nome,
            a.Ator?.Login,
            a.Acao.ToString(),
            a.Entidade,
            a.EntidadeId,
            a.Resultado.ToString(),
            a.Origem,
            a.CorrelationId,
            a.OcorridoEmUtc
        );
    }

    public async Task RegistrarAsync(
        TipoAcaoAuditoria acao,
        string entidade,
        string entidadeId,
        ResultadoAuditoria resultado,
        CancellationToken ct = default)
    {
        var registro = new Auditoria(
            _userContext.CurrentUserId,
            acao,
            entidade,
            entidadeId,
            resultado,
            _userContext.IpAddress,
            _userContext.CorrelationId
        );

        await _auditoriaRepo.AddAsync(registro, ct);
        await _uow.CommitAsync(ct);
    }
}
'@

# DependencyInjection do Application
Write-Utf8File "$root/src/Itapemirim.Ani.Application/DependencyInjection.cs" @'
namespace Itapemirim.Ani.Application;

using FluentValidation;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IPermissaoService, PermissaoService>();
        services.AddScoped<IAuthInternoService, AuthInternoService>();
        services.AddScoped<IAuditoriaService, AuditoriaService>();

        return services;
    }
}
'@

# ==============================================================================
# 3. Itapemirim.Ani.Infrastructure
# ==============================================================================
Write-Host "`n3. Gerando arquivos do projeto Infrastructure..." -ForegroundColor Yellow

# DbContext
Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/AniDbContext.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AniDbContext : DbContext, IUnitOfWork
{
    public AniDbContext(DbContextOptions<AniDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<UsuarioPerfil> UsuarioPerfis => Set<UsuarioPerfil>();
    public DbSet<PerfilPermissao> PerfilPermissoes => Set<PerfilPermissao>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ani");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AniDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await SaveChangesAsync(ct);
    }
}
'@

# Fluent API Configurations
Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/UsuarioConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuario", "ani", t =>
        {
            t.HasCheckConstraint("CK_Usuario_Status", "[Status] IN (N'Ativo', N'Inativo', N'Bloqueado')");
        });

        builder.HasKey(u => u.UsuarioId).HasName("PK_Usuario");
        builder.Property(u => u.UsuarioId).UseIdentityColumn();

        builder.Property(u => u.UsuarioGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(u => u.Nome).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Login).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(254).IsRequired(false);
        builder.Property(u => u.SenhaHash).HasMaxLength(512).IsRequired();

        builder.Property(u => u.Status)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (UsuarioStatus)Enum.Parse(typeof(UsuarioStatus), v))
            .HasDefaultValue(UsuarioStatus.Ativo)
            .IsRequired();

        builder.Property(u => u.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(u => u.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(u => u.UsuarioGuid, "UX_Usuario_UsuarioGuid").IsUnique();
        builder.HasIndex(u => u.Login, "UX_Usuario_Login").IsUnique();
        builder.HasIndex(u => u.Email, "UX_Usuario_Email").IsUnique().HasFilter("[Email] IS NOT NULL");
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PerfilConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfil", "ani");

        builder.HasKey(p => p.PerfilId).HasName("PK_Perfil");
        builder.Property(p => p.PerfilId).UseIdentityColumn();

        builder.Property(p => p.PerfilGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(p => p.Nome).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.Ativo).HasDefaultValue(true).IsRequired();

        builder.Property(p => p.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(p => p.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(p => p.PerfilGuid, "UX_Perfil_PerfilGuid").IsUnique();
        builder.HasIndex(p => p.Nome, "UX_Perfil_Nome").IsUnique();
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PermissaoConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("Permissao", "ani");

        builder.HasKey(p => p.PermissaoId).HasName("PK_Permissao");
        builder.Property(p => p.PermissaoId).UseIdentityColumn();

        builder.Property(p => p.PermissaoGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(p => p.Codigo).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.Recurso).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Acao).HasMaxLength(50).IsRequired();
        builder.Property(p => p.DscUrl).HasMaxLength(250).IsRequired(false);
        builder.Property(p => p.Ativo).HasDefaultValue(true).IsRequired();

        builder.Property(p => p.CriadoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(p => p.AtualizadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasIndex(p => p.PermissaoGuid, "UX_Permissao_PermissaoGuid").IsUnique();
        builder.HasIndex(p => p.Codigo, "UX_Permissao_Codigo").IsUnique();
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/UsuarioPerfilConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuarioPerfil", "ani", t =>
        {
            t.HasCheckConstraint("CK_UsuarioPerfil_Revogacao", "[RevogadoEmUtc] IS NULL OR [RevogadoEmUtc] >= [ConcedidoEmUtc]");
        });

        builder.HasKey(up => up.UsuarioPerfilId).HasName("PK_UsuarioPerfil");
        builder.Property(up => up.UsuarioPerfilId).UseIdentityColumn();

        builder.Property(up => up.UsuarioPerfilGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(up => up.ConcedidoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(up => up.RevogadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasOne(up => up.Usuario)
            .WithMany(u => u.PerfisAssociados)
            .HasForeignKey(up => up.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_Usuario");

        builder.HasOne(up => up.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(up => up.PerfilId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_Perfil");

        builder.HasOne(up => up.ConcedidoPor)
            .WithMany(u => u.PerfisConcedidos)
            .HasForeignKey(up => up.ConcedidoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_ConcedidoPor");

        builder.HasOne(up => up.RevogadoPor)
            .WithMany(u => u.PerfisRevogados)
            .HasForeignKey(up => up.RevogadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UsuarioPerfil_RevogadoPor");

        builder.HasIndex(up => up.UsuarioPerfilGuid, "UX_UsuarioPerfil_UsuarioPerfilGuid").IsUnique();
        builder.HasIndex(up => new { up.UsuarioId, up.PerfilId }, "UX_UsuarioPerfil_Ativo")
            .IsUnique()
            .HasFilter("[RevogadoEmUtc] IS NULL");
        builder.HasIndex(up => new { up.PerfilId, up.UsuarioId }, "IX_UsuarioPerfil_PerfilId");
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PerfilPermissaoConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;

public class PerfilPermissaoConfiguration : IEntityTypeConfiguration<PerfilPermissao>
{
    public void Configure(EntityTypeBuilder<PerfilPermissao> builder)
    {
        builder.ToTable("PerfilPermissao", "ani", t =>
        {
            t.HasCheckConstraint("CK_PerfilPermissao_Revogacao", "[RevogadoEmUtc] IS NULL OR [RevogadoEmUtc] >= [ConcedidoEmUtc]");
        });

        builder.HasKey(pp => pp.PerfilPermissaoId).HasName("PK_PerfilPermissao");
        builder.Property(pp => pp.PerfilPermissaoId).UseIdentityColumn();

        builder.Property(pp => pp.PerfilPermissaoGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(pp => pp.ConcedidoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(pp => pp.RevogadoEmUtc)
            .HasColumnType("datetime2(3)")
            .IsRequired(false);

        builder.HasOne(pp => pp.Perfil)
            .WithMany(p => p.Permissoes)
            .HasForeignKey(pp => pp.PerfilId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_Perfil");

        builder.HasOne(pp => pp.Permissao)
            .WithMany(p => p.Perfis)
            .HasForeignKey(pp => pp.PermissaoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_Permissao");

        builder.HasOne(pp => pp.ConcedidoPor)
            .WithMany(u => u.PermissoesConcedidas)
            .HasForeignKey(pp => pp.ConcedidoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_ConcedidoPor");

        builder.HasOne(pp => pp.RevogadoPor)
            .WithMany(u => u.PermissoesRevogadas)
            .HasForeignKey(pp => pp.RevogadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PerfilPermissao_RevogadoPor");

        builder.HasIndex(pp => pp.PerfilPermissaoGuid, "UX_PerfilPermissao_PerfilPermissaoGuid").IsUnique();
        builder.HasIndex(pp => new { pp.PerfilId, pp.PermissaoId }, "UX_PerfilPermissao_Ativa")
            .IsUnique()
            .HasFilter("[RevogadoEmUtc] IS NULL");
        builder.HasIndex(pp => new { pp.PermissaoId, pp.PerfilId }, "IX_PerfilPermissao_PermissaoId");
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Configurations/AuditoriaConfiguration.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;

public class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditoria", "ani", t =>
        {
            t.HasCheckConstraint("CK_Auditoria_Acao", "[Acao] IN (N'INCLUSAO', N'ALTERACAO', N'REMOCAO')");
            t.HasCheckConstraint("CK_Auditoria_Resultado", "[Resultado] IN (N'SUCESSO', N'FALHA')");
        });

        builder.HasKey(a => a.AuditoriaId).HasName("PK_Auditoria");
        builder.Property(a => a.AuditoriaId).UseIdentityColumn();

        builder.Property(a => a.AuditoriaGuid)
            .HasDefaultValueSql("NEWSEQUENTIALID()")
            .IsRequired();

        builder.Property(a => a.Acao)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (TipoAcaoAuditoria)Enum.Parse(typeof(TipoAcaoAuditoria), v))
            .IsRequired();

        builder.Property(a => a.Entidade).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntidadeId).HasMaxLength(128).IsRequired();

        builder.Property(a => a.Resultado)
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),
                v => (ResultadoAuditoria)Enum.Parse(typeof(ResultadoAuditoria), v))
            .IsRequired();

        builder.Property(a => a.Origem).HasMaxLength(100).IsRequired(false);
        builder.Property(a => a.CorrelationId).IsRequired(false);

        builder.Property(a => a.OcorridoEmUtc)
            .HasColumnType("datetime2(3)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(a => a.Ator)
            .WithMany(u => u.AuditoriasRealizadas)
            .HasForeignKey(a => a.AtorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Auditoria_Ator");

        builder.HasIndex(a => a.AuditoriaGuid, "UX_Auditoria_AuditoriaGuid").IsUnique();
        builder.HasIndex(a => a.OcorridoEmUtc, "IX_Auditoria_OcorridoEmUtc").IsDescending();
        builder.HasIndex(a => new { a.AtorUsuarioId, a.OcorridoEmUtc }, "IX_Auditoria_Ator_OcorridoEmUtc");
    }
}
'@

# Repositórios Concretos
Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Repositories/UsuarioRepository.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AniDbContext _db;

    public UsuarioRepository(AniDbContext db) => _db = db;

    public async Task<Usuario?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.UsuarioId == id, ct);
    }

    public async Task<Usuario?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.UsuarioGuid == guid, ct);
    }

    public async Task<Usuario?> GetByLoginAsync(string login, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
                    .ThenInclude(p => p.Permissoes)
                        .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(u => u.Login == login, ct);
    }

    public async Task<bool> ExistsLoginAsync(string login, long? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .AnyAsync(u => u.Login == login && (!ignorarUsuarioId.HasValue || u.UsuarioId != ignorarUsuarioId.Value), ct);
    }

    public async Task<bool> ExistsEmailAsync(string email, long? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        return await _db.Usuarios
            .AnyAsync(u => u.Email == email && (!ignorarUsuarioId.HasValue || u.UsuarioId != ignorarUsuarioId.Value), ct);
    }

    public async Task<(IEnumerable<Usuario> Itens, int Total)> GetPagedAsync(
        string? busca,
        UsuarioStatus? status,
        long? perfilId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Usuarios
            .Include(u => u.PerfisAssociados)
                .ThenInclude(up => up.Perfil)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(u => u.Nome.ToLower().Contains(termo)
                                  || u.Login.ToLower().Contains(termo)
                                  || (u.Email != null && u.Email.ToLower().Contains(termo)));
        }

        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);

        if (perfilId.HasValue)
            query = query.Where(u => u.PerfisAssociados.Any(p => p.PerfilId == perfilId.Value && p.RevogadoEmUtc == null));

        var total = await query.CountAsync(ct);

        var skip = (pageNumber - 1) * pageSize;
        var itens = await query
            .OrderByDescending(u => u.CriadoEmUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
    {
        await _db.Usuarios.AddAsync(usuario, ct);
    }

    public void Update(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Repositories/PerfilRepository.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PerfilRepository : IPerfilRepository
{
    private readonly AniDbContext _db;

    public PerfilRepository(AniDbContext db) => _db = db;

    public async Task<Perfil?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Perfis
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilId == id, ct);
    }

    public async Task<Perfil?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Perfis
            .Include(p => p.Permissoes)
                .ThenInclude(pp => pp.Permissao)
            .FirstOrDefaultAsync(p => p.PerfilGuid == guid, ct);
    }

    public async Task<bool> ExistsNomeAsync(string nome, long? ignorarPerfilId = null, CancellationToken ct = default)
    {
        return await _db.Perfis
            .AnyAsync(p => p.Nome == nome && (!ignorarPerfilId.HasValue || p.PerfilId != ignorarPerfilId.Value), ct);
    }

    public async Task<IEnumerable<Perfil>> GetAllAsync(bool apenasAtivos = false, CancellationToken ct = default)
    {
        var query = _db.Perfis.AsNoTracking().AsQueryable();
        if (apenasAtivos)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Nome).ToListAsync(ct);
    }

    public async Task AddAsync(Perfil perfil, CancellationToken ct = default)
    {
        await _db.Perfis.AddAsync(perfil, ct);
    }

    public void Update(Perfil perfil)
    {
        _db.Perfis.Update(perfil);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Repositories/PermissaoRepository.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PermissaoRepository : IPermissaoRepository
{
    private readonly AniDbContext _db;

    public PermissaoRepository(AniDbContext db) => _db = db;

    public async Task<Permissao?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _db.Permissoes.FirstOrDefaultAsync(p => p.PermissaoId == id, ct);
    }

    public async Task<Permissao?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Permissoes.FirstOrDefaultAsync(p => p.PermissaoGuid == guid, ct);
    }

    public async Task<bool> ExistsCodigoAsync(string codigo, long? ignorarPermissaoId = null, CancellationToken ct = default)
    {
        return await _db.Permissoes
            .AnyAsync(p => p.Codigo == codigo && (!ignorarPermissaoId.HasValue || p.PermissaoId != ignorarPermissaoId.Value), ct);
    }

    public async Task<IEnumerable<Permissao>> GetAllAsync(bool apenasAtivas = false, CancellationToken ct = default)
    {
        var query = _db.Permissoes.AsNoTracking().AsQueryable();
        if (apenasAtivas)
            query = query.Where(p => p.Ativo);

        return await query.OrderBy(p => p.Recurso).ThenBy(p => p.Acao).ToListAsync(ct);
    }

    public async Task AddAsync(Permissao permissao, CancellationToken ct = default)
    {
        await _db.Permissoes.AddAsync(permissao, ct);
    }

    public void Update(Permissao permissao)
    {
        _db.Permissoes.Update(permissao);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Data/Repositories/AuditoriaRepository.cs" @'
namespace Itapemirim.Ani.Infrastructure.Data.Repositories;

using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AniDbContext _db;

    public AuditoriaRepository(AniDbContext db) => _db = db;

    public async Task<Auditoria?> GetByGuidAsync(Guid guid, CancellationToken ct = default)
    {
        return await _db.Auditorias
            .Include(a => a.Ator)
            .FirstOrDefaultAsync(a => a.AuditoriaGuid == guid, ct);
    }

    public async Task<(IEnumerable<Auditoria> Itens, int Total)> GetPagedAsync(
        Guid? atorGuid,
        string? acao,
        string? entidade,
        string? resultado,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? correlationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.Auditorias
            .Include(a => a.Ator)
            .AsNoTracking()
            .AsQueryable();

        if (atorGuid.HasValue)
            query = query.Where(a => a.Ator != null && a.Ator.UsuarioGuid == atorGuid.Value);

        if (!string.IsNullOrWhiteSpace(acao))
            query = query.Where(a => a.Acao.ToString() == acao.ToUpper());

        if (!string.IsNullOrWhiteSpace(entidade))
            query = query.Where(a => a.Entidade.ToLower() == entidade.ToLower());

        if (!string.IsNullOrWhiteSpace(resultado))
            query = query.Where(a => a.Resultado.ToString() == resultado.ToUpper());

        if (dataInicio.HasValue)
            query = query.Where(a => a.OcorridoEmUtc >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(a => a.OcorridoEmUtc <= dataFim.Value);

        if (correlationId.HasValue)
            query = query.Where(a => a.CorrelationId == correlationId.Value);

        var total = await query.CountAsync(ct);

        var skip = (pageNumber - 1) * pageSize;
        var itens = await query
            .OrderByDescending(a => a.OcorridoEmUtc)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (itens, total);
    }

    public async Task AddAsync(Auditoria auditoria, CancellationToken ct = default)
    {
        await _db.Auditorias.AddAsync(auditoria, ct);
    }
}
'@

# Security
Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/Security/BcryptPasswordHasher.cs" @'
namespace Itapemirim.Ani.Infrastructure.Security;

using Itapemirim.Ani.Application.Interfaces;

public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
'@

# DependencyInjection do Infrastructure
Write-Utf8File "$root/src/Itapemirim.Ani.Infrastructure/DependencyInjection.cs" @'
namespace Itapemirim.Ani.Infrastructure;

using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Interfaces;
using Itapemirim.Ani.Infrastructure.Data;
using Itapemirim.Ani.Infrastructure.Data.Repositories;
using Itapemirim.Ani.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=portal_Itapemirim;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<AniDbContext>(options =>
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsHistoryTable("__EFMigrationsHistory", "ani");
            }));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AniDbContext>());

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPerfilRepository, PerfilRepository>();
        services.AddScoped<IPermissaoRepository, PermissaoRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
'@

# ==============================================================================
# 4. Itapemirim.Ani.Api
# ==============================================================================
Write-Host "`n4. Gerando arquivos do projeto Api..." -ForegroundColor Yellow

# UserContext implementation
Write-Utf8File "$root/src/Itapemirim.Ani.Api/Services/HttpUserContext.cs" @'
namespace Itapemirim.Ani.Api.Services;

using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public long? CurrentUserId
    {
        get
        {
            var claim = HttpContext?.User.FindFirst("user_id")?.Value
                     ?? HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? CurrentUserGuid
    {
        get
        {
            var claim = HttpContext?.User.FindFirst("user_guid")?.Value
                     ?? HttpContext?.User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var guid) ? guid : null;
        }
    }

    public string? IpAddress =>
        HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? HttpContext?.Connection.RemoteIpAddress?.ToString();

    public Guid? CorrelationId
    {
        get
        {
            if (HttpContext?.Items.TryGetValue("CorrelationId", out var id) == true && id is Guid guid)
                return guid;

            var header = HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();
            return Guid.TryParse(header, out var parsed) ? parsed : null;
        }
    }
}
'@

# Middlewares
Write-Utf8File "$root/src/Itapemirim.Ani.Api/Middlewares/CorrelationIdMiddleware.cs" @'
namespace Itapemirim.Ani.Api.Middlewares;

using Microsoft.AspNetCore.Http;

public class CorrelationIdMiddleware
{
    private const string HeaderKey = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderKey, out var correlationId) || !Guid.TryParse(correlationId, out var parsed))
        {
            parsed = Guid.NewGuid();
        }

        context.Items["CorrelationId"] = parsed;
        context.Response.Headers[HeaderKey] = parsed.ToString();

        await _next(context);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Middlewares/ExceptionHandlingMiddleware.cs" @'
namespace Itapemirim.Ani.Api.Middlewares;

using FluentValidation;
using Itapemirim.Ani.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado durante o processamento da requisição: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            EntidadeNaoEncontradaException => HttpStatusCode.NotFound,
            ValidationException => HttpStatusCode.BadRequest,
            RegraNegocioException => HttpStatusCode.UnprocessableEntity,
            DomainException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = statusCode switch
            {
                HttpStatusCode.NotFound => "Recurso não encontrado",
                HttpStatusCode.BadRequest => "Requisição inválida",
                HttpStatusCode.UnprocessableEntity => "Regra de negócio violada",
                _ => "Erro interno do servidor"
            },
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (exception is ValidationException validationEx)
        {
            problem.Extensions["errors"] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
'@

# Controllers
Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/BaseApiController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/UsuariosController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/usuarios")]
public class UsuariosController : BaseApiController
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] UsuarioFiltroRequest request, CancellationToken ct)
    {
        var result = await _service.ListarAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] UsuarioCreateRequest request,
        [FromServices] IValidator<UsuarioCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CadastrarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid guid,
        [FromBody] UsuarioUpdateRequest request,
        [FromServices] IValidator<UsuarioUpdateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.AtualizarAsync(guid, request, ct);
        return Ok(result);
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Inativar(Guid guid, CancellationToken ct)
    {
        await _service.InativarAsync(guid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/perfis")]
    public async Task<IActionResult> ObterPerfis(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPerfisAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost("{guid:guid}/perfis")]
    public async Task<IActionResult> AtribuirPerfil(Guid guid, [FromBody] AtribuirPerfilRequest request, CancellationToken ct)
    {
        await _service.AtribuirPerfilAsync(guid, request.PerfilId, ct);
        return NoContent();
    }

    [HttpDelete("{guid:guid}/perfis/{perfilGuid:guid}")]
    public async Task<IActionResult> RevogarPerfil(Guid guid, Guid perfilGuid, CancellationToken ct)
    {
        await _service.RevogarPerfilAsync(guid, perfilGuid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/permissoes-efetivas")]
    public async Task<IActionResult> ObterPermissoesEfetivas(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPermissoesEfetivasAsync(guid, ct);
        return Ok(result);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/PerfisController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Perfis;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/perfis")]
public class PerfisController : BaseApiController
{
    private readonly IPerfilService _service;

    public PerfisController(IPerfilService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivos = false, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(apenasAtivos, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] PerfilCreateRequest request,
        [FromServices] IValidator<PerfilCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(Guid guid, [FromBody] PerfilUpdateRequest request, CancellationToken ct)
    {
        var result = await _service.AtualizarAsync(guid, request, ct);
        return Ok(result);
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Inativar(Guid guid, CancellationToken ct)
    {
        await _service.InativarAsync(guid, ct);
        return NoContent();
    }

    [HttpGet("{guid:guid}/permissoes")]
    public async Task<IActionResult> ObterPermissoes(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPermissoesAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost("{guid:guid}/permissoes")]
    public async Task<IActionResult> AssociarPermissao(Guid guid, [FromBody] AssociarPermissaoRequest request, CancellationToken ct)
    {
        await _service.AssociarPermissaoAsync(guid, request.PermissaoId, ct);
        return NoContent();
    }

    [HttpDelete("{guid:guid}/permissoes/{permissaoGuid:guid}")]
    public async Task<IActionResult> RevogarPermissao(Guid guid, Guid permissaoGuid, CancellationToken ct)
    {
        await _service.RevogarPermissaoAsync(guid, permissaoGuid, ct);
        return NoContent();
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/PermissoesController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Permissoes;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/permissoes")]
public class PermissoesController : BaseApiController
{
    private readonly IPermissaoService _service;

    public PermissoesController(IPermissaoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool apenasAtivas = false, CancellationToken ct = default)
    {
        var result = await _service.ListarAsync(apenasAtivas, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] PermissaoCreateRequest request,
        [FromServices] IValidator<PermissaoCreateRequest> validator,
        CancellationToken ct)
    {
        var val = await validator.ValidateAsync(request, ct);
        if (!val.IsValid) throw new ValidationException(val.Errors);

        var result = await _service.CadastrarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorGuid), new { guid = result.Id }, result);
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Atualizar(Guid guid, [FromBody] PermissaoUpdateRequest request, CancellationToken ct)
    {
        var result = await _service.AtualizarAsync(guid, request, ct);
        return Ok(result);
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Inativar(Guid guid, CancellationToken ct)
    {
        await _service.InativarAsync(guid, ct);
        return NoContent();
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/AuditoriaController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using Itapemirim.Ani.Application.DTOs.Auditoria;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auditoria")]
public class AuditoriaController : BaseApiController
{
    private readonly IAuditoriaService _service;

    public AuditoriaController(IAuditoriaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] AuditoriaFiltroRequest request, CancellationToken ct)
    {
        var result = await _service.ListarAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObterPorGuid(Guid guid, CancellationToken ct)
    {
        var result = await _service.ObterPorGuidAsync(guid, ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
'@

Write-Utf8File "$root/src/Itapemirim.Ani.Api/Controllers/AuthInternoController.cs" @'
namespace Itapemirim.Ani.Api.Controllers;

using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[Route("api/v1/auth")]
public class AuthInternoController : BaseApiController
{
    private readonly IAuthInternoService _service;

    public AuthInternoController(IAuthInternoService service) => _service = service;

    [HttpPost("validar-credenciais")]
    public async Task<IActionResult> Validar([FromBody] ValidarCredenciaisRequest request, CancellationToken ct)
    {
        var result = await _service.ValidarAsync(request, ct);
        return Ok(result);
    }
}
'@

# Program.cs
Write-Utf8File "$root/src/Itapemirim.Ani.Api/Program.cs" @'
using Itapemirim.Ani.Api.Middlewares;
using Itapemirim.Ani.Api.Services;
using Itapemirim.Ani.Application;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Infrastructure;
using Itapemirim.Ani.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "ANI Core API — Portal Itapemirim",
        Version = "v1",
        Description = "API de Domínio do ANI Core responsável pela gestão de Usuários, Perfis, Permissões e Auditoria."
    });
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ANI Core v1");
        c.RoutePrefix = string.Empty;
    });

    // Garante que o banco SQLite seja criado automaticamente no primeiro run
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AniDbContext>();
    db.Database.EnsureCreated();

    if (db.Database.IsSqlite())
    {
        try
        {
            db.Database.ExecuteSqlRaw("ALTER TABLE Permissao ADD COLUMN DscUrl TEXT NULL;");
        }
        catch
        {
            // Coluna já existe ou tabela criada recentemente com o schema atualizado
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
'@

# appsettings.json
Write-Utf8File "$root/src/Itapemirim.Ani.Api/appsettings.json" @'
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=portal_Itapemirim;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
'@

Write-Host "`nTodas as classes foram geradas com sucesso!" -ForegroundColor Cyan
