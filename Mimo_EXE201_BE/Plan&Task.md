# ==============================================================================
# MIMO BACKEND ENGINE - EXECUTION PLAN & TASK BREAKDOWN
# Project: MIMO - Gamified Life Skills & Emergency Safety (EXE201)
# Stack: ASP.NET Core (.NET 8) Web API | EF Core | PostgreSQL (pgAdmin 4)
# Architecture: Onion Architecture (Clean Architecture)
# Target Environment: Antigravity Agent Workspace & Execution Engine
# Last Updated: Oct 20, 2026 | Version: 2.5
# ==============================================================================

## 1. TỔNG QUAN DỰ ÁN & RÀNG BUỘC KỸ THUẬT CỐT LÕI

### 1.1. Bối cảnh dự án
MIMO là nền tảng học kỹ năng sống và an toàn khẩn cấp (Sơ cấp cứu, PCCC & Thoát hiểm, An toàn số & Chống lừa đảo) ứng dụng mô hình Micro-learning kết hợp Gamification mô phỏng Duolingo và hệ thống phân lớp, bài tập của Coursera.

### 1.2. Ràng buộc hệ thống bắt buộc (System Invariants)
- **Kiến trúc:** Onion Architecture (Domain -> Application -> Infrastructure -> Presentation).
- **Tech Stack:** ASP.NET Core Web API (.NET 8), Entity Framework Core, PostgreSQL (chính), Redis (tích hợp từ Phase 2, Phase 1 dùng SQL tối ưu).
- **Nguồn chân lý duy nhất (Single Source of Truth):** Toàn bộ việc chấm điểm, trừ Tim, tính Streak, cộng XP và Combo phải thực thi tại Backend. Client tuyệt đối không nhận trường đáp án đúng (`correct_data`, `is_correct`) trước khi trả lời.
- **Múi giờ & Thời gian:** Dữ liệu DB lưu chuẩn `timestamptz` (UTC). Các phép toán chu kỳ ngày (Streak, League, Daily Reset) phải chuyển đổi sang múi giờ `Asia/Ho_Chi_Minh` (UTC+7) trước khi cắt `DateOnly`.
- **Đội ngũ & Quy mô:** 2 Backend Developers. Giai đoạn 1 tập trung bản Web phục vụ demo hội đồng EXE201.

---

## 2. PHÂN KỲ TRIỂN KHAI (ROADMAP PHASES)

Hệ thống được chia làm 10 module (`BE0` đến `BE9`) qua 3 giai đoạn:

| Giai đoạn | Module | Trọng tâm nghiệp vụ | Khối lượng (Ngày công) | Mục tiêu nghiệm thu |
|---|---|---|---|---|
| **Phase 1 (MVP Demo)** | BE0, BE1, BE2, BE3, BE4 (1 phần), BE5 (demo), BEQ | Luồng học hoàn chỉnh: Landing -> Học -> Sai trừ Tim -> Đúng cộng XP/Combo -> Hoàn thành giữ Streak -> Xem BXH SQL | ~46-49 ngày công (2 dev ≈ 3-4 tuần) | Đủ điều kiện demo trực tiếp trước hội đồng EXE201 |
| **Phase 2 (CMS & B2B)** | BE4, BE5, BE6, BE7, BE8 | Curator Studio, B2B School Portal, Class Codes, Assignment Center, Risk Heatmap, Admin Gatekeeper | ~59.5 ngày công | Mở rộng quy mô trường học và thương mại hóa B2B |
| **Phase 3 (Monetization)** | BE9, BE8-6 | Mimo Pro (VNPay/MoMo/PayOS), Vô hạn Tim, Brand Sponsorship, Báo cáo doanh thu | ~15.5 ngày công | Thu phí người dùng cá nhân và tài trợ thương hiệu |

---

## 3. LƯỢC ĐỒ THỰC THỂ DỮ LIỆU (DATABASE SCHEMA SPECIFICATION)

