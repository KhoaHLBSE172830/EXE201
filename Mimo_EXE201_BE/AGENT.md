# ==============================================================================
# MIMO BACKEND ENGINE - AGENT INSTRUCTION MANUAL & ARCHITECTURE SPECIFICATION
# Project: MIMO - Gamified Life Skills & Emergency Safety (EXE201)
# Architecture: Onion Architecture (Clean Architecture)
# Stack: ASP.NET Core (.NET 8) Web API | PostgreSQL (pgAdmin 4) | EF Core
# Timezone: Asia/Ho_Chi_Minh
# Target: Production-ready MVP & Phase-based Expansion
# ==============================================================================

## 1. TỔNG QUAN HỆ THỐNG & VAI TRÒ AGENT

### 1.1. Bối cảnh dự án
MIMO là nền tảng học kỹ năng sống và phản xạ an toàn khẩn cấp (Sơ cấp cứu, PCCC & Thoát hiểm, An toàn số & Chống lừa đảo, An toàn giao thông, Sức khỏe tinh thần) ứng dụng triệt để mô hình Micro-learning kết hợp Gamification mô phỏng Duolingo. Hệ thống biến lý thuyết khô khan thành các quyết định tình huống thực chiến 2–3 phút, kích thích người học qua các cơ chế Tim (Hearts), Chuỗi ngày (Streak), Điểm kinh nghiệm (XP), Cấp độ (Levels) và Bảng xếp hạng tuần (Weekly Leagues).

### 1.2. Hệ thống Phân quyền 5 Roles (RBAC)
Hệ thống có **5 Vai trò người dùng độc lập** được định danh bằng GUID chuẩn:

| Enum Value | Role Name | GUID Seed | Mô tả |
|---|---|---|---|
| `Admin = 1` | Admin | `11111111-1111-1111-1111-111111111111` | Chủ hệ thống / Quản trị viên tối cao |
| `Student = 2` | Student | `22222222-2222-2222-2222-222222222222` | Người học (Learner) — B2C & B2B end-user |
| `Curator = 3` | Curator | `33333333-3333-3333-3333-333333333333` | Chuyên gia biên soạn nội dung độc lập |
| `Organization = 4` | Organization | `44444444-4444-4444-4444-444444444444` | Ban giám hiệu / Quản lý nhà trường B2B |
| `Educator = 5` | Educator | `55555555-5555-5555-5555-555555555555` | Giáo viên trực thuộc trường học |

**Nguồn chân lý:** `Mimo.DomainLayer.Enum.GeneralEnum.UserRoleEnum`  
**Seed tự động:** `DatabaseSeeder.SeedRolesAsync()` khi khởi động môi trường Development.

> [!IMPORTANT]
> - **Admin**: Hệ thống chỉ cho phép **DUY NHẤT 1 Admin** — enforced bằng Partial Unique Index PostgreSQL trên cột `RoleId`.
> - **Educator** khác **Curator**: Educator thuộc biên chế một trường cụ thể (`OrganizationId` NOT NULL), Curator hoạt động độc lập với mọi trường (`OrganizationId` có thể NULL).
> - **Không thể tự đăng ký với Role Admin** — enforced trong `AuthService.RegisterAsync()` và `RegisterRequestValidator`.

### 1.2. Vai trò của Agent
Khi tiếp nhận và thực thi các task mã nguồn, Agent đóng vai trò là một **Senior .NET Backend Architect & Lead Developer**. Agent có trách nhiệm:
- Tuân thủ nghiêm ngặt nguyên lý SOLID, mô hình phân lớp cô lập của Onion Architecture.
- Đảm bảo tính mở rộng cao để dễ dàng tích hợp các phân hệ Phase 2 (Curator CMS, Organization B2B, Admin Portal) và Phase 3 (Thanh toán Mimo Pro, Brand Sponsorship).
- Bảo vệ tuyệt đối nguyên tắc **"Server là nguồn sự thật duy nhất (Single Source of Truth)"**: Toàn bộ việc chấm điểm, trừ tim, tính streak, cộng combo XP đều thực thi tại backend.
- Đảm bảo tính nhất quán dữ liệu trước các bài toán tương tranh (Concurrency), gian lận thời gian và sự cố lệch múi giờ.

---

## 2. QUY TẮC CỐT LÕI: ATTENTION & CRITICAL WARNINGS

> [!CAUTION]
> ### ⚠️ CRITICAL WARNING 1: KHÔNG BAO GIỜ TRẢ ĐÁP ÁN ĐÚNG VỀ PHÍA CLIENT
> - Endpoint lấy nội dung câu hỏi bài học (`GET /lessons/{nodeId}/questions` hoặc payload khởi tạo phiên học `POST /lessons/{nodeId}/start`) **TUYỆT ĐỐI KHÔNG** được chứa trường `is_correct`, `correct_answer`, `correct_data` hoặc bất kỳ dấu vết nào giúp client suy luận được đáp án đúng trước khi trả lời.
> - Việc để lộ đáp án ở client sẽ phá hủy toàn bộ cơ chế game: người dùng hack client để giữ Tim, farm XP vô hạn và thao túng bảng xếp hạng.
> - Việc đối chiếu và chấm điểm diễn ra 100% tại backend thông qua `POST /sessions/{id}/answer`.

