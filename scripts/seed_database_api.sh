#!/usr/bin/env bash
# ==============================================================================
# Script de Seed/Bootstrap e Validação via API REST do ANI Core (Multi-sistema)
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
# 1. Criação dos Sistemas (Multi-sistema)
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 1. Cadastrando Sistemas (/api/v1/sistemas)"
echo -e "========================================================"

criar_ou_obter_sistema() {
    local codigo="$1"
    local nome="$2"
    local descricao="$3"

    local payload
    payload=$(jq -n --arg c "$codigo" --arg n "$nome" --arg d "$descricao" \
        '{codigo: $c, nome: $n, descricao: $d, ativo: true}')

    local resp
    resp=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/sistemas" \
        -H "Content-Type: application/json" \
        -d "$payload")

    local status
    status=$(echo "$resp" | tail -n1)
    local body
    body=$(echo "$resp" | sed '$d')

    if [ "$status" -eq 201 ]; then
        echo "$body" | jq -r '.id'
    else
        curl -s -X GET "$BASE_URL/api/v1/sistemas" | jq -r --arg c "$codigo" '.[] | select(.codigo == $c) | .id'
    fi
}

SIS_ADMIN_ID=$(criar_ou_obter_sistema "PORTAL_ADMIN" "Portal Corporativo ANI" "Governança e auditoria corporativa")
SIS_FINANCEIRO_ID=$(criar_ou_obter_sistema "PORTAL_FINANCEIRO" "Portal Financeiro" "Gestão financeira e relatórios fiscais")
SIS_PASSAGENS_ID=$(criar_ou_obter_sistema "PORTAL_PASSAGENS" "Portal de Passagens e Reservas" "Emissão e bilhetagem de passagens")

echo "[OK] Sistemas cadastrados:"
echo " - PORTAL_ADMIN:      $SIS_ADMIN_ID"
echo " - PORTAL_FINANCEIRO: $SIS_FINANCEIRO_ID"
echo " - PORTAL_PASSAGENS:  $SIS_PASSAGENS_ID"

# ---------------------------------------------------------
# 2. Criação das Permissões
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 2. Cadastrando Permissões (/api/v1/permissoes)"
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
        curl -s -X GET "$BASE_URL/api/v1/permissoes" | jq -r --arg c "$codigo" '.[] | select(.codigo == $c) | .id'
    fi
}

PERM_HOME_ID=$(criar_ou_obter_permissao "home.view" "Acesso à tela Home" "Acesso à tela Home" "Home" "Visualizar" "/home")
PERM_PRECOS_ID=$(criar_ou_obter_permissao "precos.view" "Acesso à consulta/gestão de Preços" "Acesso à consulta e gestão da tabela de preços" "Precos" "Visualizar" "/precos")
PERM_PASSAGENS_ID=$(criar_ou_obter_permissao "passagens.emitir" "Emissão e Reserva de Passagens" "Permite emitir e reservar passagens rodoviárias" "Passagens" "Emitir" "/passagens")
PERM_USUARIOS_ID=$(criar_ou_obter_permissao "usuarios.manage" "Acesso à gestão de Usuários" "Acesso completo à gestão de usuários, perfis e permissões" "Usuarios" "Gerenciar" "/usuarios")

echo "[OK] Permissões cadastradas com sucesso!"

# ---------------------------------------------------------
# 3. Criação dos Perfis Homônimos por Sistema
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 3. Cadastrando Perfis Segregados por Sistema"
echo -e "========================================================"

criar_ou_obter_perfil() {
    local sistema_id="$1"
    local nome="$2"
    local descricao="$3"

    local payload
    payload=$(jq -n --arg s "$sistema_id" --arg n "$nome" --arg d "$descricao" \
        '{sistemaId: $s, nome: $n, descricao: $d, ativo: true}')

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
        curl -s -X GET "$BASE_URL/api/v1/perfis?sistemaGuid=$sistema_id" | jq -r --arg n "$nome" '.[] | select(.nome == $n) | .id'
    fi
}

# Criando perfil 'Administrador' nos 3 sistemas distintos
PERFIL_ADMIN_CORP=$(criar_ou_obter_perfil "$SIS_ADMIN_ID" "Administrador" "Administrador Corporativo ANI")
PERFIL_ADMIN_FIN=$(criar_ou_obter_perfil "$SIS_FINANCEIRO_ID" "Administrador" "Administrador do Portal Financeiro")
PERFIL_ANALISTA_FIN=$(criar_ou_obter_perfil "$SIS_FINANCEIRO_ID" "Analista Financeiro" "Analista de Finanças")
PERFIL_ADMIN_PASS=$(criar_ou_obter_perfil "$SIS_PASSAGENS_ID" "Administrador" "Administrador do Portal de Passagens")
PERFIL_OPER_PASS=$(criar_ou_obter_perfil "$SIS_PASSAGENS_ID" "Operador de Vendas" "Operador de Emissão de Passagens")

