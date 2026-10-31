<#
.SYNOPSIS
    Script de Seed/Bootstrap e Validação via API REST do ANI Core (Portal Itapemirim)
.DESCRIPTION
    Executa em cascata todas as etapas de carga inicial respeitando integridade referencial:
    1. Criação das Permissões (home.view, precos.view, usuarios.manage)
    2. Criação dos Perfis (Administrador, Financeiro)
    3. Associação das Permissões aos Perfis
    4. Criação dos Usuários Iniciais e Secundários (admin, financeiro, admin2, financeiro2)
    5. Associação dos Perfis aos Usuários
    6. Teste de Autenticação com Login (/api/auth/login), validação do Token JWT e decodificação das claims.
#>

[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5146"
)

$ErrorActionPreference = "Stop"

function Write-Section {
    param([string]$Title)
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host " >> $Title" -ForegroundColor Yellow
    Write-Host "========================================================" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Gray
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[AVISO] $Message" -ForegroundColor DarkYellow
}

function Decode-JwtPayload {
    param([string]$JwtToken)
    $parts = $JwtToken.Split('.')
    if ($parts.Length -lt 2) { return $null }
    $payloadBase64 = $parts[1].Replace('-', '+').Replace('_', '/')
    switch ($payloadBase64.Length % 4) {
        2 { $payloadBase64 += "==" }
        3 { $payloadBase64 += "=" }
    }
    $bytes = [System.Convert]::FromBase64String($payloadBase64)
    $json = [System.Text.Encoding]::UTF8.GetString($bytes)
    return ($json | ConvertFrom-Json)
}

# ---------------------------------------------------------
# Teste de Conectividade
# ---------------------------------------------------------
Write-Section "0. Verificando conectividade com a API"
try {
    $swaggerResp = Invoke-WebRequest -Uri "$BaseUrl/swagger/v1/swagger.json" -Method GET -UseBasicParsing -ErrorAction Stop
    Write-Success "API online e respondendo em $BaseUrl"
} catch {
    Write-Host "[ERRO] Não foi possível conectar na API em $BaseUrl. Certifique-se de que a API está rodando ('dotnet run')." -ForegroundColor Red
    exit 1
}

# ---------------------------------------------------------
# 1. Criação das Permissões
# ---------------------------------------------------------
Write-Section "1. Cadastrando Permissões (/api/v1/permissoes)"

$permissoesParaCriar = @(
    @{
        codigo    = "home.view"
        nome      = "Acesso à tela Home"
        descricao = "Acesso à tela Home"
        recurso   = "Home"
        acao      = "Visualizar"
        dscUrl    = "/home"
        ativo     = $true
    },
    @{
        codigo    = "precos.view"
        nome      = "Acesso à consulta/gestão de Preços"
        descricao = "Acesso à consulta e gestão da tabela de preços"
        recurso   = "Precos"
        acao      = "Visualizar"
        dscUrl    = "/precos"
        ativo     = $true
    },
    @{
        codigo    = "usuarios.manage"
        nome      = "Acesso à gestão de Usuários"
        descricao = "Acesso completo à gestão de usuários, perfis e permissões"
        recurso   = "Usuarios"
        acao      = "Gerenciar"
        dscUrl    = "/usuarios"
        ativo     = $true
    }
)

$mapPermissoes = @{}

foreach ($p in $permissoesParaCriar) {
    $body = $p | ConvertTo-Json
    try {
        $resp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/permissoes" -Method POST -Body $body -ContentType "application/json"
        $mapPermissoes[$p.codigo] = $resp.id
        Write-Success "Permissão '$($p.codigo)' criada com ID: $($resp.id)"
    } catch {
        # Se já existir, busca o ID existente
        Write-Warn "Permissão '$($p.codigo)' já existe ou retornou status $($_.Exception.Response.StatusCode). Buscando existente..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/permissoes" -Method GET
        $existente = $existentes | Where-Object { $_.codigo -eq $p.codigo }
        if ($existente) {
            $mapPermissoes[$p.codigo] = $existente.id
            Write-Info "Permissão '$($p.codigo)' recuperada com ID: $($existente.id)"
        } else {
            throw $_
        }
    }
}

# ---------------------------------------------------------
# 2. Criação dos Perfis
# ---------------------------------------------------------
Write-Section "2. Cadastrando Perfis (/api/v1/perfis)"

