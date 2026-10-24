# KẾ HOẠCH TRIỂN KHAI ĐIỀU CHỈNH (v2) — ROLE ORGANIZATION (B2B) & CURATOR + "LỚP HỌC CỦA TÔI"

> **Căn cứ đã đối chiếu:** `AGENT.md`, `AGENTFE.md`, `Kế hoạch & Task List Frontend MIMO.docx`, schema Supabase thật (31 bảng = 30 bảng nghiệp vụ + `__EFMigrationsHistory`), và code: `MimoDbContext.cs`, `BaseEntity/User/Organization/Question/Node.cs`, `GameplayService.cs`, `AuthService.cs`, `DependencyInjection.cs`, `endpoints.js`.
>
> **Chưa xem (cần xác minh khi làm):** `IUnitOfWork`/`IGenericRepository` (API khóa dòng, transaction), `LessonsController`/`MediaController` (attribute `[Authorize]`), `IQuestionGrader`, `UserStatusEnum` (có `PendingApproval` chưa), nội dung các trang trong `src/pages/*` và `businessService.js`/`curatorService.js`/`learnerService.js`.

---

## 0. TÓM TẮT THAY ĐỔI SO VỚI v1

| # | Nội dung | v1 | v2 | Lý do (đã kiểm chứng) |
|---|---|---|---|---|
| 1 | Bảng `Scenarios` + `QuestionsJson` | Tạo mới | **Bỏ.** Dùng `Questions` + `Nodes` + `NodeQuestions` có sẵn | `Questions` đã có `Status`, `CuratorId`, `Explanation`, `Source`, `ReviewedBy`, `Difficulty`; `GameplayService` chỉ đọc từ `NodeQuestions`. Bảng riêng tạo ra hai nguồn nội dung và nội dung Published sẽ không bao giờ chơi được |
| 2 | `ScenarioFeedbacks` | `ReviewerName` chuỗi tự do | Đổi thành `QuestionFeedbacks`, có `QuestionId`, `ReviewerId` | Cần biết ai góp ý, gắn đúng câu hỏi |
| 3 | "Chỉ tạo bảng mới, 0% đụng bảng cũ" | Khẳng định | **Cần 2–3 lệnh ALTER** | `Organizations` chỉ có `SeatCount`, không có `UsedSeats`; `Nodes` không có `CuratorId` |
| 4 | Gắn bài tập vào lượt chơi | Không nhắc | Sửa `GameplayService.StartLesson/CompleteSession`, thêm FK cho `LessonSessions.AssignmentId` | Cột `AssignmentId` đã có trong DB nhưng chưa có FK và chưa có code nào ghi/đọc nó. Đây là mắt xích của E2E |
| 5 | Vào lớp bằng mã | Không nêu chống race | Cập nhật atomic `UsedSeats`, rate limit, mã đủ entropy | `AGENT.md` mục 5.6 |
| 6 | Admin | "Tuyệt đối không đụng" | Giữ không làm UI Admin, **nhưng bắt buộc có 3 endpoint duyệt tối thiểu** (hoặc seed Published thủ công) | Không có Admin thì `PendingApproval` không bao giờ thành `Published`, học sinh không thấy nội dung |
| 7 | Educator | Chỉ một view | Phạm vi tối thiểu: claim mã + `GET /educator/classes` + dùng chung service lớp/bài tập | `AGENT.md` mục 1.2; `TeacherClassroomView` cần dữ liệu này |
| 8 | Route API | `/business/*`, `/classroom/*`, `/curator/scenarios` | Class/assignment dùng route chuẩn `AGENT.md`; chỉ phần riêng của trường nằm ở `/business/*` | FE và BE hiện đều tham chiếu `AGENT.md` |
| 9 | Bảng `Classes.EducatorId` nullable | (v1 đặt nullable) | **Giữ nullable, ghi nhận là sai lệch có chủ đích so với `AGENT.md`** | BGH tạo lớp trước khi có giáo viên nhận mã |
| 10 | Heatmap, Skill Gap | Không có nguồn dữ liệu | Tính từ `SessionAnswers` ⨝ `LessonSessions`, không thêm bảng | Dữ liệu đã có |
| 11 | Mốc thời gian | Không nêu | Dùng `DateTime` UTC như code hiện tại; deadline nhập theo giờ VN, lưu UTC | Code dùng `DateTime?`, Npgsql từ chối `Kind != Utc` |
| 12 | Ước lượng, thứ tự, migration, bảo mật | Thiếu | Thêm mục 6, 7, 8 | — |

---

