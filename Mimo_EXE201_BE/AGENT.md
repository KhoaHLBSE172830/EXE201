# ==============================================================================
# MIMO BACKEND ENGINE - AGENT INSTRUCTION MANUAL & ARCHITECTURE SPECIFICATION
# Project: MIMO - Gamified Life Skills & Emergency Safety (EXE201)[cite: 3, 4]
# Architecture: Onion Architecture (Clean Architecture)[cite: 1, 3]
# Stack: ASP.NET Core (.NET 8) Web API | PostgreSQL (pgAdmin 4) | EF Core[cite: 1, 3]
# Timezone: Asia/Ho_Chi_Minh (UTC+7)[cite: 1, 3]
# Methodology: Duolingo Micro-Gamification + Coursera Cohort/B2B Academic Learning[cite: 1, 4]
# Version: 3.0 (Enterprise & Full B2B/B2C Integrated Specification)
# ==============================================================================

## 1. TỔNG QUAN HỆ THỐNG & VAI TRÒ AGENT

### 1.1. Bối cảnh dự án
MIMO là nền tảng học kỹ năng sống và phản xạ an toàn khẩn cấp (Sơ cấp cứu, PCCC & Thoát hiểm, An toàn số & Chống lừa đảo, An toàn giao thông, Sức khỏe tinh thần) kết hợp giữa mô hình Micro-learning & Gamification của **Duolingo** và mô hình đào tạo theo lớp, giao bài tập, chứng nhận chuẩn hóa của **Coursera**[cite: 3, 4]. Hệ thống chuyển đổi kiến thức lý thuyết thành các tình huống tương tác 2–3 phút, đồng thời cung cấp cổng quản trị B2B hoàn chỉnh cho trường học và tổ chức giáo dục[cite: 4].

### 1.2. Hệ thống phân quyền 5 Roles (RBAC)
Hệ thống quản lý 5 vai trò người dùng độc lập, được định danh cố định bằng GUID seed khi khởi tạo cơ sở dữ liệu[cite: 3, 4]:

| Enum Value | Role Name | GUID Seed | Mô tả & Phạm vi nghiệp vụ |
|---|---|---|---|
| Admin = 1 | Admin | 11111111-1111-1111-1111-111111111111 | Quản trị viên tối cao, Người gác cổng thẩm định xuất bản học liệu (Gatekeeper), quản lý hợp đồng B2B[cite: 3, 4]. |
| Student = 2 | Student | 22222222-2222-2222-2222-222222222222 | Người học (B2C tự do hoặc học sinh thuộc lớp học B2B)[cite: 3, 4]. |
| Curator = 3 | Curator | 33333333-3333-3333-3333-333333333333 | Chuyên gia biên soạn độc lập (Bác sĩ, Cảnh sát PCCC...). Hoạt động độc lập với mọi trường (OrganizationId IS NULL)[cite: 3, 4]. |
| Organization = 4 | Organization | 44444444-4444-4444-4444-444444444444 | Đại diện trường B2B (Ban Giám Hiệu), quản lý hợp đồng Seat, cấp mã bản quyền cho giáo viên, xuất báo cáo an toàn[cite: 3, 4]. |
| Educator = 5 | Educator | 55555555-5555-5555-5555-555555555555 | Giáo viên trực thuộc một trường cụ thể (OrganizationId NOT NULL), quản lý lớp, chiếu QR, giao bài tập[cite: 3, 4]. |

*Quy tắc ràng buộc Role:*
- Toàn hệ thống chỉ cho phép duy nhất 1 Super Admin (áp dụng Partial Unique Index trên PostgreSQL)[cite: 3].
- Không thể tự đăng ký tài khoản với quyền Admin hoặc Curator qua API công khai[cite: 1, 3].
- Phân biệt rõ: **Curator** soạn học liệu chuẩn dùng chung toàn quốc; **Educator** chỉ quản lý lớp học và giao bài tại trường mình phụ trách, không có quyền chỉnh sửa ngân hàng câu hỏi gốc[cite: 4].

### 1.3. Vai trò của Agent
Khi tiếp nhận và thực thi các task mã nguồn, Agent đóng vai trò là một **Senior .NET Backend Architect & Lead Developer**. Agent có trách nhiệm:
- Tuân thủ cấu trúc Onion Architecture, nguyên lý SOLID và Domain-Driven Design (DDD)[cite: 3].
- Bảo đảm tính toàn vẹn dữ liệu cho mô hình lai (Dual Context): Tự học tự do (Self-paced) và Học theo bài tập được giao của trường (Mandatory Assignments)[cite: 1, 4].
- Thực thi nghiêm ngặt nguyên tắc **Server là nguồn sự thật duy nhất (Single Source of Truth)**[cite: 1, 3].

---

## 2. QUY TẮC CỐT LÕI: ATTENTION & CRITICAL WARNINGS

> [!CAUTION]
> ### CRITICAL WARNING 1: BẢO MẬT ĐÁP ÁN PHÍA SERVER (ZERO LEAKAGE)
> - Endpoint lấy danh sách câu hỏi (GET /lessons/{nodeId}/questions, POST /lessons/{nodeId}/start) TUYỆT ĐỐI KHÔNG serialize trường CorrectData, is_correct hoặc vị trí tọa độ đúng về client[cite: 1, 3].
> - DTO trả về chỉ chứa thông tin hiển thị (Payload): nội dung câu hỏi, danh sách lựa chọn đã xáo trộn, thẻ sắp xếp, hình ảnh[cite: 1, 3].
> - Toàn bộ logic chấm điểm, trừ Tim, cộng XP diễn ra 100% tại backend thông qua POST /sessions/{id}/answer[cite: 1, 3].

