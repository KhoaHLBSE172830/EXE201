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

Quy chuẩn: Tên bảng và cột dùng `snake_case`. Primary Key sử dụng `uuid` (UUIDv4) hoặc `bigserial` cho bảng log lớn.

### 4.1. Phân hệ Tài khoản & Phân quyền (Auth & RBAC)
* **`users`**
  - `id (UUID, PK)`: Định danh người dùng.
  - `email (VARCHAR(255), UNIQUE, NOT NULL)`: Địa chỉ email.
  - `password_hash (VARCHAR(255), NULL)`: Mã băm mật khẩu (null đối với tài khoản chỉ dùng Google OAuth).
  - `role (VARCHAR(50), NOT NULL)`: `Student`, `Curator`, `Organization`, `Admin`.
  - `status (VARCHAR(20), NOT NULL, DEFAULT 'Active')`: `Active`, `Banned`, `PendingApproval`.
  - `timezone (VARCHAR(50), NOT NULL, DEFAULT 'Asia/Ho_Chi_Minh')`: Múi giờ người dùng.
  - `total_xp (INT, NOT NULL, DEFAULT 0)`: Tổng XP tích lũy (Indexed).
  - `created_at (TIMESTAMPTZ, NOT NULL)`, `updated_at (TIMESTAMPTZ, NOT NULL)`.
* **`user_profiles`**
  - `user_id (UUID, PK, FK -> users.id ON DELETE CASCADE)`.
  - `full_name (VARCHAR(150), NOT NULL)`.
  - `avatar_url (TEXT, NULL)`.
  - `daily_goal_minutes (INT, NOT NULL, DEFAULT 10)`: Mục tiêu hàng ngày (5, 10, 15 phút).
  - `risk_survey_result (JSONB, NULL)`: Khảo sát nhận diện rủi ro ban đầu lúc onboarding.
* **`user_stats`**
  - `user_id (UUID, PK, FK -> users.id ON DELETE CASCADE)`.
  - `hearts (INT, NOT NULL, DEFAULT 5, CHECK: hearts >= 0 AND hearts <= 5)`: Số tim hiện có.
  - `hearts_updated_at (TIMESTAMPTZ, NOT NULL)`: Mốc thời gian gần nhất tính toán phục hồi tim.
  - `streak (INT, NOT NULL, DEFAULT 0)`: Số ngày chuỗi liên tiếp.
  - `last_active_date (DATE, NULL)`: Ngày học gần nhất tính theo múi giờ user (`YYYY-MM-DD`).
  - `streak_freeze_count (INT, NOT NULL, DEFAULT 0)`: Số bùa bảo vệ chuỗi đang sở hữu.
  - `is_pro (BOOLEAN, NOT NULL, DEFAULT FALSE)`: Trạng thái gói Mimo Pro (vô hạn tim).
  - `pro_expires_at (TIMESTAMPTZ, NULL)`: Hạn dùng Pro.
  - `xmin (Postgres System Column)`: Dùng cho Optimistic Concurrency Control.

### 4.2. Cấu trúc Cây Kỹ năng & Nội dung Học (Content Domain)
* **`domains`**
  - `id (UUID, PK)`, `name (VARCHAR(100), NOT NULL)`, `slug (VARCHAR(100), UNIQUE, NOT NULL)`.
  - `icon_url (TEXT, NULL)`, `order (INT, NOT NULL, DEFAULT 1)`, `status (VARCHAR(20), NOT NULL, DEFAULT 'Active')`.
* **`units`**
  - `id (UUID, PK)`, `domain_id (UUID, FK -> domains.id, NOT NULL)`.
  - `title (VARCHAR(150), NOT NULL)`, `description (TEXT, NULL)`, `order (INT, NOT NULL)`.