| Phân nhóm | Bảng (snake_case) | Thực thể C# | Thuộc tính / Khóa ngoại chính |
|---|---|---|---|
| **Người dùng** | `users` | `User` | `id (PK)`, `role_id (FK)`, `organization_id (FK, Nullable)`, `email`, `password_hash`, `status`, `timezone`, `total_xp` |
| | `user_profiles` | `UserProfile` | `user_id (PK, FK)`, `full_name`, `daily_goal_minutes`, `risk_survey_result` |
| **Trạng thái chơi** | `user_stats` | `UserStats` | `user_id (PK, FK)`, `hearts (0-5)`, `hearts_updated_at`, `streak`, `last_active_date`, `streak_freeze_count`, `is_pro` |
| **Cây học liệu** | `domains` | `Domain` | `id (PK)`, `name`, `slug`, `order`, `status` |
| | `units` | `Unit` | `id (PK)`, `domain_id (FK)`, `title`, `description`, `order` |
| | `nodes` | `Node` | `id (PK)`, `unit_id (FK)`, `title`, `type (lesson/chest/boss)`, `order`, `unlock_rule`, `xp_reward`, `status` |
| **Câu hỏi** | `questions` | `Question` | `id (PK)`, `type (choice/sequence/red_flag)`, `title`, `image_url`, `payload (JSONB)`, `correct_data (JSONB - Server only)`, `explanation`, `difficulty`, `status`, `curator_id (FK)` |
| | `node_questions` | `NodeQuestion` | `node_id (PK, FK)`, `question_id (PK, FK)`, `order (1-10)` |
| **Tiến trình** | `user_node_progresses`| `UserNodeProgress` | `id (PK)`, `user_id (FK)`, `node_id (FK)`, `status (locked/current/done)`, `completed_at` |
| | `lesson_sessions` | `LessonSession` | `id (PK)`, `user_id (FK)`, `node_id (FK)`, `assignment_id (FK, Nullable)`, `status`, `hearts_lost`, `total_xp_earned`, `current_combo_streak`, `started_at`, `expires_at` |
| | `session_answers` | `SessionAnswer` | `id (PK)`, `session_id (FK)`, `question_id (FK)`, `submitted_answer (JSONB)`, `is_correct`, `time_spent_seconds`, `xp_earned`, `idempotency_key` |
| **Luyện tập** | `wrong_questions` | `WrongQuestion` | `id (PK)`, `user_id (FK)`, `question_id (FK)`, `times_wrong`, `resolved_at`, `last_attempt_at` |
| **Sổ cái XP** | `xp_events` | `XpEvent` | `id (BIGSERIAL, PK)`, `user_id (FK)`, `amount`, `source (lesson/combo/perfect/chest/practice)`, `reference_id`, `created_at` |
| **B2B & Lớp học** | `organizations` | `Organization` | `id (PK)`, `name`, `code`, `total_seats`, `used_seats`, `contract_expires_at`, `status` |
| | `classes` | `Class` | `id (PK)`, `organization_id (FK)`, `educator_id (FK)`, `name`, `class_code`, `max_students`, `status` |
| | `class_members` | `ClassMember` | `class_id (PK, FK)`, `student_id (PK, FK)`, `joined_at`, `status` |
| | `assignments` | `Assignment` | `id (PK)`, `class_id (FK)`, `node_id (FK)`, `created_by (FK)`, `deadline`, `pass_score_threshold` |
| | `assignment_progress`| `AssignmentProgress`| `id (PK)`, `assignment_id (FK)`, `student_id (FK)`, `session_id (FK)`, `best_score`, `is_passed`, `completed_at` |
| | `teacher_licenses` | `TeacherLicense` | `id (PK)`, `organization_id (FK)`, `activation_code`, `assigned_classes_json`, `claimed_by_educator_id (FK)`, `is_claimed` |
| **League tuần** | `league_members` | `LeagueMember` | `id (PK)`, `user_id (FK)`, `tier (Bronze/Silver/Gold/Diamond)`, `week_number`, `group_id`, `weekly_xp`, `rank` |

---

## 4. CHI TIẾT DANH MỤC TASK PHASE 1 — MVP DEMO (46 NGÀY CÔNG)