> [!CAUTION]
> ### CRITICAL WARNING 2: QUẢN LÝ THỜI GIAN UTC VÀ CHU KỲ MÚI GIỜ
> - Toàn bộ mốc thời gian trong DB lưu kiểu timestamptz chuẩn UTC[cite: 3].
> - Tuyệt đối không dùng DateTime.Now[cite: 3]. Luôn dùng DateTimeOffset.UtcNow hoặc trừu tượng hóa qua IDateTimeProvider[cite: 3].
> - Các phép toán cắt ngày (Streak, hạn chốt League, deadline bài tập trường) bắt buộc convert từ UTC sang múi giờ của user (mặc định Asia/Ho_Chi_Minh - UTC+7) trước khi bóc tách DateOnly (YYYY-MM-DD)[cite: 1, 3].

> [!WARNING]
> ### ATTENTION 3: TRỪ TIM, CỘNG XP & QUÉT QR VÀO LỚP PHẢI BẢO ĐẢM TÍNH TOÀN VẸN (CONCURRENCY)
> - Các thao tác cập nhật user_stats (Tim, XP, Streak) phải được bọc trong DbTransaction và kiểm tra khóa lạc quan (Optimistic Concurrency / xmin của PostgreSQL)[cite: 1, 3].
> - Thao tác 40–50 học sinh đồng loạt quét mã QR vào lớp trong 15 giây phải xử lý bằng Pessimistic Lock (SELECT FOR UPDATE) hoặc Atomic Decrement trên số lượng seats của trường B2B để chống Race Condition vượt quá hạn mức hợp đồng[cite: 1, 4].
> - Endpoint nộp câu hỏi bắt buộc nhận header Idempotency-Key để tránh ghi nhận kết quả 2 lần do mạng chập chờn[cite: 1, 3].

> [!WARNING]
> ### ATTENTION 4: LEDGER-BASED XP SYSTEM (SỔ CÁI BẤT BIẾN)
> - Mọi biến động XP BẮT BUỘC phải insert một bản ghi vào bảng xp_events (Append-Only Ledger) kèm nguồn gốc (source) và ID tham chiếu (reference_id)[cite: 1, 3].
> - Cột total_xp ở bảng users chỉ là bản ghi snapshot để đọc nhanh, phải luôn khớp với SUM(amount) trong xp_events[cite: 1, 3].
> - Cấp bậc người dùng (Level 1–5) là hàm suy biến xác định từ tổng XP, không lưu trường độc lập gây lệch pha trạng thái[cite: 1, 3].

> [!WARNING]
> ### ATTENTION 5: QUY TRÌNH DUYỆT HỌC LIỆU BẮT BUỘC (CONTENT GATEKEEPER)
> - Câu hỏi và bài học do Curator tạo ra ban đầu BẮT BUỘC ở trạng thái Draft hoặc PendingApproval, không được để mặc định là Published[cite: 1, 4].
> - Chỉ duy nhất Admin mới có quyền chuyển trạng thái sang Published hoặc thực hiện thu hồi khẩn cấp (Emergency Rollback)[cite: 4].
> - Học sinh chỉ được phép truy vấn và làm bài với những nội dung đã có trạng thái Published[cite: 1, 4].

---

## 3. KIẾN TRÚC ONION (ONION ARCHITECTURE)

Hệ thống được module hóa thành 4 phân lớp theo nguyên lý Dependency Inversion[cite: 3]:

* Luồng phụ thuộc:
  MIMO.Presentation (WebAPI)
    └──> MIMO.Infrastructure (External Concerns)
          └──> MIMO.Application (Business Logic)
                └──> MIMO.Domain (Enterprise Core)

* Phân chia trách nhiệm:
1. MIMO.Domain: Chứa toàn bộ Entities, Value Objects, Domain Exceptions (OutOfHeartsException, SeatLimitExceededException), Domain Enums. Hoàn toàn độc lập, không tham chiếu EF Core hay WebAPI[cite: 3].
2. MIMO.Application: Chứa DTOs, CQRS Handlers/Services, Interface Repositories (IUserRepository, ILessonRepository, IClassRepository), Business Rules và FluentValidation[cite: 3].
3. MIMO.Infrastructure: Triển khai DbContext (MimoDbContext), Entity Configurations (Fluent API), Migrations Npgsql, Redis Service, Google Auth Verifier, Background Jobs chốt League[cite: 1, 3].
4. MIMO.Presentation: Cung cấp RESTful API Controllers (/api/v1/...), Global Error Handling Middleware (RFC 7807 Problem Details), JWT Authorization Handlers[cite: 1, 3].

---

## 4. LƯỢC ĐỒ CƠ SỞ DỮ LIỆU POSTGRESQL HOÀN CHỈNH

Quy chuẩn: Tên bảng và tên cột trong C# dùng PascalCase, cấu hình EF Core tự động map sang snake_case qua UseSnakeCaseNamingConvention()[cite: 1, 3]. Khóa chính sử dụng kiểu UUID (ngoại trừ bảng ledger xp_events dùng BIGSERIAL)[cite: 1, 3].

### 4.1. Phân hệ Tài khoản, Tổ chức & Phân quyền (Auth & RBAC)

* Bảng roles (Entity: Role.cs)[cite: 3]
  - id (UUID, PK): Định danh vai trò[cite: 3].
  - role_name (VARCHAR(20), UNIQUE, NOT NULL): Admin, Student, Curator, Organization, Educator[cite: 3].
  - description (VARCHAR(100), NULL)[cite: 3].
  - status (VARCHAR(20), DEFAULT 'Active')[cite: 3].
  - created_at, updated_at (TIMESTAMPTZ, NOT NULL)[cite: 3].