> [!CAUTION]
> ### ⚠️ CRITICAL WARNING 2: QUẢN LÝ THỜI GIAN & CHU KỲ MÚI GIỜ (TIMEZONE)
> - Toàn bộ các cột lưu trữ mốc thời gian trong PostgreSQL **BẮT BUỘC** dùng kiểu `timestamptz` (Timestamp with time zone) và lưu ở chuẩn **UTC**.
> - Không dùng `DateTime.Now` của server (tránh rủi ro khi deploy trên server chạy múi giờ khác). Bắt buộc sử dụng `DateTimeOffset.UtcNow` hoặc trừu tượng hóa qua `IDateTimeProvider`.
> - Logic tính chu kỳ ngày cho Streak, reset nhiệm vụ và hạn chốt League **PHẢI** convert từ UTC sang múi giờ người dùng cấu hình (mặc định là `Asia/Ho_Chi_Minh` - UTC+7) trước khi bóc tách giá trị ngày `DateOnly` (`YYYY-MM-DD`).

> [!WARNING]
> ### ⚠️ ATTENTION 3: TRỪ TIM & CỘNG XP PHẢI DÙNG DATABASE TRANSACTION & OPTIMISTIC CONCURRENCY
> - Khi học sinh thao tác nhanh, gửi đồng thời nhiều request hoặc mở ứng dụng trên hai thiết bị, các thao tác trừ Tim và cộng XP dễ phát sinh Race Condition.
> - Nghiệp vụ nộp câu hỏi và hoàn thành bài học phải được bọc trong Database Transaction.
> - Áp dụng `xmin` (Row Versioning của PostgreSQL) hoặc cơ chế Optimistic Locking trên bảng `user_stats`.
> - Endpoint nộp câu hỏi bắt buộc hỗ trợ header `Idempotency-Key` để loại trừ các payload nộp trùng lặp do mạng lag.

> [!WARNING]
> ### ⚠️ ATTENTION 4: LEDGER-BASED XP SYSTEM (SỔ CÁI XP BẤT BIẾN)
> - Điểm kinh nghiệm (XP) là tài nguyên gamification cốt lõi, không được phép cập nhật đơn thuần bằng lệnh `UPDATE users SET total_xp = total_xp + @xp`.
> - Mọi biến động XP **BẮT BUỘC** phải ghi một record vào sổ cái `xp_events` kèm theo `source` (`lesson`, `combo`, `perfect`, `chest`, `practice`) và `reference_id`.
> - Trường `total_xp` ở bảng `users` đóng vai trò là snapshot/cache để truy vấn nhanh, và phải luôn đối soát khớp với `SUM(amount)` trong bảng `xp_events`.
> - Level của người dùng (1–5) là hàm suy biến thuần túy từ `total_xp`, không lưu trường level độc lập gây lệch pha dữ liệu.

---

## 3. KIẾN TRÚC ONION (ONION ARCHITECTURE)

Hệ thống được module hóa thành 4 phân lớp theo nguyên lý Dependency Inversion:

```
┌──────────────────────────────────────────────────────────┐
│              MIMO.Presentation (WebAPI)                  │
│       Controllers, Middlewares, Filters, Swagger         │
└────────────────────────────┬─────────────────────────────┘
                             │ (References)
┌────────────────────────────▼─────────────────────────────┐
│           MIMO.Infrastructure (External Concerns)         │
│  EF Core, Postgres, Migrations, Repositories, Redis, Auth│
└────────────────────────────┬─────────────────────────────┘
                             │ (References)
┌────────────────────────────▼─────────────────────────────┐
│             MIMO.Application (Business Logic)            │
│  Commands, Queries, DTOs, Interfaces, Validators, Rules  │
└────────────────────────────┬─────────────────────────────┘
                             │ (References)
┌────────────────────────────▼─────────────────────────────┐
│                 MIMO.Domain (Enterprise Core)            │
│    Entities, Value Objects, Enums, Domain Exceptions     │
└──────────────────────────────────────────────────────────┘
```

### 3.1. Phân chia trách nhiệm từng lớp
1. **`MIMO.Domain` (Core - Trọng tâm bất biến):**
   - Chứa thực thể (`User`, `Node`, `Question`, `LessonSession`, `UserStats`, `XpEvent`), Value Objects, Domain Exceptions (`OutOfHeartsException`, `SessionExpiredException`), Domain Enums.
   - **Tuyệt đối cấm:** Không tham chiếu đến bất kỳ project nào khác, không dùng namespace ASP.NET Core hay Entity Framework Core trong lớp này.
2. **`MIMO.Application` (Tầng điều phối nghiệp vụ):**
   - Chứa Interfaces cho Data Access (`IUnitOfWork`, `IUserRepository`, `ILessonRepository`), Services (`IGameplayEngine`, `IStreakService`, `IHeartService`), DTOs, Mappings, FluentValidation.
   - Chịu trách nhiệm thực thi các workflow: bắt đầu bài học, chấm điểm đáp án, xử lý tăng chuỗi ngày, kích hoạt hồi phục tim.