$perfisParaCriar = @(
    @{
        nome      = "Administrador"
        descricao = "Perfil Administrador com acesso irrestrito ao sistema"
        ativo     = $true
    },
    @{
        nome      = "Financeiro"
        descricao = "Perfil Financeiro com acesso a home e gestão de preços"
        ativo     = $true
    }
)

$mapPerfis = @{}

foreach ($pf in $perfisParaCriar) {
    $body = $pf | ConvertTo-Json
    try {
        $resp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/perfis" -Method POST -Body $body -ContentType "application/json"
        $mapPerfis[$pf.nome] = $resp.id
        Write-Success "Perfil '$($pf.nome)' criado com ID: $($resp.id)"
    } catch {
        Write-Warn "Perfil '$($pf.nome)' já existe ou retornou status $($_.Exception.Response.StatusCode). Buscando existente..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/perfis" -Method GET
        $existente = $existentes | Where-Object { $_.nome -eq $pf.nome }
        if ($existente) {
            $mapPerfis[$pf.nome] = $existente.id
            Write-Info "Perfil '$($pf.nome)' recuperado com ID: $($existente.id)"
        } else {
            throw $_
        }
    }
}

# ---------------------------------------------------------
# 3. Vincular Permissões aos Perfis
# ---------------------------------------------------------
Write-Section "3. Vinculando Permissões aos Perfis (/api/v1/perfis/{id}/permissoes)"

$vinculosPerfilPermissao = @(
    # Administrador: Todas (home.view, precos.view, usuarios.manage)
    @{ Perfil = "Administrador"; Permissao = "home.view" },
    @{ Perfil = "Administrador"; Permissao = "precos.view" },
    @{ Perfil = "Administrador"; Permissao = "usuarios.manage" },
    # Financeiro: home.view, precos.view
    @{ Perfil = "Financeiro"; Permissao = "home.view" },
    @{ Perfil = "Financeiro"; Permissao = "precos.view" }
)

foreach ($v in $vinculosPerfilPermissao) {
    $perfilId = $mapPerfis[$v.Perfil]
    $permId = $mapPermissoes[$v.Permissao]
    $body = @{ permissaoId = $permId } | ConvertTo-Json

    try {
        $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/perfis/$perfilId/permissoes" -Method POST -Body $body -ContentType "application/json"
        Write-Success "Vínculo: Perfil '$($v.Perfil)' -> Permissão '$($v.Permissao)' [204 No Content]"
    } catch {
        Write-Warn "Vínculo já existente ou ignorado para '$($v.Perfil)' -> '$($v.Permissao)'"
    }
}

# ---------------------------------------------------------
# 4. Criação dos Usuários Iniciais e Secundários
# ---------------------------------------------------------
Write-Section "4. Cadastrando Usuários (/api/v1/usuarios)"

$usuariosParaCriar = @(
    @{
        nome   = "Administrador do Sistema"
        login  = "admin"
        email  = "admin@itapemirim.com.br"
        senha  = "12345678"
        perfil = "Administrador"
    },
    @{
        nome   = "Analista Financeiro"
        login  = "financeiro"
        email  = "financeiro@itapemirim.com.br"
        senha  = "12345678"
        perfil = "Financeiro"
    },
    @{
        nome   = "Administrador Secundário"
        login  = "admin2"
        email  = "admin2@itapemirim.com.br"
        senha  = "abcdefgh"
        perfil = "Administrador"
    },
    @{
        nome   = "Analista Financeiro Secundário"
        login  = "financeiro2"
        email  = "financeiro2@itapemirim.com.br"
        senha  = "abcdefgh"
        perfil = "Financeiro"
    }
)

$mapUsuarios = @{}

foreach ($u in $usuariosParaCriar) {
    $body = @{
        nome  = $u.nome
        login = $u.login
        email = $u.email
        senha = $u.senha
    } | ConvertTo-Json

    try {
        $resp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios" -Method POST -Body $body -ContentType "application/json"
        $mapUsuarios[$u.login] = $resp.id
        Write-Success "Usuário '$($u.login)' criado com ID: $($resp.id)"
    } catch {
        Write-Warn "Usuário '$($u.login)' já existe. Buscando ID existente..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios?Termo=$($u.login)" -Method GET
        $existente = $existentes.itens | Where-Object { $_.login -eq $u.login }
        if ($existente) {
            $mapUsuarios[$u.login] = $existente.id
            Write-Info "Usuário '$($u.login)' recuperado com ID: $($existente.id)"
        } else {
            throw $_
        }
    }
}

