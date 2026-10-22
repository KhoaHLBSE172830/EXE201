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
        public static readonly Guid StudentRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid CuratorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        public static readonly Guid OrganizationRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        public static readonly Guid EducatorRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
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
                await SeedGameplayTestDataAsync();
                await SimulationScenariosSeeder.SeedSimulationScenariosAsync(_context, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn trong quá trình Seeding dữ liệu: {Message}", ex.Message);
            }
        }

        private async Task SeedRolesAsync()
        {
            var defaultRoles = new[]
            {
                new Role { Id = AdminRoleId, RoleName = "Admin", Description = "Quản trị viên toàn hệ thống", Status = RoleStatusEnum.Active },
                new Role { Id = StudentRoleId, RoleName = "Student", Description = "Học sinh / Người học kỹ năng", Status = RoleStatusEnum.Active },
                new Role { Id = CuratorRoleId, RoleName = "Curator", Description = "Giảng viên / Biên tập kịch bản bài giảng", Status = RoleStatusEnum.Active },
                new Role { Id = OrganizationRoleId, RoleName = "Organization", Description = "Đại diện tổ chức / Trường học B2B", Status = RoleStatusEnum.Active },
                new Role { Id = EducatorRoleId, RoleName = "Educator", Description = "Giáo viên / Giảng viên thuộc trường học / tổ chức", Status = RoleStatusEnum.Active }
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

        public static readonly Guid DefaultDomainId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        public static readonly Guid DefaultUnitId = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        public static readonly Guid DefaultNode1Id = Guid.Parse("c1111111-1111-1111-1111-111111111111");
        public static readonly Guid DefaultNode2Id = Guid.Parse("c2222222-2222-2222-2222-222222222222");

        public static readonly Guid Question1Id = Guid.Parse("d1111111-1111-1111-1111-111111111111");
        public static readonly Guid Question2Id = Guid.Parse("d2222222-2222-2222-2222-222222222222");
        public static readonly Guid Question3Id = Guid.Parse("d3333333-3333-3333-3333-333333333333");

        private async Task SeedGameplayTestDataAsync()
        {
            try
            {
                // 1. Seed Domain: Sơ cấp cứu ban đầu
                var existingDomain = await _context.Domains.FirstOrDefaultAsync(d => d.Id == DefaultDomainId);
                if (existingDomain == null)
                {
                    existingDomain = new Domain
                    {
                        Id = DefaultDomainId,
                        Name = "Sơ cấp cứu ban đầu (First Aid)",
                        Slug = "first-aid",
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/2966/2966327.png",
                        Order = 1,
                        Status = "Active"
                    };
                    await _context.Domains.AddAsync(existingDomain);
                    _logger.LogInformation("Seeded Domain: Sơ cấp cứu ban đầu.");
                }

                // 2. Seed Unit: Xử lý Vết thương & Bỏng
                var existingUnit = await _context.Units.FirstOrDefaultAsync(u => u.Id == DefaultUnitId);
                if (existingUnit == null)
                {
                    existingUnit = new Unit
                    {
                        Id = DefaultUnitId,
                        DomainId = DefaultDomainId,
                        Title = "Chặng 1: Xử lý Vết thương & Bỏng",
                        Description = "Học cách xử lý nhanh các tình huống bỏng nước sôi và vết thương chảy máu.",
                        Order = 1
                    };
                    await _context.Units.AddAsync(existingUnit);
                    _logger.LogInformation("Seeded Unit: Xử lý Vết thương & Bỏng.");
                }

                // 3. Seed Nodes: Node 1 & Node 2
                var existingNode1 = await _context.Nodes.FirstOrDefaultAsync(n => n.Id == DefaultNode1Id);
                if (existingNode1 == null)
                {
                    existingNode1 = new Node
                    {
                        Id = DefaultNode1Id,
                        UnitId = DefaultUnitId,
                        Title = "Bài 1.1: Bỏng nước sôi sinh hoạt & chập điện",
                        Type = "lesson",
                        Order = 1,
                        XpReward = 10,
                        Status = "Published"
                    };
                    await _context.Nodes.AddAsync(existingNode1);
                    _logger.LogInformation("Seeded Node 1: Bỏng nước sôi sinh hoạt.");
                }

                var existingNode2 = await _context.Nodes.FirstOrDefaultAsync(n => n.Id == DefaultNode2Id);
                if (existingNode2 == null)
                {
                    existingNode2 = new Node
                    {
                        Id = DefaultNode2Id,
                        UnitId = DefaultUnitId,
                        Title = "Bài 1.2: Vết thương chảy máu khẩn cấp",
                        Type = "lesson",
                        Order = 2,
                        XpReward = 10,
                        Status = "Published"
                    };
                    await _context.Nodes.AddAsync(existingNode2);
                    _logger.LogInformation("Seeded Node 2: Vết thương chảy máu.");
                }

                // 4. Seed Questions: Choice, Sequencing, Red Flag
                var existingQ1 = await _context.Questions.FirstOrDefaultAsync(q => q.Id == Question1Id);
                if (existingQ1 == null)
                {
                    existingQ1 = new Question
                    {
                        Id = Question1Id,
                        Type = "choice",
                        Title = "Phát hiện chập điện bốc khói sau tủ lạnh, việc ĐẦU TIÊN bạn làm là gì?",
                        ImageUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=800&q=80",
                        Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Tạt ngay xô nước vào phía sau tủ lạnh\"},{\"id\":\"B\",\"text\":\"Ngắt cầu dao tổng để cô lập nguồn điện\"},{\"id\":\"C\",\"text\":\"Mở toang cửa sổ gọi to nhờ hàng xóm\"}]}",
                        CorrectData = "{\"correctOptionId\":\"B\"}",
                        Explanation = "Nước dẫn điện cực mạnh gây giật tử vong ngay tại chỗ. Việc đầu tiên bắt buộc phải làm là ngắt cầu dao tổng để cô lập hoàn toàn nguồn điện.",
                        Difficulty = "Medium",
                        Source = "Cục Cảnh sát PCCC & CNCH - Bộ Công an",
                        ReviewedBy = "Đại úy Nguyễn Văn A",
                        Status = "Published"
                    };
                    await _context.Questions.AddAsync(existingQ1);
                    _logger.LogInformation("Seeded Question 1 (Choice).");
                }

                var existingQ2 = await _context.Questions.FirstOrDefaultAsync(q => q.Id == Question2Id);
                if (existingQ2 == null)
                {
                    existingQ2 = new Question
                    {
                        Id = Question2Id,
                        Type = "sequence",
                        Title = "Sắp xếp quy trình 4 bước xử lý nạn nhân bị bỏng nước sôi:",
                        ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                        Payload = "{\"items\":[{\"id\":\"step_cover\",\"text\":\"Che vết bỏng bằng gạc sạch vô trùng\"},{\"id\":\"step_remove\",\"text\":\"Đưa nạn nhân thoát khỏi nguồn nhiệt gây bỏng\"},{\"id\":\"step_hospital\",\"text\":\"Nhanh chóng đưa nạn nhân đến cơ sở y tế\"},{\"id\":\"step_water\",\"text\":\"Ngâm/xả vết bỏng dưới vòi nước mát sạch 15-20 phút\"}]}",
                        CorrectData = "{\"correctOrder\":[\"step_remove\",\"step_water\",\"step_cover\",\"step_hospital\"]}",
                        Explanation = "Quy trình sơ cứu bỏng chuẩn y tế: 1. Đưa nạn nhân khỏi nguồn nhiệt -> 2. Xả nước mát sạch hạ nhiệt -> 3. Che phủ gạc vô trùng bảo vệ -> 4. Chuyển viện cấp cứu.",
                        Difficulty = "Medium",
                        Source = "Bộ Y tế - Hướng dẫn sơ cấp cứu ban đầu",
                        ReviewedBy = "BS. Trần Thị B - BV 115",
                        Status = "Published"
                    };
                    await _context.Questions.AddAsync(existingQ2);
                    _logger.LogInformation("Seeded Question 2 (Sequence).");
                }

                var existingQ3 = await _context.Questions.FirstOrDefaultAsync(q => q.Id == Question3Id);
                if (existingQ3 == null)
                {
                    existingQ3 = new Question
                    {
                        Id = Question3Id,
                        Type = "red_flag",
                        Title = "Chạm vào điểm bất thường có dấu hiệu lừa đảo nguy hiểm trong tin nhắn SMS sau:",
                        ImageUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=800&q=80",
                        Payload = "{\"smsHeader\":\"NganHangVCB\",\"flags\":[{\"id\":\"greeting\",\"text\":\"Kính gửi quý khách,\"},{\"id\":\"alert\",\"text\":\"Tài khoản của bạn phát hiện giao dịch lạ tại Singapore.\"},{\"id\":\"phishing_link\",\"text\":\"Vui lòng truy cập vietcombannk-otp.com để hủy\"},{\"id\":\"deadline\",\"text\":\"trong vòng 5 phút trước khi bị khóa vĩnh viễn.\"}]}",
                        CorrectData = "{\"selectedFlagId\":\"phishing_link\"}",
                        Explanation = "Tên miền 'vietcombannk-otp.com' là trang web giả mạo ngân hàng (thừa chữ n) nhằm dụ người dùng đăng nhập để đánh cắp tên tài khoản và mã OTP.",
                        Difficulty = "Hard",
                        Source = "Cục An toàn thông tin - Bộ Thông tin và Truyền thông",
                        ReviewedBy = "Chuyên gia An ninh mạng Lê C",
                        Status = "Published"
                    };
                    await _context.Questions.AddAsync(existingQ3);
                    _logger.LogInformation("Seeded Question 3 (Red Flag).");
                }

                // 5. Link Questions to Node 1 via NodeQuestions
                var link1 = await _context.NodeQuestions.AnyAsync(nq => nq.NodeId == DefaultNode1Id && nq.QuestionId == Question1Id);
                if (!link1)
                {
                    await _context.NodeQuestions.AddAsync(new NodeQuestion { NodeId = DefaultNode1Id, QuestionId = Question1Id, Order = 1 });
                }

                var link2 = await _context.NodeQuestions.AnyAsync(nq => nq.NodeId == DefaultNode1Id && nq.QuestionId == Question2Id);
                if (!link2)
                {
                    await _context.NodeQuestions.AddAsync(new NodeQuestion { NodeId = DefaultNode1Id, QuestionId = Question2Id, Order = 2 });
                }

                var link3 = await _context.NodeQuestions.AnyAsync(nq => nq.NodeId == DefaultNode1Id && nq.QuestionId == Question3Id);
                if (!link3)
                {
                    await _context.NodeQuestions.AddAsync(new NodeQuestion { NodeId = DefaultNode1Id, QuestionId = Question3Id, Order = 3 });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Hoàn tất Seeding dữ liệu Gameplay mẫu độc lập cho Nhánh B.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi Seeding dữ liệu Gameplay mẫu: {Message}", ex.Message);
            }
        }
    }
}

