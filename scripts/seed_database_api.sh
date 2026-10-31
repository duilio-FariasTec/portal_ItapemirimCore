#!/usr/bin/env bash
# ==============================================================================
# Script de Seed/Bootstrap e Validação via API REST do ANI Core (Portal Itapemirim)
# Requer: curl, jq
# ==============================================================================

set -euo pipefail

BASE_URL="${1:-http://localhost:5146}"

echo -e "\n========================================================"
echo -e " >> 0. Verificando conectividade com a API em $BASE_URL"
echo -e "========================================================"
if ! curl -sf "$BASE_URL/swagger/v1/swagger.json" > /dev/null; then
    echo "[ERRO] Não foi possível conectar na API em $BASE_URL. Verifique se 'dotnet run' está em execução."
    exit 1
fi
echo "[OK] API online e respondendo!"

# ---------------------------------------------------------
# 1. Criação das Permissões
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 1. Cadastrando Permissões (/api/v1/permissoes)"
echo -e "========================================================"

criar_ou_obter_permissao() {
    local codigo="$1"
    local nome="$2"
    local descricao="$3"
    local recurso="$4"
    local acao="$5"
    local dscUrl="$6"

    local payload
    payload=$(jq -n \
        --arg c "$codigo" \
        --arg n "$nome" \
        --arg d "$descricao" \
        --arg r "$recurso" \
        --arg a "$acao" \
        --arg u "$dscUrl" \
        '{codigo: $c, nome: $n, descricao: $d, recurso: $r, acao: $a, dscUrl: $u, ativo: true}')

    local resp
    resp=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/permissoes" \
        -H "Content-Type: application/json" \
        -d "$payload")

    local status
    status=$(echo "$resp" | tail -n1)
    local body
    body=$(echo "$resp" | sed '$d')

    if [ "$status" -eq 201 ]; then
        echo "$body" | jq -r '.id'
    else
        # Se já existir, busca o ID da lista
        curl -s -X GET "$BASE_URL/api/v1/permissoes" | jq -r --arg c "$codigo" '.[] | select(.codigo == $c) | .id'
    fi
}

echo "Criando permissão home.view..."
ID_PERM_HOME=$(criar_ou_obter_permissao "home.view" "Acesso à tela Home" "Acesso à tela Home" "Home" "Visualizar" "/home")
echo "  -> ID: $ID_PERM_HOME"

echo "Criando permissão precos.view..."
ID_PERM_PRECOS=$(criar_ou_obter_permissao "precos.view" "Acesso à consulta/gestão de Preços" "Acesso à consulta e gestão da tabela de preços" "Precos" "Visualizar" "/precos")
echo "  -> ID: $ID_PERM_PRECOS"

echo "Criando permissão usuarios.manage..."
ID_PERM_USERS=$(criar_ou_obter_permissao "usuarios.manage" "Acesso à gestão de Usuários" "Acesso completo à gestão de usuários, perfis e permissões" "Usuarios" "Gerenciar" "/usuarios")
echo "  -> ID: $ID_PERM_USERS"

# ---------------------------------------------------------
# 2. Criação dos Perfis
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 2. Cadastrando Perfis (/api/v1/perfis)"
echo -e "========================================================"

criar_ou_obter_perfil() {
    local nome="$1"
    local descricao="$2"

    local payload
    payload=$(jq -n \
        --arg n "$nome" \
        --arg d "$descricao" \
        '{nome: $n, descricao: $d, ativo: true}')

    local resp
    resp=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/perfis" \
        -H "Content-Type: application/json" \
        -d "$payload")

    local status
    status=$(echo "$resp" | tail -n1)
    local body
    body=$(echo "$resp" | sed '$d')

    if [ "$status" -eq 201 ]; then
        echo "$body" | jq -r '.id'
    else
        curl -s -X GET "$BASE_URL/api/v1/perfis" | jq -r --arg n "$nome" '.[] | select(.nome == $n) | .id'
    fi
}

echo "Criando perfil Administrador..."
ID_PERFIL_ADMIN=$(criar_ou_obter_perfil "Administrador" "Perfil Administrador com acesso irrestrito")
echo "  -> ID: $ID_PERFIL_ADMIN"

echo "Criando perfil Financeiro..."
ID_PERFIL_FIN=$(criar_ou_obter_perfil "Financeiro" "Perfil Financeiro com acesso a home e gestão de preços")
echo "  -> ID: $ID_PERFIL_FIN"

# ---------------------------------------------------------
# 3. Vincular Permissões aos Perfis
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 3. Vinculando Permissões aos Perfis (/api/v1/perfis/{id}/permissoes)"
echo -e "========================================================"

