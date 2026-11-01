namespace Itapemirim.Ani.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class AuthInternoService : IAuthInternoService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;
    private readonly IConfiguration _configuration;

    public AuthInternoService(
        IUsuarioRepository usuarioRepo,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IConfiguration configuration)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _auditoria = auditoria;
        _configuration = configuration;
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
            .Where(up => up.EstaAtivo && up.Perfil.Ativo && (up.Perfil.Sistema == null || up.Perfil.Sistema.Ativo))
            .Select(up => up.Perfil)
            .DistinctBy(p => p.PerfilId)
            .ToList();

        if (!string.IsNullOrWhiteSpace(request.SistemaCodigo))
        {
            var sistemaAlvo = request.SistemaCodigo.Trim().ToUpperInvariant();
            var temAcessoAoSistema = perfisAtivos.Any(p => p.Sistema?.Codigo == sistemaAlvo);
            if (!temAcessoAoSistema)
            {
                await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", request.Login, ResultadoAuditoria.FALHA, ct);
                throw new RegraNegocioException($"Usuário '{usuario.Login}' não possui acesso ativo ao sistema '{sistemaAlvo}'.");
            }

            perfisAtivos = perfisAtivos.Where(p => p.Sistema?.Codigo == sistemaAlvo).ToList();
        }

        var sistemasDto = perfisAtivos.Select(p => new SistemaAcessoResponse(
            p.Sistema?.SistemaGuid ?? Guid.Empty,
            p.Sistema?.Codigo ?? string.Empty,
            p.Sistema?.Nome ?? string.Empty,
            p.PerfilGuid,
            p.Nome,
            p.Permissoes
                .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
                .Select(pp => pp.Permissao.Codigo)
                .Distinct()
                .ToList()
        )).ToList();

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
                p.Sistema?.SistemaGuid ?? Guid.Empty,
                p.Sistema?.Codigo ?? string.Empty,
                p.Sistema?.Nome ?? string.Empty,
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
            sistemasDto,
            perfisDto,
            permissoesConsolidadasDto,
            roles,
            permissoesCodigos
        );
    }

    public async Task<LoginResponse> LoginAsync(ValidarCredenciaisRequest request, CancellationToken ct = default)
    {
        var usuarioAuth = await ValidarAsync(request, ct);

        var secretKey = _configuration["Jwt:SecretKey"] ?? "Itapemirim_PortalCore_SuperSecret_JwtKey_2026_Minimum32Bytes!";
        var issuer = _configuration["Jwt:Issuer"] ?? "PortalItapemirimCore";
        var audience = _configuration["Jwt:Audience"] ?? "PortalItapemirimApp";
        var expiresInSeconds = 3600;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioAuth.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuarioAuth.Id.ToString()),
            new(ClaimTypes.Name, usuarioAuth.Nome),
            new("login", usuarioAuth.Login),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuarioAuth.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, usuarioAuth.Email));
        }

        if (!string.IsNullOrWhiteSpace(request.SistemaCodigo))
        {
            claims.Add(new Claim("current_system", request.SistemaCodigo.Trim().ToUpperInvariant()));
        }

        // Claims segregadas por sistema
        foreach (var sis in usuarioAuth.Sistemas)
        {
            claims.Add(new Claim("system", sis.Codigo));
            claims.Add(new Claim("system_role", $"{sis.Codigo}:{sis.PerfilNome}"));

            foreach (var perm in sis.Permissoes)
            {
                claims.Add(new Claim("permission", $"{sis.Codigo}:{perm}"));
            }
        }

        // Claims globais/convencionais
        foreach (var role in usuarioAuth.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role));
        }

        foreach (var permissao in usuarioAuth.PermissoesCodigos)
        {
            claims.Add(new Claim("permission", permissao));
        }

        // Payload estruturado em JSON para clientes que preferem deserialização direta
        var sistemasJson = JsonSerializer.Serialize(usuarioAuth.Sistemas.Select(s => new
        {
            sistema = s.Codigo,
            perfil = s.PerfilNome,
            permissoes = s.Permissoes
        }));
        claims.Add(new Claim("systems_access", sistemasJson, JsonClaimValueTypes.JsonArray));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiresInSeconds),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtString = tokenHandler.WriteToken(token);

        return new LoginResponse(
            Token: jwtString,
            TokenType: "Bearer",
            ExpiresInSeconds: expiresInSeconds,
            Usuario: usuarioAuth
        );
    }
}