3. **`MIMO.Infrastructure` (Tầng triển khai kỹ thuật & I/O):**
   - Chứa `AppDbContext`, các file `IEntityTypeConfiguration<T>`, Npgsql connection, triển khai Repository.
   - Tích hợp dịch vụ bên thứ ba: Xác thực Google Token (`Google.Apis.Auth`), thư viện băm mật khẩu, Redis (Phase 2), Object Storage cho media.
4. **`MIMO.Presentation` / `MIMO.WebApi` (Cổng giao tiếp):**
   - ASP.NET Core Web API Controllers cấu hình theo chuẩn RESTful (`/api/v1/...`).
   - Cấu hình Global Exception Handling Middleware (chuẩn hóa lỗi RFC 7807 ProblemDetails), cấu hình JWT Bearer Authentication, RBAC Authorization Handlers, OpenAPI/Swagger UI, Rate Limiter.

---

## 4. LƯỢC ĐỒ DỮ LIỆU POSTGRESQL (SCHEMA SPECIFICATION)

Quy chuẩn: Tên bảng C# Entity dùng `PascalCase` (EF Core tự map sang `snake_case` bằng `UseSnakeCaseNamingConvention` nếu bật, hoặc dùng `[Table("...")] attribute`). Primary Key `Guid` (UUID) — tự sinh tại client trước khi Insert.

> [!NOTE]
> **Mapping thực tế DB**: Xem `Mimo.Infrastructure/Database/MimoDbContext.cs` và `Mimo.Domain/Entities/`. Phần schema bên dưới mô tả **thiết kế mục tiêu** cho Gameplay Engine (Phase 1 MVP). Các bảng đã migrate sẵn (✅) và các bảng cần tạo mới (🔲) được đánh dấu rõ ràng.

### 4.1. Phân hệ Tài khoản & Phân quyền (Auth & RBAC)

#### ✅ Bảng `Roles` (Entity: `Role.cs`)
- `Id (UUID, PK)`: Định danh vai trò.
- `RoleName (VARCHAR(20), UNIQUE, NOT NULL)`: `Admin`, `Student`, `Curator`, `Organization`, `Educator`.
- `Description (VARCHAR(100), NULL)`: Mô tả vai trò.
- `Status (VARCHAR, NOT NULL, DEFAULT Active)`: Enum `RoleStatusEnum`.
- `CreatedAt`, `UpdatedAt (TIMESTAMPTZ, NOT NULL)`: Từ `BaseEntity`.

#### ✅ Bảng `Users` (Entity: `User.cs`)
- `Id (UUID, PK)`: Định danh người dùng.
- `RoleId (UUID, FK → Roles.Id, NOT NULL)`: Phân quyền qua quan hệ FK, **không lưu string tên role**.
- `OrganizationId (UUID, FK → Organizations.Id, NULL)`: Liên kết trường — bắt buộc với Educator, tùy chọn với Student.
- `Name (VARCHAR(50), NOT NULL)`: Họ và tên đầy đủ (`NotMapped` alias: `FullName`).
- `UserName (VARCHAR(40), UNIQUE, NULL)`: Tên đăng nhập.
- `Email (VARCHAR, UNIQUE, NOT NULL)`: Email đăng nhập.
- `Password (VARCHAR(200), NOT NULL)`: BCrypt hash (`NotMapped` alias: `PasswordHash`).
- `Phone (VARCHAR(11), NULL)` (`NotMapped` alias: `PhoneNumber`).
- `Avatar (VARCHAR(1000), NULL)` (`NotMapped` alias: `AvatarUrl`).
- `AuthType (VARCHAR(50), DEFAULT 'Local')`: `Local` hoặc `Google`.
- `RefreshToken (VARCHAR(255), NULL)`, `RefreshTokenExpiryTime (TIMESTAMPTZ, NULL)`.
- `Timezone (VARCHAR(50), DEFAULT 'Asia/Ho_Chi_Minh')`.
- `TotalXp (INT, DEFAULT 0)`: Cache/Snapshot tổng XP — phải đồng bộ với `SUM(xp_events.amount)`.
- `Status (VARCHAR, DEFAULT Active)`: Enum `UserStatusEnum`.
- `LastLogin (TIMESTAMPTZ, NULL)`.
- **Partial Unique Index PostgreSQL**: `UNIQUE` trên `RoleId` với `WHERE RoleId = '11111111-...'` — đảm bảo **chỉ 1 Admin duy nhất** toàn hệ thống.

#### ✅ Bảng `UserProfiles` (Entity: `UserProfile.cs`) — quan hệ 1-1 với Users
- `UserId (UUID, PK, FK → Users.Id ON DELETE CASCADE)`.
- `FullName (VARCHAR(150), NOT NULL)`.
- `AvatarUrl (TEXT, NULL)`.
- `DailyGoalMinutes (INT, DEFAULT 10)`: Mục tiêu học (5, 10, 15 phút).
- `RiskSurveyResult (JSONB, NULL)`: Kết quả khảo sát nhận diện rủi ro onboarding.

