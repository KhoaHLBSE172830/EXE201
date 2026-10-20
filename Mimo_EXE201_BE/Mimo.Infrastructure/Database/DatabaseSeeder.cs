using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Entities;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Infrastructure.Database
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly MimoDbContext _context;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<DatabaseSeeder> _logger;

        public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string DefaultAdminUsername = "mimo.Admin";
        public const string DefaultAdminPassword = "Admin123!";
        public const string DefaultAdminEmail = "admin@mimo.app";

        public DatabaseSeeder(
            MimoDbContext context,
            ICryptoService cryptoService,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn trong quá trình Seeding dữ liệu: {Message}", ex.Message);
            }
        }

        private async Task SeedRolesAsync()
        {
            var studentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var curatorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var orgRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var defaultRoles = new[]
            {
                new Role { Id = AdminRoleId, RoleName = "Admin", Description = "Quản trị viên toàn hệ thống", Status = RoleStatusEnum.Active },
                new Role { Id = studentRoleId, RoleName = "Student", Description = "Học sinh / Người học kỹ năng", Status = RoleStatusEnum.Active },
                new Role { Id = curatorRoleId, RoleName = "Curator", Description = "Giảng viên / Biên tập kịch bản bài giảng", Status = RoleStatusEnum.Active },
                new Role { Id = orgRoleId, RoleName = "Organization", Description = "Đại diện tổ chức / Trường học B2B", Status = RoleStatusEnum.Active }
            };

            foreach (var role in defaultRoles)
            {
                var exists = await _context.Roles.AnyAsync(r => r.Id == role.Id || r.RoleName == role.RoleName);
                if (!exists)
                {
                    await _context.Roles.AddAsync(role);
                    _logger.LogInformation("Seeded Role thiếu: {RoleName}", role.RoleName);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            // Kiểm tra 1: Kiểm tra xem đã có bất kỳ User nào mang Role Admin chưa
            var existingAdminByRole = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RoleId == AdminRoleId);

            if (existingAdminByRole != null)
            {
                await EnsureUserProfileAndStatsAsync(existingAdminByRole);
                _logger.LogInformation("Tài khoản Admin duy nhất đã tồn tại (UserName: {UserName}, Email: {Email}). Đã xác thực UserProfile và UserStats.",
                    existingAdminByRole.UserName, existingAdminByRole.Email);
                return;
            }

            // Kiểm tra 2: Kiểm tra UserName 'mimo.Admin' đã bị chiếm dụng chưa (không phân biệt hoa thường)
            var existingAdminByUsername = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName != null && u.UserName.ToLower() == DefaultAdminUsername.ToLower());

            if (existingAdminByUsername != null)
            {
                _logger.LogWarning("Đã tồn tại tài khoản với UserName '{UserName}', kiểm tra dữ liệu tài khoản này.", DefaultAdminUsername);
                return;
            }

            // Kiểm tra 3: Kiểm tra Email 'admin@mimo.app' đã bị chiếm dụng chưa
            var existingAdminByEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == DefaultAdminEmail.ToLower());

            if (existingAdminByEmail != null)
            {
                _logger.LogWarning("Email '{Email}' đã được sử dụng. Không thể tạo tài khoản admin mặc định với email này.", DefaultAdminEmail);
                return;
            }

            // Đảm bảo Role Admin tồn tại trong Database
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == AdminRoleId);
            if (adminRole == null)
            {
                _logger.LogError("Không tìm thấy Role 'Admin' trong database. Hủy bỏ seeding admin.");
                return;
            }

            // Tạo tài khoản Admin duy nhất
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                RoleId = AdminRoleId,
                Role = adminRole,
                Name = "Mimo System Admin",
                UserName = DefaultAdminUsername,
                Email = DefaultAdminEmail,
                Password = _cryptoService.HashPassword(DefaultAdminPassword),
                Phone = "0900000000",
                Address = "MIMO System Headquarters",
                Status = UserStatusEnum.Active,
                AuthType = "Local",
                Timezone = "Asia/Ho_Chi_Minh",
                TotalXp = 0,
                RefreshToken = _cryptoService.GenerateRandomToken(32),
                RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                await EnsureUserProfileAndStatsAsync(adminUser);

                _logger.LogInformation("======================================================================");
                _logger.LogInformation(">>> SEEDING THÀNH CÔNG TÀI KHOẢN ADMIN DUY NHẤT CHO HỆ THỐNG MIMO <<<");
                _logger.LogInformation(">>> UserName : {UserName}", DefaultAdminUsername);
                _logger.LogInformation(">>> Email    : {Email}", DefaultAdminEmail);
                _logger.LogInformation(">>> Password : {Password}", DefaultAdminPassword);
                _logger.LogInformation("======================================================================");
            }
            catch (DbUpdateException ex)
            {
                // Xử lý race-condition nếu nhiều instance/pods cùng khởi động và seeding cùng 1 lúc
                _logger.LogWarning(ex, "Tài khoản Admin đã được tạo bởi một instance khác đang chạy đồng thời: {Message}", ex.Message);
            }
        }

        private async Task EnsureUserProfileAndStatsAsync(User user)
        {
            try
            {
                var hasProfile = await _context.UserProfiles.AnyAsync(p => p.UserId == user.Id);
                if (!hasProfile)
                {
                    await _context.UserProfiles.AddAsync(new UserProfile
                    {
                        UserId = user.Id,
                        FullName = user.Name,
                        AvatarUrl = user.Avatar,
                        DailyGoalMinutes = 15
                    });
                }

                var hasStats = await _context.UserStats.AnyAsync(s => s.UserId == user.Id);
                if (!hasStats)
                {
                    await _context.UserStats.AddAsync(new UserStats
                    {
                        UserId = user.Id,
                        Hearts = 5,
                        HeartsUpdatedAt = DateTime.UtcNow,
                        Streak = 0,
                        StreakFreezeCount = 99,
                        IsPro = true
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi đảm bảo Profile/Stats cho Admin: {Message}", ex.Message);
            }
        }
    }
}