## 1. PHẠM VI & QUYẾT ĐỊNH CẦN CHỐT

**Trong phạm vi:** Organization (B2B), Curator, trang "Lớp học của tôi" của Student, Educator ở mức tối thiểu.
**Ngoài phạm vi:** giao diện Admin, thanh toán, league, shop, xuất PDF có chữ ký số/mộc điện tử, push notification thật, import học sinh từ CSV, quản lý Domain/Unit của Curator.

### Quyết định cần chủ dự án xác nhận (đã ghi mặc định đề xuất)

| # | Câu hỏi | Mặc định v2 |
|---|---|---|
| D1 | Duyệt nội dung (Admin) làm thế nào? | **A (đề xuất):** làm 3 endpoint backend `publish / reject / emergency-rollback` + danh sách chờ duyệt, gọi qua Swagger, chưa có UI. B: chỉ seed dữ liệu `Published` thủ công (demo được nhưng luồng Curator "gửi duyệt" không khép kín) |
| D2 | Học sinh làm bài sau deadline? | Vẫn cho làm, **không tính `IsPassed`** và hiển thị "Quá hạn" |
| D3 | Học sinh rời lớp có trả ghế (seat)? | Có, nếu không còn lớp nào `Enrolled` trong trường đó: `UsedSeats - 1` và `OrganizationId = NULL` |
| D4 | Học sinh đã thuộc trường khác nhập mã lớp trường mới? | Từ chối, yêu cầu rời lớp cũ trước |
| D5 | Node đã `Published` bị Curator sửa? | Cấm sửa trực tiếp; tạo bản sao ở trạng thái `Draft` |
| D6 | Số câu tối thiểu để gửi duyệt một bài? | 10 câu (chuẩn `AGENT.md`; `CompleteSession` yêu cầu trả lời đủ mọi câu của node) |
| D7 | Mục tiêu: demo EXE201 hay chạy thật với trường? | Mặc định **demo EXE201**; các mục đánh dấu `[PROD]` chỉ cần khi chạy thật |

---

## 2. HIỆN TRẠNG ĐÃ XÁC MINH (ẢNH HƯỞNG TỚI THIẾT KẾ)

1. **DB dùng PascalCase** (`"Id"`, `"Title"`), không phải snake_case như `AGENT.md`. Bảng mới theo PascalCase. `__EFMigrationsHistory` tồn tại, nên **dùng EF migrations** (`dotnet ef migrations add`), không chạy SQL tay.
2. **Entity thừa kế `BaseEntity`** (`Guid Id` identity, `DateTime? CreatedAt/UpdatedAt`); `SaveChangesAsync` tự gán. Các bảng liên kết dạng `UserNodeProgress` dùng `Id` riêng + unique index, nên `ClassMember` dùng `Id` + `UNIQUE(ClassId, StudentId)` là đúng mẫu code, chỉ lệch `AGENT.md`.
3. **`Organizations`:** có `SeatCount`, `ExpirationDate?`, `Status`, `SubscriptionTier`; **không có `UsedSeats`**.
4. **`Questions`:** `CuratorId` nullable, `Status` varchar, đã có đủ trường thẩm định. `Nodes`: có `Status`, **không có `CuratorId`**.
5. **`LessonSessions.AssignmentId`** đã có (nullable), chưa có FK, `GameplayService` không dùng.
6. **`GameplayService`:** `StartLessonAsync(userId, nodeId)` kiểm tra node `Published` và rào cản cây kỹ năng; `CompleteSessionAsync` chỉ cập nhật tiến độ cá nhân, chưa có logic bài tập trường giao. `IdempotencyKey`, transaction, ledger XP đã đúng.
7. **JWT chỉ chứa** `userId, role, sessionId, email, status`, **không chứa `OrganizationId`**: mọi truy vấn phải tự nạp `OrganizationId` của người gọi từ DB.
8. **`AuthService` (ảnh hưởng trực tiếp tới 2 role này):**
   - `RegisterCuratorAsync` (public) tạo Curator `Active` ngay, trái `AGENT.md` ("không tự đăng ký Curator qua API công khai").
   - `RegisterOrganizationAsync` (public) tạo trường `Active`, `SeatCount = 50`, hạn 1 năm, không qua duyệt (docx FE7-1 yêu cầu "chờ duyệt").
   - `GoogleLoginAsync` cho phép client tự chọn `Role = Organization`.
   - `RegisterCuratorAsync` lưu chuyên môn vào `Users.Address`.