* **`nodes`**
  - `id (UUID, PK)`, `unit_id (UUID, FK -> units.id, NOT NULL)`.
  - `title (VARCHAR(150), NOT NULL)`.
  - `type (VARCHAR(30), NOT NULL)`: `lesson` (bài học chuẩn), `chest` (rương thưởng), `boss` (thử thách chặng).
  - `order (INT, NOT NULL)`.
  - `unlock_rule (JSONB, NULL)`: Quy tắc mở khóa (vd: `{"prerequisite_node_id": "uuid-here"}`).
  - `xp_reward (INT, NOT NULL, DEFAULT 10)`: Điểm XP thưởng khi hoàn thành node.
* **`questions`**
  - `id (UUID, PK)`.
  - `type (VARCHAR(30), NOT NULL)`: `choice` (Quyết định tình huống), `sequence` (Sắp xếp quy trình), `red_flag` (Vạch lá tìm sâu).
  - `title (TEXT, NOT NULL)`: Nội dung tình huống / câu hỏi.
  - `image_url (TEXT, NULL)`: Ảnh hoặc GIF minh họa tình huống.
  - `payload (JSONB, NOT NULL)`: Chứa các options hiển thị cho client (KHÔNG chứa trường đáp án đúng).
  - `correct_data (JSONB, NOT NULL)`: Dữ liệu thẩm định đáp án đúng lưu nội bộ server.
  - `explanation (TEXT, NOT NULL)`: Lời giải thích y tế/an toàn chi tiết sau khi trả lời.
  - `difficulty (VARCHAR(20), NOT NULL, DEFAULT 'Medium')`: `Easy`, `Medium`, `Hard`.
  - `source (VARCHAR(255), NULL)`: Nguồn tham chiếu chuyên môn (Bộ Y tế, Cục Cảnh sát PCCC...).
  - `reviewed_by (VARCHAR(150), NULL)`: Chuyên gia duyệt nội dung.
  - `status (VARCHAR(20), NOT NULL, DEFAULT 'Published')`.
* **`node_questions`**
  - `node_id (UUID, FK -> nodes.id)`, `question_id (UUID, FK -> questions.id)`.
  - `order (INT, NOT NULL)`.
  - `PRIMARY KEY (node_id, question_id)`.

### 4.3. Gameplay Engine, Luyện tập & Sổ cái XP
* **`user_node_progress`**
  - `id (UUID, PK)`, `user_id (UUID, FK -> users.id)`, `node_id (UUID, FK -> nodes.id)`.
  - `status (VARCHAR(20), NOT NULL)`: `locked`, `current`, `done`.
  - `completed_at (TIMESTAMPTZ, NULL)`.
  - `UNIQUE (user_id, node_id)`.
* **`lesson_sessions`**
  - `id (UUID, PK)`, `user_id (UUID, FK -> users.id)`, `node_id (UUID, FK -> nodes.id)`.
  - `status (VARCHAR(20), NOT NULL, DEFAULT 'in_progress')`: `in_progress`, `completed`, `failed`.
  - `hearts_lost (INT, NOT NULL, DEFAULT 0)`.
  - `total_xp_earned (INT, NOT NULL, DEFAULT 0)`.
  - `started_at (TIMESTAMPTZ, NOT NULL)`, `expires_at (TIMESTAMPTZ, NOT NULL)`, `completed_at (TIMESTAMPTZ, NULL)`.
* **`session_answers`**
  - `id (UUID, PK)`, `session_id (UUID, FK -> lesson_sessions.id)`, `question_id (UUID, FK -> questions.id)`.
  - `submitted_answer (JSONB, NOT NULL)`, `is_correct (BOOLEAN, NOT NULL)`.
  - `time_spent_seconds (INT, NOT NULL)`.
  - `xp_earned (INT, NOT NULL, DEFAULT 0)`.
  - `idempotency_key (VARCHAR(100), NULL)`.
  - `created_at (TIMESTAMPTZ, NOT NULL)`.
  - `UNIQUE (session_id, question_id)`.
* **`wrong_questions`**
  - `id (UUID, PK)`, `user_id (UUID, FK -> users.id)`, `question_id (UUID, FK -> questions.id)`.
  - `times_wrong (INT, NOT NULL, DEFAULT 1)`.
  - `resolved_at (TIMESTAMPTZ, NULL)`.
  - `last_attempt_at (TIMESTAMPTZ, NOT NULL)`.
  - `UNIQUE (user_id, question_id)`.
