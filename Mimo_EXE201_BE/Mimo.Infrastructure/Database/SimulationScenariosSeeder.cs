using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mimo.DomainLayer.Entities;

namespace Mimo.Infrastructure.Database
{
    public static class SimulationScenariosSeeder
    {
        public static readonly Guid TrafficDomainId = Guid.Parse("a1111111-0001-0000-0000-000000000001");
        public static readonly Guid FireDomainId = Guid.Parse("a1111111-0002-0000-0000-000000000002");
        public static readonly Guid FirstAidDomainId = Guid.Parse("a1111111-1111-1111-1111-111111111111"); // Kept DefaultDomainId
        public static readonly Guid SchoolSafetyDomainId = Guid.Parse("a1111111-0004-0000-0000-000000000004");

        public static async Task SeedSimulationScenariosAsync(MimoDbContext context, ILogger logger)
        {
            try
            {
                // 1. Seed 4 Domains
                var domains = new List<Domain>
                {
                    new Domain
                    {
                        Id = TrafficDomainId,
                        Name = "An toàn giao thông",
                        Slug = "traffic-safety",
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/3097/3097180.png",
                        Order = 1,
                        Status = "Active"
                    },
                    new Domain
                    {
                        Id = FireDomainId,
                        Name = "Hỏa hoạn và Thoát hiểm",
                        Slug = "fire-safety",
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/785/785116.png",
                        Order = 2,
                        Status = "Active"
                    },
                    new Domain
                    {
                        Id = FirstAidDomainId,
                        Name = "Sơ cấp cứu ban đầu (First Aid)",
                        Slug = "first-aid",
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/2966/2966327.png",
                        Order = 3,
                        Status = "Active"
                    },
                    new Domain
                    {
                        Id = SchoolSafetyDomainId,
                        Name = "Phòng chống bạo lực học đường & gia đình",
                        Slug = "school-and-home-safety",
                        IconUrl = "https://cdn-icons-png.flaticon.com/512/1041/1041883.png",
                        Order = 4,
                        Status = "Active"
                    }
                };

                foreach (var domain in domains)
                {
                    var existing = await context.Domains.FirstOrDefaultAsync(d => d.Id == domain.Id || d.Slug == domain.Slug);
                    if (existing == null)
                    {
                        await context.Domains.AddAsync(domain);
                        logger.LogInformation("Seeded Domain: {DomainName}", domain.Name);
                    }
                    else
                    {
                        existing.Name = domain.Name;
                        existing.IconUrl = domain.IconUrl;
                        existing.Order = domain.Order;
                        existing.Status = "Active";
                    }
                }
                await context.SaveChangesAsync();

                // 2. Seed 12 Units (3 units per domain for Preschool, Primary, Secondary)
                var units = GetUnitsDefinition();
                foreach (var unit in units)
                {
                    var existing = await context.Units.FirstOrDefaultAsync(u => u.Id == unit.Id);
                    if (existing == null)
                    {
                        await context.Units.AddAsync(unit);
                        logger.LogInformation("Seeded Unit: {UnitTitle}", unit.Title);
                    }
                    else
                    {
                        existing.Title = unit.Title;
                        existing.Description = unit.Description;
                        existing.Order = unit.Order;
                        existing.DomainId = unit.DomainId;
                    }
                }
                await context.SaveChangesAsync();

                // 3. Seed 24 Nodes (2 nodes per unit)
                var nodes = GetNodesDefinition();
                foreach (var node in nodes)
                {
                    var existing = await context.Nodes.FirstOrDefaultAsync(n => n.Id == node.Id);
                    if (existing == null)
                    {
                        await context.Nodes.AddAsync(node);
                        logger.LogInformation("Seeded Node: {NodeTitle}", node.Title);
                    }
                    else
                    {
                        existing.Title = node.Title;
                        existing.Type = node.Type;
                        existing.Order = node.Order;
                        existing.XpReward = node.XpReward;
                        existing.Status = "Published";
                        existing.UnitId = node.UnitId;
                    }
                }
                await context.SaveChangesAsync();

                // 4. Seed 24 Questions
                var questions = GetQuestionsDefinition();
                foreach (var q in questions)
                {
                    var existing = await context.Questions.FirstOrDefaultAsync(x => x.Id == q.Id);
                    if (existing == null)
                    {
                        await context.Questions.AddAsync(q);
                        logger.LogInformation("Seeded Question: {QuestionTitle}", q.Title);
                    }
                    else
                    {
                        existing.Title = q.Title;
                        existing.Type = q.Type;
                        existing.Payload = q.Payload;
                        existing.CorrectData = q.CorrectData;
                        existing.Explanation = q.Explanation;
                        existing.Difficulty = q.Difficulty;
                        existing.Source = q.Source;
                        existing.ReviewedBy = q.ReviewedBy;
                        existing.ImageUrl = q.ImageUrl;
                        existing.Status = "Published";
                    }
                }
                await context.SaveChangesAsync();

                // 5. Link 24 Questions to 24 Nodes (1 Question per Node)
                for (int i = 1; i <= 24; i++)
                {
                    var nodeId = GetNodeGuid(i);
                    var questionId = GetQuestionGuid(i);

                    var existsLink = await context.NodeQuestions.AnyAsync(nq => nq.NodeId == nodeId && nq.QuestionId == questionId);
                    if (!existsLink)
                    {
                        await context.NodeQuestions.AddAsync(new NodeQuestion
                        {
                            NodeId = nodeId,
                            QuestionId = questionId,
                            Order = 1
                        });
                    }
                }
                await context.SaveChangesAsync();

                logger.LogInformation(">>> SEEDING HOÀN TẤT THÀNH CÔNG 24 TÌNH HUỐNG MÔ PHỎNG (4 CHỦ ĐỀ x 3 NHÓM TUỔI) <<<");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi xảy ra trong quá trình Seeding 24 tình huống mô phỏng: {Message}", ex.Message);
            }
        }

        public static Guid GetUnitGuid(int unitIndex) =>
            Guid.Parse($"b1111111-0000-0000-0000-{unitIndex:D12}");

        public static Guid GetNodeGuid(int nodeIndex) =>
            Guid.Parse($"c1111111-0000-0000-0000-{nodeIndex:D12}");

        public static Guid GetQuestionGuid(int questionIndex) =>
            Guid.Parse($"d1111111-0000-0000-0000-{questionIndex:D12}");