9. **FE:** `endpoints.js` đang trỏ `CURATOR` vào `/curator/courses`, `/curator/lessons/{id}` (mô hình Course/Module/Lesson cũ), có sẵn `EDUCATOR`, `ADMIN`, chưa có `BUSINESS`/`CLASSES`. Thư mục thực tế là `src/pages/{admin,auth,business,curator,error,learner,public}` (khác `src/roles/*` trong `AGENTFE.md`).
10. **Docx FE:** gợi ý "React + TypeScript, Tailwind" mâu thuẫn `AGENTFE.md` (Vanilla CSS, cấm Tailwind). Lấy `AGENTFE.md` làm chuẩn. Docx FE7-5 "Phân công Curator cho lớp" mâu thuẫn đặc tả (Curator độc lập với trường): **bỏ**.

---

## 3. MÔ HÌNH DỮ LIỆU

### 3.1. Bảng mới (kế thừa `BaseEntity`, cấu hình bằng `IEntityTypeConfiguration<T>` trong thư mục Configurations — `ApplyConfigurationsFromAssembly` đã quét sẵn)

**`Classes`**
`OrganizationId` (FK, NOT NULL, Restrict) · `EducatorId` (FK Users, NULL) · `Name` (100) · `ClassCode` (varchar 10, UNIQUE, lưu **chữ hoa**) · `Grade` (50, NULL) · `Room` (50, NULL) · `FocusTopic` (150, NULL) · `MaxStudents` (INT, 50) · `Status` (`Active`/`Archived`).
Index: `UNIQUE(ClassCode)`, `(OrganizationId)`, `(EducatorId)`.

**`ClassMembers`**
`ClassId` (FK) · `StudentId` (FK Users) · `JoinedAt` · `Status` (`Enrolled`/`Dropped`).
Index: `UNIQUE(ClassId, StudentId)`, `(StudentId)`. Tham gia lại sau khi rời = cập nhật bản ghi cũ về `Enrolled`, không tạo dòng mới.

**`Assignments`**
`ClassId` (FK) · `NodeId` (FK Nodes, **NOT NULL**, vì bài tập luôn chỉ định một bài học) · `CreatedBy` (FK Users) · `Title` (150) · `Description` (TEXT, NULL) · `Deadline` (UTC) · `PassScoreThreshold` (INT, 80, CHECK 0–100) · `RewardXp` (INT, 50, CHECK ≥ 0).
Index: `(ClassId, Deadline)`, `(NodeId)`.

**`AssignmentProgress`**
`AssignmentId` (FK) · `StudentId` (FK) · `SessionId` (FK LessonSessions, NULL — phiên điểm cao nhất, theo `AGENT.md`) · `BestScore` (0–100) · `IsPassed` · `CompletedAt` (NULL).
Index: `UNIQUE(AssignmentId, StudentId)`, `(StudentId)`.

**`TeacherLicenses`**
`OrganizationId` (FK) · `ActivationCode` (varchar 32, UNIQUE, sinh ngẫu nhiên ≥ 16 ký tự) · `Note` (150, NULL) · `AssignedClassesJson` (jsonb, danh sách `ClassId`) · `ClaimedByEducatorId` (FK, NULL) · `IsClaimed` · `ExpiresAt`.
Quy tắc: chỉ ghi các `ClassId` thuộc đúng `OrganizationId`. Sau khi claim, nguồn sự thật là `Classes.EducatorId`, JSON chỉ là lịch sử cấp phát.

**`QuestionFeedbacks`** (thay `ScenarioFeedbacks`)
`QuestionId` (FK Questions) · `ReviewerId` (FK Users, NULL) · `ReviewerName` (100) · `Kind` (`Reject`/`Expert`) · `Note` (TEXT) · `Status` (`Pending`/`Resolved`) · `ResolvedAt` (NULL).
Admin `reject` tự tạo một dòng `Kind = Reject`.

**`Notifications`** *(tùy chọn, chỉ khi làm nút "Nhắc học")*
`UserId` · `Type` · `Title` · `Body` · `ReferenceId` (NULL) · `IsRead`. Chỉ thông báo trong ứng dụng; chưa có push.

### 3.2. Sửa bảng cũ (một migration, an toàn cho dữ liệu hiện có)

| Bảng | Thay đổi | Ghi chú |
|---|---|---|
| `Organizations` | `ADD "UsedSeats" INT NOT NULL DEFAULT 0` | Backfill: số `Users` role Student có `OrganizationId` = trường |
| `Nodes` | `ADD "CuratorId" UUID NULL` + FK Users (SetNull) | Xác định ai được sửa/gửi duyệt bài |
| `LessonSessions` | Thêm FK `AssignmentId → Assignments` (Restrict) + index `(AssignmentId)` | Dữ liệu cũ toàn NULL nên không lỗi |
| `Questions` *(tùy chọn)* | `ADD "AgeGroup" VARCHAR(50) NULL` | Mầm non / Tiểu học / THCS; chỉ nếu cần lọc theo lứa tuổi |