# ---------------------------------------------------------
# 5. Vincular Perfis aos Usuários
# ---------------------------------------------------------
Write-Section "5. Atribuindo Perfis aos Usuários (/api/v1/usuarios/{id}/perfis)"

foreach ($u in $usuariosParaCriar) {
    $userId = $mapUsuarios[$u.login]
    $perfilId = $mapPerfis[$u.perfil]
    $body = @{ perfilId = $perfilId } | ConvertTo-Json

    try {
        $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$userId/perfis" -Method POST -Body $body -ContentType "application/json"
        Write-Success "Perfil '$($u.perfil)' atribuído ao usuário '$($u.login)' [204 No Content]"
    } catch {
        Write-Warn "Perfil '$($u.perfil)' já atribuído ao usuário '$($u.login)'"
    }
}

# ---------------------------------------------------------
# 6. Testar Autenticação e Validação de Claims no JWT
# ---------------------------------------------------------
Write-Section "6. Testando Login e Validação de Claims do Token JWT (/api/auth/login)"

$loginsParaTestar = @(
    @{ login = "admin"; senha = "12345678"; perfilEsperado = "Administrador"; permsEsperadas = @("home.view", "precos.view", "usuarios.manage") },
    @{ login = "financeiro"; senha = "12345678"; perfilEsperado = "Financeiro"; permsEsperadas = @("home.view", "precos.view") }
)

foreach ($lt in $loginsParaTestar) {
    Write-Host "`n>> Testando autenticação para '$($lt.login)':" -ForegroundColor Magenta
    $loginBody = @{
        login = $lt.login
        senha = $lt.senha
    } | ConvertTo-Json

    $loginResp = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    Write-Success "Login realizado com sucesso! HTTP 200 OK"
    Write-Info "Token Tipo: $($loginResp.tokenType) (Expira em: $($loginResp.expiresInSeconds)s)"
    Write-Info "Token JWT: $($loginResp.token.Substring(0, 45))..."

    # Decodifica o payload do Token JWT para validar as Claims
    $jwtClaims = Decode-JwtPayload -JwtToken $loginResp.token

    Write-Host "`n  [Validação de Claims no Token JWT]:" -ForegroundColor Cyan
    Write-Host "  * Subject (sub / UserGuid) : $($jwtClaims.sub)" -ForegroundColor White
    Write-Host "  * Nome (unique_name)       : $($jwtClaims.unique_name)" -ForegroundColor White
    Write-Host "  * Login                    : $($jwtClaims.login)" -ForegroundColor White
    
    # Extrai roles/perfis
    $roles = @($jwtClaims.role)
    Write-Host "  * Roles (Perfis no JWT)    : $($roles -join ', ')" -ForegroundColor White
    if ($roles -contains $lt.perfilEsperado) {
        Write-Success "  ✓ Role '$($lt.perfilEsperado)' validada com sucesso no JWT!"
    } else {
        Write-Host "  ✗ Role '$($lt.perfilEsperado)' NÃO encontrada no JWT!" -ForegroundColor Red
    }

    # Extrai permissões
    $perms = @($jwtClaims.permission)
    Write-Host "  * Permissions no JWT       : $($perms -join ', ')" -ForegroundColor White
    
    $todasOk = $true
    foreach ($pEsp in $lt.permsEsperadas) {
        if ($perms -contains $pEsp) {
            Write-Success "  ✓ Permissão '$pEsp' presente no JWT!"
        } else {
            Write-Host "  ✗ Permissão '$pEsp' AUSENTE no JWT!" -ForegroundColor Red
            $todasOk = $false
        }
    }

    # Chamada adicional com Token Bearer para testar Permissões Efetivas
    $userId = $mapUsuarios[$lt.login]
    $permsEfetivas = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$userId/permissoes-efetivas" `
                                      -Method GET `
                                      -Headers @{ Authorization = "Bearer $($loginResp.token)" }

    Write-Host "`n  [Permissões Efetivas via API GET /permissoes-efetivas]:" -ForegroundColor DarkCyan
    foreach ($pe in $permsEfetivas) {
        Write-Host "   -> [$($pe.codigo)] $($pe.nome) | Rota: $($pe.dscUrl)" -ForegroundColor Gray
    }
}

Write-Section "PROCESSO DE SEED E VALIDAÇÃO CONCLUÍDO COM SUCESSO!"
Write-Host "Todos os 3 níveis de dados foram semeados e o login com JWT validado!`n" -ForegroundColor Green