#### ✅ Bảng `UserStats` (Entity: `UserStats.cs`) — quan hệ 1-1 với Users
- `UserId (UUID, PK, FK → Users.Id ON DELETE CASCADE)`.
- `Hearts (INT, DEFAULT 5, CHECK 0-5)`: Số tim hiện tại.
- `HeartsUpdatedAt (TIMESTAMPTZ)`: Mốc tính Lazy Heart Regeneration.
- `Streak (INT, DEFAULT 0)`: Chuỗi ngày liên tiếp.
- `LastActiveDate (DATE, NULL)`: Ngày học gần nhất theo múi giờ user.
- `StreakFreezeCount (INT, DEFAULT 0)`: Số bùa bảo vệ Streak.
- `IsPro (BOOLEAN, DEFAULT false)`: Tài khoản Mimo Pro (vô hạn tim).
- `ProExpiresAt (TIMESTAMPTZ, NULL)`: Thời hạn Pro.

> [!IMPORTANT]
> `UserStats.CalculateCurrentHearts(DateTime currentUtc)` đã được implement **sẵn trong Domain Entity** — đây là Lazy Regeneration không cần Background Worker. Agent **PHẢI** gọi method này thay vì tự tính lại.

#### ✅ Bảng `Organizations` (Entity: `Organization.cs`)
- Đại diện trường học / tổ chức B2B. FK từ `Users.OrganizationId`.
- Chứa `Code (VARCHAR, UNIQUE)`: Mã tổ chức dùng để Student/Educator gia nhập qua API Register.

---

### 4.2. Cấu trúc Cây Kỹ năng & Nội dung Học (Content Domain)

> [!WARNING]
> **Các bảng sau (4.2) CHƯA ĐƯỢC TẠO MIGRATION** — đây là thiết kế cần tạo Entity mới trong Phase 1 Gameplay Engine. Trước khi implement Task B1-B5, Agent BẮT BUỘC tạo đủ các Entity này và chạy `dotnet ef migrations add`.

#### 🔲 Bảng `Domains` (Entity mới: `Domain.cs`)
- `Id (UUID, PK)`, `Name (VARCHAR(100), NOT NULL)`, `Slug (VARCHAR(100), UNIQUE, NOT NULL)`.
- `IconUrl (TEXT, NULL)`, `Order (INT, NOT NULL, DEFAULT 1)`, `Status (VARCHAR(20), DEFAULT 'Active')`.
- **5 Chủ đề cốt lõi**: Sơ cấp cứu, PCCC & Thoát hiểm, An toàn giao thông, An toàn số, Sức khỏe tinh thần.

#### 🔲 Bảng `Units` (Entity mới: `Unit.cs`) — Chặng kỹ năng
- `Id (UUID, PK)`, `DomainId (UUID, FK → Domains.Id, NOT NULL)`.
- `Title (VARCHAR(150), NOT NULL)`, `Description (TEXT, NULL)`, `Order (INT, NOT NULL)`.

#### 🔲 Bảng `Nodes` (Entity mới: `Node.cs`) — Nút bài học trên Skill Tree
- `Id (UUID, PK)`, `UnitId (UUID, FK → Units.Id, NOT NULL)`.
- `Title (VARCHAR(150), NOT NULL)`.
- `Type (VARCHAR(30), NOT NULL)`: `lesson` | `chest` (rương thưởng) | `boss` (thử thách chặng).
- `Order (INT, NOT NULL)`: Thứ tự trong unit.
- `UnlockRule (JSONB, NULL)`: Quy tắc mở khóa, vd: `{ "prerequisite_node_id": "uuid" }`.
- `XpReward (INT, DEFAULT 10)`: XP thưởng khi hoàn thành node.

#### 🔲 Bảng `Questions` (Entity mới: `Question.cs`) — Ngân hàng câu hỏi tình huống
- `Id (UUID, PK)`.
- `Type (VARCHAR(30), NOT NULL)`: `choice` | `sequence` | `red_flag`.
- `Title (TEXT, NOT NULL)`: Nội dung tình huống.
- `ImageUrl (TEXT, NULL)`: Ảnh/GIF minh họa.
- `Payload (JSONB, NOT NULL)`: **Dữ liệu gửi về client — TUYỆT ĐỐI KHÔNG chứa đáp án đúng.**
- `CorrectData (JSONB, NOT NULL)`: Dữ liệu đáp án đúng — **CHỈ tồn tại tại server, không bao giờ serialize về client.**
- `Explanation (TEXT, NOT NULL)`: Giải thích chuyên môn sau khi học sinh trả lời.
- `Difficulty (VARCHAR(20), DEFAULT 'Medium')`: `Easy` | `Medium` | `Hard`.
- `Source (VARCHAR(255), NULL)`: Nguồn tham chiếu (Bộ Y tế, Cục PCCC...).
- `ReviewedBy (VARCHAR(150), NULL)`: Chuyên gia duyệt.
- `Status (VARCHAR(20), DEFAULT 'Published')`.

#### 🔲 Bảng `NodeQuestions` (Entity mới: `NodeQuestion.cs`) — Pivot
- `NodeId (UUID, FK → Nodes.Id)`, `QuestionId (UUID, FK → Questions.Id)`.
- `Order (INT, NOT NULL)`: Thứ tự câu hỏi trong bài học.
- `PRIMARY KEY (NodeId, QuestionId)`.

