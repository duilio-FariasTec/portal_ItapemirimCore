namespace Itapemirim.Ani.Api;

using Itapemirim.Ani.Application.Interfaces;
using Itapemirim.Ani.Domain.Entities;
using Itapemirim.Ani.Domain.Enums;
using Itapemirim.Ani.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services, bool resetDatabase = false, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AniDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" >> 0. Preparando Banco de Dados Local (Modo Offline)");
        Console.WriteLine("========================================================");
        Console.ResetColor();

        if (resetDatabase)
        {
            Console.WriteLine("[INFO] Recriando banco de dados do zero (--reset-db)...");
            await db.Database.EnsureDeletedAsync(ct);
        }

        await db.Database.EnsureCreatedAsync(ct);
        WriteSuccess("Schema e tabelas verificados/criados com sucesso.");

        // 1. Sistemas
        WriteSection("1. Criando Sistemas");
        var sistemasParaCriar = new (string Codigo, string Nome, string Descricao)[]
        {
            ("PORTAL_ADMIN", "Portal Corporativo ANI", "Governança, gestão de acessos corporativos, usuários e auditoria centralizada"),
            ("PORTAL_FINANCEIRO", "Portal Financeiro", "Gestão financeira, faturamento, conciliação e relatórios fiscais"),
            ("PORTAL_PASSAGENS", "Portal de Passagens e Reservas", "Emissão de bilhetes, controle de linhas, rotas e tarifas")
        };

        var mapSistemas = new Dictionary<string, Sistema>();
        foreach (var s in sistemasParaCriar)
        {
            var sistema = await db.Sistemas.FirstOrDefaultAsync(x => x.Codigo == s.Codigo, ct);
            if (sistema == null)
            {
                sistema = new Sistema(s.Codigo, s.Nome, s.Descricao, true);
                await db.Sistemas.AddAsync(sistema, ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Sistema '{s.Codigo}' criado com ID: {sistema.SistemaGuid}");
            }
            else
            {
                WriteInfo($"Sistema '{s.Codigo}' já existente com ID: {sistema.SistemaGuid}");
            }
            mapSistemas[s.Codigo] = sistema;
        }

        // 2. Permissões
        WriteSection("2. Criando Permissões");
        var permissoesParaCriar = new (string Codigo, string Nome, string Descricao, string Recurso, string Acao, string DscUrl)[]
        {
            ("home.view", "Acesso à tela Home", "Acesso à tela Home", "Home", "Visualizar", "/home"),
            ("precos.view", "Acesso à consulta/gestão de Preços", "Acesso à consulta e gestão da tabela de preços", "Precos", "Visualizar", "/precos"),
            ("passagens.emitir", "Emissão e Reserva de Passagens", "Permite emitir e reservar passagens rodoviárias", "Passagens", "Emitir", "/passagens"),
            ("usuarios.manage", "Acesso à gestão de Usuários", "Acesso completo à gestão de usuários, perfis e permissões", "Usuarios", "Gerenciar", "/usuarios"),
            ("relatorios.view", "Visualização de Relatórios", "Acesso a relatórios analíticos e dashboards executivos", "Relatorios", "Visualizar", "/relatorios")
        };

        var mapPermissoes = new Dictionary<string, Permissao>();
        foreach (var p in permissoesParaCriar)
        {
            var permissao = await db.Permissoes.FirstOrDefaultAsync(x => x.Codigo == p.Codigo, ct);
            if (permissao == null)
            {
                permissao = new Permissao(p.Codigo, p.Nome, p.Descricao, p.Recurso, p.Acao, p.DscUrl, true);
                await db.Permissoes.AddAsync(permissao, ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Permissão '{p.Codigo}' criada com ID: {permissao.PermissaoGuid}");
            }
            else
            {
                WriteInfo($"Permissão '{p.Codigo}' já existente com ID: {permissao.PermissaoGuid}");
            }
            mapPermissoes[p.Codigo] = permissao;
        }

        // 3. Perfis por Sistema
        WriteSection("3. Criando Perfis por Sistema");
        var perfisParaCriar = new (string SistemaCodigo, string Nome, string Descricao)[]
        {
            ("PORTAL_ADMIN", "Administrador", "Super Administrador com governança do ANI Core"),
            ("PORTAL_ADMIN", "Auditor", "Auditor de segurança e conformidade"),
            ("PORTAL_FINANCEIRO", "Administrador", "Administrador Geral do Portal Financeiro"),
            ("PORTAL_FINANCEIRO", "Analista Financeiro", "Analista operacional de finanças e precificação"),
            ("PORTAL_PASSAGENS", "Administrador", "Administrador de Bilhetagem e Operação de Passagens"),
            ("PORTAL_PASSAGENS", "Operador de Vendas", "Emissor de bilhetes e reservas em guichês e web")
        };

        var mapPerfis = new Dictionary<string, Perfil>();
        foreach (var pf in perfisParaCriar)
        {
            var sistema = mapSistemas[pf.SistemaCodigo];
            var chave = $"{pf.SistemaCodigo}:{pf.Nome}";
            var perfil = await db.Perfis.FirstOrDefaultAsync(x => x.SistemaId == sistema.SistemaId && x.Nome == pf.Nome, ct);
            if (perfil == null)
            {
                perfil = new Perfil(sistema.SistemaId, pf.Nome, pf.Descricao, true);
                await db.Perfis.AddAsync(perfil, ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Perfil '{chave}' criado com ID: {perfil.PerfilGuid}");
            }
            else
            {
                WriteInfo($"Perfil '{chave}' já existente com ID: {perfil.PerfilGuid}");
            }
            mapPerfis[chave] = perfil;
        }

        // 4. Vincular Permissões aos Perfis
        WriteSection("4. Vinculando Permissões aos Perfis");
        var vinculos = new (string Perfil, string Permissao)[]
        {
            ("PORTAL_ADMIN:Administrador", "usuarios.manage"),
            ("PORTAL_ADMIN:Administrador", "relatorios.view"),
            ("PORTAL_ADMIN:Administrador", "home.view"),
            ("PORTAL_ADMIN:Auditor", "relatorios.view"),
            ("PORTAL_ADMIN:Auditor", "home.view"),
            ("PORTAL_FINANCEIRO:Administrador", "precos.view"),
            ("PORTAL_FINANCEIRO:Administrador", "relatorios.view"),
            ("PORTAL_FINANCEIRO:Administrador", "home.view"),
            ("PORTAL_FINANCEIRO:Analista Financeiro", "precos.view"),
            ("PORTAL_FINANCEIRO:Analista Financeiro", "home.view"),
            ("PORTAL_PASSAGENS:Administrador", "passagens.emitir"),
            ("PORTAL_PASSAGENS:Administrador", "precos.view"),
            ("PORTAL_PASSAGENS:Administrador", "relatorios.view"),
            ("PORTAL_PASSAGENS:Administrador", "home.view"),
            ("PORTAL_PASSAGENS:Operador de Vendas", "passagens.emitir"),
            ("PORTAL_PASSAGENS:Operador de Vendas", "home.view")
        };

        foreach (var v in vinculos)
        {
            var perfil = mapPerfis[v.Perfil];
            var perm = mapPermissoes[v.Permissao];
            var existe = await db.PerfilPermissoes.AnyAsync(
                x => x.PerfilId == perfil.PerfilId && x.PermissaoId == perm.PermissaoId && x.RevogadoEmUtc == null, ct);

            if (!existe)
            {
                await db.PerfilPermissoes.AddAsync(new PerfilPermissao(perfil.PerfilId, perm.PermissaoId, null), ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Vínculo criado: Perfil '{v.Perfil}' -> Permissão '{v.Permissao}'");
            }
            else
            {
                WriteInfo($"Vínculo já existente: Perfil '{v.Perfil}' -> Permissão '{v.Permissao}'");
            }
        }

        // 5. Usuários
        WriteSection("5. Criando Usuários");
        var usuariosParaCriar = new (string Nome, string Login, string Email, string Senha)[]
        {
            ("Administrador do Sistema", "admin", "admin@itapemirim.com.br", "12345678"),
            ("Analista Financeiro", "financeiro", "financeiro@itapemirim.com.br", "12345678"),
            ("Operador de Bilhetagem", "operador", "operador@itapemirim.com.br", "12345678"),
            ("Duilio Farias", "duilio.farias", "duilio@fariastecnologia.com.br", "12345678")
        };

        var mapUsuarios = new Dictionary<string, Usuario>();
        foreach (var u in usuariosParaCriar)
        {
            var senhaHash = hasher.HashPassword(u.Senha);
            var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Login == u.Login, ct);
            if (usuario == null)
            {
                usuario = new Usuario(u.Nome, u.Login, u.Email, senhaHash, UsuarioStatus.Ativo);
                await db.Usuarios.AddAsync(usuario, ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Usuário '{u.Login}' criado com ID: {usuario.UsuarioGuid}");
            }
            else
            {
                usuario.AtualizarSenha(senhaHash, exigeTrocaNoProximoLogin: false);
                db.Usuarios.Update(usuario);
                await db.SaveChangesAsync(ct);
                WriteInfo($"Usuário '{u.Login}' já existente (senha atualizada para padrão) com ID: {usuario.UsuarioGuid}");
            }
            mapUsuarios[u.Login] = usuario;
        }

        // Atualiza quaisquer outros usuários existentes no banco para a senha padrão 12345678
        var senhaPadraoHash = hasher.HashPassword("12345678");
        var todosUsuarios = await db.Usuarios.ToListAsync(ct);
        foreach (var usr in todosUsuarios)
        {
            usr.AtualizarSenha(senhaPadraoHash, exigeTrocaNoProximoLogin: false);
            db.Usuarios.Update(usr);
            WriteSuccess($"Senha atualizada para '12345678' ({senhaPadraoHash}) -> Usuário: '{usr.Login}'");
        }
        await db.SaveChangesAsync(ct);

        // Sincroniza quaisquer arquivos ani.db locais adicionais (raiz, projeto Api, bin/Debug)
        var caminhosSqlite = new[]
        {
            "ani.db",
            "src/Itapemirim.Ani.Api/ani.db",
            "src/Itapemirim.Ani.Api/bin/Debug/net10.0/ani.db",
            "src/Itapemirim.Ani.Api/bin/Seeder/net10.0/ani.db"
        };

        foreach (var caminho in caminhosSqlite.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(caminho)) continue;
            try
            {
                await using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={caminho}");
                await conn.OpenAsync(ct);
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Usuario SET SenhaHash = $hash, ExigeTrocaSenhaNoProximoLogin = 0;";
                cmd.Parameters.AddWithValue("$hash", senhaPadraoHash);
                var afetados = await cmd.ExecuteNonQueryAsync(ct);
                WriteSuccess($"Banco SQLite sincronizado ({afetados} usuários): {caminho}");
            }
            catch (Exception ex)
            {
                WriteInfo($"Aviso ao sincronizar '{caminho}': {ex.Message}");
            }
        }

        // 6. Associação Usuário x Perfil
        WriteSection("6. Associando Usuários a Perfis em Sistemas Distintos");
        var associacoes = new (string Login, string Perfil)[]
        {
            ("admin", "PORTAL_ADMIN:Administrador"),
            ("admin", "PORTAL_FINANCEIRO:Administrador"),
            ("duilio.farias", "PORTAL_ADMIN:Administrador"),
            ("duilio.farias", "PORTAL_PASSAGENS:Administrador")
        };

        foreach (var a in associacoes)
        {
            var usuario = mapUsuarios[a.Login];
            var perfil = mapPerfis[a.Perfil];
            var existe = await db.UsuarioPerfis.AnyAsync(
                x => x.UsuarioId == usuario.UsuarioId && x.SistemaId == perfil.SistemaId && x.RevogadoEmUtc == null, ct);

            if (!existe)
            {
                await db.UsuarioPerfis.AddAsync(new UsuarioPerfil(usuario.UsuarioId, perfil.PerfilId, perfil.SistemaId, usuario.UsuarioId), ct);
                await db.SaveChangesAsync(ct);
                WriteSuccess($"Usuário '{a.Login}' associado a '{a.Perfil}'");
            }
            else
            {
                WriteInfo($"Usuário '{a.Login}' já associado ao sistema de '{a.Perfil}'");
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[CONCLUÍDO] Criação e inserts padrão executados diretamente no banco (sem precisar subir os serviços)!");
        Console.ResetColor();
    }

    private static void WriteSection(string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n========================================================");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($" >> {title}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================================");
        Console.ResetColor();
    }

    private static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[OK] {message}");
        Console.ResetColor();
    }

    private static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }
}

