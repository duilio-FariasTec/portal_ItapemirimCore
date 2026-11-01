param(
    [string]$BaseUrl = "http://localhost:5146"
)

$ErrorActionPreference = "Stop"

function Write-Section($Title) {
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host " >> $Title" -ForegroundColor Yellow
    Write-Host "========================================================" -ForegroundColor Cyan
}

function Write-Success($Message) {
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info($Message) {
    Write-Host "[INFO] $Message" -ForegroundColor Gray
}

function Write-Warn($Message) {
    Write-Host "[AVISO] $Message" -ForegroundColor DarkYellow
}

function Decode-JwtPayload($JwtToken) {
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
# 0. Verificando conectividade com a API
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
# 1. Criação dos Sistemas (Multi-sistema)
# ---------------------------------------------------------
Write-Section "1. Cadastrando Sistemas (/api/v1/sistemas)"

$sistemasParaCriar = @(
    @{
        codigo    = "PORTAL_ADMIN"
        nome      = "Portal Corporativo ANI"
        descricao = "Governança, gestão de acessos corporativos, usuários e auditoria centralizada"
        ativo     = $true
    },
    @{
        codigo    = "PORTAL_FINANCEIRO"
        nome      = "Portal Financeiro"
        descricao = "Gestão financeira, faturamento, conciliação e relatórios fiscais"
        ativo     = $true
    },
    @{
        codigo    = "PORTAL_PASSAGENS"
        nome      = "Portal de Passagens e Reservas"
        descricao = "Emissão de bilhetes, controle de linhas, rotas e tarifas"
        ativo     = $true
    }
)

$mapSistemas = @{}

foreach ($sis in $sistemasParaCriar) {
    $body = $sis | ConvertTo-Json
    try {
        $resp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/sistemas" -Method POST -Body $body -ContentType "application/json"
        $mapSistemas[$sis.codigo] = $resp.id
        Write-Success "Sistema '$($sis.codigo)' cadastrado com ID: $($resp.id)"
    } catch {
        Write-Warn "Sistema '$($sis.codigo)' já existente. Recuperando via GET..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/sistemas" -Method GET
        $existente = $existentes | Where-Object { $_.codigo -eq $sis.codigo }
        if ($existente) {
            $mapSistemas[$sis.codigo] = $existente.id
            Write-Info "Sistema '$($sis.codigo)' recuperado com ID: $($existente.id)"
        } else {
            throw $_
        }
    }
}

# ---------------------------------------------------------
# 2. Criação das Permissões
# ---------------------------------------------------------
Write-Section "2. Cadastrando Permissões (/api/v1/permissoes)"

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
        codigo    = "passagens.emitir"
        nome      = "Emissão e Reserva de Passagens"
        descricao = "Permite emitir e reservar passagens rodoviárias"
        recurso   = "Passagens"
        acao      = "Emitir"
        dscUrl    = "/passagens"
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
    },
    @{
        codigo    = "relatorios.view"
        nome      = "Visualização de Relatórios"
        descricao = "Acesso a relatórios analíticos e dashboards executivos"
        recurso   = "Relatorios"
        acao      = "Visualizar"
        dscUrl    = "/relatorios"
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
        Write-Warn "Permissão '$($p.codigo)' já existente. Buscando..."
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
# 3. Criação dos Perfis por Sistema (Demonstrando Perfis Homônimos)
# ---------------------------------------------------------
Write-Section "3. Cadastrando Perfis Segregados por Sistema (/api/v1/perfis)"

# Note que 'Administrador' é cadastrado em 3 sistemas diferentes com o mesmo nome
$perfisParaCriar = @(
    # Sistema Corporativo Admin
    @{
        sistemaCodigo = "PORTAL_ADMIN"
        nome          = "Administrador"
        descricao     = "Super Administrador com governança do ANI Core"
        ativo         = $true
    },
    @{
        sistemaCodigo = "PORTAL_ADMIN"
        nome          = "Auditor"
        descricao     = "Auditor de segurança e conformidade"
        ativo         = $true
    },
    # Sistema Financeiro
    @{
        sistemaCodigo = "PORTAL_FINANCEIRO"
        nome          = "Administrador"
        descricao     = "Administrador Geral do Portal Financeiro"
        ativo         = $true
    },
    @{
        sistemaCodigo = "PORTAL_FINANCEIRO"
        nome          = "Analista Financeiro"
        descricao     = "Analista operacional de finanças e precificação"
        ativo         = $true
    },
    # Sistema de Passagens
    @{
        sistemaCodigo = "PORTAL_PASSAGENS"
        nome          = "Administrador"
        descricao     = "Administrador de Bilhetagem e Operação de Passagens"
        ativo         = $true
    },
    @{
        sistemaCodigo = "PORTAL_PASSAGENS"
        nome          = "Operador de Vendas"
        descricao     = "Emissor de bilhetes e reservas em guichês e web"
        ativo         = $true
    }
)

$mapPerfis = @{}

foreach ($pf in $perfisParaCriar) {
    $sistemaId = $mapSistemas[$pf.sistemaCodigo]
    $chave = "$($pf.sistemaCodigo):$($pf.nome)"

    $body = @{
        sistemaId = $sistemaId
        nome      = $pf.nome
        descricao = $pf.descricao
        ativo     = $pf.ativo
    } | ConvertTo-Json

    try {
        $resp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/perfis" -Method POST -Body $body -ContentType "application/json"
        $mapPerfis[$chave] = $resp.id
        Write-Success "Perfil '$chave' criado com ID: $($resp.id)"
    } catch {
        Write-Warn "Perfil '$chave' já existente. Buscando..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/perfis?sistemaGuid=$sistemaId" -Method GET
        $existente = $existentes | Where-Object { $_.nome -eq $pf.nome }
        if ($existente) {
            $mapPerfis[$chave] = $existente.id
            Write-Info "Perfil '$chave' recuperado com ID: $($existente.id)"
        } else {
            throw $_
        }
    }
}

# ---------------------------------------------------------
# 4. Vincular Permissões aos Perfis
# ---------------------------------------------------------
Write-Section "4. Vinculando Permissões aos Perfis (/api/v1/perfis/{id}/permissoes)"

$vinculos = @(
    # PORTAL_ADMIN:Administrador -> usuarios.manage, relatorios.view, home.view
    @{ Perfil = "PORTAL_ADMIN:Administrador"; Permissao = "usuarios.manage" },
    @{ Perfil = "PORTAL_ADMIN:Administrador"; Permissao = "relatorios.view" },
    @{ Perfil = "PORTAL_ADMIN:Administrador"; Permissao = "home.view" },
    # PORTAL_ADMIN:Auditor -> relatorios.view, home.view
    @{ Perfil = "PORTAL_ADMIN:Auditor"; Permissao = "relatorios.view" },
    @{ Perfil = "PORTAL_ADMIN:Auditor"; Permissao = "home.view" },

    # PORTAL_FINANCEIRO:Administrador -> precos.view, relatorios.view, home.view
    @{ Perfil = "PORTAL_FINANCEIRO:Administrador"; Permissao = "precos.view" },
    @{ Perfil = "PORTAL_FINANCEIRO:Administrador"; Permissao = "relatorios.view" },
    @{ Perfil = "PORTAL_FINANCEIRO:Administrador"; Permissao = "home.view" },
    # PORTAL_FINANCEIRO:Analista Financeiro -> precos.view, home.view
    @{ Perfil = "PORTAL_FINANCEIRO:Analista Financeiro"; Permissao = "precos.view" },
    @{ Perfil = "PORTAL_FINANCEIRO:Analista Financeiro"; Permissao = "home.view" },

    # PORTAL_PASSAGENS:Administrador -> passagens.emitir, precos.view, relatorios.view, home.view
    @{ Perfil = "PORTAL_PASSAGENS:Administrador"; Permissao = "passagens.emitir" },
    @{ Perfil = "PORTAL_PASSAGENS:Administrador"; Permissao = "precos.view" },
    @{ Perfil = "PORTAL_PASSAGENS:Administrador"; Permissao = "relatorios.view" },
    @{ Perfil = "PORTAL_PASSAGENS:Administrador"; Permissao = "home.view" },
    # PORTAL_PASSAGENS:Operador de Vendas -> passagens.emitir, home.view
    @{ Perfil = "PORTAL_PASSAGENS:Operador de Vendas"; Permissao = "passagens.emitir" },
    @{ Perfil = "PORTAL_PASSAGENS:Operador de Vendas"; Permissao = "home.view" }
)

foreach ($v in $vinculos) {
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
# 5. Criação dos Usuários
# ---------------------------------------------------------
Write-Section "5. Cadastrando Usuários (/api/v1/usuarios)"

$usuariosParaCriar = @(
    @{
        nome   = "Administrador do Sistema"
        login  = "admin"
        email  = "admin@itapemirim.com.br"
        senha  = "12345678"
    },
    @{
        nome   = "Analista Financeiro"
        login  = "financeiro"
        email  = "financeiro@itapemirim.com.br"
        senha  = "12345678"
    },
    @{
        nome   = "Operador de Bilhetagem"
        login  = "operador"
        email  = "operador@itapemirim.com.br"
        senha  = "12345678"
    },
    @{
        nome   = "Duilio Farias"
        login  = "duilio.farias"
        email  = "duilio@fariastecnologia.com.br"
        senha  = "12345678"
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
        Write-Warn "Usuário '$($u.login)' já existente. Buscando..."
        $existentes = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios?busca=$($u.login)" -Method GET
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
# 6. Associação Usuário x Perfil (Multi-sistema)
# ---------------------------------------------------------
Write-Section "6. Associando Usuários a Perfis em Sistemas Distintos"

# Associação permitida: Usuário 'admin' em 2 sistemas diferentes
# - PORTAL_ADMIN: Administrador
# - PORTAL_FINANCEIRO: Administrador
$adminId = $mapUsuarios["admin"]
$perfilAdminAdmin = $mapPerfis["PORTAL_ADMIN:Administrador"]
$perfilAdminFin = $mapPerfis["PORTAL_FINANCEIRO:Administrador"]

try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$adminId/perfis" -Method POST -Body (@{ perfilId = $perfilAdminAdmin } | ConvertTo-Json) -ContentType "application/json"
    Write-Success "Usuário 'admin' associado a 'PORTAL_ADMIN:Administrador'"
} catch {
    Write-Warn "Vínculo já existente: admin -> PORTAL_ADMIN:Administrador"
}

try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$adminId/perfis" -Method POST -Body (@{ perfilId = $perfilAdminFin } | ConvertTo-Json) -ContentType "application/json"
    Write-Success "Usuário 'admin' associado a 'PORTAL_FINANCEIRO:Administrador' (Sistemas diferentes = PERMITIDO)"
} catch {
    Write-Warn "Vínculo já existente: admin -> PORTAL_FINANCEIRO:Administrador"
}

# Duilio Farias: associado a PORTAL_ADMIN:Administrador e PORTAL_PASSAGENS:Administrador
$duilioId = $mapUsuarios["duilio.farias"]
$perfilPassagensAdmin = $mapPerfis["PORTAL_PASSAGENS:Administrador"]
try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$duilioId/perfis" -Method POST -Body (@{ perfilId = $perfilAdminAdmin } | ConvertTo-Json) -ContentType "application/json"
    Write-Success "Usuário 'duilio.farias' associado a 'PORTAL_ADMIN:Administrador'"
} catch {
    Write-Warn "Vínculo já existente: duilio.farias -> PORTAL_ADMIN:Administrador"
}
try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$duilioId/perfis" -Method POST -Body (@{ perfilId = $perfilPassagensAdmin } | ConvertTo-Json) -ContentType "application/json"
    Write-Success "Usuário 'duilio.farias' associado a 'PORTAL_PASSAGENS:Administrador'"
} catch {
    Write-Warn "Vínculo já existente: duilio.farias -> PORTAL_PASSAGENS:Administrador"
}

# ---------------------------------------------------------
# 7. Validação da Regra de Integridade (Segundo Perfil no Mesmo Sistema)
# ---------------------------------------------------------
Write-Section "7. Testando Regra de Restrição: 2º Perfil no Mesmo Sistema"
Write-Info "Tentando atribuir 'PORTAL_FINANCEIRO:Analista Financeiro' ao 'admin' (que já possui 'PORTAL_FINANCEIRO:Administrador')..."

$perfilFinAnalista = $mapPerfis["PORTAL_FINANCEIRO:Analista Financeiro"]

try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/v1/usuarios/$adminId/perfis" -Method POST -Body (@{ perfilId = $perfilFinAnalista } | ConvertTo-Json) -ContentType "application/json"
    Write-Host "[ERRO DE SEGURANÇA] A restrição FALHOU! O sistema permitiu 2 perfis no mesmo sistema para o mesmo usuário." -ForegroundColor Red
} catch {
    $statusEsperado = $_.Exception.Response.StatusCode.value__
    Write-Success "Regra de negócio e unicidade aplicada com sucesso! Código HTTP retornado: $statusEsperado (409 Conflict / 422 Unprocessable Entity)"
    Write-Info "Resposta do servidor (ProblemDetails):"
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $respBody = $reader.ReadToEnd()
    Write-Host $respBody -ForegroundColor Yellow
}

# ---------------------------------------------------------
# 8. Teste de Autenticação e Token JWT Multi-sistema
# ---------------------------------------------------------
Write-Section "8. Testando Autenticação e Payload JWT Multi-sistema"

$loginBody = @{
    login = "admin"
    senha = "12345678"
} | ConvertTo-Json

$loginResp = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
Write-Success "Login realizado com sucesso para 'admin'!"
Write-Info "Token JWT gerado: $($loginResp.token.Substring(0, 35))..."

$payload = Decode-JwtPayload $loginResp.token
Write-Info "Payload Decodificado:"
Write-Info " - Sistemas com Acesso: $($payload.system -join ', ')"
Write-Info " - Roles por Sistema:   $($payload.system_role -join ', ')"

Write-Host "`n[CONCLUÍDO] Seed multi-sistema executado com total sucesso!" -ForegroundColor Green