        private static List<Unit> GetUnitsDefinition()
        {
            return new List<Unit>
            {
                // Domain 1: Traffic Safety (Units 1-3)
                new Unit { Id = GetUnitGuid(1), DomainId = TrafficDomainId, Title = "Giao thông: Mầm non (3–6 tuổi)", Description = "Kỹ năng cơ bản qua đường và an toàn khi ngồi ô tô cùng cha mẹ.", Order = 1 },
                new Unit { Id = GetUnitGuid(2), DomainId = TrafficDomainId, Title = "Giao thông: Tiểu học (6–11 tuổi)", Description = "Đội mũ bảo hiểm đúng cách và phản xạ khi gặp nguy hiểm trên đường.", Order = 2 },
                new Unit { Id = GetUnitGuid(3), DomainId = TrafficDomainId, Title = "Giao thông: THCS (11–15 tuổi)", Description = "Quy định điều khiển xe máy điện và kỹ năng ứng cứu khi chứng kiến tai nạn.", Order = 3 },

                // Domain 2: Fire Safety (Units 4-6)
                new Unit { Id = GetUnitGuid(4), DomainId = FireDomainId, Title = "Hỏa hoạn: Mầm non (3–6 tuổi)", Description = "Phản xạ khi nghe chuông báo cháy và nhận biết khói khét trong nhà.", Order = 1 },
                new Unit { Id = GetUnitGuid(5), DomainId = FireDomainId, Title = "Hỏa hoạn: Tiểu học (6–11 tuổi)", Description = "Kỹ năng gọi 114 báo cháy và thoát nạn an toàn ở chung cư cao tầng.", Order = 2 },
                new Unit { Id = GetUnitGuid(6), DomainId = FireDomainId, Title = "Hỏa hoạn: THCS (11–15 tuổi)", Description = "Biện pháp phòng ngừa khi bị kẹt trong nhà cháy và nhận thức về báo cháy giả.", Order = 3 },

                // Domain 3: First Aid (Units 7-9)
                new Unit { Id = GetUnitGuid(7), DomainId = FirstAidDomainId, Title = "Sơ cấp cứu: Mầm non (3–6 tuổi)", Description = "Báo động người lớn khi có người ngã và xử lý trầy xước nhẹ.", Order = 1 },
                new Unit { Id = GetUnitGuid(8), DomainId = FirstAidDomainId, Title = "Sơ cấp cứu: Tiểu học (6–11 tuổi)", Description = "Sơ cứu chuẩn y khoa khi chảy máu cam và bỏng nước sôi sinh hoạt.", Order = 2 },
                new Unit { Id = GetUnitGuid(9), DomainId = FirstAidDomainId, Title = "Sơ cấp cứu: THCS (11–15 tuổi)", Description = "Thủ thuật cấp cứu hóc dị vật Heimlich và theo dõi chấn thương đầu.", Order = 3 },

                // Domain 4: School & Home Safety (Units 10-12)
                new Unit { Id = GetUnitGuid(10), DomainId = SchoolSafetyDomainId, Title = "An toàn học đường: Mầm non (3–6 tuổi)", Description = "Cách ứng xử khi bị bạn tranh giành đánh bạn và bảo vệ quyền an toàn cá nhân.", Order = 1 },
                new Unit { Id = GetUnitGuid(11), DomainId = SchoolSafetyDomainId, Title = "An toàn học đường: Tiểu học (6–11 tuổi)", Description = "Phòng chống bắt nạt tinh thần, tẩy chay và ứng xử khi chứng kiến bạn bị đánh.", Order = 2 },
                new Unit { Id = GetUnitGuid(12), DomainId = SchoolSafetyDomainId, Title = "An toàn học đường: THCS (11–15 tuổi)", Description = "Đối phó bắt nạt trên không gian mạng và kiên quyết từ chối bạo lực tập thể.", Order = 3 }
            };
        }

        private static List<Node> GetNodesDefinition()
        {
            var nodes = new List<Node>();

            // Helper to add node
            void AddNode(int index, int unitIndex, string title, int order, int xp = 10)
            {
                nodes.Add(new Node
                {
                    Id = GetNodeGuid(index),
                    UnitId = GetUnitGuid(unitIndex),
                    Title = title,
                    Type = "lesson",
                    Order = order,
                    XpReward = xp,
                    Status = "Published"
                });
            }

            // Domain 1: Traffic Safety
            AddNode(1, 1, "Tình huống 1.1: Qua đường cùng mẹ", 1);
            AddNode(2, 1, "Tình huống 1.2: Ngồi ô tô cùng bố", 2);
            AddNode(3, 2, "Tình huống 1.3: Mũ bảo hiểm khi đi gần", 1);
            AddNode(4, 2, "Tình huống 1.4: Quả bóng lăn ra đường", 2);
            AddNode(5, 3, "Tình huống 1.5: Mượn xe máy điện của anh", 1);
            AddNode(6, 3, "Tình huống 1.6: Chứng kiến một vụ tai nạn", 2);

            // Domain 2: Fire Safety
            AddNode(7, 4, "Tình huống 2.1: Chuông báo cháy ở lớp", 1);
            AddNode(8, 4, "Tình huống 2.2: Khói và mùi khét ở nhà", 2);
            AddNode(9, 5, "Tình huống 2.3: Kỹ năng gọi 114 báo cháy", 1);
            AddNode(10, 5, "Tình huống 2.4: Thoát nạn cháy ở chung cư", 2);
            AddNode(11, 6, "Tình huống 2.5: Bị kẹt trong nhà phố cháy", 1);
            AddNode(12, 6, "Tình huống 2.6: Nhận thức về gọi 114 trêu đùa", 2);

            // Domain 3: First Aid
            AddNode(13, 7, "Tình huống 3.1: Khi ông bị ngã bất tỉnh", 1);
            AddNode(14, 7, "Tình huống 3.2: Xử lý trầy xước đầu gối", 2);
            AddNode(15, 8, "Tình huống 3.3: Sơ cứu chảy máu cam", 1);
            AddNode(16, 8, "Tình huống 3.4: Bỏng nước sôi sinh hoạt", 2);
            AddNode(17, 9, "Tình huống 3.5: Cấp cứu hóc dị vật căng-tin", 1);
            AddNode(18, 9, "Tình huống 3.6: Ngã đập đầu chấn thương", 2);

            // Domain 4: School & Home Safety
            AddNode(19, 10, "Tình huống 4.1: Bạn giật đồ chơi và đánh", 1);
            AddNode(20, 10, "Tình huống 4.2: Người lớn làm đau và dặn giữ bí mật", 2);
            AddNode(21, 11, "Tình huống 4.3: Biệt danh xấu và bị cô lập", 1);
            AddNode(22, 11, "Tình huống 4.4: Chứng kiến bạn bị vây đánh", 2);
            AddNode(23, 12, "Tình huống 4.5: Bị bắt nạt bôi nhọ trên mạng", 1);
            AddNode(24, 12, "Tình huống 4.6: Bị rủ vây đánh người khác", 2);

            return nodes;
        }