* Bảng organizations (Entity: Organization.cs) — Trường học / Tổ chức B2B[cite: 3, 4]
  - id (UUID, PK): Định danh tổ chức[cite: 3].
  - name (VARCHAR(150), NOT NULL): Tên trường học / tổ chức[cite: 1].
  - code (VARCHAR(20), UNIQUE, NOT NULL): Mã định danh trường[cite: 3].
  - total_seats (INT, NOT NULL, DEFAULT 0): Tổng số tài khoản bản quyền đã mua[cite: 1, 4].
  - used_seats (INT, NOT NULL, DEFAULT 0): Số tài khoản đã kích hoạt[cite: 1, 4].
  - contract_expires_at (TIMESTAMPTZ, NOT NULL): Hạn sử dụng bản quyền trường[cite: 1, 4].
  - status (VARCHAR(20), DEFAULT 'Active'): Active, Expired, Suspended[cite: 1].
  - created_at, updated_at (TIMESTAMPTZ, NOT NULL)[cite: 3].

* Bảng users (Entity: User.cs)[cite: 3]
  - id (UUID, PK): Định danh người dùng[cite: 3].
  - role_id (UUID, FK -> roles.id, NOT NULL)[cite: 3].
  - organization_id (UUID, FK -> organizations.id, NULL): Bắt buộc với Educator, tùy chọn với Student[cite: 3].
  - email (VARCHAR(255), UNIQUE, NOT NULL)[cite: 1, 3].
  - password_hash (VARCHAR(255), NULL): Mã hóa BCrypt (NULL nếu đăng nhập bằng Google)[cite: 1, 3].
  - full_name (VARCHAR(150), NOT NULL)[cite: 3].
  - phone_number (VARCHAR(15), NULL)[cite: 3].
  - avatar_url (TEXT, NULL)[cite: 3].
  - auth_type (VARCHAR(20), DEFAULT 'Local'): Local hoặc Google[cite: 3].
  - timezone (VARCHAR(50), DEFAULT 'Asia/Ho_Chi_Minh')[cite: 1, 3].
  - total_xp (INT, DEFAULT 0): Cache tổng XP[cite: 1, 3].
  - status (VARCHAR(20), DEFAULT 'Active'): Active, Banned, PendingApproval[cite: 1, 3].
  - refresh_token (VARCHAR(255), NULL), refresh_token_expires_at (TIMESTAMPTZ, NULL)[cite: 1, 3].
  - created_at, updated_at (TIMESTAMPTZ, NOT NULL)[cite: 3].
  - Index: Partial Unique Index trên role_id đảm bảo chỉ có duy nhất 1 Super Admin[cite: 3].

* Bảng user_profiles (Entity: UserProfile.cs) — Quan hệ 1-1 với Users[cite: 1, 3]
  - user_id (UUID, PK, FK -> users.id ON DELETE CASCADE)[cite: 3].
  - daily_goal_minutes (INT, DEFAULT 10): 5, 10, 15 phút/ngày[cite: 1, 3].
  - risk_survey_result (JSONB, NULL): Kết quả khảo sát nhận diện rủi ro lúc onboarding[cite: 1, 3].
  - created_at, updated_at (TIMESTAMPTZ, NOT NULL)[cite: 3].

* Bảng user_stats (Entity: UserStats.cs) — Quan hệ 1-1 với Users[cite: 1, 3]
  - user_id (UUID, PK, FK -> users.id ON DELETE CASCADE)[cite: 3].
  - hearts (INT, DEFAULT 5, CHECK 0-5): Số tim hiện tại[cite: 1, 3].
  - hearts_updated_at (TIMESTAMPTZ, NOT NULL): Mốc tính hồi tim[cite: 1, 3].
  - streak (INT, DEFAULT 0): Chuỗi ngày học liên tiếp[cite: 1, 3].
  - last_active_date (DATE, NULL): Ngày học gần nhất theo múi giờ user[cite: 1, 3].
  - streak_freeze_count (INT, DEFAULT 0): Số bùa bảo vệ streak[cite: 1, 3].
  - is_pro (BOOLEAN, DEFAULT FALSE): Trạng thái gói Pro (vô hạn tim)[cite: 1, 3].
  - pro_expires_at (TIMESTAMPTZ, NULL)[cite: 1, 3].
  - xmin (UINT): PostgreSQL system column hỗ trợ Optimistic Concurrency[cite: 1, 3].

---

### 4.2. Cấu trúc Cây Kỹ năng & Nội dung Học (Content Domain)

* Bảng domains (Entity: Domain.cs) — 5 Chủ đề lớn[cite: 1, 3]
  - id (UUID, PK), name (VARCHAR(100), NOT NULL), slug (VARCHAR(100), UNIQUE, NOT NULL)[cite: 1, 3].
  - icon_url (TEXT, NULL), order (INT, NOT NULL), status (VARCHAR(20), DEFAULT 'Active')[cite: 1, 3].

* Bảng units (Entity: Unit.cs) — Chặng kỹ năng[cite: 1, 3]
  - id (UUID, PK), domain_id (UUID, FK -> domains.id, NOT NULL)[cite: 1, 3].
  - title (VARCHAR(150), NOT NULL), description (TEXT, NULL), order (INT, NOT NULL)[cite: 1, 3].

* Bảng nodes (Entity: Node.cs) — Nút bài học trên Skill Tree[cite: 1, 3]
  - id (UUID, PK), unit_id (UUID, FK -> units.id, NOT NULL)[cite: 1, 3].
  - title (VARCHAR(150), NOT NULL)[cite: 1, 3].
  - type (VARCHAR(30), NOT NULL): lesson (bài thường) | chest (rương) | boss (thử thách chặng)[cite: 1, 3].
  - order (INT, NOT NULL): Thứ tự trong Unit[cite: 1, 3].
  - unlock_rule (JSONB, NULL): Điều kiện mở khóa (vd: { "prerequisite_node_id": "uuid" })[cite: 1, 3].
  - xp_reward (INT, DEFAULT 10): XP thưởng khi hoàn thành node[cite: 1, 3].
  - status (VARCHAR(20), DEFAULT 'Draft'): Draft, PendingApproval, Published, Archived[cite: 1].