### 3.3. Không tạo bảng cho
Heatmap, Skill Gap, Compliance, Billing: tính từ `SessionAnswers`, `LessonSessions`, `AssignmentProgress`, `Organizations`. Ảnh/âm thanh: dùng `MediaController` + `IStorageService` có sẵn.

---

## 4. BACKEND (.NET 8, theo mẫu `GameplayService`: `IGenericRepository<T>` + `IUnitOfWork`)

> Tên namespace thực tế: `Mimo.DomainLayer.*`, `Mimo.Infrastructure.Database`, `Mimo.Application.*`. Không dùng `Mimo.Domain` như v1.

### 4.1. Domain / Infrastructure
- Entities mới + navigation vào `User`, `Organization`, `Node`, `Question`, `LessonSession`.
- `DbSet` mới trong `MimoDbContext`; **cấu hình bằng class `IEntityTypeConfiguration`** (không viết thêm inline vào `OnModelCreating`, đúng mục 7.2 `AGENT.md`).
- Exception mới: `SeatLimitExceededException`, `InvalidClassCodeException` (map sang RFC 7807 trong middleware hiện có).
- Một migration duy nhất `AddB2BClassroomAndCuratorWorkflow`, kiểm tra SQL sinh ra bằng `dotnet ef migrations script` trước khi áp dụng lên Supabase.
- Seed demo **chỉ chạy ở môi trường Development** (không dùng `HasData`, tránh đẩy mã lớp cố định lên production): 3 lớp `FPT-K10`, `LHP-11B`, `VSC-12A` cho trường của `d1@gmail.com`, 1–2 bài tập trỏ vào node đã Published.

### 4.2. Điều chỉnh `GameplayService` (mắt xích E2E)

**`StartLessonAsync(userId, nodeId, assignmentId?)`**
- Nếu có `assignmentId`: kiểm tra assignment tồn tại, `NodeId` khớp, học sinh là `Enrolled` ở `Class` của assignment. Gán `session.AssignmentId`.
- Bài tập **bỏ qua rào cản cây kỹ năng** (nếu không, học sinh chưa mở khóa node sẽ không làm được bài giáo viên giao), nhưng vẫn bắt buộc node `Published` và còn tim.
- Không có `assignmentId`: hành vi cũ không đổi.

**`CompleteSessionAsync`** — trong cùng transaction hiện có, nếu `session.AssignmentId != null`:
1. `correct = số SessionAnswers IsCorrect`, `total = số câu của node`; `score = round(correct * 100 / total)`.
2. Upsert `AssignmentProgress`: nếu `score > BestScore` thì cập nhật `BestScore`, `SessionId`.
3. Nếu `score >= PassScoreThreshold` **và** `now <= Deadline` **và** chưa `IsPassed`: đặt `IsPassed = true`, `CompletedAt = now`; thưởng `RewardXp` một lần (ghi `XpEvents` với `Source = "assignment_reward"`, `ReferenceId = assignmentId`, và cộng `Users.TotalXp`, đúng quy tắc ledger).
4. Trả thêm trong `CompleteSessionResponseDto`: `AssignmentResult { Score, IsPassed, IsLate, RewardXp }`.

Endpoint `POST /lessons/{nodeId}/start` nhận body tùy chọn `{ "assignmentId": "..." }` (tương thích ngược).

### 4.3. Tham gia / rời lớp (`IClassMembershipService`)

`POST /classes/join-by-code` (Student), body `{ "classCode": "..." }`:
1. Chuẩn hóa mã (trim, upper). Không tìm thấy hoặc lớp `Archived` → cùng một thông báo chung (không lộ mã nào tồn tại).
2. Mở transaction. Nạp `Class`, đếm `Enrolled` so với `MaxStudents`.
3. Nếu học sinh đã `Enrolled` ở lớp này → trả về lớp hiện tại (idempotent).
4. Nếu `User.OrganizationId` khác trường của lớp và không null → từ chối (D4).
5. Nếu học sinh chưa thuộc trường: **atomic**
   `UPDATE "Organizations" SET "UsedSeats" = "UsedSeats" + 1 WHERE "Id" = @org AND "Status" = 'Active' AND ("ExpirationDate" IS NULL OR "ExpirationDate" > now()) AND "UsedSeats" < "SeatCount"`
   → 0 dòng bị ảnh hưởng = `SeatLimitExceededException`. Sau đó gán `User.OrganizationId`.