        private static List<Question> GetQuestionsDefinition()
        {
            var list = new List<Question>();

            // Helper tạo JSON Payload gọn gàng & đồng bộ
            string CreatePayload(string scenario, string question, string audioScript,
                string optAId, string optAText, string optAFeedback,
                string optBId, string optBText, string optBFeedback,
                string optCId, string optCText, string optCFeedback)
            {
                var obj = new
                {
                    scenario,
                    question,
                    audioScript,
                    options = new[]
                    {
                        new { id = optAId, text = optAText, feedback = optAFeedback },
                        new { id = optBId, text = optBText, feedback = optBFeedback },
                        new { id = optCId, text = optCText, feedback = optCFeedback }
                    }
                };
                return JsonSerializer.Serialize(obj);
            }

            // =========================================================================
            // CHỦ ĐỀ 1: AN TOÀN GIAO THÔNG
            // =========================================================================

            // 1.1 [Mầm non] Qua đường cùng mẹ - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(1),
                Type = "choice",
                Title = "Na (5 tuổi) đi học về cùng mẹ, đến vạch qua đường thì đèn đang đỏ. Bên kia đường có bạn vẫy gọi Na. Na nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1576765608535-5f04d1e3f289?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Na (5 tuổi) đi học về cùng mẹ. Đến vạch dành cho người đi bộ thì đèn đang đỏ. Bên kia đường có bạn vẫy tay gọi Na.",
                    "Na nên làm gì?",
                    "Đèn đang đỏ và có bạn gọi bên kia đường, bé Na nên làm gì?",
                    "A", "Buông tay mẹ, chạy sang gặp bạn.", "Sai. Xe có thể tới rất nhanh, bé nhỏ không tự qua đường được.",
                    "B", "Nắm chặt tay mẹ, chờ đèn xanh rồi cùng đi qua.", "Đúng. Có người lớn dắt và chờ đúng tín hiệu cho người đi bộ.",
                    "C", "Chờ đường vắng xe rồi tự chạy qua.", "Sai. Xe ở xa vẫn tới rất nhanh, Na chưa đủ lớn để tự qua đường."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Qua đường phải nắm tay người lớn và chờ đèn xanh.",
                Source = "Điều 30 Luật Trật tự, an toàn giao thông đường bộ 2024: trẻ em dưới 7 tuổi khi qua đường phải có người lớn dẫn dắt; mọi người có trách nhiệm giúp trẻ khi qua đường. [VB]",
                ReviewedBy = "Ủy ban An toàn giao thông Quốc gia",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 1.2 [Mầm non] Ngồi ô tô cùng bố - Shuffled Correct: A (hoán đổi B lên A)
            list.Add(new Question
            {
                Id = GetQuestionGuid(2),
                Type = "choice",
                Title = "Bin (5 tuổi, cao 1,1m) được bố đón bằng ô tô. Bin đòi ngồi ghế trước cạnh bố. Bin nên ngồi ở đâu?",
                ImageUrl = "https://images.unsplash.com/photo-1517524008697-84bbe3c3fd98?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Bin (5 tuổi, cao khoảng 1,1 m) được bố đón bằng ô tô gia đình. Bin đòi ngồi ghế phía trước cạnh bố “cho giống người lớn”.",
                    "Bin nên ngồi ở đâu?",
                    "Bé Bin 5 tuổi ngồi trên xe ô tô thì nên ngồi ở vị trí nào?",
                    "A", "Hàng ghế sau, ngồi ghế dành cho trẻ em và thắt dây an toàn.", "Đúng. Hàng ghế sau an toàn nhất và đáp ứng quy định bảo vệ trẻ nhỏ.",
                    "B", "Ghế trước cạnh bố, vì bố lái chậm.", "Sai. Ghế trước nguy hiểm hơn cho trẻ nhỏ khi phanh gấp hoặc va chạm, và luật cấm trẻ nhỏ ngồi cùng hàng ghế người lái.",
                    "C", "Ngồi hàng ghế sau nhưng quỳ nhìn ra cửa sổ cho thoáng.", "Sai. Quỳ hoặc đứng thì dây và ghế không bảo vệ được khi xe phanh gấp."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Bé ngồi hàng ghế sau, đúng ghế trẻ em, thắt dây an toàn.",
                Source = "Khoản 3 Điều 10 Luật Trật tự, an toàn giao thông đường bộ 2024 (sửa đổi bởi Luật 118/2025/QH15): trẻ dưới 10 tuổi và cao dưới 1,35m không ngồi cùng hàng ghế người lái và phải có thiết bị an toàn phù hợp. [VB]",
                ReviewedBy = "Cục Cảnh sát Giao thông - Bộ Công an",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 1.3 [Tiểu học] Mũ bảo hiểm khi đi gần - Shuffled Correct: C (hoán đổi B xuống C)
            list.Add(new Question
            {
                Id = GetQuestionGuid(3),
                Type = "choice",
                Title = "Còn vài trăm mét nữa tới trường, Minh muốn tháo mũ hoặc để quai lỏng vì gần rồi. Minh nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Minh (8 tuổi) được bố chở xe máy đến trường. Còn vài trăm mét nữa tới cổng trường, Minh muốn tháo mũ hoặc để quai lỏng vì “gần rồi, đội nóng”.",
                    "Minh nên làm gì?",
                    "Sắp đến trường rồi, Minh có nên nới lỏng hoặc tháo mũ bảo hiểm không?",
                    "A", "Tháo mũ ra vì đã gần tới trường.", "Sai. Tai nạn có thể xảy ra ở bất kỳ đoạn đường nào dù gần hay xa.",
                    "B", "Đội mũ nhưng để quai lỏng cho mát.", "Sai. Mũ không cài quai đúng quy cách coi như chưa đội, khi ngã mũ dễ văng khỏi đầu.",
                    "C", "Đội mũ vừa đầu và cài quai chắc chắn cho tới khi xuống xe.", "Đúng. Luôn đội mũ cài quai đúng quy cách trong suốt hành trình."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Đội mũ phải cài quai, kể cả khi đi quãng đường ngắn.",
                Source = "Điều 7 Nghị định 168/2024/NĐ-CP: chở người không đội mũ bảo hiểm hoặc đội mũ không cài quai đúng quy cách bị xử phạt. [VB]",
                ReviewedBy = "Ủy ban An toàn giao thông Quốc gia",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 1.4 [Tiểu học] Quả bóng lăn ra đường - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(4),
                Type = "choice",
                Title = "Quả bóng của Lan lăn ra giữa đường không có vỉa hè, từ xa có xe máy đang tới. Lan nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1516738901171-8eb4fc13bd20?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Lan (lớp 4) đi bộ về nhà trên con đường nhỏ không có vỉa hè. Quả bóng của Lan lăn ra giữa đường, từ xa có một xe máy đang chạy tới.",
                    "Lan nên làm gì?",
                    "Bóng lăn ra đường khi có xe máy đang tới, bạn Lan nên xử lý thế nào?",
                    "A", "Chạy ra nhặt ngay vì xe còn xa.", "Sai. Xe tới rất nhanh, người lái khó dừng kịp và bạn có thể gặp nguy hiểm chết người.",
                    "B", "Dừng ở mép đường, chờ xe đi qua, quan sát hai bên rồi mới nhặt, hoặc nhờ người lớn nhặt giúp.", "Đúng. Luôn ưu tiên an toàn tính mạng, chỉ nhặt khi đường hoàn toàn thông thoáng.",
                    "C", "Vừa chạy ra vừa giơ tay bắt xe dừng.", "Sai. Giơ tay không bắt buộc xe phải dừng kịp; chỉ qua đường khi đã quan sát và bảo đảm an toàn."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Nơi không có vỉa hè, đi sát mép đường bên phải; không lao ra lòng đường vì đồ vật.",
                Source = "Điều 30 Luật Trật tự, an toàn giao thông đường bộ 2024: đi bộ sát mép đường bên phải, quan sát và chỉ qua đường khi an toàn. [VB]",
                ReviewedBy = "Cục Cảnh sát Giao thông",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 1.5 [THCS] Mượn xe máy điện của anh - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(5),
                Type = "choice",
                Title = "Khoa (lớp 7, 12 tuổi) muốn mượn xe máy điện của anh để đi học vì chỉ có 2 km. Khoa nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1558981806-ec527fa84c39?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Khoa (lớp 7, 12 tuổi) muốn mượn xe máy điện của anh để đi học vì “chỉ có 2 km”. Bạn bảo rằng ít ai kiểm tra.",
                    "Khoa nên làm gì?",
                    "Học sinh lớp 7 có được điều khiển xe máy điện đến trường không?",
                    "A", "Không tự lái; nhờ người lớn đưa đón hoặc đi xe đạp, xe buýt, đợi đủ tuổi theo quy định.", "Đúng. Tuân thủ pháp luật và bảo đảm an toàn cho bản thân và người xung quanh.",
                    "B", "Chạy luôn nhưng đội mũ bảo hiểm cho an toàn.", "Sai. Đội mũ là tốt nhưng Khoa chưa đủ tuổi điều khiển, vi phạm luật và chưa có kỹ năng phản xạ.",
                    "C", "Chỉ chạy đường nhỏ, thấy công an thì rẽ đi.", "Sai. Né tránh kiểm tra không làm việc điều khiển xe trở nên an toàn hơn."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Phải đủ tuổi theo quy định mới được điều khiển xe gắn máy, kể cả xe máy điện.",
                Source = "Luật Trật tự, an toàn giao thông đường bộ 2024: người từ đủ 16 tuổi trở lên mới được điều khiển xe gắn máy, xe máy điện. [VB]",
                ReviewedBy = "Bộ Công an",
                Difficulty = "Hard",
                Status = "Published"
            });

            // 1.6 [THCS] Chứng kiến một vụ tai nạn - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(6),
                Type = "choice",
                Title = "My thấy một người ngã xe máy nằm im và chảy máu ở chân. Nhiều người đứng xem. My nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Trên đường về nhà, My (lớp 8) thấy một người đi xe máy ngã ngay trước mặt, nằm im và chảy máu ở chân. Nhiều người đứng xem, có người đang quay clip.",
                    "My nên làm gì?",
                    "Khi chứng kiến tai nạn giao thông nghiêm trọng, bạn nên phản ứng như thế nào?",
                    "A", "Kéo người bị nạn dậy và dìu ra lề đường ngay.", "Sai. Nạn nhân có thể chấn thương cột sống cổ/lưng, việc di chuyển sai cách có thể gây liệt hoặc tử vong.",
                    "B", "Quay clip đăng mạng xã hội để người nhà nạn nhân sớm biết tin.", "Sai. Không giúp được cấp cứu và xâm phạm nghiêm trọng quyền riêng tư của nạn nhân.",
                    "C", "Giữ an toàn cho mình, nhờ người lớn xung quanh giúp, gọi 115 để cấp cứu và 113 để báo công an, giữ nguyên hiện trường.", "Đúng. Gọi cứu thương sớm, báo địa chỉ chính xác và giữ nguyên hiện trường."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Gọi 115 sớm, không tự tiện di chuyển nạn nhân khi chưa có hướng dẫn y tế.",
                Source = "Hướng dẫn quy trình cấp cứu tai nạn giao thông - Tổng đài 115 và Cục Cảnh sát giao thông 113. [CM]",
                ReviewedBy = "Trung tâm Cấp cứu 115 TP.HCM",
                Difficulty = "Hard",
                Status = "Published"
            });

            // =========================================================================
            // CHỦ ĐỀ 2: HỎA HOẠN VÀ THOÁT HIỂM
            // =========================================================================

            // 2.1 [Mầm non] Chuông báo cháy ở lớp - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(7),
                Type = "choice",
                Title = "Chuông báo cháy kêu, cô giáo gọi xếp hàng. Su muốn chạy lại lấy chú gấu bông để quên. Su nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Cả lớp đang chơi thì chuông báo cháy kêu. Cô giáo gọi các bạn xếp hàng. Su muốn chạy lại lấy chú gấu bông để quên trên bàn.",
                    "Su nên làm gì?",
                    "Khi chuông báo cháy kêu ở lớp học, bé Su nên làm gì?",
                    "A", "Chạy lại lấy gấu bông rồi mới xếp hàng.", "Sai. Khói và lửa lan cực nhanh, tính mạng của bé là quan trọng nhất.",
                    "B", "Nghe lời cô, xếp hàng và đi theo cô ra khu tập trung an toàn, không quay lại.", "Đúng. Tuyệt đối nghe hiệu lệnh của giáo viên và thoát hiểm khẩn cấp.",
                    "C", "Chui xuống gầm bàn trốn.", "Sai. Trốn trong phòng kín làm cô và lính cứu hỏa khó tìm thấy bạn."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Nghe chuông báo cháy, đi theo cô ra ngoài, không quay lại lấy đồ chơi.",
                Source = "Hướng dẫn thoát nạn của Bộ Công an và Điều 14 Luật PCCC và Cứu nạn, cứu hộ 2024. [CM]",
                ReviewedBy = "Cục Cảnh sát PCCC & CNCH",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 2.2 [Mầm non] Khói và mùi khét ở nhà - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(8),
                Type = "choice",
                Title = "Bi thấy khói bay ra từ ổ điện gần tivi và ngửi thấy mùi khét. Bố mẹ đang ở phòng khác. Bi nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Bi (5 tuổi) đang xem hoạt hình thì thấy khói bay ra từ ổ điện gần tivi và ngửi thấy mùi khét. Bố mẹ đang ở phòng khác.",
                    "Bi nên làm gì?",
                    "Bé Bi thấy ổ điện bốc khói khét thì nên xử lý thế nào?",
                    "A", "Chạy ra xa, la to báo người lớn; nếu không có ai thì ra khỏi nhà và nhờ hàng xóm gọi 114.", "Đúng. Tránh xa nguồn điện chập cháy và lập tức báo động người lớn.",
                    "B", "Lại gần xem và dội nước vào ổ điện.", "Sai. Nước dẫn điện cực mạnh gây giật tử vong ngay tại chỗ.",
                    "C", "Tắt tivi rồi ngồi đợi xem có cháy thật không.", "Sai. Chần chừ sẽ làm ngạt khói độc và mất thời gian thoát thân quý giá."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Thấy khói, ngửi mùi khét: tránh xa và báo người lớn ngay.",
                Source = "Khuyến cáo an toàn PCCC hộ gia đình - Cục Cảnh sát PCCC & CNCH. [CM]",
                ReviewedBy = "Cục Cảnh sát PCCC & CNCH",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 2.3 [Tiểu học] Gọi 114 - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(9),
                Type = "choice",
                Title = "Hùng thấy lửa bốc lên từ nhà đối diện, chưa có người lớn ở gần và đang cầm điện thoại. Hùng nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Hùng (lớp 4) ở nhà với em thì thấy lửa bốc lên từ căn nhà đối diện. Chưa có người lớn nào ở gần. Hùng đang cầm điện thoại.",
                    "Hùng nên làm gì?",
                    "Phát hiện đám cháy bùng lên, em cần gọi đến số điện thoại khẩn cấp nào?",
                    "A", "Gọi 113 vì đó là số của công an trực ban.", "Sai. Số 113 là cảnh sát trật tự, báo cháy cứu hộ chuyên trách bắt buộc là 114.",
                    "B", "Quay video đăng lên mạng xã hội trước rồi mới gọi báo.", "Sai. Mỗi giây trôi qua đám cháy càng lan rộng và nguy hiểm tính mạng nhiều người.",
                    "C", "Gọi 114, nói rõ địa chỉ, cái gì đang cháy, có ai kẹt không, tên và số điện thoại của mình; giữ máy làm theo hướng dẫn.", "Đúng. 114 là số cứu hỏa toàn quốc miễn phí, cung cấp thông tin chuẩn xác và kịp thời."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Cháy thì gọi 114, nói địa chỉ chính xác trước tiên.",
                Source = "Điều 6 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024: số báo cháy thống nhất toàn quốc là 114, gọi miễn phí cước. [VB]",
                ReviewedBy = "Bộ Công an",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 2.4 [Tiểu học] Cháy ở chung cư - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(10),
                Type = "choice",
                Title = "Ở tầng 12 chung cư chuông báo cháy kêu, hành lang có khói loãng. Bà định bấm thang máy. Mai nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1517457373958-b7bdd4587205?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Buổi tối, Mai (lớp 5) ở tầng 12 chung cư cùng bà. Chuông báo cháy kêu, hành lang có khói loãng. Bà định bấm thang máy cho nhanh.",
                    "Mai nên làm gì?",
                    "Có cháy ở chung cư cao tầng, hai bà cháu nên thoát hiểm bằng cách nào?",
                    "A", "Đi thang máy cho nhanh vì tầng 12 đi bộ rất mệt.", "Sai. Khi cháy hệ thống điện có thể bị cắt bất ngờ, thang máy bị kẹt và biến thành ống hút khói độc chết người.",
                    "B", "Cúi thấp, che mũi miệng bằng khăn ẩm, cùng bà đi thang bộ thoát hiểm xuống đất.", "Đúng. Thang bộ thoát hiểm có cửa chống cháy và quạt tăng áp là lối thoát an toàn duy nhất.",
                    "C", "Chạy vào nhà vệ sinh, đóng cửa xả nước chờ cứu hộ.", "Sai. Khói độc lùa qua khe cửa và lỗ thông gió nhà vệ sinh gây ngạt nhanh chóng."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Có cháy thì không đi thang máy; cúi thấp, đi thang bộ thoát hiểm.",
                Source = "Cẩm nang PCCC chung cư cao tầng - Cục Cảnh sát PCCC & CNCH - Bộ Công an. [CM]",
                ReviewedBy = "Cục Cảnh sát PCCC & CNCH",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 2.5 [THCS] Bị kẹt trong nhà - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(11),
                Type = "choice",
                Title = "Cháy bùng lên ở tầng trệt, khói dày tràn lên cầu thang nhà phố khiến Hải và em bị kẹt ở tầng 3. Hải nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1509198397868-475647b2a1e5?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Hải (lớp 8) ở tầng 3 một ngôi nhà phố cùng em. Đám cháy bùng lên ở tầng trệt, khói dày tràn lên cầu thang.",
                    "Hải nên làm gì?",
                    "Khi đường cầu thang bộ đã bị khói lửa chặn kín, hai anh em cần xử lý thế nào?",
                    "A", "Quay vào phòng, đóng cửa, chèn khe cửa bằng chăn/khăn ướt, ra ban công/cửa sổ hô to, gọi 114 báo rõ tầng vị trí.", "Đúng. Ngăn khói độc xâm nhập, duy trì dưỡng khí và báo hiệu vị trí chính xác cho lực lượng cứu hỏa.",
                    "B", "Lao xuống cầu thang vượt qua lớp khói dày để ra cửa chính.", "Sai. Chỉ cần hít 2-3 hơi khói độc đậm đặc là nạn nhân sẽ choáng ngất và ngạt thở tử vong.",
                    "C", "Chui xuống gầm giường hoặc tủ quần áo đóng chặt cửa trốn.", "Sai. Lực lượng cứu nạn không thể định vị được và khói vẫn xâm nhập gây tử vong."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Không còn lối thoát: chặn khói, gọi 114, ra nơi thoáng báo vị trí.",
                Source = "Hướng dẫn thoát nạn khi xảy ra cháy nhà dạng ống, nhà phố - Bộ Công an. [CM]",
                ReviewedBy = "Phòng Cảnh sát PCCC & CNCH TP. Hà Nội",
                Difficulty = "Hard",
                Status = "Published"
            });

