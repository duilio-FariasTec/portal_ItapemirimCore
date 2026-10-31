# Guia de Implementação e Execução Local com SQLite — ANI Core (.NET)

Este guia documenta a configuração do **SQLite** para execução e testes locais da API sem a necessidade de instalar ou rodar uma instância do Microsoft SQL Server.

---

## 1. O que foi Implementado

O projeto agora possui **suporte híbrido**:
* **SQLite (Local / Dev)**: Criação automática do arquivo local `ani.db` no primeiro `dotnet run`, sem necessidade de passos manuais.
* **SQL Server (Homologação / PRD)**: Totalmente preservado conforme o script [ani-schema.sql](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/.sql/ani-schema.sql) com schema `ani`.

### 1.1. Pacote NuGet Instalado
No projeto [Itapemirim.Ani.Infrastructure.csproj](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Itapemirim.Ani.Infrastructure.csproj):
```powershell
dotnet add src/Itapemirim.Ani.Infrastructure/Itapemirim.Ani.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
```

### 1.2. Configuração Dinâmica do Banco de Dados
No arquivo [DependencyInjection.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/DependencyInjection.cs):
```csharp
var provider = configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = configuration.GetConnectionString("DefaultConnection");

services.AddDbContext<AniDbContext>(options =>
{
    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConn = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : "Data Source=ani.db";
        options.UseSqlite(sqliteConn);
    }
    else
    {
        var sqlServerConn = !string.IsNullOrWhiteSpace(connectionString) 
            ? connectionString 
            : "Server=localhost;Database=portal_Itapemirim;Trusted_Connection=True;TrustServerCertificate=True;";
        
        options.UseSqlServer(sqlServerConn, b =>
        {
            b.MigrationsHistoryTable("__EFMigrationsHistory", "ani");
        });
    }
});
```

### 1.3. Compatibilidade Automática no `AniDbContext.cs`
O SQLite não suporta schemas (ex: `ani.Usuario`), funções específicas do T-SQL (ex: `NEWSEQUENTIALID()`, `SYSUTCDATETIME()`) nem literais com prefixo `N'...'` em check constraints.

No [AniDbContext.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Data/AniDbContext.cs), adicionamos uma normalização automática que roda apenas quando o provedor ativo for SQLite:

```csharp
if (Database.IsSqlite())
{
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
        entity.SetSchema(null); // Remove schema 'ani'

        // Ajusta Check Constraints para sintaxe SQLite (remove prefixo N'...')
        var checkConstraints = entity.GetCheckConstraints().ToList();
        foreach (var check in checkConstraints)
        {
            var sql = check.Sql;
            entity.RemoveCheckConstraint(check.Name);
            if (!string.IsNullOrWhiteSpace(sql))
            {
                var sqliteSql = sql.Replace("N'", "'").Replace("[", "\"").Replace("]", "\"");
                entity.AddCheckConstraint(check.Name, sqliteSql);
            }
        }

        // Ajusta índices filtrados parciais
        foreach (var index in entity.GetIndexes())
        {
            var filter = index.GetFilter();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                index.SetFilter(filter.Replace("[", "\"").Replace("]", "\""));
            }
        }

        // Limpa defaults do SQL Server
        foreach (var property in entity.GetProperties())
        {
            if (property.GetDefaultValueSql()?.Contains("NEWSEQUENTIALID", StringComparison.OrdinalIgnoreCase) == true ||
                property.GetDefaultValueSql()?.Contains("SYSUTCDATETIME", StringComparison.OrdinalIgnoreCase) == true)
            {
                property.SetDefaultValueSql(null);
            }

            if (property.GetColumnType()?.Contains("datetime2", StringComparison.OrdinalIgnoreCase) == true)
            {
                property.SetColumnType(null);
            }
        }
    }
}
```

### 1.4. Criação Automática do Banco no `Program.cs`
No [Program.cs](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/Program.cs):
```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ANI Core v1");
        c.RoutePrefix = string.Empty; // Swagger abre direto na raiz http://localhost:5146/
    });

    // Cria automaticamente o ani.db e todas as tabelas e índices se ainda não existirem
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AniDbContext>();
    db.Database.EnsureCreated();
}
```

### 1.5. `appsettings.json`
No [appsettings.json](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json):
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ani.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 2. Como Rodar a API Localmente

### Passo 1: Iniciar o Servidor Web
Na raiz do repositório, execute:

```powershell
dotnet run --project src/Itapemirim.Ani.Api
```

Ou entre na pasta da API e rode:
```powershell
cd src/Itapemirim.Ani.Api
dotnet run
```

O console exibirá:
```plaintext
Now listening on: http://localhost:5146
Application started. Press Ctrl+C to shut down.
```

### Passo 2: Acessar a Documentação Interativa (Swagger)
Abra o navegador no endereço:
👉 **[http://localhost:5146](http://localhost:5146)**

---

## 3. Testando os Endpoints via PowerShell ou cURL

### 3.1. Cadastrar um Usuário (`POST /api/v1/usuarios`)
```powershell
$body = @{
    nome  = "Administrador do Sistema"
    login = "admin.itapemirim"
    email = "admin@itapemirim.com.br"
    senha = "SenhaForte@2026"
    status = "Ativo"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5146/api/v1/usuarios" -Method Post -Body $body -ContentType "application/json"
```

Retorno com o GUID público seguro:
```json
{
  "id": "e4b5fc91-031a-4d2a-89a1-cb9e47192a54",
  "nome": "Administrador do Sistema",
  "login": "admin.itapemirim",
  "email": "admin@itapemirim.com.br",
  "status": "Ativo",
  "criadoEmUtc": "2026-10-31T19:48:00Z",
  "atualizadoEmUtc": null,
  "perfisAtivos": []
}
```

### 3.2. Listar Usuários com Paginação (`GET /api/v1/usuarios`)
```powershell
Invoke-RestMethod -Uri "http://localhost:5146/api/v1/usuarios" -Method Get
```

### 3.3. Testar Login Interno do BFF (`POST /api/v1/auth/validar-credenciais`)
```powershell
$login = @{
    login = "admin.itapemirim"
    senha = "SenhaForte@2026"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5146/api/v1/auth/validar-credenciais" -Method Post -Body $login -ContentType "application/json"
```

### 3.4. Consultar Auditoria de Ações (`GET /api/v1/auditoria`)
```powershell
Invoke-RestMethod -Uri "http://localhost:5146/api/v1/auditoria" -Method Get
```

---

## 4. Como Alternar entre SQLite e SQL Server

Quando você for subir para o ambiente corporativo com SQL Server ou rodar com banco SQL Server local, basta alterar uma única linha no [appsettings.json](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json):

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=portal_Itapemirim;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
Nenhum código C# precisa ser alterado.