* Bảng questions (Entity: Question.cs) — Ngân hàng tình huống[cite: 1, 3]
  - id (UUID, PK)[cite: 1, 3].
  - type (VARCHAR(30), NOT NULL): choice (chọn quyết định), sequence (sắp xếp), red_flag (tìm điểm bất thường)[cite: 1, 3].
  - title (TEXT, NOT NULL): Kịch bản tình huống[cite: 1, 3].
  - image_url (TEXT, NULL): Hình ảnh / GIF mô phỏng[cite: 1, 3].
  - payload (JSONB, NOT NULL): Dữ liệu câu hỏi hiển thị cho client — TUYỆT ĐỐI KHÔNG CHỨA ĐÁP ÁN[cite: 1, 3].
  - correct_data (JSONB, NOT NULL): Đáp án đúng để server đối chiếu chấm điểm[cite: 1, 3].
  - explanation (TEXT, NOT NULL): Lời giải thích chuyên môn y tế/PCCC[cite: 1, 3].
  - difficulty (VARCHAR(20), DEFAULT 'Medium'): Easy, Medium, Hard[cite: 1, 3].
  - source (VARCHAR(255), NULL): Tài liệu tham chiếu chính quy[cite: 1, 3].
  - reviewed_by (VARCHAR(150), NULL): Tên chuyên gia thẩm định[cite: 1, 3].
  - curator_id (UUID, FK -> users.id, NOT NULL): Người tạo bài[cite: 4].
  - status (VARCHAR(20), DEFAULT 'Draft'): Draft, PendingApproval, Published, Rejected[cite: 1].

* Bảng node_questions (Entity: NodeQuestion.cs) — Bảng liên kết[cite: 1, 3]
  - node_id (UUID, FK -> nodes.id), question_id (UUID, FK -> questions.id)[cite: 1, 3].
  - order (INT, NOT NULL): Thứ tự câu hỏi trong bài (chuẩn 10 câu/bài)[cite: 1, 3].
  - PRIMARY KEY (node_id, question_id)[cite: 1, 3].

---

### 4.3. Gameplay Engine, Luyện tập & Sổ cái XP (Duolingo Model)

* Bảng user_node_progresses (Entity: UserNodeProgress.cs)[cite: 1, 3]
  - id (UUID, PK), user_id (UUID, FK -> users.id), node_id (UUID, FK -> nodes.id)[cite: 1, 3].
  - status (VARCHAR(20), NOT NULL): locked, current, done[cite: 1, 3].
  - completed_at (TIMESTAMPTZ, NULL)[cite: 1, 3].
  - UNIQUE (user_id, node_id)[cite: 1, 3].

* Bảng lesson_sessions (Entity: LessonSession.cs) — State Machine phiên chơi[cite: 1, 3]
  - id (UUID, PK)[cite: 1, 3].
  - user_id (UUID, FK -> users.id, NOT NULL)[cite: 1, 3].
  - node_id (UUID, FK -> nodes.id, NOT NULL)[cite: 1, 3].
  - assignment_id (UUID, FK -> assignments.id, NULL): Có giá trị nếu học sinh làm bài do trường giao[cite: 1].
  - status (VARCHAR(20), DEFAULT 'in_progress'): in_progress, completed, failed[cite: 1, 3].
  - hearts_lost (INT, DEFAULT 0): Số tim mất trong phiên[cite: 1, 3].
  - total_xp_earned (INT, DEFAULT 0): XP tích lũy trong phiên[cite: 1, 3].
  - current_combo_streak (INT, DEFAULT 0): Chuỗi câu đúng liên tiếp hiện tại[cite: 1, 2].
  - max_combo_streak (INT, DEFAULT 0): Kỷ lục chuỗi đúng trong phiên[cite: 1, 2].
  - started_at (TIMESTAMPTZ, NOT NULL)[cite: 1, 3].
  - expires_at (TIMESTAMPTZ, NOT NULL): Mặc định started_at + 30 phút[cite: 1, 3].
  - completed_at (TIMESTAMPTZ, NULL)[cite: 1, 3].

* Bảng session_answers (Entity: SessionAnswer.cs)[cite: 1, 3]
  - id (UUID, PK)[cite: 1, 3].
  - session_id (UUID, FK -> lesson_sessions.id, NOT NULL)[cite: 1, 3].
  - question_id (UUID, FK -> questions.id, NOT NULL)[cite: 1, 3].
  - submitted_answer (JSONB, NOT NULL): Payload đáp án học sinh chọn[cite: 1, 3].
  - is_correct (BOOLEAN, NOT NULL): Kết quả chấm[cite: 1, 3].
  - time_spent_seconds (INT, NOT NULL): Thời gian suy nghĩ và trả lời[cite: 1, 3].
  - xp_earned (INT, DEFAULT 0): XP nhận được (+10 XP và thưởng combo nếu có)[cite: 1, 3].
  - idempotency_key (VARCHAR(100), NULL): Token chống nộp trùng lặp[cite: 1, 3].
  - created_at (TIMESTAMPTZ, NOT NULL)[cite: 1, 3].
  - UNIQUE (session_id, question_id): Không cho phép nộp lại câu đã trả lời[cite: 1, 3].

* Bảng wrong_questions (Entity: WrongQuestion.cs) — Ngân hàng câu sai cần ôn[cite: 1, 3]
  - id (UUID, PK), user_id (UUID, FK -> users.id), question_id (UUID, FK -> questions.id)[cite: 1, 3].
  - times_wrong (INT, DEFAULT 1): Số lần chọn sai lũy kế[cite: 1, 3].
  - resolved_at (TIMESTAMPTZ, NULL): NULL nếu chưa ôn; có giá trị nếu đã làm đúng ở Practice[cite: 1, 3].
  - last_attempt_at (TIMESTAMPTZ, NOT NULL)[cite: 1, 3].
  - UNIQUE (user_id, question_id)[cite: 1, 3].

