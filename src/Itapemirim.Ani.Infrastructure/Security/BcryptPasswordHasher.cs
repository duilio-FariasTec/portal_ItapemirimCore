namespace Itapemirim.Ani.Infrastructure.Security;

using System.Security.Cryptography;
using System.Text;
using Itapemirim.Ani.Application.Interfaces;
using Microsoft.Extensions.Configuration;

public class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    private const string DefaultSecretKey = "Itapemirim_Ani_PasswordSecretKey_2026!";
    private readonly IConfiguration? _configuration;

    public BcryptPasswordHasher()
    {
    }

    public BcryptPasswordHasher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string password)
    {
        var mode = _configuration?["PasswordSecurity:Mode"] ?? "ReversibleAes";
        if (string.Equals(mode, "ReversibleAes", StringComparison.OrdinalIgnoreCase))
        {
            return EncryptAes(password);
        }

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            if (passwordHash.StartsWith("$2", StringComparison.Ordinal))
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }

            var decrypted = DecryptPassword(passwordHash);
            if (decrypted != null)
            {
                return string.Equals(password, decrypted, StringComparison.Ordinal);
            }

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    public string? DecryptPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.StartsWith("$2", StringComparison.Ordinal))
            return null;

        try
        {
            var secret = _configuration?["PasswordSecurity:SecretKey"] ?? DefaultSecretKey;
            using var aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            aes.IV = new byte[16];

            var bytes = Convert.FromBase64String(passwordHash);
            var descriptografado = aes.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(descriptografado);
        }
        catch
        {
            return null;
        }
    }

    private string EncryptAes(string plainText)
    {
        var secret = _configuration?["PasswordSecurity:SecretKey"] ?? DefaultSecretKey;
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        aes.IV = new byte[16];

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var criptografado = aes.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
        return Convert.ToBase64String(criptografado);
    }
}