# ==============================================================================
# MIMO FRONTEND ENGINE - AGENT INSTRUCTION MANUAL & ARCHITECTURE SPECIFICATION
# Project: MIMO - Gamified Life Skills & Emergency Safety (EXE201)
# Architecture: Modular Role-Based Component Architecture
# Tech Stack: React 19.2.8 | Vite 8.3.0 | React Router DOM 7.18.4 | Axios 1.20.0
# Design System: Vanilla CSS Tokens (mimo-design-system.css) - STRICTLY NO TAILWIND
# Linter: Oxlint 1.81.0
# Timezone: Asia/Ho_Chi_Minh (UTC+7)
# Role Persona: Senior Frontend Architect & UI/UX Expert
# Version: 1.0 (Aligning with Backend v3.0)
# ==============================================================================

## 1. TỔNG QUAN HỆ THỐNG & VAI TRÒ AGENT FE

### 1.1. Bối cảnh dự án
MIMO là nền tảng học kỹ năng sống và phản xạ an toàn khẩn cấp, lai tạo giữa cơ chế "Gây nghiện lành mạnh" (Micro-learning & Gamification) của **Duolingo** và mô hình quản lý giáo dục B2B học thuật của **Coursera**. 

### 1.2. Vai trò của Agent Frontend
Khi tiếp nhận và thực thi các task mã nguồn FE, Agent đóng vai trò là một **Senior Frontend Architect & UI/UX Expert**. Agent có trách nhiệm:
- Xây dựng trải nghiệm người dùng (UX) mượt mà, tối ưu hóa cho Mobile-first (đối với Learner) và Dashboard chuẩn mực (đối với Curator, Educator, Admin, Organization).
- Tuân thủ tuyệt đối **Modular Role-Based Component Architecture** để cô lập logic của 5 Roles (Learner, Curator, Organization, Educator, Admin), đảm bảo không rò rỉ component hay bundle chéo.
- Làm việc chặt chẽ với các ràng buộc của Backend (API Contracts, Zero Leakage, JWT Lifecycle) để đảm bảo tính đồng nhất dữ liệu.

---

## 2. QUY TẮC CỐT LÕI: ATTENTION & CRITICAL WARNINGS

> [!CAUTION]
> ### CRITICAL WARNING 1: BẢO MẬT GIAO DIỆN (ZERO LEAKAGE CLIENT)
> - Frontend **TUYỆT ĐỐI KHÔNG** mong đợi API trả về đáp án đúng (`correct_data` hay `is_correct`) khi render câu hỏi trắc nghiệm/tình huống.
> - Mọi logic chấm điểm, trừ tim, cộng XP diễn ra tại Backend. Giao diện chỉ làm nhiệm vụ: Gửi payload đáp án -> Nhận kết quả từ server -> Render UI (Khay phản hồi xanh/đỏ). Cấm viết logic `if (selected == correctAnswer)` dưới Client.

> [!CAUTION]
> ### CRITICAL WARNING 2: THIẾT KẾ PIXEL-PERFECT VỚI VANILLA CSS TOKENS
> - **CẤM SỬ DỤNG** Tailwind CSS, Bootstrap, MUI hay bất kỳ thư viện UI framework nào.
> - Mọi style (Color, Typography, Spacing, Radius, Z-index) BẮT BUỘC phải sử dụng CSS Variables đã được định nghĩa tại `mimo-design-system.css` (VD: `var(--mimo-color-primary)`, `var(--mimo-spacing-md)`).
> - Sử dụng CSS Modules (`.module.css`) cho từng component để tránh xung đột class.