* **`xp_events` (Append-Only Event Ledger)**
  - `id (BIGSERIAL, PK)`, `user_id (UUID, FK -> users.id, NOT NULL, Indexed)`.
  - `amount (INT, NOT NULL)`.
  - `source (VARCHAR(50), NOT NULL)`: `lesson_correct`, `combo_bonus`, `perfect_lesson`, `chest_reward`, `practice_reward`.
  - `reference_id (UUID, NULL)`.
  - `created_at (TIMESTAMPTZ, NOT NULL, Indexed)`.

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
* **Auth & Profiles:**
  - `POST /api/v1/auth/register`: Đăng ký tài khoản email + mật khẩu.
  - `POST /api/v1/auth/login`: Xác thực và trả JWT Pair (Access + Refresh).
  - `POST /api/v1/auth/google`: Tiếp nhận Google ID Token từ FE, verify bằng `Google.Apis.Auth`, tạo user nếu chưa có và phát hành JWT.
  - `POST /api/v1/auth/refresh-token`: Cấp mới Access Token bằng Refresh Token.
  - `GET /api/v1/users/me`: Lấy thông tin cá nhân, cấp độ hiện tại, số tim, streak.
  - `PATCH /api/v1/users/me/onboarding`: Cập nhật mục tiêu học tập (5/10/15 phút) và kết quả khảo sát.
* **Học liệu & Bản đồ Cây Kỹ Năng:**
  - `GET /api/v1/domains`: Danh sách 5 chủ đề lớn.
  - `GET /api/v1/path?domainId={id}`: Trả về cây lộ trình gồm các Node kèm trạng thái của người dùng (`locked`, `current`, `done`).
* **Gameplay Engine:**
  - `POST /api/v1/lessons/{nodeId}/start`: Kiểm tra tim $\ge 1$ hoặc Pro, khởi tạo session, trả về session ID và danh sách 10 câu hỏi (ẩn đáp án đúng).
  - `POST /api/v1/sessions/{id}/answer`: Nhận payload `{ questionId, submittedAnswer }`, header `Idempotency-Key`. Thực hiện chấm điểm, trừ tim nếu sai, cộng XP + combo nếu đúng, trả về kết quả kèm `explanation`.
  - `POST /api/v1/sessions/{id}/complete`: Tổng kết ván, cộng XP hoàn hảo (nếu không mất tim), cập nhật Streak, cập nhật tiến độ node sang `done` và mở khóa node liền kề.
* **Luyện tập (Practice):**
  - `GET /api/v1/practice/wrong-questions`: Lấy danh sách câu sai chưa hoàn thành của user.
  - `POST /api/v1/practice/answer`: Nộp đáp án ôn tập, nếu đúng hồi $+1\text{ Tim}$ và đánh dấu resolved.
* **Bảng xếp hạng (Weekly Leaderboard):**
  - `GET /api/v1/leaderboard/current`: Trả về top 30 người có XP tuần cao nhất (tính từ 00:00 Thứ Hai theo giờ VN) kèm vị trí xếp hạng và XP của người dùng hiện tại.

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
1. [ ] **Kiểm tra rò rỉ đáp án:** Các DTO gửi về client khi làm bài có bị vô tình serialize trường đáp án đúng không?
2. [ ] **Kiểm tra tính toàn vẹn múi giờ:** Đã lưu `timestamptz` (UTC) và convert về múi giờ `Asia/Ho_Chi_Minh` khi tính ngày Streak chưa?
3. [ ] **Kiểm tra sổ cái XP:** Điểm XP có được ghi nhận chi tiết vào `xp_events` không?
4. [ ] **Kiểm tra Race Condition & Idempotency:** Thao tác trừ tim, cộng điểm, hoàn thành bài đã có transaction và khóa lạc quan/idempotency guard chưa?
5. [ ] **Kiểm tra phụ thuộc tầng Onion:** Mã nguồn ở lớp Domain có vô tình phụ thuộc EF Core hay WebAPI không?