### BE0 — Nền tảng & Hạ tầng (7 ngày)
- [x] **BE0-1 | Must | 1.0 ngày | Khởi tạo Solution Onion Architecture:** Thiết lập 4 project (`MIMO.Domain`, `MIMO.Application`, `MIMO.Infrastructure`, `MIMO.Presentation`). Cấu hình `EditorConfig`, DI container, appsettings cho các môi trường `Development`, `Staging`, `Production`.
- [x] **BE0-2 | Must | 2.0 ngày | Database Schema & Initial Migration:** Cấu hình EF Core Fluent API, Npgsql snake_case convention, UUID extension. Sinh migration ban đầu cho toàn bộ bảng Phase 1.
- [x] **BE0-3 | Must | 2.0 ngày | Docker Compose & Deployment:** Viết `docker-compose.yml` gồm API, PostgreSQL 16 và pgAdmin 4. Thiết lập script backup DB tự động (`backup-db.sh` & `backup-db.ps1`).
- [x] **BE0-4 | Must | 1.0 ngày | API Standard & Validation:** Tích hợp Global Exception Middleware (chuẩn RFC 7807 ProblemDetails), cấu hình FluentValidation Pipeline Behavior, Swagger/OpenAPI kèm JWT Bearer Auth.
- [x] **BE0-5 | Must | 1.0 ngày | Logging & Health Checks:** Cấu hình Logging, ASP.NET Core Health Checks chuẩn RFC (`/health`), cấu hình CORS cho Web client.

### BE1 — Auth & RBAC (4.5 ngày Must + 1.5 ngày Should)
- [ ] **BE1-1 | Must | 2.0 ngày | Xác thực JWT & Local Auth:** Đăng ký, đăng nhập email + mật khẩu (BCrypt hash), cấp phát cặp Access Token (15-30p) và Refresh Token (7 ngày), thu hồi token khi đăng xuất.
- [ ] **BE1-2 | Must (Nâng từ Should) | 1.5 ngày | Google OAuth Integration:** Xác thực Google ID Token gửi từ client bằng thư viện `Google.Apis.Auth`. Tự động tạo user mới nếu chưa tồn tại.
- [ ] **BE1-3 | Must | 1.0 ngày | Phân quyền 5 Roles RBAC:** Triển khai Authorize Attribute/Policy kiểm tra RoleId theo GUID seed chuẩn (`Admin`, `Student`, `Curator`, `Organization`, `Educator`).
- [ ] **BE1-4 | Must | 1.5 ngày | Hồ sơ & Onboarding:** API `GET /api/v1/users/me`, `PATCH /api/v1/users/me/onboarding` lưu mục tiêu học (5/10/15 phút), khảo sát rủi ro ban đầu và khởi tạo `user_stats` (5 Tim, Streak 0).

### BE2 — Nội dung Học tập (8 ngày Must + 1.5 ngày Should)
- [ ] **BE2-1 | Must | 2.0 ngày | Cây học liệu Domain -> Unit -> Node:** API `GET /api/v1/domains` và `GET /api/v1/units/{id}`.
- [ ] **BE2-2 | Must | 2.0 ngày | Lộ trình học (Learning Path):** API `GET /api/v1/path?domainId={id}` trả về cây nút kèm trạng thái cá nhân (`locked`, `current`, `done`) dựa trên bảng `user_node_progresses`.
- [ ] **BE2-3 | Must | 2.0 ngày | Question Model 3 Dạng & Che Đáp Án:** Thiết kế schema JSON cho `Scenario Choice`, `Sequencing Steps`, `Spot the Red Flag`. API nạp câu hỏi che giấu 100% trường `correct_data`.
- [ ] **BE2-4 | Should | 1.5 ngày | Quản lý Media Tình huống:** Lưu trữ và phục vụ URL ảnh tĩnh/GIF tình huống (static files cho demo hoặc S3-compatible storage).
- [x] **BE2-5 | Must | 2.0 ngày | Data Seed 24 Tình Huống Kịch Bản Chuẩn Hóa:** Xây dựng danh mục seed 24 tình huống thực chiến chuẩn y tế & pháp lý (Giao thông, PCCC, Sơ cấp cứu, Phòng chống bắt nạt & An toàn số) phân bổ vào 4 Domains, 4 Units, 8 Nodes, tích hợp tự động qua `DatabaseSeeder` và `GameplaySeedData`.