* Bảng xp_events (Entity: XpEvent.cs) — Sổ cái XP bất biến (Ledger)[cite: 1, 3]
  - id (BIGSERIAL, PK): Khóa tự tăng[cite: 1, 3].
  - user_id (UUID, FK -> users.id, NOT NULL, Indexed)[cite: 1, 3].
  - amount (INT, NOT NULL): Lượng XP phát sinh (luôn > 0)[cite: 1, 3].
  - source (VARCHAR(50), NOT NULL): lesson_correct, combo_bonus, perfect_lesson, chest_reward, practice_reward[cite: 1, 3].
  - reference_id (UUID, NULL): Trỏ tới session_id hoặc node_id[cite: 1, 3].
  - created_at (TIMESTAMPTZ, NOT NULL, Indexed)[cite: 1, 3].

---

### 4.4. Phân hệ Trường học B2B & Quản lý Lớp học (Coursera Academic Model)

* Bảng classes (Entity: Class.cs) — Quản lý lớp học trường B2B[cite: 1, 4]
  - id (UUID, PK): Định danh lớp học.
  - organization_id (UUID, FK -> organizations.id, NOT NULL): Trường trực thuộc[cite: 1].
  - educator_id (UUID, FK -> users.id, NOT NULL): Giáo viên phụ trách chính[cite: 1, 4].
  - name (VARCHAR(100), NOT NULL): Tên lớp (vd: "10A1", "Khối 11 Tự nhiên").
  - class_code (VARCHAR(10), UNIQUE, NOT NULL): Mã mời ngắn gọn (6-8 ký tự) để học sinh nhập/quét QR[cite: 1, 4].
  - max_students (INT, DEFAULT 50): Sĩ số tối đa của lớp.
  - status (VARCHAR(20), DEFAULT 'Active'): Active, Archived.
  - created_at, updated_at (TIMESTAMPTZ, NOT NULL).

* Bảng class_members (Entity: ClassMember.cs) — Danh sách học sinh trong lớp[cite: 1, 4]
  - class_id (UUID, FK -> classes.id, NOT NULL)[cite: 1].
  - student_id (UUID, FK -> users.id, NOT NULL)[cite: 1].
  - joined_at (TIMESTAMPTZ, NOT NULL).
  - status (VARCHAR(20), DEFAULT 'Enrolled'): Enrolled, Dropped.
  - PRIMARY KEY (class_id, student_id).

* Bảng assignments (Entity: Assignment.cs) — Giao chuyên đề kỹ năng[cite: 1, 4]
  - id (UUID, PK).
  - class_id (UUID, FK -> classes.id, NOT NULL): Lớp được giao bài[cite: 1].
  - node_id (UUID, FK -> nodes.id, NOT NULL): Bài học chỉ định[cite: 1].
  - created_by (UUID, FK -> users.id, NOT NULL): Giáo viên giao bài.
  - title (VARCHAR(150), NOT NULL).
  - deadline (TIMESTAMPTZ, NOT NULL): Hạn nộp bài[cite: 1, 4].
  - pass_score_threshold (INT, DEFAULT 80): Ngưỡng đạt yêu cầu (% điểm hoặc số câu đúng, vd: 80%)[cite: 1, 4].
  - created_at (TIMESTAMPTZ, NOT NULL).

* Bảng assignment_progress (Entity: AssignmentProgress.cs) — Tiến độ bài tập[cite: 1, 4]
  - id (UUID, PK).
  - assignment_id (UUID, FK -> assignments.id, NOT NULL)[cite: 1].
  - student_id (UUID, FK -> users.id, NOT NULL)[cite: 1].
  - session_id (UUID, FK -> lesson_sessions.id, NULL): Phiên học có kết quả cao nhất[cite: 1].
  - best_score (INT, DEFAULT 0): Điểm cao nhất học sinh đạt được.
  - is_passed (BOOLEAN, DEFAULT FALSE): Đã vượt qua ngưỡng chuẩn hay chưa[cite: 1].
  - completed_at (TIMESTAMPTZ, NULL): Mốc hoàn thành[cite: 1].
  - UNIQUE (assignment_id, student_id).

* Bảng teacher_licenses (Entity: TeacherLicense.cs) — Mã kích hoạt giáo viên[cite: 4]
  - id (UUID, PK).
  - organization_id (UUID, FK -> organizations.id, NOT NULL): Trường cấp phép[cite: 4].
  - activation_code (VARCHAR(32), UNIQUE, NOT NULL): Mã BGH bàn giao cho giáo viên[cite: 4].
  - assigned_classes_json (JSONB, NOT NULL): Danh sách các khối/lớp phân công trước[cite: 4].
  - claimed_by_educator_id (UUID, FK -> users.id, NULL): Giáo viên đã kích hoạt[cite: 4].
  - is_claimed (BOOLEAN, DEFAULT FALSE): Trạng thái đã sử dụng mã hay chưa[cite: 4].
  - expires_at (TIMESTAMPTZ, NOT NULL): Hạn sử dụng của mã.

---

### 4.5. Gamification Mở Rộng & Bảng Xếp Hạng

