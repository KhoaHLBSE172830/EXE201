using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;

namespace Mimo.Application.Services
{
    public class AdminFeedbackService : IAdminFeedbackService
    {
        private static readonly List<AdminFeedbackItemDto> _inMemoryTickets = new()
        {
            new AdminFeedbackItemDto
            {
                Id = "fb_01",
                Type = "BUG_UI",
                LessonTitle = "Thoát hiểm khói độc chung cư",
                QuestionPrompt = "Khi chuông báo cháy reo lúc nửa đêm, mở cửa phòng ngủ thấy khói đen...",
                ReportedBy = "Lê Gia Huy (Vinschool)",
                UserEmail = "huy.lg@vinschool.edu.vn",
                Description = "Nút chọn phương án B bị che mất 1 nửa trên màn hình iPhone 13 Mini, em phải xoay ngang mới bấm được ạ.",
                CreatedAt = "2026-10-18 16:45",
                Status = "OPEN"
            },
            new AdminFeedbackItemDto
            {
                Id = "fb_02",
                Type = "MEDICAL_FEEDBACK",
                LessonTitle = "Sơ cứu gãy xương cẳng tay",
                QuestionPrompt = "Bước cố định nẹp gỗ khi nghi ngờ gãy xương kín...",
                ReportedBy = "BS. Trần Văn Hưng (Bệnh viện 115)",
                UserEmail = "hung.tv@115.hospital.vn",
                Description = "Nên nhấn mạnh thêm việc KHÔNG ĐƯỢC nắn chỉnh lại xương bị cong trước khi nẹp để tránh làm đứt bó mạch thần kinh.",
                CreatedAt = "2026-10-18 10:20",
                Status = "OPEN"
            },
            new AdminFeedbackItemDto
            {
                Id = "fb_03",
                Type = "TYPO",
                LessonTitle = "An toàn phòng chống lừa đảo deepfake",
                QuestionPrompt = "Nhận diện cuộc gọi video mạo danh công an...",
                ReportedBy = "Trần Bảo An",
                UserEmail = "an.tb@student.lhp.edu.vn",
                Description = "Từ 'chuyển tiền' ở phương án C bị viết nhầm thành 'chuyển tiềnn'.",
                CreatedAt = "2026-10-17 19:15",
                Status = "OPEN"
            }
        };

        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminFeedbackService> _logger;

        public AdminFeedbackService(
            IGenericRepository<User> userRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminFeedbackService> logger)
        {
            _userRepository = userRepository;
            _xpEventRepository = xpEventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Task<List<AdminFeedbackItemDto>> GetFeedbacksAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_inMemoryTickets.ToList());
        }

        public async Task<List<AdminFeedbackItemDto>> ResolveFeedbackAsync(string feedbackId, int xpGift, Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var ticket = _inMemoryTickets.FirstOrDefault(t => t.Id == feedbackId);
            if (ticket != null)
            {
                ticket.Status = "RESOLVED";

                // Nếu có tặng điểm XP tri ân cho người báo cáo và email tồn tại
                if (xpGift > 0 && !string.IsNullOrEmpty(ticket.UserEmail))
                {
                    var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == ticket.UserEmail);
                    if (user != null)
                    {
                        var xpEvent = new XpEvent
                        {
                            UserId = user.Id,
                            Amount = xpGift,
                            Source = "feedback_reward",
                            ReferenceId = adminUserId != Guid.Empty ? adminUserId : null,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _xpEventRepository.CreateAsync(xpEvent);
                        user.TotalXp += xpGift;
                        user.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Đã tặng +{XpGift} XP tri ân vào sổ cái cho User {UserId} ({Email}) sau khi đóng ticket {TicketId}",
                            xpGift, user.Id, user.Email, feedbackId);
                    }
                }
            }

            return _inMemoryTickets.ToList();
        }
    }
}