### BE3 — Gameplay Engine (14 ngày)
- [ ] **BE3-1 | Must | 1.5 ngày | Khởi tạo phiên học:** `POST /api/v1/lessons/{nodeId}/start`. Kiểm tra Tim > 0 (hoặc `is_pro == true`), tạo bản ghi `lesson_sessions` (TTL 30 phút), tải 10 câu hỏi ngẫu nhiên/cố định.
- [ ] **BE3-2 | Must | 3.0 ngày | Chấm điểm phía Server:** `POST /api/v1/sessions/{id}/answer`. Chấm điểm theo từng dạng câu hỏi, trừ 1 Tim nếu sai, cộng +10 XP nếu đúng, tính combo XP, cập nhật `current_combo_streak`, trả về `explanation`.
- [ ] **BE3-3 | Must | 1.5 ngày | Hearts Engine (Lazy Regeneration):** Thuật toán tính hồi 1 Tim mỗi 4 giờ dựa trên `hearts_updated_at` mỗi khi gọi API, không dùng worker nền.
- [ ] **BE3-4 | Must | 2.0 ngày | Hoàn thành bài học:** `POST /api/v1/sessions/{id}/complete`. Tính thưởng Perfect (+5 XP nếu `hearts_lost == 0`), cập nhật node thành `done`, mở khóa node kế tiếp, kích hoạt cập nhật Streak.
- [ ] **BE3-5 | Must | 2.0 ngày | Luyện tập hồi Tim (Practice Mode):** API `GET /api/v1/practice/wrong-questions` lấy danh sách câu sai. API `POST /api/v1/practice/answer` chấm điểm câu sai: nếu đúng hồi +1 Tim và cộng +5 XP.
- [ ] **BE3-6 | Must | 2.0 ngày | Dịch vụ tính Streak theo Múi giờ:** So sánh `last_active_date` với ngày hiện tại (múi giờ `Asia/Ho_Chi_Minh`), xử lý tăng chuỗi, tiêu thụ Streak Freeze nếu đứt chuỗi 1 ngày, hoặc reset về 1.
- [ ] **BE3-7 | Must | 1.0 ngày | Sổ cái XP & Level:** Ghi mọi điểm phát sinh vào `xp_events`, cập nhật snapshot `users.total_xp`, dẫn xuất cấp độ Level 1-5 từ tổng XP.
- [ ] **BE3-8 | Must | 1.0 ngày | Anti-Cheat & Concurrency Guard:** Kiểm tra header `Idempotency-Key` khi gửi câu trả lời, kiểm tra session hết hạn (sau 30p), ngăn chặn submit trùng lặp bằng unique index.

### BE4 — Gamification Mở Rộng Phase 1 (2.5 ngày)
- [ ] **BE4-1 | Must | 1.5 ngày | Quản lý Streak Freeze:** Cho phép người dùng sở hữu bùa đóng băng qua mua bằng XP hoặc nhận từ rương thưởng.
- [ ] **BE4-2 | Must | 1.0 ngày | Nút Rương thưởng (Chest Node):** Xử lý nhận thưởng ngẫu nhiên XP hoặc Freeze khi đi qua nút dạng `chest`, chặn mở lặp lại.

### BE5 — Bảng Xếp Hạng Tuần Phase 1 (3 ngày)
- [ ] **BE5-1 | Must | 2.0 ngày | Leaderboard SQL Query:** `GET /api/v1/leaderboard/current`. Truy vấn SQL tổng hợp XP trong tuần hiện tại (tính từ 00:00 Thứ Hai giờ VN) trả về Top 30 + thứ hạng của người gọi.
- [ ] **BE5-2 | Must | 1.0 ngày | Mock Data Leaderboard:** Script sinh 35-40 người dùng giả lập có XP ngẫu nhiên để bảng xếp hạng luôn hiển thị đầy đặn khi demo.