6. Tạo/khôi phục `ClassMember`, commit.

`POST /classes/{classId}/leave`: đặt `Dropped`; nếu không còn lớp `Enrolled` nào trong trường thì `UsedSeats - 1` và `OrganizationId = NULL` (D3).

Bảo vệ: rate limit 5 lần/phút/người dùng cho endpoint join và claim. Mã lớp sinh phía server: 8 ký tự từ bảng chữ không gây nhầm (bỏ `0 O 1 I L`), thử lại khi trùng. Mã dạng `FPT-K10` chỉ dùng cho seed demo.

### 4.4. Mã giáo viên & Educator tối thiểu

- `POST /business/teachers/keys` (Organization): sinh mã, kiểm tra mọi `ClassId` thuộc trường người gọi.
- `POST /organizations/teacher-licenses/claim` (người dùng đã đăng nhập, role Student, chưa thuộc trường khác): trong transaction, `UPDATE ... SET IsClaimed = true WHERE ActivationCode = @c AND IsClaimed = false AND ExpiresAt > now()` (chống claim đôi) → đổi `RoleId` sang Educator, gán `OrganizationId`, gán `Classes.EducatorId` cho các lớp được cấp.
- **JWT mang role cũ** nên endpoint claim phải trả cặp token mới (thêm `IssueTokensAsync(User)` trong `AuthService`), hoặc bắt đăng nhập lại.
- `GET /educator/classes`: lớp có `EducatorId = userId`.

### 4.5. Nhóm Organization (`/business/*`, `[Authorize(Roles = "Organization")]`)

Mọi truy vấn **bắt buộc lọc theo `OrganizationId` của người gọi** (nạp từ DB qua helper `ICurrentTenant`); truy cập lớp/assignment của trường khác trả 404.

| Endpoint | Nội dung |
|---|---|
| `GET /overview` | Số seat dùng/tổng, số lớp, số học sinh, **tỷ lệ tuân thủ**, top 3 kỹ năng yếu |
| `GET/POST /classes`, `PUT /classes/{id}`, `POST /classes/{id}/assign-teacher` | CRUD lớp; sinh `ClassCode`; gán giáo viên có sẵn của trường |
| `GET/POST /teachers/keys` (+ `DELETE` thu hồi) | Quản lý mã giáo viên |
| `GET/POST /campaigns` | "Campaign" = `Assignment`. `POST` nhận `ClassIds[]` và tạo một dòng `Assignments` cho mỗi lớp trong cùng transaction |
| `POST /campaigns/{id}/nudge` *(tùy chọn)* | Tạo `Notifications` cho học sinh chưa đạt |
| `GET /skill-gap` | Tỷ lệ sai theo Domain |
| `GET /reports` | Số liệu báo cáo dạng JSON (xuất PDF/Excel ở FE) |
| `GET /billing` | `SeatCount`, `UsedSeats`, `ExpirationDate`, `SubscriptionTier` (chỉ đọc) |

**Định nghĩa số liệu (cần chốt với chủ dự án):**
- *Tỷ lệ hoàn thành của một bài tập* = số học sinh `IsPassed` / số học sinh `Enrolled` của lớp.
- *Tỷ lệ tuân thủ của trường* = tổng học sinh đạt / tổng học sinh được giao, trên các bài tập có `Deadline` trong 30 ngày gần nhất.
- *Skill Gap theo Domain* = câu sai / tổng câu trả lời của học sinh thuộc trường, nhóm theo `LessonSessions.NodeId → Units → Domains`; chỉ hiển thị Domain có ≥ 20 câu trả lời.
- *Heatmap lớp* (`GET /classes/{classId}/heatmap`) = tỷ lệ sai theo `QuestionId` từ `SessionAnswers` của học sinh trong lớp; cờ "nguy cơ cao" khi > 50%.
- Báo cáo chỉ tổng hợp theo lớp; tên học sinh chỉ hiện ở màn xem sổ điểm của Organization/Educator thuộc đúng trường.

### 4.6. Nhóm Curator (`/curator/*`, `[Authorize(Roles = "Curator")]`)

**Đơn vị làm việc là "gói bài học" = `Node` + các `Question` gắn qua `NodeQuestions`.** Chỉ Node `Published` có câu hỏi `Published` mới chơi được.

Máy trạng thái (cùng một bộ giá trị cho `Node` và `Question`):

