# Guia e Script de Criação das Classes da Solução — ANI Core (.NET)

Este documento contém o guia completo e o script PowerShell para gerar **todas as classes, interfaces, DTOs, configurações do EF Core, controladores REST e injeção de dependência** da solução `Itapemirim.Ani`.

> [!NOTE]
> O script abaixo já foi salvo e testado no repositório em [criar_todas_classes.ps1](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/scripts/criar_todas_classes.ps1), e a compilação via `dotnet build Itapemirim.Ani.sln` foi validada com **0 Erros**.

---

## 1. Como Executar o Script

Abra o terminal do PowerShell na raiz do projeto (`c:\Users\Duilio Farias\source\repos\portal_ItapemirimCore`) e execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\criar_todas_classes.ps1
```

Para validar a compilação de todos os projetos:

```powershell
dotnet build Itapemirim.Ani.sln
```

---

## 2. Mapa dos Arquivos e Classes Criados por Projeto

### 2.1. `src/Itapemirim.Ani.Domain` (Models & Regras de Domínio)
* **`Enums/`**:
  * [UsuarioStatus.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Enums/UsuarioStatus.cs) (`Ativo`, `Inativo`, `Bloqueado`)
  * [TipoAcaoAuditoria.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Enums/TipoAcaoAuditoria.cs) (`INCLUSAO`, `ALTERACAO`, `REMOCAO`)
  * [ResultadoAuditoria.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Enums/ResultadoAuditoria.cs) (`SUCESSO`, `FALHA`)
* **`Exceptions/`**:
  * [DomainException.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Exceptions/DomainException.cs)
  * [EntidadeNaoEncontradaException.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Exceptions/EntidadeNaoEncontradaException.cs)
  * [RegraNegocioException.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Exceptions/RegraNegocioException.cs)
* **`Entities/`**:
  * [Usuario.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/Usuario.cs): Entidade com `UsuarioId` (long), `UsuarioGuid` (Guid público), `Nome`, `Login`, `Email`, `SenhaHash`, `Status` e coleções de navegação.
  * [Perfil.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/Perfil.cs): Entidade com `PerfilId`, `PerfilGuid`, `Nome`, `Descricao`, `Ativo`.
  * [Permissao.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/Permissao.cs): Entidade com `PermissaoId`, `PermissaoGuid`, `Codigo`, `Nome`, `Recurso`, `Acao`.
  * [UsuarioPerfil.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/UsuarioPerfil.cs): Vínculo com histórico (`ConcedidoEmUtc`, `RevogadoEmUtc`, `EstaAtivo`).
  * [PerfilPermissao.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/PerfilPermissao.cs): Vínculo com histórico de permissões no perfil.
  * [Auditoria.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/Auditoria.cs): Registro de eventos com `AuditoriaGuid`, `Ator`, `Acao`, `Entidade`, `CorrelationId`.
* **`Interfaces/`**:
  * [IUnitOfWork.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Interfaces/IUnitOfWork.cs)
  * [IUsuarioRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Interfaces/IUsuarioRepository.cs)
  * [IPerfilRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Interfaces/IPerfilRepository.cs)
  * [IPermissaoRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Interfaces/IPermissaoRepository.cs)
  * [IAuditoriaRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Interfaces/IAuditoriaRepository.cs)

---

### 2.2. `src/Itapemirim.Ani.Application` (Services & DTOs)
* **`Common/`**:
  * [PagedResult.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Common/PagedResult.cs)
  * [PagedRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Common/PagedRequest.cs)
* **`DTOs/`**:
  * `Usuarios/`: [UsuarioCreateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioCreateRequest.cs), [UsuarioUpdateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioUpdateRequest.cs), [UsuarioResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioResponse.cs), [UsuarioFiltroRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/UsuarioFiltroRequest.cs), [AtribuirPerfilRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/AtribuirPerfilRequest.cs), [PermissaoEfetivaResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Usuarios/PermissaoEfetivaResponse.cs).
  * `Perfis/`: [PerfilCreateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilCreateRequest.cs), [PerfilUpdateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilUpdateRequest.cs), [PerfilResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilResponse.cs), [PerfilResumidoResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Perfis/PerfilResumidoResponse.cs), [AssociarPermissaoRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Perfis/AssociarPermissaoRequest.cs).
  * `Permissoes/`: [PermissaoCreateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoCreateRequest.cs), [PermissaoUpdateRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoUpdateRequest.cs), [PermissaoResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Permissoes/PermissaoResponse.cs).
  * `Auth/`: [ValidarCredenciaisRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Auth/ValidarCredenciaisRequest.cs), [UsuarioAutenticadoResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Auth/UsuarioAutenticadoResponse.cs).
  * `Auditoria/`: [AuditoriaFiltroRequest.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Auditoria/AuditoriaFiltroRequest.cs), [AuditoriaResponse.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DTOs/Auditoria/AuditoriaResponse.cs).
* **`Interfaces/`**:
  * [IPasswordHasher.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs)
  * [IUserContext.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IUserContext.cs)
  * [IUsuarioService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IUsuarioService.cs)
  * [IPerfilService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IPerfilService.cs)
  * [IPermissaoService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IPermissaoService.cs)
  * [IAuthInternoService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IAuthInternoService.cs)
  * [IAuditoriaService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IAuditoriaService.cs)
* **`Services/`**:
  * [UsuarioService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/UsuarioService.cs): Regras de cadastro, verificação de duplicidade, inativação lógica, concessão/revogação de perfis e cálculo de permissões efetivas.
  * [PerfilService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/PerfilService.cs): Manutenção de perfis e vinculação de permissões.
  * [PermissaoService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/PermissaoService.cs): Gestão do catálogo de permissões.
  * [AuthInternoService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs): Validação de credenciais para o BFF via hash BCrypt com retorno seguro dos claims.
  * [AuditoriaService.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuditoriaService.cs): Registro automático e consulta de auditoria.
* **`Validators/`** (FluentValidation):
  * [UsuarioCreateValidator.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Validators/UsuarioCreateValidator.cs)
  * [UsuarioUpdateValidator.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Validators/UsuarioUpdateValidator.cs)
  * [PerfilCreateValidator.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Validators/PerfilCreateValidator.cs)
  * [PermissaoCreateValidator.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Validators/PermissaoCreateValidator.cs)
* [DependencyInjection.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/DependencyInjection.cs): Registro de todos os validadores e serviços no contêiner do .NET.

---

### 2.3. `src/Itapemirim.Ani.Infrastructure` (Data & Segurança)
* **`Data/`**:
  * [AniDbContext.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/AniDbContext.cs): DbContext configurado no schema `ani` implementando `IUnitOfWork`.
  * `Configurations/`:
    * [UsuarioConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/UsuarioConfiguration.cs)
    * [PerfilConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PerfilConfiguration.cs)
    * [PermissaoConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PermissaoConfiguration.cs)
    * [UsuarioPerfilConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/UsuarioPerfilConfiguration.cs)
    * [PerfilPermissaoConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/PerfilPermissaoConfiguration.cs)
    * [AuditoriaConfiguration.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Configurations/AuditoriaConfiguration.cs)
  * `Repositories/`:
    * [UsuarioRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Repositories/UsuarioRepository.cs)
    * [PerfilRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Repositories/PerfilRepository.cs)
    * [PermissaoRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Repositories/PermissaoRepository.cs)
    * [AuditoriaRepository.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/Repositories/AuditoriaRepository.cs)
* **`Security/`**:
  * [BcryptPasswordHasher.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Security/BcryptPasswordHasher.cs): Hashing seguro usando BCrypt (fator de trabalho 12).
* [DependencyInjection.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/DependencyInjection.cs): Injeção do `AniDbContext`, repositórios e `IPasswordHasher`.

---

### 2.4. `src/Itapemirim.Ani.Api` (Controllers REST & Middlewares)
* **`Middlewares/`**:
  * [CorrelationIdMiddleware.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Middlewares/CorrelationIdMiddleware.cs): Propaga e injeta o cabeçalho `X-Correlation-ID`.
  * [ExceptionHandlingMiddleware.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Middlewares/ExceptionHandlingMiddleware.cs): Tratamento global devolvendo `ProblemDetails` RFC 7807 tipado para validações e exceções de negócio.
* **`Services/`**:
  * [HttpUserContext.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Services/HttpUserContext.cs): Extrai Ator, IP e CorrelationId da sessão HTTP.
* **`Controllers/`**:
  * [BaseApiController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/BaseApiController.cs)
  * [UsuariosController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/UsuariosController.cs): Mapeia `/api/v1/usuarios`.
  * [PerfisController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/PerfisController.cs): Mapeia `/api/v1/perfis`.
  * [PermissoesController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/PermissoesController.cs): Mapeia `/api/v1/permissoes`.
  * [AuditoriaController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/AuditoriaController.cs): Mapeia `/api/v1/auditoria`.
  * [AuthInternoController.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Controllers/AuthInternoController.cs): Mapeia `/api/v1/auth/validar-credenciais`.
* [Program.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Program.cs): Configuração do pipeline HTTP e Swagger.
* [appsettings.json](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json): Connection string e níveis de log.

---

## 3. Próximo Passo: Execução da Migration do Banco de Dados

Com o código compilando e as configurações mapeando 100% do schema `ani` com as colunas GUID, o próximo passo recomendado é gerar a migration inicial do EF Core apontando para a sua instância do SQL Server:

```powershell
dotnet ef migrations add InitialCreate --project src/Itapemirim.Ani.Infrastructure --startup-project src/Itapemirim.Ani.Api
```

E para aplicar no banco de dados local:

```powershell
dotnet ef database update --project src/Itapemirim.Ani.Infrastructure --startup-project src/Itapemirim.Ani.Api
```