### BEQ — Đảm bảo Chất lượng & Kiểm thử (7 ngày)
- [ ] **BEQ-1 | Must | 2.0 ngày | Unit Tests:** Viết test bao phủ logic Tim (Lazy Regen), Streak (ca biên: qua nửa đêm, đổi múi giờ), Combo XP, hết Tim giữa bài.
- [ ] **BEQ-2 | Must | 2.0 ngày | Integration Tests:** Kiểm thử luồng tích hợp: Đăng ký -> Start lesson -> Trả lời sai (mất tim) -> Trả lời đúng (cộng XP) -> Complete lesson -> Kiểm tra Streak -> Xem BXH.
- [ ] **BEQ-3 | Must | 3.0 ngày | Demo Dry-run & Bug Fixing:** Diễn tập kịch bản demo hội đồng, kiểm tra hiệu năng, chuẩn bị script reset database về trạng thái ban đầu.

---

## 5. DANH MỤC NHIỆM VỤ PHASE 2 & PHASE 3

### Phase 2 — Curator CMS, B2B School & Super Admin (59.5 ngày công)
- **BE4-3 (2.5d):** Huy hiệu thành tích (Badges Engine).
- **BE4-4 (1.5d):** Thử thách Boss Challenge cuối chặng.
- **BE5-3 (2.0d):** Gom nhóm ngẫu nhiên 30 người cùng hạng League (Đồng -> Kim Cương).
- **BE5-4 (3.0d):** Background Job chốt tuần 00:00 Thứ Hai: Thăng hạng (Top 1-7), Rớt hạng (Top 25-30), Thưởng Top 1-3.
- **BE6-1..6 (14.0d):** Curator Studio: CRUD cây kỹ năng, Scenario Builder, Presigned Upload URL, Question Bank, Phê duyệt xuất bản (Gatekeeper Workflow), Phân tích chất lượng câu hỏi.
- **BE7-1..10 (23.5d):** B2B Organization & Educator: Quản lý tổ chức, Sinh Class Code / QR, Quét QR đồng thời có khóa Concurrency, Import học sinh Excel/CSV, Assignment Center (Giao chuyên đề, deadline, ngưỡng 80%), Dashboard Risk Heatmap, Xuất báo cáo an toàn PDF/Excel, Quản lý Seat hợp đồng.
- **BE8-1..5 (13.0d):** Super Admin: Quản trị User/RBAC 5 roles, Khôi phục Streak khi có sự cố, Cấu hình tham số gamification lưu DB có cache, Quản trị khách hàng B2B, Thống kê DAU/MAU/Retention D1-D7-D30.

### Phase 3 — Doanh thu & Thông báo (15.5 ngày công)
- **BE9-1..2 (6.5d):** Gói Mimo Pro (49k/tháng, 399k/năm), tích hợp cổng thanh toán (VNPay/PayOS), Webhook xử lý thanh toán, kích hoạt quyền vô hạn Tim.
- **BE9-3 (3.0d):** Hệ thống Push Notification nhắc giữ Streak, hồi phục Tim, thông báo bài tập mới.
- **BE9-4..5 & BE8-6 (6.0d):** Tài trợ thương hiệu CSR, Hóa đơn hợp đồng B2B, Báo cáo doanh thu đa nguồn.

---

## 6. QUY TẮC NGHIỆP VỤ CẦN CHỐT & RỦI RO KỸ THUẬT

### 6.1. Quy tắc nghiệp vụ bắt buộc (Business Logic Consensus)
1. **Reset Streak khi bỏ lỡ:**
   - Khi qua ngày bỏ lỡ mà không học: Hiển thị Streak = 0 trên giao diện.
   - Khi hoàn thành bài học kế tiếp: Bắt đầu chuỗi mới với `streak = 1`.
2. **Quy tắc Streak Freeze:**
   - Mỗi bùa Streak Freeze chỉ bảo vệ cho đúng **01 ngày** bỏ lỡ.
   - Nếu bỏ lỡ $\Delta \text{days} \ge 2$ ngày liên tiếp: Cần $\Delta \text{days}$ bùa để cứu chuỗi. Nếu không đủ bùa, chuỗi bị reset về 1 khi học lại.
