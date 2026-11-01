# Documentação de Ajustes: Segregação de Perfis por Sistema (Multi-sistema / Multi-portal)

## 1. Visão Geral da Evolução

Esta evolução arquitetural implementa o suporte a **múltiplos sistemas/portais** no **ANI Core (Portal Itapemirim)**. O domínio foi expandido para permitir a segregação de acessos por sistema, a criação de perfis homônimos vinculados a portais distintos (ex: "Administrador" no Portal Financeiro e "Administrador" no Portal de Passagens) e a aplicação da regra mandatória: **um usuário não pode possuir mais de um perfil ativo no mesmo sistema**.

---

## 2. Regras de Negócio e Integridade Implementadas

1. **Entidade `Sistema` (ou Portal):**
   - Identificadores: `SistemaId` (BIGINT identity clustered) e `SistemaGuid` (UUID externo).
   - Campos: `Codigo` (slug único em maiúsculas, ex: `"PORTAL_FINANCEIRO"`), `Nome`, `Descricao`, `Ativo`, `CriadoEmUtc`, `AtualizadoEmUtc`.
2. **Escopo do Perfil por Sistema:**
   - O `Perfil` pertence a um único `Sistema` através da chave estrangeira `SistemaId`.
   - Nomes de perfil podem se repetir entre sistemas diferentes, mas devem ser únicos no mesmo sistema (índice único composto `SistemaId + Nome`).
3. **Restrição Mandatória de Associação Usuário x Perfil:**
   - Um usuário pode atuar em múltiplos sistemas.
   - **Regra:** Um usuário **NÃO pode possuir mais de um perfil ativo no mesmo sistema**.
   - **Garantia Física no Banco:** Coluna `SistemaId` adicionada à tabela `UsuarioPerfil` com índice único condicional:
     ```sql
     CREATE UNIQUE INDEX [UX_UsuarioPerfil_UsuarioSistema_Ativo]
         ON [ani].[UsuarioPerfil] ([UsuarioId], [SistemaId])
         WHERE [RevogadoEmUtc] IS NULL;
     ```
   - **Garantia na Camada de Aplicação:** O `UsuarioService` valida antes de persistir e dispara `ConflitoException` caso o usuário já possua perfil ativo no sistema.
   - **Padronização RFC 7807:** O middleware mapeia a violação para `409 Conflict` (ou `422 Unprocessable Entity`), retornando detalhes padronizados do conflito.
4. **Estratégia de Token JWT Multi-sistema:**
   - Emissão de claims escopadas por sistema:
     - `system`: Lista de códigos de sistemas aos quais o usuário tem acesso.
     - `system_role`: Formato `{SISTEMA_CODIGO}:{PERFIL_NOME}`.
     - `permission`: Formato `{SISTEMA_CODIGO}:{PERMISSAO_CODIGO}`.
     - `systems_access`: JSON estruturado contendo os acessos consolidados por sistema.
     - `current_system`: Identificador do sistema quando o login solicita um contexto específico via `sistemaCodigo`.

---

## 3. Inventário de Arquivos Criados e Alterados

### 3.1. ANI.Domain (`src/Itapemirim.Ani.Domain`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `Entities/Sistema.cs` | **Criado** | Nova entidade de domínio para sistemas/portais com validações e métodos de atualização/desativação. |
| `Exceptions/ConflitoException.cs` | **Criado** | Exceção de domínio para conflitos de estado e unicidade (HTTP 409). |
| `Interfaces/ISistemaRepository.cs` | **Criado** | Interface do repositório de sistemas (`GetByCodigoAsync`, `ExistsCodigoAsync`, etc.). |
| `Entities/Perfil.cs` | **Alterado** | Adicionada FK `SistemaId`, navegação `Sistema` e construtor com vínculo de sistema. |
| `Entities/UsuarioPerfil.cs` | **Alterado** | Adicionada FK `SistemaId` e navegação `Sistema` para garantir integridade física no banco. |
| `Interfaces/IPerfilRepository.cs` | **Alterado** | Adicionado método `ExistsNomeNoSistemaAsync` e filtro opcional por `sistemaId`. |