* Bảng league_members (Entity: LeagueMember.cs)[cite: 1, 3]
  - id (UUID, PK).
  - user_id (UUID, FK -> users.id, NOT NULL).
  - tier (VARCHAR(20), NOT NULL): Bronze, Silver, Gold, Diamond[cite: 1, 2].
  - week_number (VARCHAR(10), NOT NULL): Định dạng YYYY-WW (vd: 2026-W43).
  - group_id (UUID, NOT NULL): Nhóm đấu 30 người[cite: 1, 2].
  - weekly_xp (INT, DEFAULT 0): XP tích lũy trong tuần[cite: 1].
  - rank (INT, DEFAULT 0): Vị trí trên bảng đấu[cite: 1].
  - updated_at (TIMESTAMPTZ, NOT NULL).
  - UNIQUE (user_id, week_number).

* Bảng badges & user_badges[cite: 1]
  - Quản lý danh mục huy hiệu thành tích và lịch sử mở khóa huy hiệu của từng học sinh[cite: 1, 4].

---

## 5. THUẬT TOÁN & LOGIC NGHIỆP VỤ CỐT LÕI

### 5.1. Thuật toán Quản lý & Hồi phục Tim (Hearts Engine)
- Người học thông thường có tối đa 5 Tim[cite: 1, 4]. Khi chọn sai 1 quyết định: trừ 1 Tim[cite: 2, 4]. Khi còn 0 Tim: không thể bắt đầu bài học mới[cite: 2, 4].
- Người dùng có is_pro = true: Vô hạn Tim, luôn vượt qua bước kiểm tra Tim[cite: 1, 4].
- Lazy Heart Regeneration (Tính lười khi người dùng tương tác):
  Khi client truy vấn hồ sơ hoặc gọi POST /lessons/{nodeId}/start, backend tính toán[cite: 1, 3]:
  delta_t = CurrentUtc - user_stats.hearts_updated_at
  recovered_hearts = floor(delta_t / 4 hours)
  new_hearts = min(5, current_hearts + recovered_hearts)
  Nếu new_hearts > current_hearts:
  - Cập nhật hearts = new_hearts[cite: 1, 3].
  - Dịch chuyển mốc: hearts_updated_at = hearts_updated_at + (recovered_hearts * 4 hours)[cite: 1, 3].
- Chế độ Luyện tập hồi Tim (Practice to Earn Heart):
  - Hệ thống lấy ngẫu nhiên câu hỏi từ wrong_questions có resolved_at IS NULL[cite: 1, 3].
  - Trả lời đúng: Đánh dấu resolved_at = UtcNow, hồi phục +1 Tim (tối đa 5), ghi nhận +5 XP vào xp_events (source = 'practice_reward')[cite: 1, 3].

### 5.2. Thuật toán Tính Chuỗi Ngày (Daily Streak Logic)
Logic được thực thi duy nhất tại thời điểm hoàn thành bài học (POST /sessions/{id}/complete)[cite: 1, 3]:
1. Lấy ngày hiện tại theo múi giờ user:
   D_current = DateOnly.FromDateTime(TimezoneHelper.ToUserTime(UtcNow, user.Timezone))[cite: 1, 3]
2. Lấy D_last = user_stats.last_active_date[cite: 1, 3].
3. Các trường hợp xảy ra:
   - TH1: D_current == D_last: Đã học bài trước đó trong cùng ngày -> Giữ nguyên streak[cite: 2, 3].
   - TH2: D_current == D_last + 1 ngày: Học liên tục ngày kế tiếp -> streak = streak + 1, cập nhật D_last = D_current[cite: 2, 3].
   - TH3: D_current >= D_last + 2 ngày: Bị ngắt quãng:
     - Số ngày vắng mặt: delta_days = D_current - D_last - 1.
     - Nếu streak_freeze_count >= delta_days: Tiêu thụ delta_days bùa, bảo toàn streak, cập nhật D_last = D_current, trả về cờ freeze_consumed = true[cite: 1, 2].
     - (Quy tắc MVP): Nếu delta_days == 1 và có >= 1 bùa freeze: Tiêu thụ 1 bùa, giữ streak[cite: 1]. Ngược lại: Chuỗi bị đứt -> Reset streak = 1, cập nhật D_last = D_current[cite: 1, 2].

### 5.3. Quy tắc Điểm thưởng, Combo và Cấp bậc
- Điểm chuẩn: Trả lời đúng một câu: +10 XP[cite: 1, 2].
- Cơ chế Combo Streak trong ván:
  - Nếu trả lời ĐÚNG: Tăng current_combo_streak += 1[cite: 1, 3].
    - Chuỗi 3 câu đúng: Thưởng thêm +2 XP[cite: 1, 3].
    - Chuỗi 5 câu đúng: Thưởng thêm +3 XP[cite: 1, 3].
    - Chuỗi 7 câu đúng: Thưởng thêm +4 XP[cite: 1, 3].
    - Chuỗi 10 câu đúng: Thưởng thêm +5 XP[cite: 1, 3].
  - Nếu trả lời SAI: Lập tức reset current_combo_streak = 0[cite: 3].
- Thưởng bài học hoàn hảo (Perfect Lesson): Hoàn thành bài 10 câu mà không mất tim nào (hearts_lost == 0): Thưởng thêm +5 XP[cite: 1, 2].
- Cấp bậc người dùng (Level 1–5 suy biến từ total_xp)[cite: 2, 3]:
  - Level 1: 0 - 199 XP (Tân binh an toàn)[cite: 2, 3]
  - Level 2: 200 - 499 XP (Người phòng vệ - Defender)[cite: 2, 3]
  - Level 3: 500 - 999 XP (Người giải cứu - Rescuer)[cite: 2, 3]
  - Level 4: 1000 - 1999 XP (Chuyên gia an toàn - Safety Guardian)[cite: 2, 3]
  - Level 5: >= 2000 XP (Người làm chủ cuộc sống - Life Master)[cite: 2, 3]