3. **Phân loại đáp án & Trừ Tim:**
   - Mỗi phương án trong `Scenario Choice` có nhãn: `Correct` (Đúng), `PartiallyCorrect` (Chưa tối ưu), `Incorrect` (Sai nguy hiểm).
   - `Correct`: +10 XP, không trừ Tim.
   - `PartiallyCorrect`: +3 XP, không trừ Tim, hiển thị nhắc nhở cảnh báo.
   - `Incorrect`: 0 XP, -1 Tim, hiển thị giải thích hậu quả nguy hiểm.
4. **Combo Streak trong bài:**
   - Chuỗi đúng 3 câu: Thưởng +2 XP.
   - Chuỗi đúng 5 câu: Thưởng +3 XP.
   - Chuỗi đúng 7 câu: Thưởng +4 XP.
   - Chuỗi đúng 10 câu (bài hoàn hảo): Thưởng +5 XP.
   - Khi trả lời **SAI**: `current_combo_streak` lập tức bị reset về 0.
5. **Hết Tim giữa bài học:**
   - Session bị chuyển trạng thái `failed`.
   - Các điểm XP đã nhận từ câu trả lời đúng trước đó vẫn được giữ trong sổ cái `xp_events`.
   - Bài học **KHÔNG** được tính là hoàn thành: không tăng Streak, không mở khóa node kế tiếp.

### 6.2. Kiểm soát rủi ro kỹ thuật
- **Client Cheating:** Tuyệt đối không serialize `correct_data` về client. Toàn bộ logic kiểm tra đáp án nằm ở `SessionAnswerService`.
- **Tương tranh dữ liệu (Race Condition):** Bọc toàn bộ logic trừ Tim, cộng XP và hoàn thành bài trong Transaction. Áp dụng Optimistic Concurrency (`xmin` của PostgreSQL).
- **Tránh quá tải khi học sinh quét QR vào lớp:** Sử dụng `SELECT ... FOR UPDATE` khi kiểm tra hạn mức seat của trường B2B để chống cấp phát vượt quá thỏa thuận hợp đồng.

---

## 7. KỊCH BẢN CHIA VIỆC CHO 2 DEVELOPERS .NET (PHASE 1)

Với nhân sự gồm 2 Backend Developers, dự án chia làm 3 cấp độ hoàn thiện:

| Phương án | Phạm vi chức năng | Khối lượng | Thời gian ước tính |
|---|---|---|---|
| **Mức A (Khung chạy được)** | Scaffold Onion Architecture, DB schema, Đăng nhập email/pass, GET /path, Seed 1 bài 10 câu, Chơi bài (chấm điểm, Tim, XP, mở khóa) | 4 ngày công | 2 ngày làm việc (chạy nước rút) |
| **Mức B (Có Google & Streak)** | Toàn bộ Mức A + Google OAuth + Dịch vụ Streak tính theo múi giờ `Asia/Ho_Chi_Minh` | 6 ngày công | 3 ngày làm việc |
| **Mức C (Demo đủ luồng EXE201)** | Toàn bộ Mức B + Seed đủ 30 câu (3 bài Sơ cấp cứu, PCCC, Chống lừa đảo) + Ôn câu sai (Practice earn heart) + BXH tuần bằng SQL + Mock 35 user đua top + Combo/Perfect XP + Profile Onboarding + Unit/Integration test | 14 ngày công | 7 ngày làm việc |

### Phân công chi tiết (Khuyến nghị Mức C):
- **Developer 1 (Auth + Content + Seed):**
  - Thực hiện các task: `BE0-1`, `BE0-2`, `BE0-3`, `BE1-1`, `BE1-2`, `BE1-3`, `BE1-4`, `BE2-1`, `BE2-2`, `BE2-3`, `BE2-5`.
  - Kết quả bàn giao: Toàn bộ tầng DB, Xác thực JWT/Google, API Cây kỹ năng và file JSON seed 15 tình huống chuẩn.