### 3.2. ANI.Infrastructure (`src/Itapemirim.Ani.Infrastructure`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `Data/Configurations/SistemaConfiguration.cs` | **Criado** | Mapeamento Fluent API da tabela `Sistema` (índices únicos em `SistemaGuid` e `Codigo`). |
| `Data/Repositories/SistemaRepository.cs` | **Criado** | Implementação do repositório `ISistemaRepository` com EF Core. |
| `Data/Configurations/PerfilConfiguration.cs` | **Alterado** | Mapeamento da FK `FK_Perfil_Sistema` e índice composto `UX_Perfil_Sistema_Nome`. |
| `Data/Configurations/UsuarioPerfilConfiguration.cs` | **Alterado** | Mapeamento da FK `FK_UsuarioPerfil_Sistema` e índice único condicional `UX_UsuarioPerfil_UsuarioSistema_Ativo`. |
| `Data/Repositories/PerfilRepository.cs` | **Alterado** | Inclusão de `Sistema` nas consultas e implementação de `ExistsNomeNoSistemaAsync`. |
| `Data/Repositories/UsuarioRepository.cs` | **Alterado** | `Include` navegacional de `Sistema` nos perfis associados ao usuário. |
| `Data/AniDbContext.cs` | **Alterado** | Inclusão do `DbSet<Sistema> Sistemas`. |
| `DependencyInjection.cs` | **Alterado** | Registro de escopo para `ISistemaRepository, SistemaRepository`. |

### 3.3. ANI.Application (`src/Itapemirim.Ani.Application`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `DTOs/Sistemas/SistemaResponse.cs` | **Criado** | DTO de saída detalhado do sistema. |
| `DTOs/Sistemas/SistemaResumidoResponse.cs` | **Criado** | DTO de saída resumido do sistema. |
| `DTOs/Sistemas/SistemaCreateRequest.cs` | **Criado** | DTO de entrada para cadastro de sistema. |
| `DTOs/Sistemas/SistemaUpdateRequest.cs` | **Criado** | DTO de entrada para atualização de sistema. |
| `DTOs/Auth/SistemaAcessoResponse.cs` | **Criado** | DTO que agrupa os acessos do usuário por sistema. |
| `Interfaces/ISistemaService.cs` | **Criado** | Interface do serviço de aplicação de sistemas. |
| `Services/SistemaService.cs` | **Criado** | Implementação das regras de negócio de cadastro, listagem, atualização e auditoria de sistemas. |
| `Validators/SistemaCreateValidator.cs` | **Criado** | Validador FluentValidation para criação de sistema. |
| `Validators/SistemaUpdateValidator.cs` | **Criado** | Validador FluentValidation para atualização de sistema. |
| `DTOs/Perfis/PerfilCreateRequest.cs` | **Alterado** | Adicionado campo obrigatório `Guid SistemaId`. |
| `DTOs/Perfis/PerfilUpdateRequest.cs` | **Alterado** | Adicionado campo opcional `Guid? SistemaId`. |
| `DTOs/Perfis/PerfilResponse.cs` | **Alterado** | Exibição de `SistemaId`, `SistemaCodigo` e `SistemaNome`. |
| `DTOs/Perfis/PerfilResumidoResponse.cs` | **Alterado** | Exibição das propriedades do sistema vinculado. |
| `DTOs/Auth/PerfilUsuarioAutenticadoResponse.cs` | **Alterado** | Inclusão de dados do sistema no retorno de perfis autenticados. |
| `DTOs/Auth/UsuarioAutenticadoResponse.cs` | **Alterado** | Inclusão de lista agrupada `List<SistemaAcessoResponse> Sistemas`. |
| `DTOs/Auth/ValidarCredenciaisRequest.cs` | **Alterado** | Adicionado parâmetro opcional `SistemaCodigo` para login contextualizado. |
| `Interfaces/IPerfilService.cs` | **Alterado** | `ListarAsync` com suporte a filtro opcional por `sistemaGuid`. |
| `Services/PerfilService.cs` | **Alterado** | Validação de sistema existente e unicidade de nome dentro do mesmo sistema. |
| `Services/UsuarioService.cs` | **Alterado** | **Validação mandatória de perfil único por sistema** lançando `ConflitoException`. |
| `Services/AuthInternoService.cs` | **Alterado** | Emissão de claims JWT escopadas (`system`, `system_role`, `permission:SISTEMA:CODIGO`). |
| `Validators/PerfilCreateValidator.cs` | **Alterado** | Validação de preenchimento obrigatório do `SistemaId`. |
| `DependencyInjection.cs` | **Alterado** | Registro de `ISistemaService, SistemaService`. |

