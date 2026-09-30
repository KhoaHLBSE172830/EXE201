using System.Security.Cryptography;
using Mimo.Application.Services;

namespace Mimo.Infrastructure.Core.Crypto;

public class CryptoService : ICryptoService
{
    private const int WorkFactor = 10;

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateRandomToken(int length)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var data = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[data[i] % validChars.Length];
        }

        return new string(chars);
    }
}