- **Developer 2 (Gameplay Engine + Gamification + BXH):**
  - Thực hiện các task: `BE3-1`, `BE3-2`, `BE3-3`, `BE3-4`, `BE3-5`, `BE3-6`, `BE3-7`, `BE3-8`, `BE4-1`, `BE4-2`, `BE5-1`, `BE5-2`, `BEQ-1`, `BEQ-2`.
  - Kết quả bàn giao: Engine chơi bài, Quản lý Tim, Xử lý chuỗi Streak, Chấm điểm 3 dạng câu, Sổ cái XP, BXH SQL và Test suite.

---

## 8. CẤU TRÚC 15 TÌNH HUỐNG THỰC TẾ SEED MẪU (30 CÂU HỎI)

15 tình huống thực chiến được tổ chức thành 3 bài học hoàn chỉnh (mỗi bài 10 câu hỏi):

| Bài | Lĩnh vực | Danh mục tình huống (5 tình huống x 2 câu = 10 câu) | Cơ cấu dạng câu |
|---|---|---|---|
| **Bài 1** | **Sơ cấp cứu ban đầu** | 1. Bỏng nước sôi sinh hoạt<br>2. Đuối nước cứu hộ<br>3. Hóc dị vật đường thở (Heimlich)<br>4. Ngừng tuần hoàn hô hấp (CPR)<br>5. Gãy xương cẳng tay kín | 7 câu Quyết định tình huống (Choice)<br>3 câu Sắp xếp quy trình (Sequence) |
| **Bài 2** | **PCCC & Thoát hiểm** | 1. Cháy chung cư mini ban đêm<br>2. Chập điện tủ lạnh bốc khói<br>3. Rò rỉ khí gas nhà bếp<br>4. Sử dụng bình chữa cháy mini<br>5. Thoát khỏi hành lang ngạt khói độc | 7 câu Quyết định tình huống (Choice)<br>3 câu Sắp xếp quy trình (Sequence) |
| **Bài 3** | **An toàn số & Chống lừa đảo** | 1. Video call Deepfake mạo danh công an<br>2. SMS chứa link trúng thưởng giả mạo<br>3. Lừa cài mã độc chiếm quyền OTP ngân hàng<br>4. Rò rỉ thông tin căn cước công dân<br>5. Bẫy tuyển cộng tác viên online | 5 câu Quyết định tình huống (Choice)<br>5 câu Vạch lá tìm sâu (Spot the Red Flag) |

*Yêu cầu bắt buộc đối với từng câu hỏi:*
- Phải có đầy đủ giải thích chuyên môn y tế / an toàn cứu hộ.
- Ghi rõ nguồn tài liệu chính thức (`source`): Cục Cảnh sát PCCC & CNCH, Bộ Y tế, Cục An toàn thông tin.
- Có tên chuyên gia thẩm định (`reviewed_by`). Tình huống y tế chưa có người duyệt chuyên môn không được kích hoạt live.

---

## 9. HƯỚNG DẪN DÀNH CHO ANTIGRAVITY AGENT KHI TRIỂN KHAI

Khi AI Agent thực thi từng task từ danh mục trên:
1. **Kiểm tra phụ thuộc (Dependencies):** Không bắt đầu task nếu các task phụ thuộc chưa hoàn thành.
2. **Kiểm tra tầng Onion:**
   - Entity và Enums chỉ nằm tại `MIMO.Domain`.
   - DTOs, Repository Interfaces, CQRS Handlers nằm tại `MIMO.Application`.
   - DbContext, Repository Implementations, External Services nằm tại `MIMO.Infrastructure`.
   - Controllers, Filters, Middlewares nằm tại `MIMO.Presentation`.
3. **Audit bảo mật:** Tuyệt đối không để lộ trường đáp án đúng trong các ViewModel/DTO của bài học.
4. **Viết mã hoàn chỉnh:** Không dùng comment giữ chỗ `// TODO`. Cung cấp mã nguồn hoàn chỉnh có xử lý ngoại lệ và ghi log đầy đủ.