---

### 4.3. Gameplay Engine, Luyện tập & Sổ cái XP

> [!WARNING]
> Tất cả các bảng trong Section 4.3 đều **CHƯA CÓ ENTITY/MIGRATION** và cần được tạo trước khi triển khai Task B1-B5.

#### 🔲 Bảng `UserNodeProgresses` (Entity mới: `UserNodeProgress.cs`)
- `Id (UUID, PK)`, `UserId (UUID, FK → Users.Id)`, `NodeId (UUID, FK → Nodes.Id)`.
- `Status (VARCHAR(20), NOT NULL)`: `locked` | `current` | `done`.
- `CompletedAt (TIMESTAMPTZ, NULL)`.
- `UNIQUE (UserId, NodeId)`.

#### 🔲 Bảng `LessonSessions` (Entity mới: `LessonSession.cs`) — State Machine phiên học
- `Id (UUID, PK)`, `UserId (UUID, FK → Users.Id)`, `NodeId (UUID, FK → Nodes.Id)`.
- `Status (VARCHAR(20), DEFAULT 'in_progress')`: `in_progress` | `completed` | `failed`.
- `HeartsLost (INT, DEFAULT 0)`: Số tim đã mất trong phiên.
- `TotalXpEarned (INT, DEFAULT 0)`: Tổng XP kiếm được trong phiên.
- `StartedAt (TIMESTAMPTZ, NOT NULL)`, `ExpiresAt (TIMESTAMPTZ, NOT NULL)` *(= StartedAt + 30 phút)*.
- `CompletedAt (TIMESTAMPTZ, NULL)`.

> [!CAUTION]
> Phiên học có TTL 30 phút. Nếu `ExpiresAt < UtcNow` khi nhận answer request → mark `failed` và từ chối chấm điểm. **Không được extend thời gian phiên.**

#### 🔲 Bảng `SessionAnswers` (Entity mới: `SessionAnswer.cs`)
- `Id (UUID, PK)`, `SessionId (UUID, FK → LessonSessions.Id)`, `QuestionId (UUID, FK → Questions.Id)`.
- `SubmittedAnswer (JSONB, NOT NULL)`: Đáp án học sinh gửi lên.
- `IsCorrect (BOOLEAN, NOT NULL)`: Kết quả chấm.
- `TimeSpentSeconds (INT, NOT NULL)`: Thời gian trả lời.
- `XpEarned (INT, DEFAULT 0)`: XP cộng cho câu này (bao gồm combo nếu có).
- `IdempotencyKey (VARCHAR(100), NULL)`: Guard chống submit trùng.
- `CreatedAt (TIMESTAMPTZ, NOT NULL)`.
- `UNIQUE (SessionId, QuestionId)`: Mỗi câu chỉ được nộp 1 lần trong 1 phiên.

#### 🔲 Bảng `WrongQuestions` (Entity mới: `WrongQuestion.cs`)
- `Id (UUID, PK)`, `UserId (UUID, FK → Users.Id)`, `QuestionId (UUID, FK → Questions.Id)`.
- `TimesWrong (INT, DEFAULT 1)`: Số lần làm sai lũy kế.
- `ResolvedAt (TIMESTAMPTZ, NULL)`: NULL = chưa ôn xong; NOT NULL = đã trả lời đúng trong Practice.
- `LastAttemptAt (TIMESTAMPTZ, NOT NULL)`.
- `UNIQUE (UserId, QuestionId)`: Upsert — tăng `TimesWrong` nếu đã tồn tại.

#### 🔲 Bảng `XpEvents` (Entity mới: `XpEvent.cs`) — Append-Only Ledger
- `Id (BIGINT / BIGSERIAL, PK)`: Auto-increment, không dùng UUID.
- `UserId (UUID, FK → Users.Id, NOT NULL, Indexed)`.
- `Amount (INT, NOT NULL)`: Lượng XP thay đổi (luôn dương).
- `Source (VARCHAR(50), NOT NULL)`: `lesson_correct` | `combo_bonus` | `perfect_lesson` | `chest_reward` | `practice_reward`.
- `ReferenceId (UUID, NULL)`: FK mềm trỏ đến SessionId hoặc NodeId nguồn gốc.
- `CreatedAt (TIMESTAMPTZ, NOT NULL, Indexed)`.

---

## 5. CƠ CHẾ LOGIC GAMEPLAY & THUẬT TOÁN GAMIFICATION

