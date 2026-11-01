namespace Itapemirim.Ani.Application.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Itapemirim.Ani.Application.DTOs.Auth;
using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Domain.Exceptions;
using Itapemirim.Ani.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class AuthInternoService : IAuthInternoService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditoriaService _auditoria;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    private const string MensagemPadraoResetSenha =
        "Se as informações informadas corresponderem a uma conta ativa, você receberá um e-mail com as instruções e a senha temporária de acesso.";

    public AuthInternoService(
        IUsuarioRepository usuarioRepo,
        IUnitOfWork uow,
        IPasswordHasher hasher,
        IAuditoriaService auditoria,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _usuarioRepo = usuarioRepo;
        _uow = uow;
        _hasher = hasher;
        _auditoria = auditoria;
        _emailService = emailService;
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
            permissoesCodigos,
            usuario.ExigeTrocaSenhaNoProximoLogin
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
            new("requires_password_change", usuarioAuth.ExigeTrocaSenhaNoProximoLogin.ToString().ToLowerInvariant()),
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

    public async Task<ResetSenhaResponse> ResetarSenhaAsync(ResetSenhaDto request, CancellationToken ct = default)
    {
        var identificador = request.ValorIdentificador;
        var usuario = await _usuarioRepo.GetByLoginOrEmailAsync(identificador, ct);

        if (usuario == null || usuario.Status != UsuarioStatus.Ativo)
        {
            await _auditoria.RegistrarAsync(
                TipoAcaoAuditoria.ALTERACAO,
                "ResetSenha",
                identificador,
                ResultadoAuditoria.FALHA,
                ct);

            return new ResetSenhaResponse(MensagemPadraoResetSenha, DateTime.UtcNow);
        }

        var novaSenhaTemporaria = GerarSenhaTemporariaForte();
        var novoHash = _hasher.HashPassword(novaSenhaTemporaria);

        usuario.AtualizarSenha(novoHash, exigeTrocaNoProximoLogin: true);
        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        var destinatario = !string.IsNullOrWhiteSpace(usuario.Email)
            ? usuario.Email
            : $"{usuario.Login}@itapemirim.local";

        var assunto = "Portal Corporativo ANI — Redefinição de Senha de Acesso";
        var corpoHtml = MontarTemplateHtmlResetSenha(usuario, novaSenhaTemporaria);

        await _emailService.EnviarAsync(destinatario, assunto, corpoHtml, ct);

        await _auditoria.RegistrarAsync(
            TipoAcaoAuditoria.ALTERACAO,
            "ResetSenha",
            usuario.UsuarioGuid.ToString(),
            ResultadoAuditoria.SUCESSO,
            ct);

        var alsoWriteToFile = bool.TryParse(_configuration["Email:AlsoWriteToFile"], out var writeFile) && writeFile;

        return new ResetSenhaResponse(
            MensagemPadraoResetSenha,
            DateTime.UtcNow,
            SenhaTemporaria: alsoWriteToFile ? novaSenhaTemporaria : null);
    }

    public async Task<TrocarSenhaResponse> TrocarSenhaAsync(TrocarSenhaDto request, CancellationToken ct = default)
    {
        var identificador = request.ValorUsuario;
        var usuario = await _usuarioRepo.GetByLoginOrEmailAsync(identificador, ct);

        if (usuario == null || !_hasher.VerifyPassword(request.SenhaAtual, usuario.SenhaHash))
        {
            await _auditoria.RegistrarAsync(
                TipoAcaoAuditoria.ALTERACAO,
                "TrocaSenha",
                identificador,
                ResultadoAuditoria.FALHA,
                ct);

            throw new RegraNegocioException("Usuário ou senha atual inválidos.");
        }

        if (usuario.Status != UsuarioStatus.Ativo)
        {
            await _auditoria.RegistrarAsync(
                TipoAcaoAuditoria.ALTERACAO,
                "TrocaSenha",
                usuario.UsuarioGuid.ToString(),
                ResultadoAuditoria.FALHA,
                ct);

            throw new RegraNegocioException($"Não é possível alterar a senha pois o usuário está com status '{usuario.Status}'.");
        }

        if (_hasher.VerifyPassword(request.NovaSenha, usuario.SenhaHash))
        {
            throw new RegraNegocioException("A nova senha deve ser diferente da senha atual.");
        }

        var novoHash = _hasher.HashPassword(request.NovaSenha);
        usuario.ConfirmarTrocaSenha(novoHash);

        _usuarioRepo.Update(usuario);
        await _uow.CommitAsync(ct);

        await _auditoria.RegistrarAsync(
            TipoAcaoAuditoria.ALTERACAO,
            "TrocaSenha",
            usuario.UsuarioGuid.ToString(),
            ResultadoAuditoria.SUCESSO,
            ct);

        return new TrocarSenhaResponse("Senha alterada com sucesso.", DateTime.UtcNow);
    }

    private static string GerarSenhaTemporariaForte(int tamanho = 12)
    {
        const string maiusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minusculas = "abcdefghijkmnopqrstuvwxyz";
        const string digitos = "23456789";
        const string especiais = "@#$!%*?&";
        const string todos = maiusculas + minusculas + digitos + especiais;

        var chars = new char[tamanho];
        chars[0] = maiusculas[RandomNumberGenerator.GetInt32(maiusculas.Length)];
        chars[1] = minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)];
        chars[2] = digitos[RandomNumberGenerator.GetInt32(digitos.Length)];
        chars[3] = especiais[RandomNumberGenerator.GetInt32(especiais.Length)];

        for (int i = 4; i < tamanho; i++)
        {
            chars[i] = todos[RandomNumberGenerator.GetInt32(todos.Length)];
        }

        // Embaralhamento Fisher-Yates criptograficamente seguro
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static string MontarTemplateHtmlResetSenha(Usuario usuario, string novaSenhaTemporaria)
    {
        var nomeSeguro = WebUtility.HtmlEncode(usuario.Nome);
        var loginSeguro = WebUtility.HtmlEncode(usuario.Login);
        var senhaSegura = WebUtility.HtmlEncode(novaSenhaTemporaria);
        var dataHoraUtc = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss 'UTC'");

        return $@"<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Redefinição de Senha — Portal Corporativo ANI</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f6f8; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1f2937;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f4f6f8; padding: 32px 16px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 16px rgba(0,0,0,0.08); border: 1px solid #e5e7eb;"">
                    <tr>
                        <td style=""background-color: #111827; padding: 24px 32px; border-bottom: 4px solid #f2c230;"">
                            <span style=""display: inline-block; background-color: #f2c230; color: #111827; font-weight: 800; font-size: 13px; padding: 4px 10px; border-radius: 4px; letter-spacing: 0.08em;"">ANI CORE</span>
                            <h1 style=""margin: 12px 0 0 0; color: #ffffff; font-size: 20px; font-weight: 600;"">Portal Corporativo Itapemirim</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 32px;"">
                            <p style=""margin: 0 0 16px 0; font-size: 16px;"">Olá, <strong>{nomeSeguro}</strong>,</p>
                            <p style=""margin: 0 0 20px 0; font-size: 14px; line-height: 1.6; color: #4b5563;"">
                                Recebemos uma solicitação de redefinição de senha para a sua conta associada ao login <strong>{loginSeguro}</strong> no <strong>Portal Corporativo ANI</strong>.
                            </p>
                            <div style=""background-color: #fffbeb; border: 1px solid #fde68a; border-left: 4px solid #f59e0b; border-radius: 8px; padding: 20px; margin: 24px 0; text-align: center;"">
                                <p style=""margin: 0 0 8px 0; font-size: 12px; text-transform: uppercase; letter-spacing: 0.08em; color: #92400e; font-weight: 700;"">Sua Nova Senha Temporária</p>
                                <p style=""margin: 0; font-family: 'Courier New', Courier, monospace; font-size: 24px; font-weight: 700; color: #111827; letter-spacing: 2px;"">{senhaSegura}</p>
                            </div>
                            <p style=""margin: 0 0 16px 0; font-size: 14px; line-height: 1.6; color: #4b5563;"">
                                Utilize esta senha temporária para realizar seu próximo acesso. Por questões de segurança, o sistema solicitará a troca da senha no seu próximo login (<code>ExigeTrocaSenhaNoProximoLogin = true</code>).
                            </p>
                            <hr style=""border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;"">
                            <p style=""margin: 0; font-size: 12px; color: #6b7280; line-height: 1.5;"">
                                <strong>Registro de Segurança:</strong> Operação processada em {dataHoraUtc}.<br>
                                Caso você não tenha solicitado esta alteração, entre em contato imediatamente com a equipe de Segurança da Informação / Suporte TI.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #f9fafb; padding: 16px 32px; text-align: center; border-top: 1px solid #e5e7eb;"">
                            <p style=""margin: 0; font-size: 11px; color: #9ca3af;"">
                                &copy; {DateTime.UtcNow.Year} Grupo Itapemirim — ANI Autorizador &amp; Governança de Acessos. Mensagem automática, não responda.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