### 3.4. ANI.Api (`src/Itapemirim.Ani.Api`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `Controllers/SistemasController.cs` | **Criado** | Controller REST para operações CRUD completas de `Sistemas`. |
| `Controllers/PerfisController.cs` | **Alterado** | Ação `Listar` com suporte a query parameter `[FromQuery] Guid? sistemaGuid`. |
| `Middlewares/ExceptionHandlingMiddleware.cs` | **Alterado** | Mapeamento de `ConflitoException` para status HTTP `409 Conflict` (RFC 7807 ProblemDetails). |
| `Program.cs` | **Alterado** | Estratégia de recriação automática do SQLite local caso a tabela `Sistema` não exista. |

### 3.5. Scripts e Banco de Dados (`scripts/` e `.sql/`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `.sql/ani-schema.sql` | **Alterado** | DDL SQL Server atualizado com tabela `[ani].[Sistema]`, colunas `SistemaId` e índices de unicidade. |
| `scripts/seed_database_api.ps1` | **Alterado** | Script PowerShell de seed atualizado com 3 sistemas, perfis homônimos, testes de vinculação e validação de conflito 409. |
| `scripts/seed_database_api.sh` | **Alterado** | Script Bash equivalente para Linux/macOS e CI/CD. |

### 3.6. Testes Automatizados (`tests/`)
| Arquivo | Tipo | Descrição |
| :--- | :--- | :--- |
| `Itapemirim.Ani.UnitTests/UsuarioPerfilRestricaoTests.cs` | **Criado** | Testes unitários com xUnit validando a proibição de mais de um perfil no mesmo sistema e a permissão entre sistemas diferentes. |

---

## 4. Estratégia de Migração e Banco de Dados Local (SQLite)

Para o ambiente de desenvolvimento local usando SQLite:
- O [Program.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Program.cs) detecta automaticamente se a tabela `Sistema` já existe no banco `ani.db`.
- Para evitar bloqueios do Windows (`IOException: The process cannot access the file ... ani.db because it is being used by another process`), a conexão aberta para inspeção é fechada e as pools de conexão ADO.NET são limpas com `Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools()`.
- Se o banco de dados local existente estiver desatualizado (sem a tabela `Sistema`), ele é recriado via `EnsureDeleted()` + `EnsureCreated()`, aplicando todo o novo esquema com integridade referencial.
- Em execuções subsequentes, os dados são preservados normalmente.

---

## 5. Exemplos de Chamadas e Validação da API (cURL)

### 5.1. Criação dos Sistemas
```bash
# 1. Portal Corporativo ANI (Administração Central)
curl -X POST "http://localhost:5146/api/v1/sistemas" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "PORTAL_ADMIN",
    "nome": "Portal Corporativo ANI",
    "descricao": "Governança, gestão de acessos corporativos, usuários e auditoria centralizada",
    "ativo": true
  }'

# 2. Portal Financeiro
curl -X POST "http://localhost:5146/api/v1/sistemas" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "PORTAL_FINANCEIRO",
    "nome": "Portal Financeiro",
    "descricao": "Gestão financeira e relatórios de conciliação",
    "ativo": true
  }'

# 3. Portal de Passagens
curl -X POST "http://localhost:5146/api/v1/sistemas" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "PORTAL_PASSAGENS",
    "nome": "Portal de Passagens e Reservas",
    "descricao": "Emissão de bilhetes e controle de rotas",
    "ativo": true
  }'
```