### 5.1. Thuật toán Quản lý & Hồi phục Tim (Hearts Service)
- **Quy tắc cơ bản:** Học sinh thông thường có tối đa 5 Tim. Chọn sai 1 quyết định: `-1 Tim`. Hết Tim (0 Tim): Không thể khởi tạo phiên học bài mới.
- **Tài khoản Pro (`is_pro = true`):** Luôn bỏ qua điều kiện kiểm tra số tim.
- **Lazy Heart Regeneration (Thuật toán hồi tim không dùng Background Worker):**
  Khi client truy vấn trạng thái hoặc gọi bắt đầu bài học, backend thực hiện tính lười:
  $$\Delta t = \text{CurrentUtc} - \text{hearts\_updated\_at}$$
  $$\text{recovered\_hearts} = \left\lfloor \frac{\Delta t}{4 \text{ hours}} \right\rfloor$$
  $$\text{new\_hearts} = \min(5, \text{current\_hearts} + \text{recovered\_hearts})$$
  Nếu $\text{new\_hearts} > \text{current\_hearts}$:
  - Cập nhật `hearts = new_hearts`.
  - Cập nhật lại `hearts_updated_at = hearts_updated_at + (\text{recovered\_hearts} \times 4 \text{ hours})`.
  - Lưu vào database trong transaction.
- **Cơ chế Luyện tập kiếm Tim (Practice to Earn Heart):**
  - Lấy danh sách các câu hỏi từ bảng `wrong_questions` có `resolved_at IS NULL`.
  - Khi người học trả lời đúng câu sai: đánh dấu `resolved_at = NowUtc`, cộng $+1\text{ Tim}$ (nếu $\text{hearts} < 5$) và ghi nhận $+5\text{ XP}$ (nguồn `practice_reward`).

### 5.2. Thuật toán Kiểm tra & Cập nhật Chuỗi ngày (Daily Streak Calculation)
Logic được thực thi duy nhất tại thời điểm hoàn thành bài học (`POST /sessions/{id}/complete`):
1. Chuyển đổi thời gian hiện tại từ UTC sang múi giờ của người học (mặc định: `Asia/Ho_Chi_Minh`):
   $$D_{\text{current}} = \text{DateOnly.FromDateTime}(\text{TimezoneHelper.ToUserTime}(\text{UtcNow}, \text{user.Timezone}))$$
2. Lấy $D_{\text{last}} = \text{user\_stats.last\_active\_date}$.
3. Xét các trường hợp:
   - **Trường hợp 1:** $D_{\text{current}} == D_{\text{last}}$
     $\Rightarrow$ Đã học bài trước đó trong ngày: Giữ nguyên `streak`, không tăng thêm.
   - **Trường hợp 2:** $D_{\text{current}} == D_{\text{last}} + 1\text{ day}$
     $\Rightarrow$ Học đều đặn ngày tiếp theo: `streak = streak + 1`, cập nhật $D_{\text{last}} = D_{\text{current}}$.
   - **Trường hợp 3:** $D_{\text{current}} \ge D_{\text{last}} + 2\text{ days}$ (Bỏ lỡ từ 1 ngày trở lên):
     - Nếu $\text{streak\_freeze\_count} \ge 1$: Tiêu thụ 1 bùa (`streak_freeze_count = streak_freeze_count - 1`), giữ nguyên `streak`, cập nhật $D_{\text{last}} = D_{\text{current}}$, trả về cờ thông báo bảo vệ chuỗi thành công (`freeze_consumed = true`).
     - Nếu không có bùa freeze: Reset chuỗi về `streak = 1`, cập nhật $D_{\text{last}} = D_{\text{current}}$.

### 5.3. Quy tắc Điểm thưởng XP, Combo & Level
* **Điểm thưởng chuẩn:** Trả lời đúng một câu hỏi: $+10\text{ XP}$.
* **Điểm thưởng Combo liên hoàn trong cùng một bài học (10 câu):**
  - Đúng 3 câu liên tiếp: $+2\text{ XP}$.
  - Đúng 5 câu liên tiếp: $+3\text{ XP}$.
  - Đúng 7 câu liên tiếp: $+4\text{ XP}$.
  - Đúng 10 câu liên tiếp: $+5\text{ XP}$.
* **Thưởng Hoàn hảo (Perfect Lesson):** Hoàn thành bài học 10 câu mà không mất tim nào (`hearts_lost == 0`): $+5\text{ XP}$.
* **Công thức xác định Level (Dẫn xuất từ `total_xp`):**
  $$Level = \begin{cases} 
  1 & \text{khi } 0 \le \text{total\_xp} < 200 \quad \text{(Tân binh an toàn)} \\
  2 & \text{khi } 200 \le \text{total\_xp} < 500 \quad \text{(Người phòng vệ - Defender)} \\
  3 & \text{khi } 500 \le \text{total\_xp} < 1000 \quad \text{(Người giải cứu - Rescuer)} \\
  4 & \text{khi } 1000 \le \text{total\_xp} < 2000 \quad \text{(Chuyên gia an toàn - Safety Guardian)} \\
  5 & \text{khi } \text{total\_xp} \ge 2000 \quad \text{(Người làm chủ cuộc sống - Life Master)}
  \end{cases}$$

### 5.4. Chống gian lận phiên học (Anti-Cheat & Session Guard)
- Mỗi phiên học (`lesson_sessions`) có thời gian sống tối đa `expires_at = started_at + 30 minutes`. Nếu hết hạn, phiên bị chuyển thành `failed` và từ chối nộp câu hỏi.
- Không thể nộp lại câu hỏi đã có kết quả trong cùng một phiên học (`UNIQUE(session_id, question_id)`).
- Nếu giữa bài học người dùng hết Tim (`hearts == 0` và `is_pro == false`), phiên học kết thúc với trạng thái `failed`. Toàn bộ XP từ các câu trả lời đúng trước đó được giữ lại trong sổ cái `xp_events`, nhưng **KHÔNG** cập nhật Streak và **KHÔNG** mở khóa bài tiếp theo.