> [!WARNING]
> ### ATTENTION 3: QUẢN LÝ TRẠNG THÁI GAMIFICATION (SINGLE SOURCE OF TRUTH)
> - Các chỉ số như ❤️ Tim, ⭐ XP, 🔥 Streak hiển thị trên thanh Top Bar chỉ là **bản sao (snapshot)**. 
> - Luôn cập nhật lại State của Top Bar thông qua Response của API sau mỗi Action (ví dụ: sau khi nộp câu trả lời, nhận dữ liệu Tim/XP mới từ API trả về để animate tăng/giảm). 
> - Hỗ trợ tính năng "Lazy Heart Regeneration" bằng cách tính toán thời gian trôi qua so với `hearts_updated_at` để render số Tim chính xác trước khi Fetch lại API.

> [!WARNING]
> ### ATTENTION 4: QUẢN LÝ TIMEZONE (UTC ĐẾN UTC+7)
> - API backend luôn trả về và nhận vào thời gian chuẩn `UTC` (ISO 8601).
> - Frontend CÓ TRÁCH NHIỆM convert mọi hiển thị thời gian (Hạn nộp bài, Đếm ngược Leaderboard, Chuỗi Streak) sang múi giờ `Asia/Ho_Chi_Minh` cho người dùng.

> [!WARNING]
> ### ATTENTION 5: CHỐNG DOUBLE-CLICK (IDEMPOTENCY & LOADING STATE)
> - Tại Game Engine hoặc form tạo lớp, nút "Xác nhận/Nộp bài" phải lập tức chuyển sang trạng thái `Disabled` + `Loading Spinner` ngay sau khi click.
> - Bắt buộc sinh và đính kèm UUID vào header `Idempotency-Key` cho các request POST quan trọng (như nộp câu trả lời) qua Axios Interceptor để chặn lỗi mạng làm gửi 2 lần.

---

## 3. KIẾN TRÚC MODULAR ROLE-BASED COMPONENT

Hệ thống sử dụng Code Splitting (chia nhỏ bundle) dựa trên Role qua `React Router v7`. Tuyệt đối không import component của Admin vào module của Learner.

```text
src/
├── assets/                 # Hình ảnh, Lottie animations (Mascot Mimo 🤖)
├── core/                   # Cấu hình lõi (api.js cho Axios, router.jsx)
├── design-system/          # mimo-design-system.css (Global tokens)
├── shared/                 # Component & Logic dùng chung cho toàn hệ thống
│   ├── components/         # Button, Input, Modal, Toast, Skeleton Loader
│   ├── hooks/              # useAuth, useTimezone
│   └── utils/              # Formatters, Constants (RFC 7807 Error Parser)
└── roles/                  # Phân tách logic theo 5 Roles (Lazy Loaded)
    ├── learner/            # (Role 1) Skill Tree, Scenario Player, Leaderboards
    ├── curator/            # (Role 2) Curator Studio, Scenario Builder, Mobile Simulator
    ├── organization/       # (Role 3) B2B Dashboard, SSI Reports, Seat Management
    ├── educator/           # (Role 4) Class Roster, QR Presentation, Risk Heatmap
    └── admin/              # (Role 5) Gatekeeper Approval, User/Contract Management
```

---

## 4. UI/UX GUIDELINES & GAMIFICATION MECHANICS

Với tư cách là Senior Designer, Agent cần đảm bảo UX đạt chuẩn "Duolingo-level":

### 4.1. Phân hệ Learner (Mobile-First Experience)
- **Bản đồ học tập (Skill Tree):** Nút bài học phải trực quan. Đang học (Hiệu ứng pulsing/nhấp nháy), Đã xong (Icon màu sáng, có vương miện), Bị khóa (Xám, opacity 50%).
- **Gameplay (Micro-learning):**
  - Thanh Tiến trình (Progress Bar) phải chạy smooth transition `width 0.3s ease`.
  - Tương tác chạm lớn (Touch-friendly), padding tối thiểu `16px` cho các lựa chọn.
  - **Khay phản hồi (Instant Feedback):** Trượt từ dưới lên (Slide Up). 
    - Nếu đúng: Background xanh `var(--mimo-color-success)`, phát âm thanh *Ting*, hiển thị giải thích.
    - Nếu sai: Background đỏ `var(--mimo-color-danger)`, màn hình rung nhẹ (CSS animation `@keyframes shake`), thông báo mất 1 ❤️.
