# Proposta e Implementação: Simplificação e Reversibilidade na Geração de Senha

## 1. Contexto e Objetivo

Em [`AuthInternoService.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs#L340-L401), a geração e a reversibilidade de senhas foram simplificadas e parametrizadas via [`appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json):

1. **[`GerarSenhaTemporariaForte`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs#L340-L348)**: Simplificado de 27 linhas (com embaralhamento manual *Fisher-Yates*) para 3 linhas usando `RandomNumberGenerator.GetString` nativo do .NET 8 e respeitando o tamanho parametrizado em `PasswordSecurity:TempPasswordLength`.
2. **Reversibilidade com Chave de Configuração (`CriptografarSenha` / `VoltarSenha` e `BcryptPasswordHasher`)**: Permite criptografar e **voltar (descriptografar)** a senha original utilizando **AES-256** (com derivação `SHA256` da `PasswordSecurity:SecretKey`) ou **XOR + Base64** (`GerarSenhaReversivelSimples` / `VoltarSenhaReversivelSimples`).

---

## 2. Configuração no `appsettings.json`

Adicionada a seção `"PasswordSecurity"` em [`src/Itapemirim.Ani.Api/appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json#L18-L23):

```json
{
  "PasswordSecurity": {
    "Mode": "ReversibleAes",
    "SecretKey": "Itapemirim_Ani_PasswordSecretKey_2026!",
    "TempPasswordLength": 12,
    "DefaultPrefix": "Ita@"
  }
}
```

| Chave | Descrição |
| :--- | :--- |
| `PasswordSecurity:Mode` | Define o modo de operação (`"ReversibleAes"` ou `"Bcrypt"`). |
| `PasswordSecurity:SecretKey` | Chave secreta usada para criptografar/gerar e **voltar (descriptografar)** a senha. |
| `PasswordSecurity:TempPasswordLength` | Tamanho padrão da senha temporária gerada (`12`). |
| `PasswordSecurity:DefaultPrefix` | Prefixo opcional para padronizar senhas temporárias (`Ita@`). |

---

## 3. Arquivos Alterados

| Arquivo | Alteração Realizada |
| :--- | :--- |
| [`src/Itapemirim.Ani.Api/appsettings.json`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Api/appsettings.json) | Inclusão do bloco `"PasswordSecurity"` com `"Mode"`, `"SecretKey"`, `"TempPasswordLength"` e `"DefaultPrefix"`. |
| [`src/Itapemirim.Ani.Application/Services/AuthInternoService.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Services/AuthInternoService.cs) | Simplificação de `GerarSenhaTemporariaForte` com `RandomNumberGenerator.GetString` e adição dos métodos `CriptografarSenha`, `VoltarSenha`, `GerarSenhaReversivelSimples` e `VoltarSenhaReversivelSimples`. |
| [`src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Application/Interfaces/IPasswordHasher.cs) | Adição do método `string? DecryptPassword(string passwordHash)`. |
| [`src/Itapemirim.Ani.Infrastructure/Security/BcryptPasswordHasher.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/src/Itapemirim.Ani.Infrastructure/Security/BcryptPasswordHasher.cs) | Suporte híbrido a `"ReversibleAes"` e `"Bcrypt"` (compatível com hashes BCrypt `$2a$`/`$2b$` existentes e senhas reversíveis AES-256). |
| [`tests/Itapemirim.Ani.UnitTests/ResetSenhaTests.cs`](file:///c:/Users/Duilio%20Farias/source/repos/portal_ItapemirimCore/tests/Itapemirim.Ani.UnitTests/ResetSenhaTests.cs) | Inclusão de testes unitários validando a geração e reversão (`VoltarSenha` e `VoltarSenhaReversivelSimples`) com a chave do `appsettings.json`. |