---

## 6. QUY CÁCH API & GIAO THỨC TRUYỀN THÔNG (REST API CONTRACTS)

### 6.1. Định dạng phản hồi lỗi (RFC 7807 Problem Details)
Mọi lỗi nghiệp vụ hoặc exception hệ thống phải được chuyển đổi thành cấu trúc chuẩn:
```json
{
  "type": "https://mimo.edu.vn/errors/out-of-hearts",
  "title": "Out of Hearts",
  "status": 400,
  "detail": "Bạn đã hết Tim để tiếp tục bài học. Hãy tham gia chế độ Luyện tập để hồi tim.",
  "instance": "/api/v1/lessons/9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d/start",
  "extensions": {
    "currentHearts": 0,
    "nextHeartInSeconds": 8420
  }
}
```

### 6.2. Danh mục Endpoints Cốt lõi Phase 1 (MVP)

#### ✅ Đã implement (Controller hiện có)
* **Auth & Profiles:**
  - `POST /api/v1/auth/register`: Đăng ký tài khoản email + mật khẩu (hỗ trợ 4 Role: Student, Curator, Organization, Educator).
  - `POST /api/v1/auth/login`: Xác thực và trả JWT Pair (Access Token + Refresh Token).
  - `POST /api/v1/auth/google`: Tiếp nhận Google ID Token từ FE, verify bằng `Google.Apis.Auth`, tạo user nếu chưa có và phát hành JWT.
  - `POST /api/v1/auth/refresh-token`: Cấp mới Access Token bằng Refresh Token còn hạn.
  - `POST /api/v1/auth/logout`: Thu hồi session Redis và vô hiệu hóa Refresh Token.
  - `POST /api/v1/auth/force-logout/{userId}`: **[Admin only]** Buộc đăng xuất mọi phiên của một user.
  - `GET /api/v1/auth/admin-only`: Endpoint test quyền Admin.
  - `GET /api/v1/auth/curator-only`: Endpoint test quyền Curator/Admin.
  - `GET /api/v1/auth/organization-only`: Endpoint test quyền Organization/Admin.
  - `GET /api/v1/auth/educator-only`: Endpoint test quyền Educator/Admin.
  - `GET /api/v1/users/me`: Lấy thông tin cá nhân, level (dẫn xuất từ XP), số tim (Lazy Regen), streak.
  - `PATCH /api/v1/users/me/onboarding`: Cập nhật mục tiêu học tập và kết quả khảo sát.
  - `PUT /api/v1/users/me/profile`: Cập nhật họ tên, avatar, phone, timezone.
  - `PUT /api/v1/users/me/change-password`: Đổi mật khẩu và tự động thu hồi session khác.

#### 🔲 Cần implement (Phase 1 Gameplay Engine — Task B1-B5)
* **Học liệu & Bản đồ Cây Kỹ Năng:**
  - `GET /api/v1/domains`: Danh sách 5 chủ đề lớn.
  - `GET /api/v1/path?domainId={id}`: Trả về cây lộ trình gồm các Node kèm trạng thái của người dùng (`locked`, `current`, `done`).
* **Gameplay Engine:**
  - `POST /api/v1/lessons/{nodeId}/start`: Kiểm tra tim ≥ 1 hoặc Pro, khởi tạo `LessonSession` (TTL 30 phút), trả về session ID và danh sách 10 câu hỏi **không có đáp án đúng**.
  - `POST /api/v1/sessions/{id}/answer`: Nhận `{ questionId, submittedAnswer }` + header `Idempotency-Key`. Chấm điểm 3 loại câu hỏi (choice/sequence/red_flag), trừ tim nếu sai, cộng XP + Combo nếu đúng, trả kết quả kèm `explanation`. Bọc trong DB Transaction.
  - `POST /api/v1/sessions/{id}/complete`: Tổng kết phiên, thưởng Perfect (+5 XP nếu `hearts_lost == 0`), cập nhật tiến độ node `done`, mở khóa node tiếp theo, gọi `IStreakService.UpdateStreakAsync()`.
* **Luyện tập (Practice):**
  - `GET /api/v1/practice/wrong-questions`: Lấy danh sách câu sai (`resolved_at IS NULL`) của user.
  - `POST /api/v1/practice/answer`: Nộp đáp án ôn tập — nếu đúng: `resolved_at = UtcNow`, `hearts + 1`, `+5 XP` (`source = practice_reward`).
* **Bảng xếp hạng (Weekly Leaderboard):**
  - `GET /api/v1/leaderboard/current`: Top 30 user có XP tuần cao nhất (tính từ 00:00 Thứ Hai giờ VN) kèm vị trí của người dùng hiện tại.

---

## 7. TIÊU CHUẨN KỸ THUẬT & QUY TẮC VIẾT MÃ (CODING STANDARDS)

