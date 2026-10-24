using System;
using System.Collections.Generic;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Database
{
    public static class GamificationSeedData
    {
        public static List<DailyQuest> GetDailyQuests()
        {
            return new List<DailyQuest>
            {
                new DailyQuest
                {
                    Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
                    Title = "Đạt 40 XP hôm nay",
                    QuestType = "XpGoal",
                    Target = 40,
                    RewardType = "XP",
                    RewardAmount = 15,
                    Icon = "⚡",
                    IsActive = true
                },
                new DailyQuest
                {
                    Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
                    Title = "Hoàn thành 2 bài học kỹ năng",
                    QuestType = "LessonsCount",
                    Target = 2,
                    RewardType = "XP",
                    RewardAmount = 20,
                    Icon = "🎯",
                    IsActive = true
                },
                new DailyQuest
                {
                    Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"),
                    Title = "1 bài học Hoàn hảo (Không mất tim)",
                    QuestType = "PerfectLesson",
                    Target = 1,
                    RewardType = "Heart",
                    RewardAmount = 1,
                    Icon = "✨",
                    IsActive = true
                }
            };
        }

        public static List<ShopItem> GetShopItems()
        {
            return new List<ShopItem>
            {
                new ShopItem
                {
                    Id = "freeze",
                    Title = "Bùa Đóng Băng Streak",
                    Description = "Bảo vệ chuỗi ngày học của bạn không bị đứt gãy nếu quên luyện tập 1 ngày.",
                    CostXp = 200,
                    Icon = "🛡️",
                    Color = "#0284c7",
                    BgColor = "#e0f2fe",
                    Status = "Active"
                },
                new ShopItem
                {
                    Id = "hearts_refill",
                    Title = "Gói Hồi Đầy 5 Tim",
                    Description = "Khôi phục ngay lập tức 5 Trái Tim để tiếp tục học mà không cần chờ thời gian hồi phục.",
                    CostXp = 150,
                    Icon = "❤️",
                    Color = "#e11d48",
                    BgColor = "#ffe4e6",
                    Status = "Active"
                },
                new ShopItem
                {
                    Id = "pro_trial_7d",
                    Title = "Trải nghiệm Pro 7 Ngày",
                    Description = "Mở khóa Trái Tim Vô Hạn và toàn quyền luyện tập không giới hạn trong 7 ngày.",
                    CostXp = 500,
                    Icon = "👑",
                    Color = "#d97706",
                    BgColor = "#fef3c7",
                    Status = "Active"
                },
                new ShopItem
                {
                    Id = "badge_hero",
                    Title = "Huy hiệu Người Bảo Vệ",
                    Description = "Huy hiệu danh dự độc quyền hiển thị trên hồ sơ cá nhân và bảng xếp hạng.",
                    CostXp = 300,
                    Icon = "🎖️",
                    Color = "#7c3aed",
                    BgColor = "#ede9fe",
                    Status = "Active"
                }
            };
        }

        public static List<Badge> GetBadges()
        {
            return new List<Badge>
            {
                new Badge
                {
                    Id = "first_step",
                    Title = "Khởi đầu vững chắc",
                    Category = "general",
                    Description = "Hoàn thành bài học kỹ năng sinh tồn đầu tiên.",
                    Icon = "🌱",
                    Rarity = "Phổ biến",
                    CriteriaType = "FirstLesson",
                    CriteriaValue = 1
                },
                new Badge
                {
                    Id = "first_aid_expert",
                    Title = "Bàn tay vàng sơ cứu",
                    Category = "first_aid",
                    Description = "Nắm vững quy trình sơ cứu bỏng, chảy máu cam và hóc dị vật Heimlich.",
                    Icon = "🩹",
                    Rarity = "Khá hiếm",
                    CriteriaType = "CompletedTopic",
                    CriteriaValue = 1
                },
                new Badge
                {
                    Id = "fire_hero",
                    Title = "Người hùng PCCC",
                    Category = "fire_safety",
                    Description = "Thuần thục kỹ năng thoát hiểm hỏa hoạn nhà chung cư và gọi tổng đài 114.",
                    Icon = "🔥",
                    Rarity = "Khá hiếm",
                    CriteriaType = "CompletedTopic",
                    CriteriaValue = 2
                },
                new Badge
                {
                    Id = "traffic_master",
                    Title = "Hiệp sĩ đường phố",
                    Category = "traffic_safety",
                    Description = "Hiểu rõ luật trật tự an toàn giao thông đường bộ và nguyên tắc đi bộ an toàn.",
                    Icon = "🚦",
                    Rarity = "Khá hiếm",
                    CriteriaType = "CompletedTopic",
                    CriteriaValue = 3
                },
                new Badge
                {
                    Id = "school_safety_hero",
                    Title = "Vệ sĩ học đường",
                    Category = "school_safety",
                    Description = "Thành thạo ứng phó bạo lực học đường, bắt nạt không gian mạng và tổng đài 111.",
                    Icon = "🛡️",
                    Rarity = "Khá hiếm",
                    CriteriaType = "CompletedTopic",
                    CriteriaValue = 4
                },
                new Badge
                {
                    Id = "streak_7days",
                    Title = "Chiến binh kiên trì",
                    Category = "general",
                    Description = "Duy trì ngọn lửa học tập liên tục trong 7 ngày không gián đoạn.",
                    Icon = "⚡",
                    Rarity = "Hiếm",
                    CriteriaType = "StreakDays",
                    CriteriaValue = 7
                },
                new Badge
                {
                    Id = "perfect_warrior",
                    Title = "Hoàn hảo tuyệt đối",
                    Category = "general",
                    Description = "Đạt độ chính xác 100% trong 5 bài học mô phỏng liên tiếp.",
                    Icon = "⭐",
                    Rarity = "Hiếm",
                    CriteriaType = "PerfectLessons",
                    CriteriaValue = 5
                },
                new Badge
                {
                    Id = "safety_champion",
                    Title = "Bậc thầy an toàn",
                    Category = "general",
                    Description = "Tích lũy đạt tổng cộng 500 XP kinh nghiệm kỹ năng sống.",
                    Icon = "🏆",
                    Rarity = "Huyền thoại",
                    CriteriaType = "TotalXp",
                    CriteriaValue = 500
                }
            };
        }

        public static List<HandbookArticle> GetHandbookArticles()
        {
            return new List<HandbookArticle>
            {
                new HandbookArticle
                {
                    Id = "emergency_dials",
                    Pillar = "Hotline khẩn cấp",
                    Title = "4 Số điện thoại cứu nạn cứu hộ quốc gia bắt buộc phải thuộc lòng",
                    Badge = "Sống còn",
                    ReadTime = "2 phút",
                    Order = 1,
                    EmergencyDials = "[{\"name\":\"Tổng đài Quốc gia Bảo vệ Trẻ em\",\"dial\":\"111\"},{\"name\":\"Công an & Trật tự an ninh\",\"dial\":\"113\"},{\"name\":\"Cứu nạn cứu hộ & PCCC\",\"dial\":\"114\"},{\"name\":\"Cấp cứu y tế toàn quốc\",\"dial\":\"115\"}]",
                    KeyProtocol = "111 - Trẻ em & Bạo lực gia đình/học đường; 113 - An ninh trật tự & tội phạm; 114 - Hỏa hoạn cháy nổ; 115 - Cấp cứu y tế. Tất cả đều MIỄN PHÍ CƯỚC GỌI.",
                    LegalBasis = "Điều 6 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024; Luật Trẻ em 2016; Luật Phòng chống bạo lực gia đình 2022. [VB]",
                    ContentMarkdown = @"### 1. Bốn đầu số cứu nạn quốc gia
- **111 - Tổng đài Quốc gia Bảo vệ Trẻ em:** Tiếp nhận tin báo 24/7 về bạo lực học đường, ngược đãi trẻ em, tư vấn pháp lý và tâm lý.
- **113 - Công an phản ứng nhanh:** Tiếp nhận vụ việc về an ninh trật tự, trộm cướp, đe dọa bạo lực khẩn cấp.
- **114 - Cứu hỏa và Cứu nạn, cứu hộ:** Báo cháy, chập điện, kẹt thang máy, đuối nước, sạt lở.
- **115 - Cấp cứu y tế:** Cấp cứu người bất tỉnh, tai nạn giao thông nghiêm trọng, đột quỵ, chấn thương nặng.

### 2. Nguyên tắc khi gọi cứu hộ
1. **Nói địa chỉ đầu tiên:** Địa chỉ càng chính xác, lực lượng cứu nạn đến càng nhanh.
2. **Nêu rõ tình trạng:** Có bao nhiêu người gặp nạn, có ai bị kẹt hay chảy máu không.
3. **Giữ máy:** Không dập máy trước khi điều phối viên hướng dẫn xong."
                },
                new HandbookArticle
                {
                    Id = "first_aid_burns",
                    Pillar = "Sơ cấp cứu",
                    Title = "Quy trình sơ cứu bỏng nước sôi và nhiệt độ cao chuẩn y khoa",
                    Badge = "Y tế khẩn cấp",
                    ReadTime = "3 phút",
                    Order = 2,
                    EmergencyDials = "[{\"name\":\"Cấp cứu y tế\",\"dial\":\"115\"}]",
                    KeyProtocol = "Làm mát dưới vòi nước sạch chảy nhẹ 15–20 phút. TUYỆT ĐỐI KHÔNG dùng đá lạnh, không bôi kem đánh răng hay nước mắm.",
                    LegalBasis = "Hướng dẫn chuyên môn y tế của Bệnh viện Đại học Y Dược TP.HCM và Vinmec. [CM]",
                    ContentMarkdown = @"### 1. Các bước sơ cứu tức thì
1. **Đưa vết bỏng dưới vòi nước sạch:** Cho nước mát (nhiệt độ phòng, 15–20°C) chảy nhẹ lên vùng da bị bỏng trong 15 đến 20 phút. Việc này giúp hạ nhiệt lớp biểu bì, giảm tổn thương mô sâu và giảm đau rát.
2. **Che phủ nhẹ nhàng:** Dùng gạc vô khuẩn hoặc khăn sạch, ẩm che lên vết thương.
3. **Đến cơ sở y tế:** Nếu vết bỏng phồng rộp diện rộng, xuất hiện ở mặt, khớp, hoặc ở trẻ nhỏ, hãy đưa đến bệnh viện ngay.

### 2. Những sai lầm NGUY HIỂM cần tránh
- ❌ **Không chườm đá lạnh:** Nước đá gây co mạch đột ngột, làm bỏng lạnh sâu thêm và hoại tử mô.
- ❌ **Không bôi kem đánh răng, mỡ trăn, nước mắm:** Gây kích ứng hóa chất và nguy cơ nhiễm trùng máu cực kỳ cao.
- ❌ **Không tự ý chọc vỡ bóng nước:** Bóng nước là màng bảo vệ tự nhiên chống vi khuẩn."
                },
                new HandbookArticle
                {
                    Id = "first_aid_nosebleed",
                    Pillar = "Sơ cấp cứu",
                    Title = "Sơ cứu chảy máu cam đúng tư thế",
                    Badge = "Kỹ năng sống",
                    ReadTime = "3 phút",
                    Order = 3,
                    KeyProtocol = "Ngồi thẳng, hơi cúi đầu về phía trước, bóp chặt cánh mũi mềm 10 phút. KHÔNG ngửa đầu ra sau.",
                    LegalBasis = "Khuyến cáo y khoa của Bác sĩ chuyên khoa Nhi - BV Nhi Trung ương & BV Tâm Anh. [CM]",
                    ContentMarkdown = @"### 1. Tư thế chuẩn khi chảy máu cam
1. **Ngồi thẳng lưng, đầu hơi cúi về phía trước:** Giúp máu chảy ra ngoài cửa mũi, không chảy ngược vào cổ họng.
2. **Dùng ngón cái và ngón trỏ bóp chặt 2 cánh mũi mềm:** Bóp liên tục trong 10 đến 15 phút.
3. **Thở bằng miệng:** Bình tĩnh thở đều bằng miệng, nhổ máu trong họng ra nếu có.

### 2. Điều tuyệt đối tránh
- ❌ **Không ngửa đầu ra sau:** Máu sẽ chảy ngược xuống họng gây sặc, nôn mửa hoặc tắc đường thở.
- ❌ **Không nhét khăn giấy sâu vào mũi:** Giấy vệ sinh không vô khuẩn có thể gây rách niêm mạc và nhiễm khuẩn."
                },
                new HandbookArticle
                {
                    Id = "heimlich_choking",
                    Pillar = "Sơ cấp cứu",
                    Title = "Thủ thuật Heimlich cấp cứu dị vật đường thở",
                    Badge = "Sinh tử",
                    ReadTime = "4 phút",
                    Order = 4,
                    EmergencyDials = "[{\"name\":\"Cấp cứu y tế\",\"dial\":\"115\"}]",
                    KeyProtocol = "Chỉ thực hiện khi nạn nhân nghẹt thở hoàn toàn, tím tái, không nói được, không ho được. Nhờ người gọi 115 ngay lập tức.",
                    LegalBasis = "Phác đồ Cấp cứu Đường thở của Bệnh viện Nhi Trung ương và Hội đồng Hồi sức Quốc tế. [CM]",
                    ContentMarkdown = @"### 1. Nhận diện dấu hiệu hóc dị vật nguy kịch
- Hai tay ôm cổ (dấu hiệu phổ quát).
- Mặt tím tái, không thể nói, không thể khóc, không thể ho ra tiếng.

### 2. Các bước thực hiện thủ thuật Heimlich (Trẻ lớn & Người lớn)
1. Đứng phía sau nạn nhân, vòng hai tay ôm lấy eo.
2. Một tay nắm đấm, đặt ngón cái lên vùng thượng vị (trên rốn, dưới mũi xương ức).
3. Tay kia nắm chặt lấy bàn tay đấm, giật mạnh dứt khoát theo hướng **VÀO TRONG VÀ LÊN TRÊN**.
4. Lặp lại 5 lần cho đến khi dị vật bật ra ngoài hoặc nạn nhân thở lại được.

*Lưu ý: Nếu nạn nhân vẫn còn ho mạnh được, hãy khuyến khích ho để tống dị vật ra ngoài tự nhiên, không vội can thiệp.*"
                },
                new HandbookArticle
                {
                    Id = "fire_apartment",
                    Pillar = "PCCC & Thoát hiểm",
                    Title = "Kỹ năng thoát hiểm hỏa hoạn ở nhà cao tầng và chung cư",
                    Badge = "Thoát nạn",
                    ReadTime = "4 phút",
                    Order = 5,
                    EmergencyDials = "[{\"name\":\"Cứu hỏa PCCC\",\"dial\":\"114\"}]",
                    KeyProtocol = "TUYỆT ĐỐI KHÔNG DÙNG THANG MÁY. Cúi thấp men tường, dùng khăn ướt che mũi miệng, đi thang bộ thoát hiểm có đèn EXIT.",
                    LegalBasis = "Tài liệu tập huấn Cứu nạn Cứu hộ - Cục Cảnh sát PCCC & CNCH, Bộ Công an. [VB/CM]",
                    ContentMarkdown = @"### 1. Nguyên tắc vàng khi có chuông báo cháy
1. **Không hoảng loạn, không quay lại lấy tài sản:** Đồ đạc có thể mua lại, tính mạng là duy nhất.
2. **Khăn ẩm che mũi miệng:** 90% trường hợp thương vong do ngạt khói độc khí CO. Khăn ẩm đóng vai trò lọc khói.
3. **Cúi thấp người hoặc bò:** Khói độc nhẹ hơn không khí nên sẽ bốc lên trần. Lớp không khí thở được nằm sát sàn nhà (dưới 50cm).
4. **Luôn đi thang bộ thoát hiểm:** Thang máy có thể mất điện, kẹt giữa các tầng và trở thành ống hút khói chết người.

### 2. Khi cửa phòng nóng rực
- Không mở cửa ngay. Dùng mu bàn tay chạm nhẹ vào tay nắm cửa. Nếu tay nắm nóng, khói và lửa đã bao trùm hành lang bên ngoài. Hãy giữ cửa đóng kín và chuyển sang phương án cô lập phòng."
                },
                new HandbookArticle
                {
                    Id = "fire_trapped",
                    Pillar = "PCCC & Thoát hiểm",
                    Title = "Xử trí sinh tồn khi bị kẹt trong phòng ngập khói",
                    Badge = "Sinh tồn",
                    ReadTime = "3 phút",
                    Order = 6,
                    EmergencyDials = "[{\"name\":\"Cứu hỏa PCCC\",\"dial\":\"114\"}]",
                    KeyProtocol = "Đóng cửa ngăn khói, lấy chăn/khăn ướt chèn khe cửa, ra ban công thoáng kêu gọi cứu nạn, gọi ngay 114 báo số phòng/tầng.",
                    LegalBasis = "Hướng dẫn thoát nạn trong phòng kín - Điều 14 Luật PCCC 2024. [VB/CM]",
                    ContentMarkdown = @"### 1. Cách cô lập và tạo vùng thở an toàn
1. **Đóng kín các cửa phòng:** Ngăn ngọn lửa và khói lùa vào phòng.
2. **Chèn khe cửa:** Nhúng ướt khăn tắm, chăn, quần áo rồi nhét chặt vào khe hở dưới chân cửa và quanh mép cửa.
3. **Di chuyển ra ban công hoặc cửa sổ:** Mở cửa sổ đón gió tươi, hô to hoặc dùng vải màu sáng vẫy ra bên ngoài để người cứu hộ nhìn thấy.
4. **Gọi 114:** Nói rõ địa chỉ nhà, số tầng, số phòng và số người đang bị kẹt.

### 2. Điều cấm kỵ
- ❌ **Tuyệt đối không trốn vào nhà vệ sinh:** Nhà vệ sinh là phòng kín, khói lọt vào sẽ gây ngạt nhanh chóng và lính cứu hộ rất khó phát hiện."
                },
                new HandbookArticle
                {
                    Id = "traffic_crossing_rules",
                    Pillar = "An toàn giao thông",
                    Title = "Quy tắc qua đường và đi bộ an toàn cho học sinh",
                    Badge = "Giao thông",
                    ReadTime = "3 phút",
                    Order = 7,
                    KeyProtocol = "Trẻ dưới 7 tuổi bắt buộc có người lớn dắt; qua đường tại vạch kẻ khi đèn xanh; đi sát mép đường bên phải khi không có vỉa hè.",
                    LegalBasis = "Điều 30 Luật Trật tự, an toàn giao thông đường bộ 2024. [VB]",
                    ContentMarkdown = @"### 1. Luật quy định cho người đi bộ (Điều 30 Luật 2024)
- **Nơi có vỉa hè:** Bắt buộc đi trên vỉa hè, lề đường.
- **Nơi không có vỉa hè:** Đi sát mép đường bên phải theo chiều đi của mình.
- **Trẻ em dưới 7 tuổi:** Bắt buộc phải có người lớn nắm tay dẫn dắt khi qua đường.

### 2. Quy tắc quan sát 3 bước khi qua đường
1. **Dừng lại ở mép đường:** Không vội lao ra lòng đường dù bạn bè vẫy gọi hay đồ vật lăn ra.
2. **Quan sát trái - phải - trái:** Đợi các phương tiện đi chậm lại hoặc nhường đường.
3. **Bình tĩnh bước đi:** Đi đều bước, luôn nhìn bao quát xung quanh cho đến khi sang bờ bên kia an toàn."
                },
                new HandbookArticle
                {
                    Id = "school_bullying_111",
                    Pillar = "Phòng chống bạo lực",
                    Title = "Kỹ năng tự bảo vệ trước bạo lực học đường và bắt nạt trên mạng",
                    Badge = "Bảo vệ trẻ em",
                    ReadTime = "4 phút",
                    Order = 8,
                    EmergencyDials = "[{\"name\":\"Tổng đài Quốc gia Bảo vệ Trẻ em\",\"dial\":\"111\"},{\"name\":\"Cảnh sát 113\",\"dial\":\"113\"}]",
                    KeyProtocol = "Hô to 'Dừng lại!', rời đi ngay, kể cho người lớn tin cậy, gọi 111. Bắt nạt mạng: Chụp màn hình bằng chứng, chặn, báo cáo.",
                    LegalBasis = "Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP; Điều 6 Luật Trẻ em 2016; Luật An ninh mạng. [VB]",
                    ContentMarkdown = @"### 1. Nhận biết bạo lực học đường
Bạo lực học đường không chỉ là đánh đập mà còn bao gồm:
- Lăng mạ, chê bai ngoại hình, đặt biệt danh ác ý.
- Cô lập, xúi giục cả lớp tẩy chay bạn.
- Đe dọa trấn lột tiền, đồ dùng học tập.

### 2. Cách xử lý thông minh và an toàn
1. **Hô to dứt khoát:** Nhìn thẳng vào đối tượng và nói to: *'Dừng lại! Bạn không được phép làm thế với mình!'* để thu hút sự chú ý của người xung quanh.
2. **Rời khỏi khu vực nguy hiểm:** Tìm đến phòng giám thị, phòng y tế hoặc nơi có thầy cô giáo.
3. **Kể lại không giấu giếm:** Nói với cha mẹ, thầy cô chủ nhiệm. Giữ bí mật về bạo lực chỉ khiến kẻ bắt nạt lấn tới.
4. **Bắt nạt trên mạng:** Chụp lại toàn bộ màn hình tin nhắn, bài đăng chế giễu (làm bằng chứng), không nhắn tin chửi lại, bấm chặn và báo cáo quản trị viên."
                }
            };
        }
    }
}
