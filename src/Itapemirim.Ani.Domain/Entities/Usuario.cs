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
    public bool ExigeTrocaSenhaNoProximoLogin { get; private set; }
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
        ExigeTrocaSenhaNoProximoLogin = false;
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

    public void AtualizarSenha(string novoHash, bool exigeTrocaNoProximoLogin = false)
    {
        SenhaHash = novoHash;
        ExigeTrocaSenhaNoProximoLogin = exigeTrocaNoProximoLogin;
        AtualizadoEmUtc = DateTime.UtcNow;
    }

    public void ConfirmarTrocaSenha(string novoHash)
    {
        SenhaHash = novoHash;
        ExigeTrocaSenhaNoProximoLogin = false;
        AtualizadoEmUtc = DateTime.UtcNow;
    }
}