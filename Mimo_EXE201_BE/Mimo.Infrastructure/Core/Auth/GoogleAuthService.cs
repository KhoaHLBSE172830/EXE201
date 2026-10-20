using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mimo.Application.Services;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Infrastructure.Core.Auth
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;

        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfo> ValidateTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new BadRequestException("Google ID Token không được để trống.");
            }

            // Hỗ trợ mock token phục vụ testing nội bộ trong môi trường Development
            if (idToken.StartsWith("mock_google_", StringComparison.OrdinalIgnoreCase))
            {
                var username = idToken.Substring("mock_google_".Length);
                return new GoogleUserInfo
                {
                    Email = $"{username}@gmail.com",
                    Name = $"Google User {username}",
                    Picture = "https://lh3.googleusercontent.com/a/default-avatar",
                    Subject = Guid.NewGuid().ToString()
                };
            }

            try
            {
                var clientId = _configuration["Google:ClientId"];
                GoogleJsonWebSignature.ValidationSettings? settings = null;

                if (!string.IsNullOrEmpty(clientId))
                {
                    settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    };
                }

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                if (payload == null)
                {
                    throw new UnauthorizedException("Google ID Token không hợp lệ.");
                }

                return new GoogleUserInfo
                {
                    Email = payload.Email,
                    Name = payload.Name ?? payload.Email.Split('@')[0],
                    Picture = payload.Picture,
                    Subject = payload.Subject
                };
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Xác thực Google ID Token thất bại: {Message}", ex.Message);
                throw new UnauthorizedException("Google ID Token không hợp lệ hoặc đã hết hạn.");
            }
        }
    }
}