### 5.2. Criação de Perfis Homônimos em Sistemas Distintos
*(Nomes iguais são permitidos em sistemas diferentes)*

```bash
# Perfil "Administrador" no Portal Financeiro
curl -X POST "http://localhost:5146/api/v1/perfis" \
  -H "Content-Type: application/json" \
  -d '{
    "sistemaId": "<GUID_PORTAL_FINANCEIRO>",
    "nome": "Administrador",
    "descricao": "Admin Financeiro",
    "ativo": true
  }'

# Perfil "Administrador" no Portal de Passagens
curl -X POST "http://localhost:5146/api/v1/perfis" \
  -H "Content-Type: application/json" \
  -d '{
    "sistemaId": "<GUID_PORTAL_PASSAGENS>",
    "nome": "Administrador",
    "descricao": "Admin Passagens",
    "ativo": true
  }'

# Perfil "Analista Financeiro" no Portal Financeiro
curl -X POST "http://localhost:5146/api/v1/perfis" \
  -H "Content-Type: application/json" \
  -d '{
    "sistemaId": "<GUID_PORTAL_FINANCEIRO>",
    "nome": "Analista Financeiro",
    "descricao": "Operação Financeira",
    "ativo": true
  }'
```

### 5.3. Associação Bem-Sucedida do Usuário a Sistemas Diferentes
```bash
# Atribui "Administrador" do Portal Financeiro (Retorna 204 No Content)
curl -i -X POST "http://localhost:5146/api/v1/usuarios/<GUID_USUARIO>/perfis" \
  -H "Content-Type: application/json" \
  -d '{ "perfilId": "<GUID_PERFIL_ADMIN_FINANCEIRO>" }'

# Atribui "Administrador" do Portal de Passagens ao mesmo usuário (Retorna 204 No Content)
curl -i -X POST "http://localhost:5146/api/v1/usuarios/<GUID_USUARIO>/perfis" \
  -H "Content-Type: application/json" \
  -d '{ "perfilId": "<GUID_PERFIL_ADMIN_PASSAGENS>" }'
```

### 5.4. Tentativa de Associar 2º Perfil no Mesmo Sistema (Violação da Regra)
```bash
# Tentativa de atribuir "Analista Financeiro" ao mesmo usuário no Portal Financeiro
curl -i -X POST "http://localhost:5146/api/v1/usuarios/<GUID_USUARIO>/perfis" \
  -H "Content-Type: application/json" \
  -d '{ "perfilId": "<GUID_PERFIL_ANALISTA_FINANCEIRO>" }'
```

#### Resposta Retornada (`409 Conflict` - RFC 7807):
```http
HTTP/1.1 409 Conflict
Content-Type: application/problem+json

{
  "type": "about:blank",
  "title": "Conflito de estado ou unicidade violada",
  "status": 409,
  "detail": "O usuário já possui o perfil 'Administrador' ativo no sistema 'Portal Financeiro'. Não é permitido atribuir mais de um perfil no mesmo sistema.",
  "instance": "/api/v1/usuarios/<GUID_USUARIO>/perfis"
}
```

---

## 6. Como Rodar e Testar Localmente

1. Reiniciar a API para carregar os novos assemblies compilados:
   ```powershell
   dotnet run --project src/Itapemirim.Ani.Api
   ```
2. Executar o script de seed automatizado:
   ```powershell
   .\scripts\seed_database_api.ps1
   ```
3. Executar os testes unitários:
   ```powershell
   dotnet test tests/Itapemirim.Ani.UnitTests
   ```
   *Resultado: 3 testes aprovados, 0 falhas.*
