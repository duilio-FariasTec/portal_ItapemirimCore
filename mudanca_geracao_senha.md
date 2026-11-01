# Proposta de Mudança: Simplificação e Reversibilidade na Geração de Senha

## 1. Contexto Atual

Atualmente, em [`AuthInternoService.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs#L340-L367), a geração e o armazenamento de senhas funcionam em duas etapas independentes:

1. **[`GerarSenhaTemporariaForte`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs#L340-L367)**: Realiza um sorteio aleatório (`RandomNumberGenerator`) de 12 caracteres e aplica o embaralhamento *Fisher-Yates*.
2. **[`BcryptPasswordHasher.HashPassword`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Security/BcryptPasswordHasher.cs#L9-L12)**: Aplica um hash **BCrypt** (unidirecional), impossibilitando reverter o valor salvo no banco de dados para descobrir a senha em texto puro.

---

## 2. Configuração no `appsettings.json`

Para parametrizar a geração de senha e permitir a **reversão (descriptografia)** de forma segura e desacoplada do código-fonte, recomenda-se adicionar a seção `"PasswordSecurity"` no arquivo [`appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json):

```json
{
  "PasswordSecurity": {
    "Mode": "ReversibleAes",
    "SecretKey": "Itapemirim_Ani_PasswordSecretKey_2026!",
    "TempPasswordLength": 8,
    "DefaultPrefix": "Ita@"
  }
}
```

| Chave | Descrição |
| :--- | :--- |
| `PasswordSecurity:Mode` | Define o modo de operação (`"ReversibleAes"`, `"ReversibleXor"`, ou `"Random"`). |
| `PasswordSecurity:SecretKey` | Chave secreta usada para criptografar/gerar e **voltar (descriptografar)** a senha. |
| `PasswordSecurity:TempPasswordLength` | Tamanho padrão da senha temporária gerada (ex.: `8`). |
| `PasswordSecurity:DefaultPrefix` | Prefixo opcional para padronizar senhas temporárias simples (ex.: `Ita@`). |

---

## 3. Opções de Implementação

### Opção 1: Criptografia Simétrica Reversível (AES com chave do `appsettings.json`)
Ideal quando é necessário **transformar uma senha em código e conseguir voltar (descriptografar) para a senha original** a qualquer momento usando a `SecretKey` configurada no [`appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json).

> Dica: Ao derivar a chave com `SHA256.HashData`, a `SecretKey` no `appsettings.json` pode ter **qualquer tamanho** sem causar erro de tamanho de bloco no AES.

```csharp
using System.Security.Cryptography;
using System.Text;

private string CriptografarSenha(string senhaPura)
{
    var secret = _configuration["PasswordSecurity:SecretKey"]
        ?? throw new InvalidOperationException("Chave 'PasswordSecurity:SecretKey' não configurada no appsettings.json.");

    using var aes = Aes.Create();
    aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(secret)); // Garante 32 bytes (AES-256)
    aes.IV = new byte[16]; // IV zerado para saída determinística e simples

    var bytes = Encoding.UTF8.GetBytes(senhaPura);
    var criptografado = aes.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
    return Convert.ToBase64String(criptografado);
}

private string VoltarSenha(string senhaCriptografada)
{
    var secret = _configuration["PasswordSecurity:SecretKey"]
        ?? throw new InvalidOperationException("Chave 'PasswordSecurity:SecretKey' não configurada no appsettings.json.");

    using var aes = Aes.Create();
    aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    aes.IV = new byte[16];

    var bytes = Convert.FromBase64String(senhaCriptografada);
    var descriptografado = aes.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length);
    return Encoding.UTF8.GetString(descriptografado);
}
```

---

### Opção 2: Geração de Senha Temporária Mais Simples (.NET 8) + Reversão Rápida (XOR + Base64)

#### 2A. Simplificando o método `GerarSenhaTemporariaForte` atual (de 27 linhas para 3 linhas)
No .NET 8, o método `RandomNumberGenerator.GetString` substitui todo o loop e o embaralhamento manual:

```csharp
private string GerarSenhaTemporariaSimples()
{
    var tamanho = int.TryParse(_configuration["PasswordSecurity:TempPasswordLength"], out var len) ? len : 8;
    const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#$!";
    return RandomNumberGenerator.GetString(caracteres, tamanho);
}
```

#### 2B. Gerador de Senha Curta Reversível (XOR com `SecretKey` + Base64Url)
Se o objetivo é gerar uma senha curta a partir de um texto base (ex.: login ou código) e conseguir **reverter para o valor original** usando a `SecretKey` do `appsettings.json`:

```csharp
private string GerarSenhaReversivelSimples(string textoOriginal)
{
    var chave = _configuration["PasswordSecurity:SecretKey"] ?? "ChavePadraoItapemirim";
    var dados = Encoding.UTF8.GetBytes(textoOriginal);
    var chaveBytes = Encoding.UTF8.GetBytes(chave);

    for (int i = 0; i < dados.Length; i++)
    {
        dados[i] ^= chaveBytes[i % chaveBytes.Length];
    }

    return Convert.ToBase64String(dados);
}

private string VoltarSenhaReversivelSimples(string senhaGerada)
{
    var chave = _configuration["PasswordSecurity:SecretKey"] ?? "ChavePadraoItapemirim";
    var dados = Convert.FromBase64String(senhaGerada);
    var chaveBytes = Encoding.UTF8.GetBytes(chave);

    for (int i = 0; i < dados.Length; i++)
    {
        dados[i] ^= chaveBytes[i % chaveBytes.Length];
    }

    return Encoding.UTF8.GetString(dados);
}
```

---

### Opção 3: Voltar para a Senha Anterior do Usuário (Rollback de Reset)
Caso a necessidade de **"voltar a senha"** seja permitir que o usuário ou administrador **desfaça o reset e restaure a senha anterior**:

1. Adicionar a propriedade `SenhaHashAnterior` na entidade [`Usuario.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Domain/Entities/Usuario.cs):
```csharp
public string? SenhaHashAnterior { get; private set; }

public void AtualizarSenha(string novoHash, bool exigeTrocaNoProximoLogin = false)
{
    SenhaHashAnterior = SenhaHash; // Guarda a senha anterior antes de sobrescrever
    SenhaHash = novoHash;
    ExigeTrocaSenhaNoProximoLogin = exigeTrocaNoProximoLogin;
    DataAtualizacao = DateTime.UtcNow;
}

public void RestaurarSenhaAnterior()
{
    if (string.IsNullOrWhiteSpace(SenhaHashAnterior))
        throw new RegraNegocioException("Não há senha anterior registrada para restauração.");

    SenhaHash = SenhaHashAnterior;
    SenhaHashAnterior = null;
    ExigeTrocaSenhaNoProximoLogin = false;
    DataAtualizacao = DateTime.UtcNow;
}
```

---

## 4. Resumo de Impacto nos Arquivos

| Arquivo | Alteração Sugerida |
| :--- | :--- |
| [`src/Itapemirim.Ani.Api/appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json) | Inclusão do bloco `"PasswordSecurity"` com `"SecretKey"` e `"TempPasswordLength"`. |
| [`src/Itapemirim.Ani.Application/Services/AuthInternoService.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs) | Substituição de `GerarSenhaTemporariaForte` pelo método simplificado e inclusão dos métodos `CriptografarSenha` / `VoltarSenha` consumindo `_configuration["PasswordSecurity:SecretKey"]`. |
| [`src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs) | *(Opcional)* Inclusão do método `string DecryptPassword(string encryptedPassword)` caso o `IPasswordHasher` passe a operar com criptografia reversível. |

