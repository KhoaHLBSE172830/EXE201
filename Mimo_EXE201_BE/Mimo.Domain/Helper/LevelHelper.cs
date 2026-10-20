namespace Mimo.DomainLayer.Helper
{
    public static class LevelHelper
    {
        public class LevelInfo
        {
            public int Level { get; set; }
            public string Title { get; set; } = string.Empty;
            public int CurrentXp { get; set; }
            public int MinXpForLevel { get; set; }
            public int MaxXpForLevel { get; set; }
            public int XpToNextLevel { get; set; }
            public double ProgressPercent { get; set; }
        }

        /// <summary>
        /// Công thức xác định Level dẫn xuất từ total_xp theo AGENT.md Section 5.3
        /// Level 1: 0 - 199 XP (Tân binh an toàn)
        /// Level 2: 200 - 499 XP (Người phòng vệ - Defender)
        /// Level 3: 500 - 999 XP (Người giải cứu - Rescuer)
        /// Level 4: 1000 - 1999 XP (Chuyên gia an toàn - Safety Guardian)
        /// Level 5: >= 2000 XP (Người làm chủ cuộc sống - Life Master)
        /// </summary>
        public static LevelInfo GetLevelInfo(int totalXp)
        {
            if (totalXp < 0) totalXp = 0;

            if (totalXp < 200)
            {
                return new LevelInfo
                {
                    Level = 1,
                    Title = "Tân binh an toàn",
                    CurrentXp = totalXp,
                    MinXpForLevel = 0,
                    MaxXpForLevel = 200,
                    XpToNextLevel = 200 - totalXp,
                    ProgressPercent = Math.Round((double)totalXp / 200 * 100, 1)
                };
            }
            if (totalXp < 500)
            {
                return new LevelInfo
                {
                    Level = 2,
                    Title = "Người phòng vệ (Defender)",
                    CurrentXp = totalXp,
                    MinXpForLevel = 200,
                    MaxXpForLevel = 500,
                    XpToNextLevel = 500 - totalXp,
                    ProgressPercent = Math.Round((double)(totalXp - 200) / 300 * 100, 1)
                };
            }
            if (totalXp < 1000)
            {
                return new LevelInfo
                {
                    Level = 3,
                    Title = "Người giải cứu (Rescuer)",
                    CurrentXp = totalXp,
                    MinXpForLevel = 500,
                    MaxXpForLevel = 1000,
                    XpToNextLevel = 1000 - totalXp,
                    ProgressPercent = Math.Round((double)(totalXp - 500) / 500 * 100, 1)
                };
            }
            if (totalXp < 2000)
            {
                return new LevelInfo
                {
                    Level = 4,
                    Title = "Chuyên gia an toàn (Safety Guardian)",
                    CurrentXp = totalXp,
                    MinXpForLevel = 1000,
                    MaxXpForLevel = 2000,
                    XpToNextLevel = 2000 - totalXp,
                    ProgressPercent = Math.Round((double)(totalXp - 1000) / 1000 * 100, 1)
                };
            }

            return new LevelInfo
            {
                Level = 5,
                Title = "Người làm chủ cuộc sống (Life Master)",
                CurrentXp = totalXp,
                MinXpForLevel = 2000,
                MaxXpForLevel = 2000,
                XpToNextLevel = 0,
                ProgressPercent = 100.0
            };
        }
    }
}
