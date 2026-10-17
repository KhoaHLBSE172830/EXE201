using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Auth;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IJwtService _jwtService;
        private readonly ICryptoService _cryptoService;

        public AuthService(
            IGenericRepository<User> userRepository,
            IGenericRepository<Organization> orgRepository,
            IJwtService jwtService,
            ICryptoService cryptoService)
        {
            _userRepository = userRepository;
            _orgRepository = orgRepository;
            _jwtService = jwtService;
            _cryptoService = cryptoService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1. Kiểm tra Email đã tồn tại chưa
            var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (existingUser != null)
            {
                throw new ConflictException("Email này đã được sử dụng trong hệ thống.");
            }

            Guid? organizationId = null;

            // 2. Xử lý logic theo Role
            if (request.Role == UserRoleEnum.Organization)
            {
                // Đăng ký cho đại diện Organization mới
                var orgName = string.IsNullOrWhiteSpace(request.OrganizationName) ? $"{request.FullName}'s Org" : request.OrganizationName.Trim();
                var orgCode = "ORG_" + _cryptoService.GenerateRandomToken(6).ToUpper();

                var newOrg = new Organization
                {
                    Name = orgName,
                    Code = orgCode,
                    Email = normalizedEmail,
                    PhoneNumber = request.PhoneNumber,
                    Status = OrganizationStatusEnum.Active,
                    SeatCount = 50,
                    ExpirationDate = DateTime.UtcNow.AddYears(1)
                };

                var createdOrg = await _orgRepository.CreateAsync(newOrg);
                organizationId = createdOrg.Id;
            }
            else if (!string.IsNullOrWhiteSpace(request.OrganizationCode))
            {
                // Học sinh hoặc giảng viên đăng ký kèm mã trường/lớp
                var code = request.OrganizationCode.Trim().ToUpper();
                var existingOrg = await _orgRepository.FirstOrDefaultAsync(o => o.Code.ToUpper() == code);
                if (existingOrg == null)
                {
                    throw new BadRequestException("Mã tổ chức/trường học không tồn tại.");
                }
                organizationId = existingOrg.Id;
            }

            // 3. Băm mật khẩu và tạo refresh token
            var passwordHash = _cryptoService.HashPassword(request.Password);
            var refreshToken = _cryptoService.GenerateRandomToken(32);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = request.Role,
                Status = UserStatusEnum.Active,
                PhoneNumber = request.PhoneNumber,
                OrganizationId = organizationId,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP)
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // 4. Sinh JWT Access Token
            var sessionId = Guid.NewGuid();
            var accessToken = _jwtService.GenerateToken(
                createdUser.Id,
                createdUser.Role.ToString(),
                sessionId,
                createdUser.Email,
                createdUser.Status,
                JwtConst.ACCESS_TOKEN_EXP
            );

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = JwtConst.ACCESS_TOKEN_EXP,
                User = MapToUserProfile(createdUser)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1. Tìm user theo Email
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, nameof(User.Organization));
            if (user == null)
            {
                throw new BadRequestException("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 2. Xác thực mật khẩu
            if (!_cryptoService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new BadRequestException("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 3. Kiểm tra trạng thái tài khoản
            if (user.Status != UserStatusEnum.Active)
            {
                throw new BadRequestException("Tài khoản đang bị khóa hoặc chưa được kích hoạt.");
            }

            // 4. Cập nhật Refresh Token mới
            var refreshToken = _cryptoService.GenerateRandomToken(32);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP);
            await _userRepository.UpdateAsync(user);

            // 5. Sinh JWT Token
            var sessionId = Guid.NewGuid();
            var accessToken = _jwtService.GenerateToken(
                user.Id,
                user.Role.ToString(),
                sessionId,
                user.Email,
                user.Status,
                JwtConst.ACCESS_TOKEN_EXP
            );

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = JwtConst.ACCESS_TOKEN_EXP,
                User = MapToUserProfile(user)
            };
        }

        public async Task<UserProfileDto> GetCurrentUserAsync(Guid userId)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId, nameof(User.Organization));
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            return MapToUserProfile(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new BadRequestException("Refresh token không hợp lệ.");
            }

            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.RefreshToken == request.RefreshToken,
                nameof(User.Organization));

            if (user == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.");
            }

            // Xoay vòng Refresh Token (Token Rotation)
            var newRefreshToken = _cryptoService.GenerateRandomToken(32);
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP);
            await _userRepository.UpdateAsync(user);

            var sessionId = Guid.NewGuid();
            var newAccessToken = _jwtService.GenerateToken(
                user.Id,
                user.Role.ToString(),
                sessionId,
                user.Email,
                user.Status,
                JwtConst.ACCESS_TOKEN_EXP
            );

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                ExpiresIn = JwtConst.ACCESS_TOKEN_EXP,
                User = MapToUserProfile(user)
            };
        }

        private static UserProfileDto MapToUserProfile(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