```
Draft ──submit──▶ PendingApproval ──publish (Admin)──▶ Published
  ▲                    │                                   │
  └──── reject (Admin, kèm lý do) ◀──┘     emergency-rollback (Admin) ──▶ Draft
```

| Quy tắc | Chi tiết |
|---|---|
| Quyền sở hữu | Chỉ sửa/xóa `Question` có `CuratorId = userId`, `Node` có `CuratorId = userId`; nội dung `Published`/`PendingApproval` bị khóa sửa (D5) |
| Gửi duyệt | `POST /lessons/{nodeId}/submit-approval`: yêu cầu đủ số câu (D6), mọi câu hợp lệ; chuyển cả node và các câu `Draft` sang `PendingApproval` |
| Validate khi lưu | Theo `Type`: `choice` 3–4 lựa chọn và đúng 1 đáp án, `sequence` 4–5 thẻ, `red_flag` có vùng nghi ngờ; `CorrectData` phải khớp `Payload` (chạy thử bằng `IQuestionGrader`); **từ chối `Payload` chứa khóa như `isCorrect`, `correct`, `answer`** (Zero Leakage) |
| Nguồn/chuyên gia | Gửi duyệt yêu cầu `Source` và `ReviewedBy` không rỗng |
| DTO | **DTO cho Curator (có `CorrectData`) tách hẳn `QuestionDisplayDto` của học sinh**; không tái sử dụng, không bao giờ trả `CorrectData` qua endpoint của Student |
| Simulator | `POST /curator/preview/answer`: chấm thử bằng `IQuestionGrader`, không trừ tim, không ghi XP (FE không tự chấm) |

| Endpoint | Nội dung |
|---|---|
| `GET /profile` | Thông tin từ `Users` + số câu theo trạng thái |
| `GET/POST /questions`, `GET/PUT/DELETE /questions/{id}` | Question Bank (lọc theo `Status`, `Type`, `Difficulty`) |
| `POST /questions/{id}/submit-approval` | Gửi từng câu (theo `AGENT.md` mục 6.5) |
| `GET/POST /lessons`, `PUT /lessons/{nodeId}`, `PUT /lessons/{nodeId}/questions` | Tạo node trong `Unit` có sẵn, gắn và sắp xếp câu hỏi |
| `GET /analytics` | Tỷ lệ sai theo câu hỏi, đáp án sai phổ biến (từ `SessionAnswers`), chỉ với câu của tác giả |
| `GET /feedback`, `POST /feedback/{id}/resolve` | Xem và đánh dấu đã xử lý (`QuestionFeedbacks`) |

Upload ảnh/GIF tái sử dụng `POST /media/upload/question-image`; cần xác minh `MediaController` cho phép role Curator.

### 4.7. Admin tối thiểu (theo D1-A, không có UI)

`GET /admin/content-approval?status=PendingApproval` · `POST /admin/content-approval/{nodeId}/publish` (Node và các câu liên quan → `Published`) · `POST .../reject` (bắt buộc `reason`, tạo `QuestionFeedbacks`) · `POST .../emergency-rollback` (về `Draft`). Chỉ `[Authorize(Roles = "Admin")]`; ghi log thao tác.

### 4.8. Sửa `AuthService` (bắt buộc cho Curator/Organization)

1. `RegisterCuratorAsync`: tạo tài khoản ở trạng thái `PendingApproval` (xác nhận enum), chỉ Admin kích hoạt. *Nếu chưa muốn có bước Admin cho demo, giữ `Active` nhưng ghi rõ là tạm thời.*
2. `GoogleLoginAsync`: bỏ nhận `Role = Organization` từ client; mặc định luôn là Student.
3. `RegisterOrganizationAsync` `[PROD]`: chuyển sang chờ duyệt (khớp docx FE7-1).

### 4.9. Đăng ký DI & quy ước
`IClassManagementService`, `IClassMembershipService`, `IAssignmentService`, `IBusinessService`, `ICuratorContentService`, `IAdminContentService`, `ICurrentTenant` trong `DependencyInjection.cs`. Mọi hàm async nhận `CancellationToken`; truy vấn chỉ đọc dùng `AsNoTracking()`; FluentValidation cho mọi request DTO.

---

## 5. FRONTEND (React 19 / Vite, Vanilla CSS Tokens, **không Tailwind**, theo `AGENTFE.md`)