### 5.4. Xử lý Hai Bối Cảnh Học Tập (Dual Learning Context)
Khi hoàn thành một phiên học (POST /sessions/{id}/complete):
1. Tiến trình cá nhân (Duolingo): Đánh dấu user_node_progresses của node đó thành done, mở khóa node kế tiếp trên bản đồ[cite: 1, 3].
2. Tiến trình bài tập trường giao (Coursera):
   - Nếu lesson_sessions.assignment_id khác NULL:
   - Tính toán tỷ lệ điểm đạt được trong phiên: score = (correct_answers / 10) * 100%.
   - Đối chiếu với assignments.pass_score_threshold: Nếu score >= threshold thì đánh dấu is_passed = true[cite: 1].
   - Cập nhật bản ghi assignment_progress cho học sinh: lưu best_score, gán session_id và cập nhật completed_at[cite: 1].

### 5.5. Quy trình Kiểm duyệt Học liệu (Gatekeeper Workflow)
1. Curator soạn thảo: Khởi tạo câu hỏi hoặc node ở trạng thái Draft[cite: 1, 4]. Curator được quyền sửa, xóa, chạy thử nghiệm trên giả lập[cite: 4].
2. Gửi duyệt: Curator gọi API chuyển trạng thái sang PendingApproval[cite: 1, 4].
3. Admin phê duyệt (The Gatekeeper):
   - Admin kiểm tra tính chuẩn xác của tài liệu tham khảo (source) và chuyên gia thẩm định (reviewed_by)[cite: 1, 3].
   - Admin chuyển trạng thái thành Published (chính thức live toàn quốc) hoặc Rejected (kèm lý do yêu cầu sửa đổi)[cite: 1, 4].
   - Khi phát hiện sai sót khẩn cấp: Admin gọi lệnh Emergency Rollback để hạ nội dung từ Published về Draft[cite: 4].
4. Bộ lọc phía Client: Mọi API lấy lộ trình học tập của học sinh (GET /path, POST /lessons/{id}/start) bắt buộc kèm điều kiện WHERE status = 'Published'[cite: 1].

### 5.6. Quét QR Đồng Thời Vào Lớp (Concurrency Protection)
Khi học sinh gửi yêu cầu gia nhập lớp (POST /classes/join-by-code):
1. Mở một Database Transaction.
2. Thực hiện truy vấn khóa tổ chức B2B tương ứng:
   SELECT id, total_seats, used_seats, contract_expires_at, status FROM organizations WHERE id = @orgId FOR UPDATE;
3. Kiểm tra các điều kiện:
   - status == 'Active' và contract_expires_at > UtcNow[cite: 1].
   - used_seats < total_seats: Nếu đã hết chỗ, ném exception SeatLimitExceededException[cite: 1].
4. Nếu học sinh chưa từng thuộc trường này: Tăng used_seats = used_seats + 1, cập nhật users.organization_id = @orgId[cite: 1, 3].
5. Thêm bản ghi vào class_members[cite: 1].
6. Commit Transaction.

---

## 6. DANH MỤC REST API CONTRACTS (CHUẨN RFC 7807)

### 6.1. Auth & Profiles
- POST /api/v1/auth/register: Đăng ký tài khoản email/mật khẩu[cite: 1, 3].
- POST /api/v1/auth/login: Xác thực và trả JWT Pair (Access + Refresh Token)[cite: 1, 3].
- POST /api/v1/auth/google: Đăng nhập bằng Google ID Token[cite: 1, 3].
- POST /api/v1/auth/refresh-token: Cấp lại Access Token mới[cite: 1, 3].
- GET /api/v1/users/me: Thông tin cá nhân, số tim (Lazy Regen), streak, level[cite: 1, 3].
- PATCH /api/v1/users/me/onboarding: Cập nhật mục tiêu học hàng ngày và khảo sát an toàn[cite: 1, 3].

### 6.2. Learning Path & Gameplay Engine
- GET /api/v1/domains: Danh sách 5 chủ đề lớn[cite: 1, 3].
- GET /api/v1/path?domainId={id}: Bản đồ bài học (trạng thái: locked, current, done)[cite: 1, 3].
- POST /api/v1/lessons/{nodeId}/start: Kiểm tra tim >= 1 hoặc Pro, khởi tạo LessonSession (TTL 30 phút), trả về danh sách 10 câu hỏi che giấu đáp án[cite: 1, 3].
- POST /api/v1/sessions/{id}/answer: Header Idempotency-Key. Body: { questionId, submittedAnswer }. Server chấm điểm, trừ Tim nếu sai, cộng XP/Combo nếu đúng, trả về kết quả kèm explanation[cite: 1, 3].
- POST /api/v1/sessions/{id}/complete: Chốt phiên, thưởng bài học hoàn hảo, cập nhật Streak, mở khóa node tiếp theo và cập nhật bài tập trường giao (nếu có)[cite: 1, 3].

### 6.3. Practice & Leaderboards
- GET /api/v1/practice/wrong-questions: Danh sách các câu hỏi đã từng làm sai[cite: 1, 3].
- POST /api/v1/practice/answer: Nộp đáp án ôn tập -> Nếu đúng: Hồi +1 Tim, cộng +5 XP[cite: 1, 3].
- GET /api/v1/leaderboard/current: Top 30 người có XP tuần cao nhất cùng vị trí của người dùng hiện tại[cite: 1, 3].

### 6.4. Phân hệ B2B Trường học & Giáo viên (Coursera Module)
- POST /api/v1/organizations/teacher-licenses/claim: Giáo viên nhập mã kích hoạt nhận quyền Educator và danh sách lớp phụ trách[cite: 4].
- GET /api/v1/educator/classes: Danh sách các lớp giáo viên đang quản lý[cite: 4].
- POST /api/v1/classes/{classId}/assignments: Giáo viên giao bài tập tình huống cho lớp (chọn node, đặt deadline, ngưỡng đạt)[cite: 1, 4].
- GET /api/v1/classes/{classId}/heatmap: Bản đồ nhiệt rủi ro an toàn của lớp (thống kê các câu hỏi sai nhiều nhất)[cite: 1, 4].
- POST /api/v1/classes/join-by-code: Học sinh nhập mã lớp hoặc quét QR để tham gia vào lớp[cite: 1, 4].
- GET /api/v1/student/assignments: Danh sách các bài tập trường giao, deadline và trạng thái đạt[cite: 1, 4].

