namespace Itapemirim.Ani.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

        var perfis = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .Select(up => up.Perfil.Nome)
            .Distinct()
            .ToList();

        var permissoes = usuario.PerfisAssociados
            .Where(up => up.EstaAtivo && up.Perfil.Ativo)
            .SelectMany(up => up.Perfil.Permissoes)
            .Where(pp => pp.EstaAtivo && pp.Permissao.Ativo)
            .Select(pp => pp.Permissao.Codigo)
            .Distinct()
            .ToList();

        await _auditoria.RegistrarAsync(TipoAcaoAuditoria.ALTERACAO, "SessaoLogin", usuario.UsuarioGuid.ToString(), ResultadoAuditoria.SUCESSO, ct);

        return new UsuarioAutenticadoResponse(
            usuario.UsuarioGuid,
            usuario.Nome,
            usuario.Login,
            usuario.Email,
            usuario.Status.ToString(),
            perfis,
            permissoes
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

        foreach (var perfil in usuarioAuth.Perfis)
        {
            claims.Add(new Claim(ClaimTypes.Role, perfil));
            claims.Add(new Claim("role", perfil));
        }

        foreach (var permissao in usuarioAuth.Permissoes)
        {
            claims.Add(new Claim("permission", permissao));
        }

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