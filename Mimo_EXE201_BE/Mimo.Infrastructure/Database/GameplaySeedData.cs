using System;
using System.Collections.Generic;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Database
{
    public static class GameplaySeedData
    {
        // 1. Domains
        public static readonly Guid DomainFirstAidId = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        public static readonly Guid DomainFireSafetyId = Guid.Parse("a2222222-2222-2222-2222-222222222222");
        public static readonly Guid DomainTrafficSafetyId = Guid.Parse("a3333333-3333-3333-3333-333333333333");
        public static readonly Guid DomainCyberSafetyId = Guid.Parse("a4444444-4444-4444-4444-444444444444");

        // 2. Units
        public static readonly Guid UnitFirstAidId = Guid.Parse("b1111111-1111-1111-1111-111111111111");
        public static readonly Guid UnitFireSafetyId = Guid.Parse("b2222222-2222-2222-2222-222222222222");
        public static readonly Guid UnitTrafficSafetyId = Guid.Parse("b3333333-3333-3333-3333-333333333333");
        public static readonly Guid UnitCyberSafetyId = Guid.Parse("b4444444-4444-4444-4444-444444444444");

        // 3. Nodes (8 Nodes cho 4 Chặng)
        public static readonly Guid Node1Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"); // Sơ cấp cứu 1
        public static readonly Guid Node2Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"); // Sơ cấp cứu 2
        public static readonly Guid Node3Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"); // PCCC 1
        public static readonly Guid Node4Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"); // PCCC 2
        public static readonly Guid Node5Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"); // Giao thông 1
        public static readonly Guid Node6Id = Guid.Parse("c6666666-6666-6666-6666-666666666666"); // Giao thông 2
        public static readonly Guid Node7Id = Guid.Parse("c7777777-7777-7777-7777-777777777777"); // An toàn thân thể & Bạo lực 1
        public static readonly Guid Node8Id = Guid.Parse("c8888888-8888-8888-8888-888888888888"); // An toàn mạng & Pháp lý 2

        // Question IDs d0000001 - d0000024
        public static Guid GetQuestionId(int index) => Guid.Parse($"d0000000-0000-0000-0000-{index:D12}");

        public static List<Domain> GetDomains()
        {
            return new List<Domain>
            {
                new Domain
                {
                    Id = DomainFirstAidId,
                    Name = "Sơ cấp cứu ban đầu (First Aid)",
                    Slug = "first-aid",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2966/2966327.png",
                    Order = 1,
                    Status = "Active"
                },
                new Domain
                {
                    Id = DomainFireSafetyId,
                    Name = "PCCC & Kỹ năng Thoát hiểm (Fire Safety)",
                    Slug = "fire-safety",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/785/785116.png",
                    Order = 2,
                    Status = "Active"
                },
                new Domain
                {
                    Id = DomainTrafficSafetyId,
                    Name = "An toàn Giao thông (Traffic Safety)",
                    Slug = "traffic-safety",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2830/2830305.png",
                    Order = 3,
                    Status = "Active"
                },
                new Domain
                {
                    Id = DomainCyberSafetyId,
                    Name = "Phòng chống Bạo lực & An toàn số (Cyber & Personal Safety)",
                    Slug = "cyber-personal-safety",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1161/1161388.png",
                    Order = 4,
                    Status = "Active"
                }
            };
        }

        public static List<Unit> GetUnits()
        {
            return new List<Unit>
            {
                new Unit
                {
                    Id = UnitFirstAidId,
                    DomainId = DomainFirstAidId,
                    Title = "Chặng 1: Sơ cứu Chấn thương & Khẩn cấp y tế",
                    Description = "Kỹ năng sơ cứu khi bị ngã, trầy xước, chảy máu cam, bỏng nước sôi và hóc dị vật.",
                    Order = 1
                },
                new Unit
                {
                    Id = UnitFireSafetyId,
                    DomainId = DomainFireSafetyId,
                    Title = "Chặng 2: Ứng phó Hỏa hoạn & Thoát nạn khẩn cấp",
                    Description = "Quy tắc báo cháy 114, thoát hiểm chung cư, cách xử lý khi kẹt trong nhà phố ngạt khói.",
                    Order = 1
                },
                new Unit
                {
                    Id = UnitTrafficSafetyId,
                    DomainId = DomainTrafficSafetyId,
                    Title = "Chặng 3: Văn hóa Giao thông đường bộ văn minh",
                    Description = "Quy tắc qua đường, thắt dây an toàn ô tô, đội mũ bảo hiểm và quy định điều khiển xe điện.",
                    Order = 1
                },
                new Unit
                {
                    Id = UnitCyberSafetyId,
                    DomainId = DomainCyberSafetyId,
                    Title = "Chặng 4: Bảo vệ Thân thể & Phòng chống Bắt nạt",
                    Description = "Kỹ năng tự vệ trước bạo lực học đường, xâm hại cơ thể, bắt nạt mạng và trách nhiệm pháp lý.",
                    Order = 1
                }
            };
        }

        public static List<Node> GetNodes()
        {
            return new List<Node>
            {
                new Node
                {
                    Id = Node1Id,
                    UnitId = UnitFirstAidId,
                    Title = "Bài 1.1: Sơ cứu Ngã, Trầy xước & Chảy máu cam",
                    Type = "lesson",
                    Order = 1,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node2Id,
                    UnitId = UnitFirstAidId,
                    Title = "Bài 1.2: Bỏng nước sôi, Hóc dị vật & Đập đầu",
                    Type = "lesson",
                    Order = 2,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node3Id,
                    UnitId = UnitFireSafetyId,
                    Title = "Bài 2.1: Báo động Cháy & Kỹ năng gọi 114",
                    Type = "lesson",
                    Order = 1,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node4Id,
                    UnitId = UnitFireSafetyId,
                    Title = "Bài 2.2: Thoát hiểm Chung cư & Nhà phố ngạt khói",
                    Type = "lesson",
                    Order = 2,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node5Id,
                    UnitId = UnitTrafficSafetyId,
                    Title = "Bài 3.1: Đi bộ an toàn, Ghế ô tô & Mũ bảo hiểm",
                    Type = "lesson",
                    Order = 1,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node6Id,
                    UnitId = UnitTrafficSafetyId,
                    Title = "Bài 3.2: Xe máy điện & Ứng phó khi gặp Tai nạn",
                    Type = "lesson",
                    Order = 2,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node7Id,
                    UnitId = UnitCyberSafetyId,
                    Title = "Bài 4.1: Phòng chống Xâm hại & Bắt nạt học đường",
                    Type = "lesson",
                    Order = 1,
                    XpReward = 10,
                    Status = "Published"
                },
                new Node
                {
                    Id = Node8Id,
                    UnitId = UnitCyberSafetyId,
                    Title = "Bài 4.2: An toàn Không gian mạng & Trách nhiệm pháp lý",
                    Type = "lesson",
                    Order = 2,
                    XpReward = 10,
                    Status = "Published"
                }
            };
        }

        public static List<Question> GetQuestions()
        {
            return new List<Question>
            {
                // ==================== TOPIC 1: AN TOÀN GIAO THÔNG (1 - 6) ====================
                new Question
                {
                    Id = GetQuestionId(1),
                    Type = "choice",
                    Title = "[Qua đường cùng mẹ] Na (5 tuổi) đi học về cùng mẹ. Đến vạch dành cho người đi bộ thì đèn đang đỏ. Bên kia đường có bạn vẫy tay gọi Na.\n\n👉 Na nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1577495508048-b635879837f1?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Buông tay mẹ, chạy sang gặp bạn.\"},{\"id\":\"B\",\"text\":\"Nắm chặt tay mẹ, chờ đèn xanh rồi cùng đi qua.\"},{\"id\":\"C\",\"text\":\"Chờ đường vắng xe rồi tự chạy qua.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Qua đường phải nắm tay người lớn và chờ đèn xanh.\n\nCăn cứ pháp lý: Điều 30 Luật Trật tự, an toàn giao thông đường bộ 2024: trẻ em dưới 7 tuổi khi qua đường phải có người lớn dẫn dắt; mọi người có trách nhiệm giúp trẻ khi qua đường.\n\nGiải thích: Đúng. Có người lớn dắt và chờ đúng tín hiệu cho người đi bộ.",
                    Difficulty = "Easy",
                    Source = "Điều 30 Luật Trật tự, an toàn giao thông đường bộ 2024",
                    ReviewedBy = "Cục Cảnh sát Giao thông - Bộ Công an (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(2),
                    Type = "choice",
                    Title = "[Ngồi ô tô cùng bố] Bin (5 tuổi, cao khoảng 1,1 m) được bố đón bằng ô tô gia đình. Bin đòi ngồi ghế phía trước cạnh bố “cho giống người lớn”.\n\n👉 Bin nên ngồi ở đâu?",
                    ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Ghế trước cạnh bố, vì bố lái chậm.\"},{\"id\":\"B\",\"text\":\"Hàng ghế sau, ngồi ghế dành cho trẻ em và thắt dây an toàn.\"},{\"id\":\"C\",\"text\":\"Ngồi hàng ghế sau nhưng quỳ nhìn ra cửa sổ cho thoáng.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Bé ngồi hàng ghế sau, đúng ghế trẻ em, thắt dây an toàn.\n\nCăn cứ pháp lý: Khoản 3 Điều 10 Luật Trật tự, an toàn giao thông đường bộ 2024 (sửa đổi bởi Luật 118/2025/QH15): chở trẻ dưới 10 tuổi và cao dưới 1,35 m không cho ngồi cùng hàng ghế với người lái (trừ xe chỉ có 1 hàng ghế) và phải dùng thiết bị an toàn phù hợp.\n\nGiải thích: Đúng. Hàng ghế sau và ghế chuyên dụng bảo vệ trẻ tối ưu khi phanh gấp hoặc va chạm.",
                    Difficulty = "Easy",
                    Source = "Khoản 3 Điều 10 Luật TTATGTĐB 2024 (sửa đổi 2025)",
                    ReviewedBy = "Ủy ban An toàn Giao thông Quốc gia (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(3),
                    Type = "choice",
                    Title = "[Mũ bảo hiểm khi đi gần] Minh (8 tuổi) được bố chở xe máy đến trường. Còn vài trăm mét nữa tới cổng trường, Minh muốn tháo mũ hoặc để quai lỏng vì “gần rồi, đội nóng”.\n\n👉 Minh nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1558981806-ec527fa84c39?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Tháo mũ ra vì đã gần tới trường.\"},{\"id\":\"B\",\"text\":\"Đội mũ vừa đầu và cài quai chắc chắn cho tới khi xuống xe.\"},{\"id\":\"C\",\"text\":\"Đội mũ nhưng để quai lỏng cho mát.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Đội mũ phải cài quai, kể cả khi đi quãng đường ngắn.\n\nCăn cứ pháp lý: Điều 7 Nghị định 168/2024/NĐ-CP: chở người không đội mũ bảo hiểm hoặc không cài quai đúng quy cách bị xử phạt.\n\nGiải thích: Mũ không cài quai đúng quy cách coi như chưa đội, khi va chạm mũ sẽ văng ra không bảo vệ được vùng đầu.",
                    Difficulty = "Medium",
                    Source = "Điều 7 Nghị định 168/2024/NĐ-CP",
                    ReviewedBy = "Ủy ban ATGT Quốc gia (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(4),
                    Type = "choice",
                    Title = "[Quả bóng lăn ra đường] Lan (lớp 4) đi bộ về nhà trên con đường nhỏ không có vỉa hè. Quả bóng của Lan lăn ra giữa đường, từ xa có một xe máy đang chạy tới.\n\n👉 Lan nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1508873696983-2df5293cb32f?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Chạy ra nhặt ngay vì xe còn xa.\"},{\"id\":\"B\",\"text\":\"Dừng ở mép đường, chờ xe đi qua, quan sát hai bên rồi mới nhặt, hoặc nhờ người lớn nhặt giúp.\"},{\"id\":\"C\",\"text\":\"Vừa chạy ra vừa giơ tay bắt xe dừng.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Nơi không có vỉa hè, đi sát mép đường bên phải; không lao ra lòng đường vì đồ vật.\n\nCăn cứ pháp lý: Điều 30 Luật 2024: đường không có vỉa hè, lề đường thì người đi bộ đi sát mép đường bên phải theo chiều đi; nơi không có đèn/vạch/cầu vượt/hầm phải quan sát và chỉ qua đường khi bảo đảm an toàn.",
                    Difficulty = "Medium",
                    Source = "Điều 30 Luật TTATGTĐB 2024",
                    ReviewedBy = "Cục CSGT - Bộ Công an (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(5),
                    Type = "choice",
                    Title = "[Mượn xe máy điện của anh] Khoa (lớp 7, 12 tuổi) muốn mượn xe máy điện của anh để đi học vì “chỉ có 2 km”. Bạn bảo rằng ít ai kiểm tra.\n\n👉 Khoa nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Chạy luôn nhưng đội mũ bảo hiểm cho an toàn.\"},{\"id\":\"B\",\"text\":\"Không tự lái; nhờ người lớn đưa đón hoặc đi xe đạp, xe buýt, đợi đủ tuổi theo quy định.\"},{\"id\":\"C\",\"text\":\"Chỉ chạy đường nhỏ, thấy công an thì rẽ đi.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Phải đủ tuổi theo quy định mới được điều khiển xe gắn máy, kể cả xe máy điện.\n\nCăn cứ pháp lý: Luật Trật tự, an toàn giao thông đường bộ 2024: người từ đủ 16 tuổi trở lên mới được điều khiển xe gắn máy (bao gồm cả xe máy điện).\n\nGiải thích: Đội mũ là tốt nhưng Khoa chưa đủ tuổi điều khiển, vi phạm quy định và chưa có kỹ năng phản xạ xử lý tình huống nguy hiểm.",
                    Difficulty = "Hard",
                    Source = "Luật Trật tự, an toàn giao thông đường bộ 2024",
                    ReviewedBy = "Cục CSGT - Bộ Công an (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(6),
                    Type = "choice",
                    Title = "[Chứng kiến một vụ tai nạn] Trên đường về nhà, My (lớp 8) thấy một người đi xe máy ngã ngay trước mặt, nằm im và chảy máu ở chân. Nhiều người đứng xem, có người đang quay clip.\n\n👉 My nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1516549655169-df83a0774514?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Kéo người bị nạn dậy và dìu ra lề đường ngay.\"},{\"id\":\"B\",\"text\":\"Giữ an toàn cho mình, nhờ người lớn xung quanh giúp, gọi 115 để cấp cứu và 113 để báo công an, giữ nguyên hiện trường.\"},{\"id\":\"C\",\"text\":\"Quay clip đăng mạng để mọi người biết.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Gọi 115 sớm, không tự di chuyển người bị nạn.\n\nCăn cứ pháp lý & Y tế: Hướng dẫn tổng đài 115 và lực lượng công an: 115 (cấp cứu y tế), 113 (công an), giữ nguyên hiện trường, không tự ý di chuyển nạn nhân khi chưa có hướng dẫn y tế tránh làm gãy xương thêm hoặc liệt tủy sống.",
                    Difficulty = "Hard",
                    Source = "Hướng dẫn cấp cứu 115 & Cục CSGT",
                    ReviewedBy = "Trung tâm Cấp cứu 115 & Lực lượng Công an (Tag: CM)",
                    Status = "Published"
                },

                // ==================== TOPIC 2: PCCC & THOÁT HIỂM (7 - 12) ====================
                new Question
                {
                    Id = GetQuestionId(7),
                    Type = "choice",
                    Title = "[Chuông báo cháy ở lớp] Cả lớp đang chơi thì chuông báo cháy kêu. Cô giáo gọi các bạn xếp hàng. Su muốn chạy lại lấy chú gấu bông để quên trên bàn.\n\n👉 Su nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1583324113626-70df0f4deaab?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Chạy lại lấy gấu bông rồi mới xếp hàng.\"},{\"id\":\"B\",\"text\":\"Nghe lời cô, xếp hàng và đi theo cô ra khu tập trung an toàn, không quay lại.\"},{\"id\":\"C\",\"text\":\"Chui xuống gầm bàn trốn.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Nghe chuông báo cháy, đi theo cô ra ngoài, không quay lại lấy đồ.\n\nCăn cứ pháp lý: Hướng dẫn thoát nạn của Cục Cảnh sát PCCC & CNCH - Bộ Công an; Điều 14 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024 cấm cản trở hoặc làm mất tác dụng lối thoát nạn.",
                    Difficulty = "Easy",
                    Source = "Điều 14 Luật PCCC và CNCH 2024",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH - Bộ Công an (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(8),
                    Type = "choice",
                    Title = "[Khói và mùi khét ở nhà] Bi (5 tuổi) đang xem hoạt hình thì thấy khói bay ra từ ổ điện gần tivi và ngửi thấy mùi khét. Bố mẹ đang ở phòng khác.\n\n👉 Bi nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Lại gần xem và dội nước vào ổ điện.\"},{\"id\":\"B\",\"text\":\"Chạy ra xa, la to báo người lớn; nếu không có ai thì ra khỏi nhà và nhờ hàng xóm gọi 114.\"},{\"id\":\"C\",\"text\":\"Tắt tivi rồi ngồi đợi xem có cháy thật không.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Thấy khói, ngửi mùi khét: tránh xa và báo người lớn ngay.\n\nCăn cứ pháp lý: Khuyến cáo an toàn PCCC hộ gia đình: la to báo hiệu cho người xung quanh và nhờ gọi 114, không tự ý dội nước vào nguồn điện đang cháy vì nước dẫn điện gây giật tử vong.",
                    Difficulty = "Easy",
                    Source = "Khuyến cáo an toàn PCCC hộ gia đình",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(9),
                    Type = "choice",
                    Title = "[Gọi 114] Hùng (lớp 4) ở nhà với em thì thấy lửa bốc lên từ căn nhà đối diện. Chưa có người lớn nào ở gần. Hùng đang cầm điện thoại.\n\n👉 Hùng nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Gọi 113 vì đó là số của công an.\"},{\"id\":\"B\",\"text\":\"Gọi 114, nói rõ địa chỉ, cái gì đang cháy, có ai bị kẹt không, tên và số điện thoại của mình; giữ máy và làm theo hướng dẫn.\"},{\"id\":\"C\",\"text\":\"Quay video đăng mạng trước rồi mới gọi.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Cháy thì gọi 114, nói địa chỉ trước tiên.\n\nCăn cứ pháp lý: Điều 6 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024: 114 là số thống nhất toàn quốc cho báo cháy và CNCH; miễn phí toàn bộ cước gọi.",
                    Difficulty = "Medium",
                    Source = "Điều 6 Luật PCCC và CNCH 2024",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(10),
                    Type = "choice",
                    Title = "[Cháy ở chung cư] Buổi tối, Mai (lớp 5) ở tầng 12 chung cư cùng bà. Chuông báo cháy kêu, hành lang có khói loãng. Bà định bấm thang máy cho nhanh.\n\n👉 Mai nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Đi thang máy cho nhanh.\"},{\"id\":\"B\",\"text\":\"Cúi thấp, che mũi miệng bằng khăn ẩm, cùng bà đi thang bộ thoát hiểm xuống đất.\"},{\"id\":\"C\",\"text\":\"Chạy vào nhà vệ sinh, đóng cửa chờ cứu hộ.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Có cháy thì không đi thang máy; cúi thấp, đi thang bộ thoát hiểm.\n\nCăn cứ pháp lý: Cẩm nang PCCC Bộ Công an: tuyệt đối không dùng thang máy khi xảy ra hỏa hoạn (dễ mất điện kẹt lồng thang và khói lọt vào gây ngạt), không ẩn nấp trong phòng vệ sinh.",
                    Difficulty = "Medium",
                    Source = "Cẩm nang PCCC Bộ Công an",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(11),
                    Type = "choice",
                    Title = "[Bị kẹt trong nhà] Hải (lớp 8) ở tầng 3 một ngôi nhà phố cùng em. Đám cháy bùng lên ở tầng trệt, khói dày tràn lên cầu thang.\n\n👉 Hải nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1517524008697-84bbe3c3fd98?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Lao xuống cầu thang qua lớp khói.\"},{\"id\":\"B\",\"text\":\"Quay vào phòng, đóng cửa, chèn khe cửa bằng khăn hoặc chăn ướt, ra ban công hoặc cửa sổ hô to, gọi 114 báo rõ tầng và vị trí, chờ lực lượng cứu nạn.\"},{\"id\":\"C\",\"text\":\"Chui xuống gầm giường hoặc tủ quần áo.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Không còn lối thoát: chặn khói, gọi 114, ra nơi thoáng báo vị trí.\n\nCăn cứ y tế & Cứu hộ: Khuyến cáo thoát nạn nhà phố của Cục Cảnh sát PCCC: cách ly khói độc bằng vải/chăn ướt chèn khe cửa, phát tín hiệu tại ban công/cửa sổ, gọi 114 báo vị trí chính xác.",
                    Difficulty = "Hard",
                    Source = "Khuyến cáo thoát nạn nhà phố - Cục CS PCCC",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(12),
                    Type = "choice",
                    Title = "[Bạn rủ gọi 114 để trêu] Trong nhóm chat lớp, một bạn rủ Tuấn (lớp 9) gọi 114 báo “nhà bạn X đang cháy” để trêu cho vui, và nói chẳng ai biết là mình gọi.\n\n👉 Tuấn nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1534536281715-e28d76689b4d?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Gọi một lần cho vui.\"},{\"id\":\"B\",\"text\":\"Từ chối, nói rõ không nên làm; nếu bạn vẫn rủ thì báo thầy cô hoặc cha mẹ.\"},{\"id\":\"C\",\"text\":\"Nhờ bạn khác gọi hộ để mình không dính vào.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Chỉ gọi 114 khi có cháy hoặc tình huống cứu nạn thật.\n\nCăn cứ pháp lý: Điều 14 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024: nghiêm cấm báo cháy giả, báo tin cứu nạn cứu hộ giả; người vi phạm sẽ bị xử phạt vi phạm hành chính hoặc truy cứu trách nhiệm hình sự.",
                    Difficulty = "Hard",
                    Source = "Điều 14 Luật PCCC và CNCH 2024",
                    ReviewedBy = "Cục Cảnh sát PCCC & CNCH (Tag: VB)",
                    Status = "Published"
                },

                // ==================== TOPIC 3: SƠ CẤP CỨU BAN ĐẦU (13 - 18) ====================
                new Question
                {
                    Id = GetQuestionId(13),
                    Type = "choice",
                    Title = "[Ông bị ngã] Ông của Tít ngã ở sân, nằm im và không trả lời khi Tít gọi. Trong nhà chỉ có Tít và ông. Điện thoại của mẹ để trên bàn.\n\n👉 Tít nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Chạy đi chơi và chờ ông tự dậy.\"},{\"id\":\"B\",\"text\":\"Gọi to nhờ hàng xóm; nếu biết dùng điện thoại thì gọi 115 và nói: “Ông con bị ngã, không trả lời. Nhà con ở số…, đường…”\"},{\"id\":\"C\",\"text\":\"Lay mạnh và kéo ông ngồi dậy.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Thấy người nằm im không trả lời: gọi người lớn và gọi 115.\n\nCăn cứ y tế: Quy trình tiếp nhận cấp cứu 115: gọi báo vị trí chính xác, tình trạng tri giác của nạn nhân, giữ liên lạc để nhận hướng dẫn sơ cấp cứu từ điều phối viên y tế. Tuyệt đối không lay giật mạnh người ngã.",
                    Difficulty = "Easy",
                    Source = "Quy trình tiếp nhận cấp cứu 115",
                    ReviewedBy = "Trung tâm Cấp cứu 115 (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(14),
                    Type = "choice",
                    Title = "[Trầy đầu gối] Mộc (6 tuổi) ngã xe đạp, trầy đầu gối, chảy ít máu và có dính đất cát. Mộc định lấy lá cây đắp lên cho mau lành.\n\n👉 Mộc nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1579684385127-1ef15d508118?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Đắp lá cây hoặc bôi nước mắm lên vết thương.\"},{\"id\":\"B\",\"text\":\"Báo người lớn, rửa nhẹ bằng nước sạch, dùng gạc hoặc khăn sạch ép lên vết thương cho tới khi máu ngừng chảy rồi băng lại.\"},{\"id\":\"C\",\"text\":\"Lấy tay chà mạnh cho sạch đất.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Trầy xước: báo người lớn, rửa nước sạch, ép gạc sạch, băng lại.\n\nCăn cứ y tế: Hướng dẫn sơ cứu ngoại khoa cơ bản (Vinmec): rửa sạch bằng nước sinh hoạt sạch, dùng gạc vô khuẩn ép nhẹ cầm máu, không đắp lá cây dân gian tránh uốn ván hoặc nhiễm trùng máu.",
                    Difficulty = "Easy",
                    Source = "Hướng dẫn sơ cứu ngoại khoa cơ bản - Vinmec",
                    ReviewedBy = "BS. CKII Lê Hoàng Minh (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(15),
                    Type = "choice",
                    Title = "[Chảy máu cam] Giờ ra chơi, quả bóng đập vào mũi Nam khiến máu chảy ra. Một bạn bảo Nam ngửa đầu lên trời cho máu khỏi chảy.\n\n👉 Nam nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1584515933487-779824d29309?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Ngửa đầu ra sau và nằm xuống.\"},{\"id\":\"B\",\"text\":\"Ngồi thẳng, hơi cúi đầu về phía trước, bóp chặt phần mềm hai cánh mũi khoảng 10 phút, thở bằng miệng và không nhả tay ra kiểm tra liên tục.\"},{\"id\":\"C\",\"text\":\"Nhét giấy thật sâu vào mũi.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Chảy máu cam: ngồi, cúi đầu về trước, bóp cánh mũi 10 phút.\n\nCăn cứ y tế: Phác đồ sơ cứu chảy máu mũi Nhi khoa: cúi nhẹ đầu về phía trước tránh nuốt máu gây nôn/sặc phổi, bóp chặt cánh mũi liên tục 10-15 phút.",
                    Difficulty = "Medium",
                    Source = "Phác đồ sơ cứu chảy máu mũi Nhi khoa",
                    ReviewedBy = "Bệnh viện Nhi Đồng (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(16),
                    Type = "choice",
                    Title = "[Bỏng nước sôi] Lan (lớp 5) bị nước nóng từ phích đổ vào mu bàn tay, da đỏ và rát. Một bạn khuyên chườm đá hoặc bôi kem đánh răng cho nhanh mát.\n\n👉 Lan nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Chườm đá lạnh lên vết bỏng.\"},{\"id\":\"B\",\"text\":\"Báo người lớn, đưa tay dưới vòi nước mát sạch chảy nhẹ khoảng 15–20 phút, rồi che bằng gạc hoặc vải sạch; đi khám nếu da rộp nhiều.\"},{\"id\":\"C\",\"text\":\"Bôi kem đánh răng hoặc nước mắm lên vết bỏng.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Bỏng: xả nước mát chảy nhẹ 15–20 phút; không đá lạnh, không kem đánh răng, không nước mắm.\n\nCăn cứ y tế: Khuyến cáo sơ cứu bỏng (BV ĐH Y Dược TP.HCM, BV Nhi Trung ương): ngâm xả dưới vòi nước mát sạch 15-20 phút, không dùng đá lạnh gây bỏng nhiệt âm, không bôi chất lạ gây nhiễm khuẩn hoại tử da.",
                    Difficulty = "Medium",
                    Source = "Khuyến cáo sơ cứu bỏng - BV ĐH Y Dược & BV Nhi TW",
                    ReviewedBy = "BS. Trần Thị B - BV 115 (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(17),
                    Type = "choice",
                    Title = "[Bạn bị nghẹn] Ở căng-tin, Phúc (lớp 7) đang ăn thạch thì ôm cổ, không nói được, không ho được và mặt tím tái.\n\n👉 Em nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1579684385127-1ef15d508118?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Bắt Phúc uống thật nhiều nước.\"},{\"id\":\"B\",\"text\":\"Nhờ người lớn gọi 115 và gọi thầy cô ngay. Nếu đã được tập huấn: đứng sau Phúc, vòng tay ôm ngang bụng, một tay nắm đấm đặt ở vùng thượng vị (trên rốn, dưới xương ức), ấn mạnh vào trong và lên trên 5 lần, lặp lại cho tới khi dị vật ra hoặc người cấp cứu tới.\"},{\"id\":\"C\",\"text\":\"Dốc ngược Phúc và dùng tay móc họng.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Thủ thuật Heimlich: Chỉ can thiệp khi bạn không ho được, không thở được; luôn gọi người lớn và 115.\n\nCăn cứ y tế: Hướng dẫn thủ thuật Heimlich xử trí tắc nghẽn đường thở do dị vật (BV Nhi Trung ương): chỉ áp dụng khi nạn nhân tắc thở hoàn toàn; tuyệt đối không móc họng mù vì dễ đẩy dị vật lọt sâu hơn.",
                    Difficulty = "Hard",
                    Source = "Hướng dẫn thủ thuật Heimlich - BV Nhi Trung ương",
                    ReviewedBy = "Khoa Cấp cứu BV Nhi Trung ương (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(18),
                    Type = "choice",
                    Title = "[Bạn ngã đập đầu] Giờ thể dục, Dũng ngã đập đầu xuống nền sân, vẫn tỉnh và bảo “không sao”. Một lúc sau Dũng nôn và kêu chóng mặt, nhức đầu.\n\n👉 Em nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1516549655169-df83a0774514?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Cho Dũng ngồi dậy chơi tiếp vì bạn nói không sao.\"},{\"id\":\"B\",\"text\":\"Báo thầy cô ngay, để Dũng nằm hoặc ngồi yên, nhờ thầy cô hoặc nhân viên y tế gọi 115 hoặc đưa đi viện; tiếp tục theo dõi các dấu hiệu nôn, nhức đầu, lơ mơ.\"},{\"id\":\"C\",\"text\":\"Cho Dũng uống thuốc giảm đau rồi để bạn ngủ.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Ngã đập đầu rồi nôn, nhức đầu, lơ mơ: báo người lớn và cấp cứu ngay.\n\nCăn cứ y tế: Phác đồ chấn thương sọ não kín: nôn ói, chóng mặt, lơ mơ là dấu hiệu cảnh báo tụ máu ngoài màng cứng hoặc chấn thương tiến triển cần nhập viện kiểm tra CT/theo dõi y khoa lập tức.",
                    Difficulty = "Hard",
                    Source = "Phác đồ chấn thương sọ não kín - Hội Phẫu thuật Thần kinh",
                    ReviewedBy = "BS. CKII Ngoại Thần kinh (Tag: CM)",
                    Status = "Published"
                },

                // ==================== TOPIC 4: AN TOÀN THÂN THỂ & MẠNG (19 - 24) ====================
                new Question
                {
                    Id = GetQuestionId(19),
                    Type = "choice",
                    Title = "[Bạn giật đồ chơi và đánh] Bi đang xếp hình thì Tèo giật mất khối xếp hình rồi đánh vào tay Bi.\n\n👉 Bi nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1502086223501-7ea6ecd79368?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Đánh lại Tèo cho bõ tức.\"},{\"id\":\"B\",\"text\":\"Nói to “Dừng lại! Đừng đánh mình!” rồi đi tìm cô giáo kể lại.\"},{\"id\":\"C\",\"text\":\"Im lặng khóc và không kể với ai.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Bị đánh thì nói “dừng lại” và đi tìm cô giáo.\n\nCăn cứ pháp lý: Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP; nguyên tắc ứng phó xâm phạm cơ thể dành cho mầm non (nói dừng lại - giữ khoảng cách an toàn - báo cô giáo).",
                    Difficulty = "Easy",
                    Source = "Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP",
                    ReviewedBy = "Cục Trẻ em - Bộ LĐTB&XH (Tag: VB_CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(20),
                    Type = "choice",
                    Title = "[Người lớn làm con đau và dặn giữ bí mật] Một người lớn trong nhà đánh Bống rất đau, rồi dặn: “Đừng kể với ai nhé”.\n\n👉 Bống nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1485546246426-74dc88dec4d9?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Giữ bí mật vì sợ bị đánh tiếp.\"},{\"id\":\"B\",\"text\":\"Kể với một người lớn mà con tin như ông bà, cô giáo, hoặc nhờ họ gọi 111.\"},{\"id\":\"C\",\"text\":\"Tự bỏ ra ngoài đi chơi xa.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Con có quyền được an toàn; bí mật làm con đau thì phải kể cho người lớn con tin tưởng.\n\nCăn cứ pháp lý: Khoản 6 Điều 4, Điều 6 Luật Trẻ em 2016; Điều 22 Nghị định 130/2021/NĐ-CP; Luật Phòng, chống bạo lực gia đình 2022 (Tổng đài Quốc gia Bảo vệ Trẻ em 111).",
                    Difficulty = "Easy",
                    Source = "Luật Trẻ em 2016 & Tổng đài Quốc gia 111",
                    ReviewedBy = "Tổng đài Quốc gia Bảo vệ Trẻ em 111 (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(21),
                    Type = "choice",
                    Title = "[Biệt danh và bị cô lập] Một nhóm bạn gọi Hà (lớp 4) bằng biệt danh chê bề ngoài và rủ cả lớp không chơi với Hà. Hà buồn và không muốn đến lớp.\n\n👉 Hà nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Im lặng chịu đựng, hy vọng các bạn sẽ chán.\"},{\"id\":\"B\",\"text\":\"Kể với cha mẹ hoặc cô giáo chủ nhiệm để được giúp, và tìm những bạn thân đáng tin.\"},{\"id\":\"C\",\"text\":\"Gọi lại các bạn bằng biệt danh khác để trả đũa.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Bị chê bai, bị cô lập cũng là bắt nạt; hãy kể với người lớn.\n\nCăn cứ pháp lý: Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP: bạo lực học đường bao gồm lăng mạ, xúc phạm danh dự, nhân phẩm, cô lập, tẩy chay gây tổn hại tinh thần nghiêm trọng.",
                    Difficulty = "Medium",
                    Source = "Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP",
                    ReviewedBy = "Vụ Giáo dục Chính trị và CTHSSV - Bộ GD&ĐT (Tag: VB)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(22),
                    Type = "choice",
                    Title = "[Chứng kiến bạn bị đánh] Ở góc sân, Quân thấy hai bạn lớn hơn đẩy và đánh Tín. Mấy bạn xung quanh đứng xem và cười.\n\n👉 Quân nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Đứng xem cùng và cười theo.\"},{\"id\":\"B\",\"text\":\"Không xông vào một mình mà chạy đi báo thầy cô hoặc bảo vệ ngay, sau đó hỏi thăm Tín.\"},{\"id\":\"C\",\"text\":\"Lao vào đánh hai bạn kia để bảo vệ Tín.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Chứng kiến bạo lực: báo người lớn ngay, không cổ vũ, không đánh trả.\n\nCăn cứ pháp lý: Quy chế phòng ngừa bạo lực học đường theo Nghị định 80/2017/NĐ-CP: nghiêm cấm hành vi cổ vũ, quay video tiếp tay, yêu cầu can thiệp an toàn bằng cách báo ngay giám thị/giáo viên.",
                    Difficulty = "Medium",
                    Source = "Quy chế phòng ngừa bạo lực học đường NĐ 80/2017",
                    ReviewedBy = "Phòng Công tác Học sinh Sinh viên (Tag: CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(23),
                    Type = "choice",
                    Title = "[Bị bắt nạt trên mạng] Một nhóm bạn lập nhóm chat và đăng ảnh chế giễu My (lớp 8), kèm lời lẽ xúc phạm. Nhiều bạn thả cười và chia sẻ lại.\n\n👉 My nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Đăng bài chửi lại để trả đũa.\"},{\"id\":\"B\",\"text\":\"Không đáp trả; chụp màn hình lưu bằng chứng (nội dung, tên tài khoản, thời gian); chặn và báo cáo nội dung trên nền tảng; kể với cha mẹ hoặc thầy cô; có thể gọi 111 để được tư vấn.\"},{\"id\":\"C\",\"text\":\"Xóa hết tin nhắn, tắt điện thoại và không kể với ai.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Bị bắt nạt trên mạng: lưu bằng chứng, không đáp trả, kể với người lớn.\n\nCăn cứ pháp lý: Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP; Hướng dẫn an toàn trên không gian mạng của Tổng đài 111 và Cục Trẻ em. Giữ bằng chứng là yếu tố cốt lõi để nhà trường và cơ quan chức năng xử lý.",
                    Difficulty = "Hard",
                    Source = "Nghị định 80/2017/NĐ-CP & Cục Trẻ em",
                    ReviewedBy = "Cục An toàn Thông tin & Tổng đài 111 (Tag: VB_CM)",
                    Status = "Published"
                },
                new Question
                {
                    Id = GetQuestionId(24),
                    Type = "choice",
                    Title = "[Bị rủ vây đánh một bạn] Nhóm bạn lớp 9 (14–15 tuổi) rủ Long “dằn mặt” một bạn bằng cách vây đánh sau giờ học và quay clip đăng mạng.\n\n👉 Long nên làm gì?",
                    ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=800&q=80",
                    Payload = "{\"options\":[{\"id\":\"A\",\"text\":\"Tham gia cho khỏi bị coi là nhát.\"},{\"id\":\"B\",\"text\":\"Từ chối, khuyên các bạn dừng lại và báo thầy cô hoặc cha mẹ để ngăn trước khi sự việc xảy ra.\"},{\"id\":\"C\",\"text\":\"Không đánh nhưng đứng quay clip.\"}]}",
                    CorrectData = "{\"correctOptionId\":\"B\"}",
                    Explanation = "Bài học: Từ chối tham gia và báo người lớn; đánh bạn là hành vi vi phạm pháp luật nghiêm trọng.\n\nCăn cứ pháp lý: Điều 5 Luật Xử lý vi phạm hành chính 2012; Điều 12 và Điều 134 Bộ luật Hình sự 2015 (sửa đổi 2017): người từ đủ 14 đến dưới 16 tuổi chịu trách nhiệm hành chính và hình sự về tội cố ý gây thương tích rất nghiêm trọng hoặc đặc biệt nghiêm trọng. Người đứng quay clip cũng là đồng phạm tiếp tay phát tán thông tin độc hại.",
                    Difficulty = "Hard",
                    Source = "Điều 12 & 134 Bộ luật Hình sự 2015 (sửa đổi 2017)",
                    ReviewedBy = "Luật sư Đoàn Luật sư TP.HCM (Tag: VB)",
                    Status = "Published"
                }
            };
        }

        public static List<NodeQuestion> GetNodeQuestions()
        {
            var list = new List<NodeQuestion>();

            // Node 1 (Sơ cứu 1): Q13, Q14, Q15
            list.Add(new NodeQuestion { NodeId = Node1Id, QuestionId = GetQuestionId(13), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node1Id, QuestionId = GetQuestionId(14), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node1Id, QuestionId = GetQuestionId(15), Order = 3 });

            // Node 2 (Sơ cứu 2): Q16, Q17, Q18
            list.Add(new NodeQuestion { NodeId = Node2Id, QuestionId = GetQuestionId(16), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node2Id, QuestionId = GetQuestionId(17), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node2Id, QuestionId = GetQuestionId(18), Order = 3 });

            // Node 3 (PCCC 1): Q7, Q8, Q9
            list.Add(new NodeQuestion { NodeId = Node3Id, QuestionId = GetQuestionId(7), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node3Id, QuestionId = GetQuestionId(8), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node3Id, QuestionId = GetQuestionId(9), Order = 3 });

            // Node 4 (PCCC 2): Q10, Q11, Q12
            list.Add(new NodeQuestion { NodeId = Node4Id, QuestionId = GetQuestionId(10), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node4Id, QuestionId = GetQuestionId(11), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node4Id, QuestionId = GetQuestionId(12), Order = 3 });

            // Node 5 (Giao thông 1): Q1, Q2, Q3
            list.Add(new NodeQuestion { NodeId = Node5Id, QuestionId = GetQuestionId(1), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node5Id, QuestionId = GetQuestionId(2), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node5Id, QuestionId = GetQuestionId(3), Order = 3 });

            // Node 6 (Giao thông 2): Q4, Q5, Q6
            list.Add(new NodeQuestion { NodeId = Node6Id, QuestionId = GetQuestionId(4), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node6Id, QuestionId = GetQuestionId(5), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node6Id, QuestionId = GetQuestionId(6), Order = 3 });

            // Node 7 (An toàn thân thể 1): Q19, Q20, Q21
            list.Add(new NodeQuestion { NodeId = Node7Id, QuestionId = GetQuestionId(19), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node7Id, QuestionId = GetQuestionId(20), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node7Id, QuestionId = GetQuestionId(21), Order = 3 });

            // Node 8 (An toàn mạng 2): Q22, Q23, Q24
            list.Add(new NodeQuestion { NodeId = Node8Id, QuestionId = GetQuestionId(22), Order = 1 });
            list.Add(new NodeQuestion { NodeId = Node8Id, QuestionId = GetQuestionId(23), Order = 2 });
            list.Add(new NodeQuestion { NodeId = Node8Id, QuestionId = GetQuestionId(24), Order = 3 });

            return list;
        }
    }
}