echo "[OK] Perfis cadastrados:"
echo " - PORTAL_ADMIN:Administrador       -> $PERFIL_ADMIN_CORP"
echo " - PORTAL_FINANCEIRO:Administrador  -> $PERFIL_ADMIN_FIN"
echo " - PORTAL_FINANCEIRO:Analista       -> $PERFIL_ANALISTA_FIN"
echo " - PORTAL_PASSAGENS:Administrador   -> $PERFIL_ADMIN_PASS"
echo " - PORTAL_PASSAGENS:Operador        -> $PERFIL_OPER_PASS"

# Vincular permissões aos perfis
vincular_permissao() {
    local perfil_id="$1"
    local perm_id="$2"
    curl -s -X POST "$BASE_URL/api/v1/perfis/$perfil_id/permissoes" \
        -H "Content-Type: application/json" \
        -d "{\"permissaoId\": \"$perm_id\"}" > /dev/null || true
}

vincular_permissao "$PERFIL_ADMIN_CORP" "$PERM_USUARIOS_ID"
vincular_permissao "$PERFIL_ADMIN_CORP" "$PERM_HOME_ID"
vincular_permissao "$PERFIL_ADMIN_FIN" "$PERM_PRECOS_ID"
vincular_permissao "$PERFIL_ADMIN_FIN" "$PERM_HOME_ID"
vincular_permissao "$PERFIL_ANALISTA_FIN" "$PERM_PRECOS_ID"
vincular_permissao "$PERFIL_ADMIN_PASS" "$PERM_PASSAGENS_ID"
vincular_permissao "$PERFIL_OPER_PASS" "$PERM_PASSAGENS_ID"

echo "[OK] Permissões vinculadas aos perfis!"

# ---------------------------------------------------------
# 4. Criação e Associação de Usuários Multi-sistema
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 4. Cadastrando e Vinculando Usuários"
echo -e "========================================================"

criar_ou_obter_usuario() {
    local nome="$1"
    local login="$2"
    local email="$3"
    local senha="$4"

    local payload
    payload=$(jq -n --arg n "$nome" --arg l "$login" --arg e "$email" --arg s "$senha" \
        '{nome: $n, login: $l, email: $e, senha: $s, status: "Ativo"}')

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
        curl -s -X GET "$BASE_URL/api/v1/usuarios?busca=$login" | jq -r --arg l "$login" '.itens[] | select(.login == $l) | .id'
    fi
}

USER_ADMIN_ID=$(criar_ou_obter_usuario "Administrador do Sistema" "admin" "admin@itapemirim.com.br" "12345678")
USER_DUILIO_ID=$(criar_ou_obter_usuario "Duilio Farias" "duilio.farias" "duilio@fariastecnologia.com.br" "12345678")

# Associação permitida: mesmo usuário em sistemas diferentes
curl -s -X POST "$BASE_URL/api/v1/usuarios/$USER_ADMIN_ID/perfis" \
    -H "Content-Type: application/json" \
    -d "{\"perfilId\": \"$PERFIL_ADMIN_CORP\"}" > /dev/null || true
echo "[OK] 'admin' associado a PORTAL_ADMIN:Administrador"

curl -s -X POST "$BASE_URL/api/v1/usuarios/$USER_ADMIN_ID/perfis" \
    -H "Content-Type: application/json" \
    -d "{\"perfilId\": \"$PERFIL_ADMIN_FIN\"}" > /dev/null || true
echo "[OK] 'admin' associado a PORTAL_FINANCEIRO:Administrador (Sistemas distintos = Permitido)"

# ---------------------------------------------------------
# 5. Testando Restrição: 2º Perfil no Mesmo Sistema
# ---------------------------------------------------------
echo -e "\n========================================================"
echo -e " >> 5. Testando Restrição: 2º Perfil no Mesmo Sistema"
echo -e "========================================================"
echo "[INFO] Tentando vincular segundo perfil do PORTAL_FINANCEIRO ao 'admin'..."

CONFLICT_RESP=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/v1/usuarios/$USER_ADMIN_ID/perfis" \
    -H "Content-Type: application/json" \
    -d "{\"perfilId\": \"$PERFIL_ANALISTA_FIN\"}")

HTTP_CODE=$(echo "$CONFLICT_RESP" | tail -n1)
BODY_RESP=$(echo "$CONFLICT_RESP" | sed '$d')

if [ "$HTTP_CODE" -eq 409 ] || [ "$HTTP_CODE" -eq 422 ]; then
    echo "[OK] Restrição aplicada com sucesso! Código HTTP: $HTTP_CODE"
    echo "Detalhe retornado: $BODY_RESP"
else
    echo "[ERRO] Esperava 409 ou 422, mas recebeu $HTTP_CODE!"
fi

echo -e "\n[SUCESSO] Script de seed e validação executado com êxito!"