### 5.1. Nền tảng
- `endpoints.js`: **thay** khối `CURATOR` cũ bằng route mục 4.6; **thêm** `BUSINESS`, `CLASSES`, `STUDENT`, `ORGANIZATIONS.CLAIM_LICENSE`; giữ nguyên các khối đang dùng. Quyết định số phận các khối `LEARNER.*` và `EDUCATOR.*` cũ để tránh hai bộ endpoint song song.
- `businessService.js`, `curatorService.js`, `learnerService.js`: gọi API thật; **chỉ giữ mock khi bật cờ môi trường** (ví dụ `VITE_USE_MOCK`), không để fallback im lặng che lỗi API.
- Route guard theo role: `/business/*` chỉ Organization, `/studio/*` chỉ Curator, `/learn/classroom` Student (Educator xem `TeacherClassroomView`). Chuyển hướng sau claim mã (role đổi).
- Deadline: người dùng nhập theo giờ VN, gửi UTC ISO; hiển thị qua `convertToUserTimezone` (`AGENTFE.md` ATTENTION 4). Nút gửi form: `Disabled` + spinner; `Idempotency-Key` qua interceptor (xác minh interceptor đã có).

### 5.2. Student — "Lớp học của tôi" (`/learn/classroom`, docx FE5-3)
- Chưa vào lớp: ô nhập mã → `POST /classes/join-by-code`; lỗi hiển thị theo RFC 7807 (mã sai, lớp đầy, hết ghế, đã thuộc trường khác).
- Đã vào lớp: tên lớp, giáo viên, danh sách bài tập (tên, deadline giờ VN, trạng thái *Chưa làm / Đạt / Chưa đạt / Quá hạn*, điểm cao nhất).
- "Làm bài ngay" → mở Scenario Player với `nodeId` + `assignmentId` (gọi `start` có body). Màn tổng kết hiển thị `AssignmentResult`.
- "Rời lớp" có hộp thoại xác nhận (nêu rõ việc trả ghế).
- Học sinh có thể vào bằng link/QR dạng `/learn/classroom?code=XXXX` (tự điền mã).

### 5.3. Organization (`src/pages/business/`)
1. `BusinessDashboard`: `GET /overview`.
2. `BusinessClassesPage`: danh sách/tạo/sửa lớp, hiển thị `ClassCode`; nút "Chiếu mã QR" toàn màn hình (sinh QR từ link `?code=`; chiếm ~60% màn hình theo `AGENTFE.md`). Chọn thư viện QR nhỏ, được duyệt.
3. `BusinessTeachersPage`: sinh/thu hồi mã, chọn lớp phụ trách.
4. `BusinessCampaignsPage`: chọn nhiều lớp + node Published + deadline + ngưỡng đạt; xem tiến độ.
5. `BusinessSkillGapPage`: biểu đồ nhiệt bằng CSS gradient vàng→đỏ.
6. `BusinessReportsPage`: hiển thị và xuất CSV/in PDF từ trình duyệt (chưa có chữ ký số).
7. `BusinessBillingPage`: chỉ đọc.

### 5.4. Curator (`src/pages/curator/`)
1. `StudioDashboard`: hồ sơ, số câu theo trạng thái, phản hồi chờ xử lý.
2. **Question Bank** (thêm so với v1; docx FE6-4): danh sách, lọc, tạo/sửa.
3. `CuratorBuilderPage`: biểu mẫu cho 3 dạng câu, tải ảnh, soạn giải thích, gắn câu vào node, **Mobile Simulator** dùng `POST /curator/preview/answer` (không chấm ở client), nút "Gửi duyệt" với danh sách lỗi validate.
4. `CuratorAssetsPage`: dùng `MEDIA.UPLOAD*` hiện có.
5. `CuratorAnalyticsPage`, `CuratorFeedbackPage`.
6. `CuratorGuidelinesPage`: giữ nguyên.
*Hoãn:* quản lý Domain/Unit (docx FE6-2), lịch sử phiên bản (FE6-6).

### 5.5. Educator tối thiểu
`TeacherClassroomView`: sổ điểm lớp (`GET /classes/{id}/progress`) và Heatmap (`GET /classes/{id}/heatmap`); trang nhập mã giáo viên.

---

## 6. THỨ TỰ TRIỂN KHAI & ƯỚC LƯỢNG (ngày công, sai số ±30%)