### 6.5. Curator Studio & Admin Gatekeeper
- POST /api/v1/curator/questions: Tạo câu hỏi tình huống mới (trạng thái Draft)[cite: 1, 4].
- POST /api/v1/curator/questions/{id}/submit-approval: Gửi bài lên hàng chờ Admin duyệt (PendingApproval)[cite: 1, 4].
- POST /api/v1/admin/content-approval/{id}/publish: [Admin only] Phê duyệt xuất bản học liệu (Published)[cite: 4].
- POST /api/v1/admin/content-approval/{id}/reject: [Admin only] Từ chối xuất bản và gửi yêu cầu chỉnh sửa[cite: 4].
- POST /api/v1/admin/content-approval/{id}/emergency-rollback: [Admin only] Thu hồi khẩn cấp bài học live[cite: 4].

---

## 7. TIÊU CHUẨN KỸ THUẬT & CODE CONVENTIONS

### 7.1. Chuẩn C# & ASP.NET Core (.NET 8)
- Sử dụng cú pháp C# hiện đại: file-scoped namespace, primary constructor, required properties, pattern matching[cite: 3].
- Tất cả phương thức async phải truyền CancellationToken cancellationToken = default[cite: 3].
- Tuyệt đối cấm sử dụng .Result hoặc .Wait() gây nghẽn ThreadPool[cite: 3].
- Tách biệt logic kiểm tra dữ liệu đầu vào bằng FluentValidation[cite: 3].

### 7.2. Chuẩn EF Core & PostgreSQL Conventions
- Đăng ký tự động chuyển đổi định danh snake_case trong DbContextOptions: optionsBuilder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();[cite: 1, 3]
- Bắt buộc cấu hình quan hệ bằng Fluent API độc lập (IEntityTypeConfiguration<TEntity>), không viết lẫn vào OnModelCreating[cite: 3].
- Toàn bộ các câu truy vấn chỉ đọc (read-only queries) bắt buộc gắn .AsNoTracking()[cite: 3].
- Sử dụng DeleteBehavior.Restrict trên các quan hệ trọng yếu để tránh xóa dây chuyền dữ liệu người học[cite: 3].

### 7.3. Cấu trúc Seed Dữ liệu Demo (15 Tình huống Chuẩn)
Dự án yêu cầu chuẩn bị file seed dữ liệu gồm 15 tình huống thực tế (tương đương 30 câu hỏi) chia đều cho 3 bài học demo của Phase 1[cite: 1]:
1. Bài 1: Sơ cấp cứu ban đầu (5 tình huống - 10 câu): Bỏng nước sôi sinh hoạt, đuối nước, hóc dị vật đường thở (Heimlich), ngừng tuần hoàn hô hấp (CPR), gãy xương kín[cite: 1, 4].
2. Bài 2: PCCC & Thoát hiểm (5 tình huống - 10 câu): Cháy chung cư mini ban đêm, chập điện tủ lạnh bốc khói, rò rỉ khí gas nhà bếp, sử dụng bình chữa cháy mini, thoát khỏi hành lang ngạt khói độc[cite: 1, 4].
3. Bài 3: An toàn số & Chống lừa đảo (5 tình huống - 10 câu): Cuộc gọi video deepfake mạo danh công an, tin nhắn SMS chứa link trúng thưởng giả mạo, thủ đoạn lừa cài app độc hại chiếm quyền OTP, rò rỉ thông tin cá nhân căn cước, bẫy tuyển cộng tác viên online[cite: 1, 4].

Mỗi câu hỏi seed bắt buộc có đầy đủ: title, type, payload, correct_data, explanation, difficulty, source (Bộ Y tế / Cục Cảnh sát PCCC / Bộ Công an), reviewed_by[cite: 1, 3].

---

## 8. CHECKLIST KIỂM SOÁT CHẤT LƯỢNG KHI SINH MÃ (AGENT SELF-AUDIT)

Trước khi bàn giao bất kỳ file mã nguồn nào, Agent bắt buộc phải tự đối chiếu checklist sau:
1. [ ] Chống lộ đáp án: DTO trả về cho màn hình làm bài tuyệt đối không serialize trường CorrectData[cite: 1, 3].
2. [ ] Tính toán Múi giờ: Các mốc thời gian lưu UTC, nhưng tính Streak và chu kỳ ngày phải convert sang Asia/Ho_Chi_Minh[cite: 1, 3].
3. [ ] Sổ cái XP: Mọi cộng dồn XP đều tạo một record vào XpEvents, không cập nhật độc lập cột TotalXp ở bảng Users[cite: 1, 3].
4. [ ] Combo State: Khi học sinh trả lời sai, đã reset current_combo_streak = 0 trong session chưa[cite: 3]?
5. [ ] Trạng thái xuất bản: Bài học mới tạo phải là Draft hoặc PendingApproval; API học sinh làm bài chỉ truy vấn nội dung có trạng thái Published[cite: 1, 4].
6. [ ] Race Condition khi quét QR: Thao tác tham gia lớp học có kiểm tra và trừ seat của trường bằng Transaction và Lock an toàn chưa[cite: 1, 4]?
7. [ ] Tuân thủ Onion Architecture: Tầng Domain không phụ thuộc tầng Infrastructure/WebAPI; Controller chỉ gọi qua tầng Application[cite: 3].