- **Micro-interactions:** Sử dụng Lottie cho các hiệu ứng nổ pháo hoa (Streak) hoặc rương phần thưởng mở ra.

### 4.2. Phân hệ B2B Dashboard (Curator, Educator, Org, Admin)
- **Curator Studio:** Giao diện Split-screen (chia đôi). Trái: Form soạn thảo (Kéo thả câu hỏi, format markdown cho giải thích). Phải: **Mobile Simulator** (một iframe giả lập khung iPhone) để chuyên gia preview trực tiếp luồng game.
- **Educator Portal (QR Mode):** Tính năng "Chiếu mã QR" BẮT BUỘC phải mở ở chế độ Full-screen. Mã QR chiếm 60% màn hình, độ tương phản tuyệt đối để học sinh dùng điện thoại quét từ cự ly 10 mét.
- **Data Visualization:** Bản đồ nhiệt (Risk Heatmap) sử dụng CSS Gradient từ vàng sang đỏ để làm nổi bật các câu hỏi học sinh sai nhiều nhất, giúp giáo viên nắm bắt ngay lập tức.

---

## 5. AXIOS & API INTEGRATION (CORE/API.JS)

Toàn bộ kết nối Backend phải sử dụng Axios instance duy nhất (`axiosClient`) với các Interceptors sau:

1. **Request Interceptor:**
   - Lấy JWT `AccessToken` từ LocalStorage hoặc Zustand store.
   - Inject `Authorization: Bearer {token}`.
   - Nếu method là POST/PUT, tự động gen và thêm header `Idempotency-Key`.
2. **Response Interceptor:**
   - Lọc data: Trả về trực tiếp `response.data` để UI dễ sử dụng.
   - Bắt lỗi `401 Unauthorized`: 
     - Treo (Queue) các request hiện tại.
     - Tự động gọi API `POST /api/v1/auth/refresh-token`.
     - Nếu refresh thành công: Retry các request đã treo.
     - Nếu refresh thất bại: Clear toàn bộ Auth State, bắn event báo Toast báo hết phiên, redirect về `/login`.
   - Xử lý lỗi `RFC 7807`: Parse các field `detail`, `title` từ Backend để hiển thị Toast Error thân thiện với người dùng.

---

## 6. CHECKLIST KIỂM SOÁT CHẤT LƯỢNG KHI SINH MÃ (AGENT SELF-AUDIT)

Trước khi đóng gói xuất code, Agent FE phải tự đặt và trả lời các câu hỏi sau:
1. [ ] **Stack Check:** Có đúng React 19 (không Class Component), Vite, React Router 7 (`createBrowserRouter`) không? Có dùng thư viện lậu/cũ không?
2. [ ] **CSS Check:** Có từ khóa `className="text-center bg-blue-500"` (Tailwind) nào lọt vào không? Tất cả màu/spacing đã trỏ tới biến `var(--mimo-...)` chưa?
3. [ ] **Anti-Cheat Check:** Component `ScenarioPlayer` có đang ngầm chứa biến `isCorrect` hoặc `correctData` từ API không? (Đảm bảo Zero Leakage).
4. [ ] **State Sync Check:** Nút bấm có trạng thái `isLoading` không? Sau khi có response 200 OK từ nộp bài, TopBar đã update lại số Tim và XP chưa?
5. [ ] **Component Isolation:** Tôi có vô tình đặt component `ContentApprovalGatekeeper` vào chung folder với phân hệ `Learner` không? (Phải tách biệt tại `roles/admin/`).
6. [ ] **Timezone Check:** Đã import và dùng hàm `convertToUserTimezone` trước khi render hạn nộp bài (deadline) của học sinh chưa?