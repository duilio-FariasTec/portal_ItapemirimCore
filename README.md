# Portal Itapemirim — ANI Core API

API REST em **.NET 10** construída segundo os princípios de **Clean Architecture**, responsável pelo núcleo de governança, autenticação, catálogo de permissões por tela/rota (`DscUrl`), perfis e auditoria do ecossistema **Portal Itapemirim**.

---

## 📋 Pré-requisitos

Antes de iniciar, certifique-se de ter instalado no seu ambiente de desenvolvimento:

1. **.NET 10 SDK** (ou versão mais recente):
   * **Windows / Geral**: [Download Oficial .NET SDK (Microsoft)](https://dotnet.microsoft.com/download)
   * **macOS (via Homebrew)**:
     ```bash
     brew install --cask dotnet-sdk
     ```
     *(Ou baixe o instalador `.pkg` para processadores **Apple Silicon (Arm64 - M1/M2/M3/M4)** ou **Intel (x64)** no site da Microsoft)*.
   * Para validar a instalação em qualquer sistema:
     ```bash
     dotnet --version
     # Deve exibir 10.0.x
     ```

2. **Git**:
   * **Windows**: [Download Oficial do Git](https://git-scm.com/downloads)
   * **macOS**: Já incluso com o Xcode Command Line Tools (`xcode-select --install`) ou via Homebrew (`brew install git`).
   * Para validar:
     ```bash
     git --version
     ```

3. **Dependências para o Seed no macOS / Linux (Opcional)**:
   * Para rodar o script de seed automatizado [seed_database_api.sh](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/scripts/seed_database_api.sh):
     ```bash
     brew install jq curl
     ```

4. **Editor / IDE Recomendada**:
   * [Visual Studio Code](https://code.visualstudio.com/) (com a extensão *C# Dev Kit*) — excelente para macOS e Windows
   * [JetBrains Rider](https://www.jetbrains.com/rider/) (multiplataforma)
   * [Visual Studio 2022 / 2026](https://visualstudio.microsoft.com/) (apenas Windows)

> [!TIP]
> **Banco de Dados Local Zero-Config**:
> Por padrão, a aplicação vem configurada para rodar localmente com **SQLite**, criando e atualizando as tabelas automaticamente na inicialização (`EnsureCreated`). Não é necessário instalar ou configurar o SQL Server para desenvolver e testar localmente!

---

## 🚀 Como Rodar o Projeto do Zero (Passo a Passo)

### 1. Clonar o Repositório do GitHub

Abra o terminal (PowerShell, Command Prompt ou Bash) no diretório onde deseja salvar o projeto e execute:

```bash
git clone https://github.com/duilio-FariasTec/portal_ItapemirimCore.git
cd portal_ItapemirimCore
```

---

### 2. Restaurar Dependências e Compilar a Solução

Na raiz do projeto (`portal_ItapemirimCore`), execute:

```bash
dotnet build Itapemirim.Ani.sln
```

*(Esse comando restaura automaticamente todos os pacotes NuGet e compila todos os 5 projetos da solução garantindo 0 Erros).*

---

### 3. Rodar os Testes Automatizados (Opcional)

Para validar a integridade dos domínios e regras de negócio:

```bash
dotnet test Itapemirim.Ani.sln
```

---

### 4. Executar a API

#### No Windows (PowerShell / Prompt):
```powershell
cd .\src\Itapemirim.Ani.Api
dotnet run
```
*(Ou diretamente da raiz)*:
```powershell
dotnet run --project .\src\Itapemirim.Ani.Api\Itapemirim.Ani.Api.csproj
```

#### No macOS / Linux (Terminal / Zsh / Bash):
```bash
cd src/Itapemirim.Ani.Api
dotnet run
```
*(Ou diretamente da raiz)*:
```bash
dotnet run --project src/Itapemirim.Ani.Api/Itapemirim.Ani.Api.csproj
```

> [!NOTE]
> **Certificados HTTPS no macOS**:
> Se for a primeira vez rodando uma aplicação ASP.NET Core no Mac, execute o comando abaixo para confiar no certificado de desenvolvimento SSL local:
> ```bash
> dotnet dev-certs https --trust
> ```

---

### 5. Acessar a Documentação Interativa (Swagger)

Com a API em execução, abra seu navegador no endereço:

* **Swagger UI**: [http://localhost:5146](http://localhost:5146) (ou [https://localhost:7146](https://localhost:7146))
* **Swagger JSON**: `http://localhost:5146/swagger/v1/swagger.json`

---

## ⚡ Carga Inicial de Dados (Seed Automático via API)

Com a API rodando, você pode popular todo o banco de dados inicial (permissões, perfis, usuários e vínculos) executando os scripts de automação inclusos no repositório em outro terminal:

### No Windows (PowerShell):
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\seed_database_api.ps1
```

### No Linux / Mac / Git Bash:
```bash
chmod +x ./scripts/seed_database_api.sh
./scripts/seed_database_api.sh
```

### 🔑 Credenciais Criadas no Seed:

| Usuário | Login | Senha | Perfil | Permissões Associadas |
| :--- | :--- | :--- | :--- | :--- |
| **Administrador** | `admin` | `12345678` | Administrador | `home.view`, `precos.view`, `usuarios.manage` |
| **Analista Financeiro** | `financeiro` | `12345678` | Financeiro | `home.view`, `precos.view` |
| **Admin Secundário** | `admin2` | `abcdefgh` | Administrador | `home.view`, `precos.view`, `usuarios.manage` |
| **Financeiro Secundário** | `financeiro2` | `abcdefgh` | Financeiro | `home.view`, `precos.view` |

O script também testa a rota `POST /api/auth/login`, gerando o **Token JWT** e validando suas claims (`sub`, `login`, `role` e `permission`).

---

## 🏗️ Estrutura da Solução (Clean Architecture)

```text
portal_ItapemirimCore/
├── .doc/                               # Especificações de arquitetura e endpoints
├── .sql/                               # Script oficial SQL Server (ani-schema.sql)
├── scripts/                            # Scripts de automação, seed e testes
│   ├── seed_database_api.ps1           # Automação de seed em PowerShell
│   └── seed_database_api.sh            # Automação de seed em Bash
├── src/
│   ├── Itapemirim.Ani.Domain/          # Entidades (Usuario, Perfil, Permissao), Enums e Contratos
│   ├── Itapemirim.Ani.Application/     # Services, DTOs, FluentValidation e Auth/JWT
│   ├── Itapemirim.Ani.Infrastructure/  # EF Core DbContext, Repositórios e Persistência
│   └── Itapemirim.Ani.Api/             # Web API REST, Controllers, Middlewares e Swagger
└── tests/
    └── Itapemirim.Ani.UnitTests/       # Testes unitários com xUnit
```

---

## 🗄️ Alternando entre SQLite e SQL Server

A infraestrutura possui suporte a provedores múltiplos configurável pelo arquivo `src/Itapemirim.Ani.Api/appsettings.json`:

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ani.db"
  }
}
```

* **Para SQLite (Local)**: Deixe `"DatabaseProvider": "Sqlite"`. O arquivo `ani.db` será criado e gerenciado automaticamente.
* **Para SQL Server**: Altere para `"DatabaseProvider": "SqlServer"` e aponte a string de conexão para a sua instância SQL Server onde o script [.sql/ani-schema.sql](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/.sql/ani-schema.sql) foi executado.

---

## 🔒 Segurança e Anti-Fraude (IDOR Protection)

* **GUIDs Públicos**: Todas as rotas de API expõem exclusivamente identificadores em formato **GUID / UUID** (`UsuarioGuid`, `PerfilGuid`, `PermissaoGuid`), impedindo ataques de enumeração sequencial ou força bruta.
* **Hash de Senhas**: Senhas são criptografadas com **BCrypt** com salt individual.
* **Auditoria de Operações**: Todas as ações críticas (inclusão, alteração, remoção e tentativas de login) são auditadas em banco de dados.

---

## 🍎 Guia Rápido para macOS (Terminal / Zsh)

Se você estiver em um Mac (Apple Silicon M1/M2/M3/M4 ou Intel), você pode rodar todo o fluxo pelo terminal com a sequência abaixo:

```bash
# 1. Instalar o .NET 10 SDK e utilitários (caso ainda não tenha via Homebrew)
brew install --cask dotnet-sdk
brew install git jq curl

# 2. Confiar no certificado HTTPS de desenvolvimento local do Mac
dotnet dev-certs https --trust

# 3. Clonar o repositório
git clone https://github.com/duilio-FariasTec/portal_ItapemirimCore.git
cd portal_ItapemirimCore

# 4. Compilar a solução
dotnet build Itapemirim.Ani.sln

# 5. Executar os testes unitários
dotnet test Itapemirim.Ani.sln

# 6. Rodar a API
cd src/Itapemirim.Ani.Api
dotnet run
```

Em uma **segunda aba do terminal** (mantendo a API rodando na primeira), execute a carga inicial de dados:
```bash
cd portal_ItapemirimCore
chmod +x ./scripts/seed_database_api.sh
./scripts/seed_database_api.sh
```