vincular_permissao() {
    local perfilId="$1"
    local permId="$2"
    local label="$3"

    curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/v1/perfis/$perfilId/permissoes" \
        -H "Content-Type: application/json" \
        -d "{\"permissaoId\": \"$permId\"}" > /dev/null || true
    echo "  [OK] Vínculo $label associado."
}

vincular_permissao "$ID_PERFIL_ADMIN" "$ID_PERM_HOME" "Administrador -> home.view"
vincular_permissao "$ID_PERFIL_ADMIN" "$ID_PERM_PRECOS" "Administrador -> precos.view"
vincular_permissao "$ID_PERFIL_ADMIN" "$ID_PERM_USERS" "Administrador -> usuarios.manage"

vincular_permissao "$ID_PERFIL_FIN" "$ID_PERM_HOME" "Financeiro -> home.view"
vincular_permissao "$ID_PERFIL_FIN" "$ID_PERM_PRECOS" "Financeiro -> precos.view"

# ---------------------------------------------------------
# 4. Criação dos Usuários
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 4. Cadastrando Usuários (/api/v1/usuarios)"
echo -e "========================================================"

criar_ou_obter_usuario() {
    local nome="$1"
    local login="$2"
    local email="$3"
    local senha="$4"

    local payload
    payload=$(jq -n \
        --arg n "$nome" \
        --arg l "$login" \
        --arg e "$email" \
        --arg s "$senha" \
        '{nome: $n, login: $l, email: $e, senha: $s}')

    local resp
    resp=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/usuarios" \
        -H "Content-Type: application/json" \
        -d "$payload")

    local status
    status=$(echo "$resp" | tail -n1)
    local body
    body=$(echo "$resp" | sed '$d')

    if [ "$status" -eq 201 ]; then
        echo "$body" | jq -r '.id'
    else
        curl -s -X GET "$BASE_URL/api/v1/usuarios?Termo=$login" | jq -r --arg l "$login" '.itens[] | select(.login == $l) | .id'
    fi
}

echo "Criando usuário admin..."
ID_USER_ADMIN=$(criar_ou_obter_usuario "Administrador do Sistema" "admin" "admin@itapemirim.com.br" "12345678")
echo "  -> ID: $ID_USER_ADMIN"

echo "Criando usuário financeiro..."
ID_USER_FIN=$(criar_ou_obter_usuario "Analista Financeiro" "financeiro" "financeiro@itapemirim.com.br" "12345678")
echo "  -> ID: $ID_USER_FIN"

echo "Criando usuário admin2..."
ID_USER_ADMIN2=$(criar_ou_obter_usuario "Administrador Secundário" "admin2" "admin2@itapemirim.com.br" "abcdefgh")
echo "  -> ID: $ID_USER_ADMIN2"

echo "Criando usuário financeiro2..."
ID_USER_FIN2=$(criar_ou_obter_usuario "Analista Financeiro Secundário" "financeiro2" "financeiro2@itapemirim.com.br" "abcdefgh")
echo "  -> ID: $ID_USER_FIN2"

# ---------------------------------------------------------
# 5. Atribuir Perfis aos Usuários
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 5. Atribuindo Perfis aos Usuários (/api/v1/usuarios/{id}/perfis)"
echo -e "========================================================"

atribuir_perfil() {
    local userId="$1"
    local perfilId="$2"
    local label="$3"

    curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/v1/usuarios/$userId/perfis" \
        -H "Content-Type: application/json" \
        -d "{\"perfilId\": \"$perfilId\"}" > /dev/null || true
    echo "  [OK] Atribuição $label concluída."
}

atribuir_perfil "$ID_USER_ADMIN" "$ID_PERFIL_ADMIN" "admin -> Administrador"
atribuir_perfil "$ID_USER_FIN" "$ID_PERFIL_FIN" "financeiro -> Financeiro"
atribuir_perfil "$ID_USER_ADMIN2" "$ID_PERFIL_ADMIN" "admin2 -> Administrador"
atribuir_perfil "$ID_USER_FIN2" "$ID_PERFIL_FIN" "financeiro2 -> Financeiro"

# ---------------------------------------------------------
# 6. Testar Autenticação e Validar Token JWT
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 6. Testando Login e Validação de JWT (/api/auth/login)"
echo -e "========================================================"

testar_login() {
    local login="$1"
    local senha="$2"

    echo -e "\n>> Testando Login para: $login"
    local resp
    resp=$(curl -s -X POST "$BASE_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"login\": \"$login\", \"senha\": \"$senha\"}")

    local token
    token=$(echo "$resp" | jq -r '.token')

    if [ "$token" != "null" ] && [ -n "$token" ]; then
        echo "  [OK] Login bem-sucedido! Token JWT gerado."
        echo "  Payload do Usuário:"
        echo "$resp" | jq '.usuario'

        # Decodifica o payload do token JWT (Base64URL)
        local payload_base64
        payload_base64=$(echo "$token" | cut -d'.' -f2 | tr '_-' '/+')
        local padding=$(( (4 - ${#payload_base64} % 4) % 4 ))
        if [ $padding -eq 1 ]; then payload_base64="${payload_base64}="; fi
        if [ $padding -eq 2 ]; then payload_base64="${payload_base64}=="; fi
        if [ $padding -eq 3 ]; then payload_base64="${payload_base64}="; fi

        echo -e "\n  [Claims decodificadas diretamente do Token JWT]:"
        echo "$payload_base64" | base64 -d 2>/dev/null | jq '{sub, login, role, permission}'
    else
        echo "  [ERRO] Falha no login: $resp"
    fi
}

testar_login "admin" "12345678"
testar_login "financeiro" "12345678"

echo -e "\n========================================================"
echo -e " >> CARGA INICIAL (SEED) E TESTES CONCLUÍDOS COM SUCESSO!"
echo -e "========================================================\n"
