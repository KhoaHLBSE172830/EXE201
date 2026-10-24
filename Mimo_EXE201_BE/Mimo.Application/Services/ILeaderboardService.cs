using System;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Leaderboard;

namespace Mimo.Application.Services
{
    public interface ILeaderboardService
    {
        /// <summary>
        /// Task BE5-1: Lấy bảng xếp hạng tuần hiện tại (Top 30 + vị trí của người dùng hiện tại)
        /// Tổng hợp điểm XP từ bảng xp_events từ 00:00:00 Thứ Hai theo giờ Việt Nam.
        /// </summary>
        Task<LeaderboardResponseDto> GetCurrentLeaderboardAsync(Guid? currentUserId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Task BE5-2: Sinh dữ liệu 35-40 người dùng giả lập có XP ngẫu nhiên để bảng xếp hạng luôn hiển thị đầy đặn khi demo
        /// </summary>
        Task<int> SeedMockLeaderboardUsersAsync(int count = 35, CancellationToken cancellationToken = default);
    }
}
