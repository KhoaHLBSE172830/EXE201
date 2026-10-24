namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO cấu hình cân bằng tham số game hóa (Gamification Balancing Settings)
    /// </summary>
    public class GamificationSettingsDto
    {
        // 1. Cơ chế Tim (Hearts)
        public int MaxHearts { get; set; } = 5;
        public int HeartRefillMinutes { get; set; } = 240; // 4 tiếng hồi 1 tim
        public int PracticeQuestionsToRefill { get; set; } = 3;
        public int HeartRefillPriceXP { get; set; } = 150;

        // 2. Cơ chế Chuỗi ngày (Streak)
        public int StreakMinLessonsDaily { get; set; } = 1;
        public int FreezePriceXP { get; set; } = 200;
        public int MaxFreezeHolding { get; set; } = 2;

        // 3. Cơ chế Giải đấu (Leagues)
        public int CohortSize { get; set; } = 30; // 30 người / 1 bảng
        public int PromotionCount { get; set; } = 7; // Top 7 thăng hạng
        public int DemotionCount { get; set; } = 5; // Top 5 rớt hạng
        public int Top1RewardXP { get; set; } = 100;
        public string Top1Badge { get; set; } = "Huy hiệu Quán quân Tuần";

        // 4. Chế độ bảo vệ & Anti-cheat
        public int AutoBanBotThresholdXP { get; set; } = 10000;
    }
}
