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
        private readonly ILeaderboardService _leaderboardService;
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
            ILeaderboardService leaderboardService,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _cryptoService = cryptoService;
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedGameplayDataAsync();
                await SeedGamificationDataAsync();
                await SeedB2BClassroomsAndAssignmentsAsync();
                await _leaderboardService.SeedMockLeaderboardUsersAsync();
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

        private async Task SeedGameplayDataAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu đồng bộ danh mục kịch bản đào tạo kỹ năng sinh tồn (24 tình huống chuẩn hóa)...");

                // 0. Dọn dẹp dữ liệu kiểm thử legacy nếu tồn tại (ID d1111111... đến d6666666...)
                var legacyQuestionIds = new[]
                {
                    Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                    Guid.Parse("d2222222-2222-2222-2222-222222222222"),
                    Guid.Parse("d3333333-3333-3333-3333-333333333333"),
                    Guid.Parse("d4444444-4444-4444-4444-444444444444"),
                    Guid.Parse("d5555555-5555-5555-5555-555555555555"),
                    Guid.Parse("d6666666-6666-6666-6666-666666666666")
                };

                var legacyNodeQuestions = await _context.NodeQuestions
                    .Where(nq => legacyQuestionIds.Contains(nq.QuestionId))
                    .ToListAsync();
                if (legacyNodeQuestions.Any())
                {
                    _context.NodeQuestions.RemoveRange(legacyNodeQuestions);
                    await _context.SaveChangesAsync();
                }

                var legacyQuestions = await _context.Questions
                    .Where(q => legacyQuestionIds.Contains(q.Id))
                    .ToListAsync();
                if (legacyQuestions.Any())
                {
                    _context.Questions.RemoveRange(legacyQuestions);
                    await _context.SaveChangesAsync();
                }

                // 1. Đồng bộ 4 Lĩnh vực (Domains)
                var domains = GameplaySeedData.GetDomains();
                foreach (var domain in domains)
                {
                    var existing = await _context.Domains.FirstOrDefaultAsync(d => d.Id == domain.Id);
                    if (existing == null)
                    {
                        await _context.Domains.AddAsync(domain);
                        _logger.LogInformation("Seeded Domain: {Name}", domain.Name);
                    }
                    else
                    {
                        existing.Name = domain.Name;
                        existing.Slug = domain.Slug;
                        existing.IconUrl = domain.IconUrl;
                        existing.Order = domain.Order;
                        existing.Status = domain.Status;
                    }
                }
                await _context.SaveChangesAsync();

                // 2. Đồng bộ 4 Chặng học (Units)
                var units = GameplaySeedData.GetUnits();
                foreach (var unit in units)
                {
                    var existing = await _context.Units.FirstOrDefaultAsync(u => u.Id == unit.Id);
                    if (existing == null)
                    {
                        await _context.Units.AddAsync(unit);
                        _logger.LogInformation("Seeded Unit: {Title}", unit.Title);
                    }
                    else
                    {
                        existing.DomainId = unit.DomainId;
                        existing.Title = unit.Title;
                        existing.Description = unit.Description;
                        existing.Order = unit.Order;
                    }
                }
                await _context.SaveChangesAsync();

                // 3. Đồng bộ 8 Bài học (Nodes)
                var nodes = GameplaySeedData.GetNodes();
                foreach (var node in nodes)
                {
                    var existing = await _context.Nodes.FirstOrDefaultAsync(n => n.Id == node.Id);
                    if (existing == null)
                    {
                        await _context.Nodes.AddAsync(node);
                        _logger.LogInformation("Seeded Node: {Title}", node.Title);
                    }
                    else
                    {
                        existing.UnitId = node.UnitId;
                        existing.Title = node.Title;
                        existing.Type = node.Type;
                        existing.Order = node.Order;
                        existing.XpReward = node.XpReward;
                        existing.Status = node.Status;
                    }
                }
                await _context.SaveChangesAsync();

                // 4. Đồng bộ 24 Câu hỏi Tình huống (Questions)
                var questions = GameplaySeedData.GetQuestions();
                foreach (var q in questions)
                {
                    var existing = await _context.Questions.FirstOrDefaultAsync(item => item.Id == q.Id);
                    if (existing == null)
                    {
                        await _context.Questions.AddAsync(q);
                    }
                    else
                    {
                        existing.Type = q.Type;
                        existing.Title = q.Title;
                        existing.ImageUrl = q.ImageUrl;
                        existing.Payload = q.Payload;
                        existing.CorrectData = q.CorrectData;
                        existing.Explanation = q.Explanation;
                        existing.Difficulty = q.Difficulty;
                        existing.Source = q.Source;
                        existing.ReviewedBy = q.ReviewedBy;
                        existing.Status = q.Status;
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã đồng bộ hoàn tất 24 câu hỏi tình huống thực tế có cơ sở pháp lý và y tế.");

                // 5. Liên kết Câu hỏi vào Bài học (NodeQuestions - 3 câu/bài)
                var nodeQuestions = GameplaySeedData.GetNodeQuestions();
                foreach (var nq in nodeQuestions)
                {
                    var existing = await _context.NodeQuestions
                        .FirstOrDefaultAsync(x => x.NodeId == nq.NodeId && x.QuestionId == nq.QuestionId);
                    if (existing == null)
                    {
                        await _context.NodeQuestions.AddAsync(nq);
                    }
                    else
                    {
                        existing.Order = nq.Order;
                    }
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã đồng bộ liên kết 24 câu hỏi vào 8 Nodes thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Seeding dữ liệu Gameplay 24 tình huống: {Message}", ex.Message);
            }
        }

        private async Task SeedGamificationDataAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu đồng bộ danh mục Gamification (Quests, Shop, Badges, Handbook)...");

                // 1. Quests
                var quests = GamificationSeedData.GetDailyQuests();
                foreach (var q in quests)
                {
                    var existing = await _context.DailyQuests.FirstOrDefaultAsync(item => item.Id == q.Id);
                    if (existing == null)
                    {
                        await _context.DailyQuests.AddAsync(q);
                    }
                    else
                    {
                        existing.Title = q.Title;
                        existing.QuestType = q.QuestType;
                        existing.Target = q.Target;
                        existing.RewardType = q.RewardType;
                        existing.RewardAmount = q.RewardAmount;
                        existing.Icon = q.Icon;
                        existing.IsActive = q.IsActive;
                    }
                }
                await _context.SaveChangesAsync();

                // 2. Shop Items
                var shopItems = GamificationSeedData.GetShopItems();
                foreach (var s in shopItems)
                {
                    var existing = await _context.ShopItems.FirstOrDefaultAsync(item => item.Id == s.Id);
                    if (existing == null)
                    {
                        await _context.ShopItems.AddAsync(s);
                    }
                    else
                    {
                        existing.Title = s.Title;
                        existing.Description = s.Description;
                        existing.CostXp = s.CostXp;
                        existing.Icon = s.Icon;
                        existing.Color = s.Color;
                        existing.BgColor = s.BgColor;
                        existing.Status = s.Status;
                    }
                }
                await _context.SaveChangesAsync();

                // 3. Badges
                var badges = GamificationSeedData.GetBadges();
                foreach (var b in badges)
                {
                    var existing = await _context.Badges.FirstOrDefaultAsync(item => item.Id == b.Id);
                    if (existing == null)
                    {
                        await _context.Badges.AddAsync(b);
                    }
                    else
                    {
                        existing.Title = b.Title;
                        existing.Category = b.Category;
                        existing.Description = b.Description;
                        existing.Icon = b.Icon;
                        existing.Rarity = b.Rarity;
                        existing.CriteriaType = b.CriteriaType;
                        existing.CriteriaValue = b.CriteriaValue;
                    }
                }
                await _context.SaveChangesAsync();

                // 4. Handbook Articles
                var articles = GamificationSeedData.GetHandbookArticles();
                foreach (var a in articles)
                {
                    var existing = await _context.HandbookArticles.FirstOrDefaultAsync(item => item.Id == a.Id);
                    if (existing == null)
                    {
                        await _context.HandbookArticles.AddAsync(a);
                    }
                    else
                    {
                        existing.Pillar = a.Pillar;
                        existing.Title = a.Title;
                        existing.Badge = a.Badge;
                        existing.ReadTime = a.ReadTime;
                        existing.EmergencyDials = a.EmergencyDials;
                        existing.KeyProtocol = a.KeyProtocol;
                        existing.LegalBasis = a.LegalBasis;
                        existing.ContentMarkdown = a.ContentMarkdown;
                        existing.Order = a.Order;
                    }
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã đồng bộ hoàn tất dữ liệu Gamification (3 Quests, 4 ShopItems, 8 Badges, 8 HandbookArticles).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Seeding Gamification data: {Message}", ex.Message);
            }
        }

        private async Task SeedB2BClassroomsAndAssignmentsAsync()
        {
            try
            {
                // 1. Tạo hoặc lấy Tổ chức mẫu THPT FPT Cần Thơ
                var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Code == "FPT-CT");
                if (org == null)
                {
                    org = new Organization
                    {
                        Name = "THPT FPT Cần Thơ",
                        Code = "FPT-CT",
                        Email = "fpt.cantho@fpt.edu.vn",
                        SeatCount = 100,
                        UsedSeats = 0,
                        ExpirationDate = DateTime.UtcNow.AddYears(1),
                        Status = OrganizationStatusEnum.Active,
                        SubscriptionTier = SubscriptionTierEnum.Enterprise
                    };
                    await _context.Organizations.AddAsync(org);
                    await _context.SaveChangesAsync();
                }

                // Liên kết tài khoản B2B (d1@gmail.com / b2b_test@gmail.com) vào trường này nếu có
                var b2bUsers = await _context.Users.Where(u => u.Email == "d1@gmail.com" || u.Email == "b2b_test@gmail.com").ToListAsync();
                foreach (var b2bUser in b2bUsers)
                {
                    if (!b2bUser.OrganizationId.HasValue)
                    {
                        b2bUser.OrganizationId = org.Id;
                        _context.Users.Update(b2bUser);
                    }
                }
                await _context.SaveChangesAsync();

                // 2. Tạo 3 lớp mẫu (FPT-K10, LHP-11B, VSC-12A) theo MyClassroomPage.jsx
                var classFpt = await _context.Classes.FirstOrDefaultAsync(c => c.ClassCode == "FPT-K10");
                if (classFpt == null)
                {
                    classFpt = new Class
                    {
                        OrganizationId = org.Id,
                        Name = "Lớp 10A1 — THPT FPT Cần Thơ",
                        ClassCode = "FPT-K10",
                        Grade = "Khối 10",
                        Room = "Phòng 302 - Toà Alpha",
                        FocusTopic = "Kỹ năng PCCC & Thoát hiểm đám cháy",
                        MaxStudents = 50,
                        Status = "Active"
                    };
                    await _context.Classes.AddAsync(classFpt);
                }

                var classLhp = await _context.Classes.FirstOrDefaultAsync(c => c.ClassCode == "LHP-11B");
                if (classLhp == null)
                {
                    classLhp = new Class
                    {
                        OrganizationId = org.Id,
                        Name = "Lớp 11 Sinh — THPT Chuyên Lê Hồng Phong TP.HCM",
                        ClassCode = "LHP-11B",
                        Grade = "Khối 11",
                        Room = "Phòng Lab Sinh học 2",
                        FocusTopic = "Sơ cấp cứu CPR & Dị vật Heimlich",
                        MaxStudents = 50,
                        Status = "Active"
                    };
                    await _context.Classes.AddAsync(classLhp);
                }

                var classVsc = await _context.Classes.FirstOrDefaultAsync(c => c.ClassCode == "VSC-12A");
                if (classVsc == null)
                {
                    classVsc = new Class
                    {
                        OrganizationId = org.Id,
                        Name = "Lớp 12A3 — Vinschool Central Park",
                        ClassCode = "VSC-12A",
                        Grade = "Khối 12",
                        Room = "Phòng Innovation Hub 1",
                        FocusTopic = "An toàn thông tin & Chống lừa đảo mạng",
                        MaxStudents = 50,
                        Status = "Active"
                    };
                    await _context.Classes.AddAsync(classVsc);
                }
                await _context.SaveChangesAsync();

                // 3. Giao bài tập mẫu trỏ vào Node Published đầu tiên
                var publishedNode = await _context.Nodes.FirstOrDefaultAsync(n => n.Status == "Published");
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == AdminRoleId);

                if (publishedNode != null && adminUser != null && classFpt != null)
                {
                    var existingAssignment = await _context.Assignments.FirstOrDefaultAsync(a => a.ClassId == classFpt.Id && a.NodeId == publishedNode.Id);
                    if (existingAssignment == null)
                    {
                        var assignment = new Assignment
                        {
                            ClassId = classFpt.Id,
                            NodeId = publishedNode.Id,
                            CreatedBy = adminUser.Id,
                            Title = "Thực hành kỹ năng an toàn & thoát nạn khẩn cấp",
                            Description = "Học sinh hoàn thành bài học tình huống và đạt điểm từ 80% trở lên trước hạn chót.",
                            Deadline = DateTime.UtcNow.AddDays(7),
                            PassScoreThreshold = 80,
                            RewardXp = 50
                        };
                        await _context.Assignments.AddAsync(assignment);
                        await _context.SaveChangesAsync();
                    }
                }

                // 4. Tạo mã kích hoạt Giáo viên mẫu
                var license = await _context.TeacherLicenses.FirstOrDefaultAsync(l => l.ActivationCode == "TEACH-FPT-2026");
                if (license == null && classFpt != null)
                {
                    license = new TeacherLicense
                    {
                        OrganizationId = org.Id,
                        ActivationCode = "TEACH-FPT-2026",
                        Note = "Giấy phép giáo viên chủ nhiệm Khối 10",
                        AssignedClassesJson = System.Text.Json.JsonSerializer.Serialize(new[] { classFpt.Id }),
                        IsClaimed = false,
                        ExpiresAt = DateTime.UtcNow.AddYears(1)
                    };
                    await _context.TeacherLicenses.AddAsync(license);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Đã đồng bộ hoàn tất dữ liệu Seeding B2B Classrooms & Assignments (3 lớp FPT-K10, LHP-11B, VSC-12A).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Seeding B2B Classroom data: {Message}", ex.Message);
            }
        }
    }
}