### 7.1. Chuẩn C# & ASP.NET Core (.NET 8)
- Sử dụng cấu trúc C# hiện đại: `file-scoped namespace`, `primary constructor`, `init-only properties`, `pattern matching`.
- Tất cả phương thức I/O, Database, External Call bắt buộc dùng Async/Await kèm `CancellationToken cancellationToken = default`.
- Tuyệt đối cấm sử dụng `.Result`, `.Wait()` hoặc `GetAwaiter().GetResult()` gây nghẽn ThreadPool và Deadlock.
- Tách biệt hoàn toàn Request Validation bằng `FluentValidation`. Không nhồi nhét logic validate định dạng vào Controller Action.

### 7.2. Chuẩn EF Core & PostgreSQL Conventions
- Đăng ký Snake Case Naming Convention toàn diện:
  ```csharp
  optionsBuilder.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
  ```
- Định nghĩa schema bằng Fluent API độc lập (`IEntityTypeConfiguration<TEntity>`), không sử dụng DataAnnotations trên Domain Entities.
- Đối với các query chỉ đọc (read-only queries), bắt buộc gắn `.AsNoTracking()` để tối ưu hóa bộ nhớ và hiệu năng.
- Không lạm dụng Cascade Delete bừa bãi. Sử dụng `DeleteBehavior.Restrict` cho các quan hệ cha con giữa Node, Question, User để tránh thảm họa xóa nhầm dữ liệu học tập.

### 7.3. Cấu trúc Seed Dữ liệu Demo (15 Tình huống Chuẩn)
Dự án yêu cầu chuẩn bị file seed dữ liệu gồm 15 tình huống thực tế (tương đương 30 câu hỏi) chia đều cho 3 bài học demo của Phase 1:
1. **Bài 1: Sơ cấp cứu ban đầu (5 tình huống - 10 câu):** Bỏng nước sôi sinh hoạt, đuối nước, hóc dị vật đường thở (Heimlich), ngừng tuần hoàn hô hấp (CPR), gãy xương kín.
2. **Bài 2: PCCC & Thoát hiểm (5 tình huống - 10 câu):** Cháy chung cư mini ban đêm, chập điện tủ lạnh bốc khói, rò rỉ khí gas nhà bếp, sử dụng bình chữa cháy mini, thoát khỏi hành lang ngạt khói độc.
3. **Bài 3: An toàn số & Chống lừa đảo (5 tình huống - 10 câu):** Cuộc gọi video deepfake mạo danh công an, tin nhắn SMS chứa link trúng thưởng giả mạo, thủ đoạn lừa cài app độc hại chiếm quyền OTP, rò rỉ thông tin cá nhân căn cước, bẫy tuyển cộng tác viên online.

Mỗi câu hỏi seed bắt buộc có đầy đủ: `title`, `type`, `payload`, `correct_data`, `explanation`, `difficulty`, `source` (Bộ Y tế / Cục Cảnh sát PCCC / Bộ Công an), `reviewed_by`.

---

## 8. CHECKLIST KIỂM SOÁT CHẤT LƯỢNG KHI SINH MÃ (AGENT SELF-AUDIT)

Trước khi cung cấp mã nguồn hoặc hoàn tất một chức năng, Agent phải tự kiểm tra:
1. [ ] **Kiểm tra rò rỉ đáp án:** Các DTO gửi về client khi làm bài có bị vô tình serialize trường `CorrectData` / `correct_data` không? DTO trả lời chỉ được chứa `Payload` (options hiển thị), không được chứa đáp án.
2. [ ] **Kiểm tra tính toàn vẹn múi giờ:** Đã lưu UTC và convert sang `Asia/Ho_Chi_Minh` khi tính `DateOnly` cho Streak chưa? Trên Linux dùng `TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")`.
3. [ ] **Kiểm tra sổ cái XP:** Mọi thay đổi XP đều ghi vào bảng `XpEvents` (Append-Only Ledger)? Không được UPDATE trực tiếp `Users.TotalXp` mà không ghi XpEvent kèm theo.
4. [ ] **Kiểm tra Race Condition & Idempotency:** Thao tác trừ tim và cộng XP đã bọc trong `DbTransaction`? `SessionAnswers` đã có ràng buộc `UNIQUE(SessionId, QuestionId)`? `IdempotencyKey` header đã được kiểm tra trước khi chấm điểm?
5. [ ] **Kiểm tra phụ thuộc tầng Onion:** Mã nguồn ở lớp Domain có vô tình phụ thuộc EF Core hay WebAPI không?
6. [ ] **Kiểm tra RoleId FK:** Code mới khi cần kiểm tra role của user phải dùng `user.Role.RoleName` (sau Include FK) hoặc parse từ JWT Claim `payload.Role`, **không dùng enum index hardcode** như `roleId == 2` thay vì `UserRoleEnum.Student`.
7. [ ] **Kiểm tra Entity còn thiếu:** Trước khi implement Gameplay Task B1-B5, xác nhận 7 Entity Gameplay (`Domain`, `Unit`, `Node`, `Question`, `NodeQuestion`, `LessonSession`, `SessionAnswer`, `WrongQuestion`, `XpEvent`, `UserNodeProgress`) đã được tạo Entity class và Migration thành công chưa?