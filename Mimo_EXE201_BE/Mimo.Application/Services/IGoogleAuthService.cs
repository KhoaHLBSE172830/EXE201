using System.Threading.Tasks;

namespace Mimo.Application.Services
{
    public class GoogleUserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public string Subject { get; set; } = string.Empty;
    }

    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> ValidateTokenAsync(string idToken);
    }
}