            // 2.6 [THCS] Bạn rủ gọi 114 để trêu - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(12),
                Type = "choice",
                Title = "Trong nhóm chat lớp, một bạn rủ Tuấn gọi 114 báo cháy giả nhà bạn X để trêu đùa. Tuấn nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1577563908411-5077b6dc7624?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Trong nhóm chat lớp, một bạn rủ Tuấn (lớp 9) gọi 114 báo “nhà bạn X đang cháy” để trêu cho vui, và nói chẳng ai biết là mình gọi.",
                    "Tuấn nên làm gì?",
                    "Khi bạn bè rủ gọi điện thoại báo cháy giả trêu đùa, em phản ứng ra sao?",
                    "A", "Gọi một lần cho vui vì nghĩ cảnh sát không tìm ra được.", "Sai. Báo cháy giả là hành vi vi phạm pháp luật nghiêm trọng, cơ quan chức năng dễ dàng truy tìm nguồn gọi.",
                    "B", "Nhờ bạn khác gọi hộ để mình không bị liên lụy.", "Sai. Đồng lõa tiếp tay cho hành vi quấy rối lực lượng cứu nạn khẩn cấp.",
                    "C", "Từ chối, nói rõ đây là hành vi cấm; nếu bạn vẫn làm thì báo thầy cô hoặc cha mẹ.", "Đúng. Nhận thức trách nhiệm công dân, ngăn chặn hành vi làm lãng phí nguồn lực cứu người thật."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Chỉ gọi 114 khi có cháy hoặc tình huống cứu nạn thật.",
                Source = "Điều 14 Luật Phòng cháy, chữa cháy và cứu nạn, cứu hộ 2024: nghiêm cấm báo cháy giả và báo tin cứu nạn giả, xử phạt vi phạm hành chính rất nặng. [VB]",
                ReviewedBy = "Bộ Công an",
                Difficulty = "Hard",
                Status = "Published"
            });

            // =========================================================================
            // CHỦ ĐỀ 3: SƠ CẤP CỨU
            // =========================================================================

            // 3.1 [Mầm non] Ông bị ngã - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(13),
                Type = "choice",
                Title = "Ông của Tít ngã ở sân, nằm im không trả lời. Trong nhà chỉ có 2 ông cháu và điện thoại của mẹ để trên bàn. Tít nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1576765608535-5f04d1e3f289?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Ông của Tít ngã ở sân, nằm im và không trả lời khi Tít gọi. Trong nhà chỉ có Tít và ông. Điện thoại của mẹ để trên bàn.",
                    "Tít nên làm gì?",
                    "Thấy ông nằm im không cử động, bé Tít phải làm gì đầu tiên?",
                    "A", "Chạy đi chơi và chờ ông tự dậy.", "Sai. Ông đang gặp nguy hiểm sức khỏe cần được người lớn cấp cứu ngay.",
                    "B", "Gọi to nhờ hàng xóm; nếu biết bấm điện thoại thì gọi 115 nói: “Ông con ngã không dậy được, nhà con ở số…”", "Đúng. Kêu cứu người lớn xung quanh hoặc bấm 115 báo tin chính xác.",
                    "C", "Lay mạnh và cố gắng kéo ông ngồi dậy.", "Sai. Bé nhỏ kéo có thể làm ông gãy xương hoặc chấn thương nặng hơn."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Thấy người nằm im không trả lời: gọi người lớn và gọi 115.",
                Source = "Quy trình xử lý tai nạn người cao tuổi - Cấp cứu 115 Việt Nam. [CM]",
                ReviewedBy = "Bác sĩ Bệnh viện Lão khoa TW",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 3.2 [Mầm non] Trầy đầu gối - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(14),
                Type = "choice",
                Title = "Mộc ngã xe trầy đầu gối dính đất cát và chảy ít máu. Mộc định lấy lá cây đắp lên. Mộc nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Mộc (6 tuổi) ngã xe đạp, trầy đầu gối, chảy ít máu và có dính đất cát. Mộc định lấy lá cây đắp lên cho mau lành.",
                    "Mộc nên làm gì?",
                    "Bị trầy xước chân chảy máu, bé Mộc nên sơ cứu thế nào?",
                    "A", "Báo người lớn, rửa nhẹ bằng nước sạch, dùng gạc/khăn sạch ép lên vết thương cho tới khi cầm máu rồi băng lại.", "Đúng. Nước sạch và gạc vô khuẩn giúp làm sạch và ngăn ngừa nhiễm trùng uốn ván.",
                    "B", "Đắp lá cây hoặc bôi nước mắm lên vết thương.", "Sai. Lá cây bẩn và vi khuẩn trong đất cát sẽ gây nhiễm trùng máu nguy hiểm.",
                    "C", "Lấy bàn tay chà xát thật mạnh cho sạch hết đất cát.", "Sai. Chà mạnh làm rách mô tế bào, đau đớn và đưa vi khuẩn vào sâu hơn."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Trầy xước: báo người lớn, rửa nước sạch, ép gạc sạch, băng lại.",
                Source = "Hướng dẫn xử trí vết thương hở ngoài da - Bệnh viện Nhi Trung ương. [CM]",
                ReviewedBy = "BS. Bệnh viện Nhi Đồng 1",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 3.3 [Tiểu học] Chảy máu cam - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(15),
                Type = "choice",
                Title = "Bóng đập vào mũi Nam khiến máu chảy ra. Bạn bảo Nam ngửa đầu lên trời cho máu ngừng. Nam nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Giờ ra chơi, quả bóng đập vào mũi Nam khiến máu chảy ra. Một bạn bảo Nam ngửa đầu lên trời cho máu khỏi chảy.",
                    "Nam nên làm gì?",
                    "Khi bị chảy máu cam, tư thế sơ cứu nào dưới đây là đúng y khoa?",
                    "A", "Ngửa cổ ra phía sau và nằm ngửa xuống đất.", "Sai. Máu chảy ngược vào họng và phế quản gây sặc thở, buồn nôn hoặc hít vào phổi.",
                    "B", "Vo tròn giấy ăn nhét thật sâu vào bên trong lỗ mũi.", "Sai. Giấy thô ráp có thể làm xước niêm mạc mũi nặng hơn và gây nhiễm khuẩn.",
                    "C", "Ngồi thẳng, hơi cúi đầu về trước, bóp chặt phần mềm hai cánh mũi khoảng 10 phút, thở bằng miệng.", "Đúng. Cúi nhẹ giúp máu không chảy xuống họng, bóp cánh mũi tạo áp lực cầm máu tự nhiên."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Chảy máu cam: ngồi, cúi đầu về trước, bóp cánh mũi 10 phút.",
                Source = "Hướng dẫn sơ cứu chảy máu mũi - Bệnh viện Tai Mũi Họng TW & Vinmec. [CM]",
                ReviewedBy = "Bác sĩ Chuyên khoa Tai Mũi Họng",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 3.4 [Tiểu học] Bỏng nước sôi - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(16),
                Type = "choice",
                Title = "Lan bị nước sôi đổ vào mu bàn tay, da đỏ và rát. Bạn khuyên chườm đá hoặc bôi kem đánh răng. Lan nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Lan (lớp 5) bị nước nóng từ phích đổ vào mu bàn tay, da đỏ và rát. Một bạn khuyên chườm đá hoặc bôi kem đánh răng cho nhanh mát.",
                    "Lan nên làm gì?",
                    "Cách sơ cứu vết bỏng nước sôi chuẩn nhất trong 15 phút đầu tiên là gì?",
                    "A", "Lấy đá lạnh trong tủ lạnh chườm trực tiếp lên vết bỏng.", "Sai. Nhiệt độ quá lạnh đột ngột gây bỏng lạnh và hoại tử mô da.",
                    "B", "Báo người lớn, đưa tay dưới vòi nước mát sạch chảy nhẹ 15–20 phút, che bằng gạc sạch; đi khám nếu rộp bóng nước.", "Đúng. Hạ nhiệt độ tức thì bằng nước mát sạch là biện pháp chuẩn y khoa số 1.",
                    "C", "Bôi kem đánh răng, mỡ trăn hoặc nước mắm lên vết thương.", "Sai. Giữ nhiệt làm vết bỏng ăn sâu vào biểu bì và gây nhiễm trùng nặng nề."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Bỏng: nước mát chảy nhẹ 15–20 phút; không dùng đá, không kem đánh răng, không nước mắm.",
                Source = "Hướng dẫn sơ cấp cứu bỏng ban đầu - Viện Bỏng Quốc gia Lê Hữu Trác. [CM]",
                ReviewedBy = "Viện Bỏng Quốc gia",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 3.5 [THCS] Bạn bị nghẹn - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(17),
                Type = "choice",
                Title = "Ở căng-tin, Phúc đang ăn thạch thì ôm cổ, mặt tím tái, không nói và không ho được. Em nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1532938911079-1b06ac7ceec7?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Ở căng-tin, Phúc (lớp 7) đang ăn thạch thì ôm cổ, không nói được, không ho được và mặt tím tái.",
                    "Em nên làm gì?",
                    "Phát hiện bạn bị nghẹn dị vật đường thở nguy kịch, thao tác xử lý nào đúng?",
                    "A", "Nhờ gọi 115 và thầy cô; nếu đã tập huấn: đứng sau bạn, vòng tay ôm ngang bụng, nắm đấm đặt trên rốn, đẩy mạnh vào trong và lên trên (Heimlich).", "Đúng. Thủ thuật Heimlich tạo áp lực đẩy dị vật bật ra khỏi đường thở kịp thời cứu sống bạn.",
                    "B", "Lấy chai nước ngọt hoặc nước suối bắt bạn uống thật nhiều.", "Sai. Đường thở đang bị bít kín, uống nước sẽ tràn vào phổi gây ngạt thở ngay tức khắc.",
                    "C", "Dốc ngược đầu bạn xuống và dùng ngón tay móc sâu vào trong họng.", "Sai. Ngón tay có thể đẩy viên thạch tụt sâu vào phế quản hơn."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Chỉ can thiệp khi bạn không ho được, không thở được; luôn gọi người lớn và 115.",
                Source = "Cẩm nang xử trí dị vật đường thở - Bệnh viện Nhi Trung ương & Hội Chữ thập đỏ. [CM]",
                ReviewedBy = "Bác sĩ Cấp cứu Nhi khoa",
                Difficulty = "Hard",
                Status = "Published"
            });

            // 3.6 [THCS] Bạn ngã đập đầu - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(18),
                Type = "choice",
                Title = "Dũng ngã đập đầu xuống sân, bảo “không sao” nhưng một lúc sau nôn ói, chóng mặt và nhức đầu. Em nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1579684385127-1ef15d508118?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Giờ thể dục, Dũng ngã đập đầu xuống nền sân, vẫn tỉnh và bảo “không sao”. Một lúc sau Dũng nôn và kêu chóng mặt, nhức đầu.",
                    "Em nên làm gì?",
                    "Khi bạn bị chấn thương đầu xuất hiện triệu chứng nôn và chóng mặt, em cần làm gì?",
                    "A", "Cho Dũng ngồi dậy tham gia chạy thể dục tiếp vì bạn bảo chịu được.", "Sai. Vận động mạnh làm tăng áp lực nội sọ nếu có chảy máu não.",
                    "B", "Tự ý lấy thuốc giảm đau hoặc an thần cho Dũng uống rồi để bạn nằm ngủ.", "Sai. Tuyệt đối không cho uống thuốc vì che lấp các dấu hiệu hôn mê thần kinh.",
                    "C", "Báo thầy cô ngay, để Dũng nằm yên, nhờ gọi 115 hoặc đưa đi viện; theo dõi sát các dấu hiệu nôn và lơ mơ.", "Đúng. Nôn và nhức đầu sau va đập là dấu hiệu chấn thương sọ não cần bác sĩ chụp chiếu khẩn cấp."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Ngã đập đầu rồi nôn, nhức đầu, lơ mơ: báo người lớn và gọi cấp cứu ngay.",
                Source = "Hướng dẫn theo dõi chấn thương sọ não kín ở trẻ em - Bệnh viện Hữu nghị Việt Đức. [CM]",
                ReviewedBy = "Bác sĩ Khoa Phẫu thuật Thần kinh",
                Difficulty = "Hard",
                Status = "Published"
            });

            // =========================================================================
            // CHỦ ĐỀ 4: BẠO LỰC VÀ BẮT NẠT HỌC ĐƯỜNG, GIA ĐÌNH
            // =========================================================================

            // 4.1 [Mầm non] Bạn giật đồ chơi và đánh - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(19),
                Type = "choice",
                Title = "Bi đang xếp hình thì Tèo giật mất khối xếp hình rồi đánh vào tay Bi. Bi nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1502086223501-7ea6ecd79368?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Bi đang xếp hình thì Tèo giật mất khối xếp hình rồi đánh vào tay Bi.",
                    "Bi nên làm gì?",
                    "Bị bạn giật đồ và đánh vào tay ở lớp, bé Bi nên ứng xử thế nào?",
                    "A", "Đánh lại Tèo thật mạnh cho bõ tức.", "Sai. Đánh nhau làm cả hai bạn cùng đau đớn và vi phạm nội quy.",
                    "B", "Nói to “Dừng lại! Đừng đánh mình!” rồi đi tìm cô giáo kể lại.", "Đúng. Thể hiện thái độ kiên quyết và tìm sự can thiệp bảo vệ của cô giáo.",
                    "C", "Im lặng ngồi khóc một mình và không kể với ai.", "Sai. Không kể cô thì bạn Tèo sẽ tiếp tục bắt nạt Bi vào lần sau."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Bị đánh thì nói “dừng lại” và đi tìm cô giáo.",
                Source = "Nghị định 80/2017/NĐ-CP quy định về môi trường giáo dục an toàn, lành mạnh, thân thiện, phòng chống bạo lực học đường. [VB]",
                ReviewedBy = "Chuyên gia Tâm lý Giáo dục Mầm non",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 4.2 [Mầm non, Tiểu học] Người lớn làm con đau và dặn giữ bí mật - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(20),
                Type = "choice",
                Title = "Một người lớn trong nhà đánh Bống rất đau, rồi dặn: “Đừng kể với ai nhé”. Bống nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1543269865-cbf427effbad?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Một người lớn trong nhà đánh Bống rất đau, rồi dặn: “Đừng kể với ai nhé”.",
                    "Bống nên làm gì?",
                    "Nếu bị người lớn làm con đau và bắt giữ bí mật, con cần phải làm gì?",
                    "A", "Kể với một người lớn mà con tin như ông bà, cô giáo, hoặc nhờ họ gọi Tổng đài 111.", "Đúng. Con có quyền được an toàn; bí mật khiến con sợ hãi hoặc đau đớn bắt buộc phải nói ra.",
                    "B", "Giữ bí mật tuyệt đối vì sợ bị đánh đau hơn nữa.", "Sai. Không ai có quyền bạo lực làm tổn thương con, im lặng sẽ khiến bạo lực tiếp diễn.",
                    "C", "Tự gom quần áo bỏ nhà đi chơi thật xa.", "Sai. Trẻ nhỏ lang thang ngoài đường một mình đối mặt với nguy cơ bị bắt cóc và tai nạn."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Con có quyền được an toàn; bí mật làm con đau thì phải kể cho người lớn con tin.",
                Source = "Luật Trẻ em 2016 (Điều 6) và Tổng đài Quốc gia Bảo vệ Trẻ em 111 hoạt động miễn phí 24/7. [VB]",
                ReviewedBy = "Cục Trẻ em - Bộ Lao động, Thương binh và Xã hội",
                Difficulty = "Easy",
                Status = "Published"
            });

            // 4.3 [Tiểu học] Biệt danh và bị cô lập - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(21),
                Type = "choice",
                Title = "Một nhóm bạn gọi Hà bằng biệt danh chê bề ngoài và rủ cả lớp tẩy chay Hà. Hà nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Một nhóm bạn gọi Hà (lớp 4) bằng biệt danh chê bề ngoài và rủ cả lớp không chơi với Hà. Hà buồn và không muốn đến lớp.",
                    "Hà nên làm gì?",
                    "Bị các bạn trong lớp đặt biệt danh xúc phạm và cô lập, em nên xử lý như thế nào?",
                    "A", "Im lặng chịu đựng một mình, hy vọng các bạn sẽ tự chán.", "Sai. Chịu đựng cô lập sẽ khiến tâm lý em bị tổn thương nặng nề và các bạn tiếp tục lấn tới.",
                    "B", "Đặt lại các biệt danh xấu xí khác để chửi bới trả đũa các bạn.", "Sai. Trả đũa làm xung đột leo thang và có thể biến em thành người sai phạm.",
                    "C", "Kể với cha mẹ hoặc cô giáo chủ nhiệm để được giúp đỡ, và kết nối với những người bạn tích cực.", "Đúng. Cô lập và lăng mạ là bạo lực tinh thần cần được nhà trường can thiệp chấn chỉnh."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Bị chê bai, bị cô lập cũng là bạo lực học đường; hãy kể với người lớn tin cậy.",
                Source = "Khoản 5 Điều 2 Nghị định 80/2017/NĐ-CP: bạo lực học đường gồm cả xúc phạm nhân phẩm, danh dự và cô lập người học. [VB]",
                ReviewedBy = "Chuyên viên Phòng Tư vấn Tâm lý Học đường",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 4.4 [Tiểu học] Chứng kiến bạn bị đánh - Correct: B
            list.Add(new Question
            {
                Id = GetQuestionGuid(22),
                Type = "choice",
                Title = "Ở góc sân trường, Quân thấy hai bạn lớn hơn đẩy ngã và đánh Tín. Mấy bạn xung quanh đứng cười. Quân nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1577896851231-70ef18881754?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Ở góc sân, Quân thấy hai bạn lớn hơn đẩy và đánh Tín. Mấy bạn xung quanh đứng xem và cười.",
                    "Quân nên làm gì?",
                    "Chứng kiến bạn học cùng trường bị bắt nạt vây đánh, hành động đúng đắn là gì?",
                    "A", "Đứng xem cùng và cười hùa theo cho vui.", "Sai. Đứng cười và cổ vũ là hành vi vô cảm, tiếp tay trực tiếp cho bạo lực học đường.",
                    "B", "Không xông vào một mình mà chạy đi báo thầy cô hoặc bảo vệ ngay, sau đó hỏi thăm Tín.", "Đúng. Báo cáo khẩn cấp người có thẩm quyền can thiệp an toàn và thể hiện tình bạn đúng lúc.",
                    "C", "Một mình lao vào đánh trả hai bạn lớn để thể hiện tinh thần nghĩa hiệp.", "Sai. Thiếu an toàn có thể khiến em bị đánh thương tích nghiêm trọng."
                ),
                CorrectData = "{\"correctOptionId\":\"B\"}",
                Explanation = "Bài học: Chứng kiến bạo lực: báo người lớn ngay, không cổ vũ, không đánh trả.",
                Source = "Bộ Giáo dục và Đào tạo - Hướng dẫn phòng chống bạo lực học đường và xây dựng văn hóa trường học lành mạnh. [CM]",
                ReviewedBy = "Chuyên gia Công tác Xã hội Học đường",
                Difficulty = "Medium",
                Status = "Published"
            });

            // 4.5 [THCS] Bị bắt nạt trên mạng - Shuffled Correct: A
            list.Add(new Question
            {
                Id = GetQuestionGuid(23),
                Type = "choice",
                Title = "Một nhóm bạn lập group chat chế ảnh dìm và đăng lời lẽ xúc phạm My (lớp 8), nhiều bạn thả tim chia sẻ. My nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Một nhóm bạn lập nhóm chat và đăng ảnh chế giễu My (lớp 8), kèm lời lẽ xúc phạm. Nhiều bạn thả cười và chia sẻ lại.",
                    "My nên làm gì?",
                    "Khi là nạn nhân của bạo lực trên không gian mạng (cyberbullying), em nên hành động ra sao?",
                    "A", "Không đáp trả; chụp màn hình lưu bằng chứng (tài khoản, thời gian); chặn và báo cáo nội dung; kể với cha mẹ/thầy cô hoặc gọi 111.", "Đúng. Bảo tồn chứng cứ số, ngăn chặn tương tác độc hại và nhờ cơ quan bảo vệ trẻ em can thiệp.",
                    "B", "Đăng bài viết công khai chửi bới bóc phốt lại cả nhóm.", "Sai. Khiêu khích làm bùng nổ bạo lực mạng và có thể khiến em vi phạm quy tắc an ninh mạng.",
                    "C", "Xóa hết toàn bộ tin nhắn, khóa điện thoại và chịu đựng một mình trong im lặng.", "Sai. Mất toàn bộ bằng chứng pháp lý và khiến kẻ bắt nạt tiếp tục lộng hành."
                ),
                CorrectData = "{\"correctOptionId\":\"A\"}",
                Explanation = "Bài học: Bị bắt nạt trên mạng: lưu bằng chứng, không đáp trả, kể với người lớn.",
                Source = "Luật An ninh mạng 2018 và Tổng đài Quốc gia Bảo vệ Trẻ em 111 (có kênh Zalo và ứng dụng tư vấn trực tuyến). [VB/CM]",
                ReviewedBy = "Cục An toàn Thông tin - Bộ TT&TT",
                Difficulty = "Hard",
                Status = "Published"
            });

            // 4.6 [THCS] Bị rủ vây đánh một bạn - Shuffled Correct: C
            list.Add(new Question
            {
                Id = GetQuestionGuid(24),
                Type = "choice",
                Title = "Nhóm bạn lớp 9 (14–15 tuổi) rủ Long dằn mặt một bạn sau giờ học và quay clip đăng mạng. Long nên làm gì?",
                ImageUrl = "https://images.unsplash.com/photo-1529156069898-49953e39b3ac?auto=format&fit=crop&w=800&q=80",
                Payload = CreatePayload(
                    "Nhóm bạn lớp 9 (14–15 tuổi) rủ Long “dằn mặt” một bạn bằng cách vây đánh sau giờ học và quay clip đăng mạng.",
                    "Long nên làm gì?",
                    "Khi bị rủ rê tham gia đánh hội đồng và quay clip bạo lực học sinh, em lựa chọn phương án nào?",
                    "A", "Tham gia đánh để bạn bè trong nhóm không chê mình là kẻ hèn nhát.", "Sai. Từ đủ 14 tuổi đã phải chịu trách nhiệm xử phạt hành chính và truy cứu trách nhiệm hình sự nếu gây thương tích.",
                    "B", "Không trực tiếp đánh mà chỉ đứng ngoài cầm điện thoại quay clip.", "Sai. Quay clip phát tán lên mạng là hành vi tiếp tay, làm nhục người khác và vi phạm pháp luật hình sự.",
                    "C", "Từ chối, khuyên các bạn dừng lại và báo thầy cô hoặc cha mẹ để ngăn chặn trước khi xảy ra sự việc.", "Đúng. Tỉnh táo ngăn ngừa hành vi phạm tội, bảo vệ an toàn cho bạn học và tương lai của chính mình."
                ),
                CorrectData = "{\"correctOptionId\":\"C\"}",
                Explanation = "Bài học: Từ chối tham gia và báo người lớn; đánh bạn là vi phạm pháp luật.",
                Source = "Điều 5 Luật Xử lý vi phạm hành chính 2012 và Điều 12, Điều 134 Bộ luật Hình sự 2015: người từ đủ 14 tuổi chịu trách nhiệm về cố ý gây thương tích rất nghiêm trọng. [VB]",
                ReviewedBy = "Luật sư Đoàn Luật sư TP. Hà Nội",
                Difficulty = "Hard",
                Status = "Published"
            });

            return list;
        }
    }
}
