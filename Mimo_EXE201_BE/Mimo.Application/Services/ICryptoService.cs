namespace Mimo.Application.Services;

public interface ICryptoService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateRandomToken(int length);
}