| Giai đoạn | Nội dung | BE | FE |
|---|---|---|---|
| **P0** | Chốt D1–D7; sửa `AuthService` (mục 4.8); entities, configuration, migration, seed Development | 2 | 0,5 |
| **P1** | Lớp học lõi: join/leave + seat atomic, class CRUD, assignment, hook `GameplayService`, API học sinh. **Mốc đầu tiên chạy được E2E Org → Student** | 6 | 4 (service + guard + `MyClassroomPage`) |
| **P2** | Organization: overview, teacher keys + claim, skill-gap, heatmap, reports, billing; Educator tối thiểu | 5 | 10 |
| **P3** | Curator: question/node CRUD, validate, máy trạng thái, preview, analytics, feedback; Admin tối thiểu | 7 | 14 |
| **P4** | Rate limit, kiểm thử tích hợp, tập dượt demo, cập nhật `AGENT.md`/`AGENTFE.md` | 2 | 2 |
| **Tổng** | | **≈ 22** | **≈ 30,5** |

P1 và phần nền của P3 (CRUD Question/Node) độc lập nhau và có thể chạy song song giữa hai nhóm. Khi BE chưa xong endpoint nào, FE dựng bằng mock qua cờ môi trường.

---

## 7. KIỂM THỬ

**Tự động (xUnit + PostgreSQL thật qua Testcontainers hoặc DB test, không dùng InMemory vì cần `FOR UPDATE`/`UPDATE` có điều kiện):**
1. 60 yêu cầu `join-by-code` đồng thời vào trường còn 50 ghế → đúng 50 thành công, `UsedSeats = 50`.
2. Hai lần claim cùng một mã song song → đúng một lần thành công.
3. `CompleteSession` với `AssignmentId`: điểm 70% / 80% / 100%, trước và sau deadline → `IsPassed`, `BestScore`, XP ledger đúng, `SUM(XpEvents) = Users.TotalXp`.
4. Organization A truy cập lớp/assignment của trường B → 404 trên mọi endpoint `/business` và `/classes`.
5. Payload chứa khóa đáp án bị từ chối; endpoint Student không bao giờ trả `CorrectData`.
6. Curator sửa nội dung của Curator khác hoặc nội dung `Published` → 403/409.
7. Học sinh chỉ thấy nội dung `Published`.

**E2E thủ công theo luồng v1, bổ sung thứ tự:**
`d1@gmail.com` (Organization) tạo lớp, giao bài → `d@gmail.com` (Student) nhập mã, thấy bài → làm bài (node đang bị khóa trên cây kỹ năng vẫn mở được) → quay lại Organization thấy điểm và tỷ lệ → `c1@gmail.com` (Curator) soạn gói 10 câu, gửi duyệt → Admin (Swagger) publish → Student thấy bài mới trên bản đồ.

**Tiêu chí build:** `dotnet build` 0 lỗi, không thêm warning mới so với hiện tại (không đặt chuẩn "0 warning" tuyệt đối); `npm run build` và `oxlint` không lỗi.

---

## 8. RỦI RO & GHI CHÚ

| Rủi ro | Giảm thiểu |
|---|---|
| Hai hệ nội dung cũ/mới (Course/Module/Lesson vs Domain/Unit/Node) làm Curator FE nối nhầm | Xóa khối `CURATOR` cũ trong `endpoints.js`; xác nhận bộ bảng Course không còn dùng cho học sinh |
| Npgsql từ chối `DateTime` không phải UTC | DTO nhận `DateTimeOffset`, lưu `.UtcDateTime` |
| Heatmap chậm khi dữ liệu lớn | Index `(AssignmentId)`, `(UserId)` trên `LessonSessions`; chỉ tính trong 30–90 ngày; giữ truy vấn gộp ở DB |
| Rò rỉ dữ liệu giữa các trường | `ICurrentTenant` bắt buộc trong mọi service B2B + test số 4 |
| Mã lớp bị dò | Mã 8 ký tự ngẫu nhiên, rate limit, thông báo lỗi chung |
| Dữ liệu học sinh là trẻ vị thành niên | Báo cáo gộp theo lớp; chỉ Organization/Educator đúng trường xem danh sách tên |
| Xuất báo cáo có chữ ký số / mộc điện tử | Ngoài phạm vi; ghi rõ trên giao diện là "bản nháp, chưa có chữ ký số" `[PROD]` |
| Push notification thật | Ngoài phạm vi; chỉ in-app qua `Notifications` (tùy chọn) |

### Cập nhật tài liệu sau khi làm
Sửa `AGENT.md`: (a) tên cột PascalCase thay vì snake_case; (b) `organizations.SeatCount/ExpirationDate/UsedSeats` thay `total_seats/contract_expires_at`; (c) `classes.EducatorId` nullable; (d) `class_members` dùng `Id` + unique; (e) bổ sung `AssignmentProgress`, `QuestionFeedbacks`, `Nodes.CuratorId`. Sửa `AGENTFE.md`: thư mục thực tế `src/pages/*`.
