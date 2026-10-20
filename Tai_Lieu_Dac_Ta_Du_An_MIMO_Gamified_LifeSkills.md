# ĐẶC TẢ CHI TIẾT DỰ ÁN: MIMO — NỀN TẢNG HỌC KỸ NĂNG SỐNG & AN TOÀN GAMIFIED
> **Phiên bản:** 2.0 (Chuẩn hóa theo mô hình Micro-learning & Gamification)  
> **Dự án:** EXE201 — Khởi nghiệp & Phát triển Sản phẩm  
> **Định vị:** *"Duolingo for Life Skills & Emergency Safety"*  
> **Slogan:** *"Học kỹ năng sống. Luyện phản xạ an toàn. Một quyết định đúng mỗi ngày."*

---

## MỤC LỤC
1. [TỔNG QUAN & ĐỊNH VỊ CHIẾN LƯỢC](#1-tổng-quan--định-vị-chiến-lược)
2. [CÁC PHÂN HỆ VÀ VAI TRÒ NGƯỜI DÙNG (ROLES)](#2-các-phân-hệ-và-vai-trò-người-dùng-roles)
3. [CẤU TRÚC HỌC LIỆU & BÀI HỌC (CONTENT ARCHITECTURE)](#3-cấu-trúc-học-liệu--bài-học-content-architecture)
4. [HỆ THỐNG GAMIFICATION & RETENTION (BỘ NÃO GIỮ CHÂN)](#4-hệ-thống-gamification--retention-bộ-não-giữ-chân)
5. [CHI TIẾT CHỨC NĂNG THEO TỪNG PHÂN HỆ](#5-chi-tiết-chức-năng-theo-từng-phân-hệ)
6. [LUỒNG XỬ LÝ NGHIỆP VỤ (CORE PROCESS FLOWS)](#6-luồng-xử-lý-nghiệp-vụ-core-process-flows)
7. [MÔ HÌNH KINH DOANH & DOANH THU (BUSINESS MODEL)](#7-mô-hình-kinh-doanh--doanh-thu-business-model)
8. [LỘ TRÌNH TRIỂN KHAI MVP (EXE201 ROADMAP)](#8-lộ-trình-triển-khai-mvp-exe201-roadmap)

---

## 1. TỔNG QUAN & ĐỊNH VỊ CHIẾN LƯỢC

### 1.1. Bối cảnh & Vấn đề thị trường (Problem Statement)
* **Kỹ năng sống & an toàn khẩn cấp (Sơ cấp cứu, PCCC, chống lừa đảo, bạo lực học đường)** là kiến thức sinh tồn thiết yếu nhưng cách giáo dục truyền thống đang thất bại:
  * **Sách vở & Cẩm nang:** Quá dài, khô khan, đọc xong quên ngay, không thể thực hành phản xạ khi tai nạn xảy ra.
  * **Video TikTok/YouTube:** Rời rạc, xem mang tính giải trí, không có cơ chế đo lường mức độ hiểu và không tạo thành thói quen.
  * **Mô hình E-learning cũ (Udemy/Coursera):** Bắt người học ngồi xem video bài giảng 30–45 phút, tỷ lệ bỏ cuộc lên tới 90%.

### 1.2. Giải pháp của MIMO (The Solution)
MIMO ứng dụng triệt để **công thức thành công của Duolingo** vào lĩnh vực kỹ năng đời sống:
* **Micro-learning:** Chia nhỏ kiến thức thành từng tình huống mô phỏng **2–3 phút**.
* **Active Decision Making:** Học thông qua việc ra quyết định trực tiếp trước các tình huống hiểm nghèo thay vì xem video thụ động.
* **Gamification "Gây nghiện lành mạnh":** Đánh vào tâm lý chuỗi ngày (Streak), tranh tài bảng xếp hạng (Leaderboards), cơ chế tim (Hearts), điểm kinh nghiệm (XP) và linh vật đồng hành (Mascot Robot Mimo 🤖).

---

## 2. CÁC PHÂN HỆ VÀ VAI TRÒ NGƯỜI DÙNG (ROLES)

Hệ thống được thiết kế hoàn chỉnh gồm **5 Vai trò độc lập (5 Distinct Roles)**:

```
┌────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│                                  HỆ THỐNG PHÂN QUYỀN MIMO (5 ROLES)                                    │
└────────────────────────────────────────────────────────────────────────────────────────────────────────┘
        │                   │                       │                      │                   │
        ▼                   ▼                       ▼                      ▼                   ▼
  [ 1. LEARNER ]     [ 2. CURATOR ]        [ 3. ORGANIZATION ]     [ 4. EDUCATOR ]       [ 5. ADMIN ]
   (Người học)      (Chuyên gia ĐỘC LẬP)      (Ban Giám Hiệu)     (Giáo viên trường)   (Chủ hệ thống)
• Học theo lộ trình • Độc lập với trường    • Đại diện trường B2B • Trực thuộc trường  • Quản trị hệ thống
• Xử lý tình huống  • Bác sĩ, CS PCCC...    • Mua & quản lý gói   • Nhận mã kích hoạt  • Thẩm định Curator
• Đua BXH, Streak   • Soạn kịch bản chuẩn     bản quyền (Seats)   • Quản lý lớp học    • Duyệt bài phát hành
• Làm bài tập trường• Viết giải thích y tế  • Cấp mã cho giáo viên• Chiếu QR vào lớp  • Cấu hình Game
• Nhận xét, huy hiệu• Không thuộc trường    • Xuất báo cáo Sở     • Giao bài, nhắc nộp • Doanh thu B2B/B2C
```

### 🎓 Role 1: Người học (Learner / Student - B2C & B2B End-User)
* **Đối tượng:** Học sinh các cấp (Tiểu học, THCS, THPT), sinh viên đại học hoặc người học cá nhân tự do; bao gồm cả học sinh tham gia theo lớp do trường/tổ chức giáo dục quản lý.
* **Nhu cầu:** Học kỹ năng an toàn sinh tồn một cách lôi cuốn, dễ hiểu, tương tác trực tiếp qua bài tập tình huống dạng game, tích lũy điểm kinh nghiệm (XP), giữ chuỗi ngày (Streak), bảo toàn Tim và hoàn thành nhiệm vụ được giao từ nhà trường.
* **Môi trường tiếp cận:** Web App (responsive mobile-first) hoặc Mobile App.

### ✍️ Role 2: Chuyên gia Biên soạn Độc lập (Content Curator - Global Domain Expert)
* **Đối tượng:** Chuyên gia y tế cấp cứu (Bác sĩ 115), Cán bộ Cảnh sát PCCC & CNCH, Chuyên gia an toàn giao thông, Kỹ sư an ninh mạng, Giảng viên chuyên ngành an toàn sinh tồn.
* **Đặc tính cốt lõi:** **Độc lập hoàn toàn với các trường học** (không thuộc biên chế bất kỳ trường nào). Được ký hợp đồng chuyên môn và chịu sự thẩm định, phê chuẩn trực tiếp từ Ban Quản trị MIMO.
* **Nhu cầu:** Cung cấp công cụ xưởng sáng tạo (Curator Studio) để thiết kế lộ trình học tập (Skill Tree), biên soạn ngân hàng tình huống tương tác thực tế, viết giải thích y khoa/kỹ thuật an toàn chuẩn mực, chơi thử nghiệm trên Mobile Simulator và gửi bài lên hàng chờ Admin duyệt phát hành cho **toàn bộ người học trên toàn hệ thống**.
* **Môi trường tiếp cận:** Curator Studio Web Portal.

### 🏛️ Role 3: Ban Giám Hiệu / Quản lý Nhà trường (Organization / School Admin - B2B Client)
* **Đối tượng:** Ban Giám hiệu nhà trường (Hiệu trưởng, Hiệu phó), Phòng Đào tạo/Công tác học sinh, Quản lý doanh nghiệp hoặc trung tâm giáo dục kỹ năng.
* **Nhu cầu:** 
  * Mua và quản lý hợp đồng bản quyền B2B số lượng lớn (Seat licenses);
  * Giám sát Chỉ số Sức khỏe An toàn Học đường (School Safety Index - SSI) cấp vĩ mô toàn trường;
  * Sinh **Mã bản quyền Giáo viên** (Teacher Activation Codes) kèm phân công lớp phụ trách để bàn giao cho các thầy/cô;
  * Xuất file Báo cáo Khảo sát An toàn Học đường chính quy (định dạng PDF chuẩn thể thức có chữ ký số) nộp Sở GD&ĐT và Công an PCCC địa phương.
* **Môi trường tiếp cận:** School Organization B2B Dashboard trên Web.

### 👨‍🏫 Role 4: Giáo viên Trường học (School Educator / Teacher)
* **Đối tượng:** Giáo viên chủ nhiệm, Giáo viên bộ môn (Giáo dục Quốc phòng - An ninh, Sinh học, Hoạt động trải nghiệm, GDCD) trực thuộc một trường học cụ thể (`OrganizationId`).
* **Cơ chế cấp quyền:** Được Ban Giám Hiệu trường cấp Mã bản quyền Giáo viên; giáo viên nhập mã để tự động nhận các lớp học được phân công phụ trách.
* **Nhu cầu:**
  * Quản lý danh sách học sinh theo từng lớp học cụ thể (vd: Lớp 10A1, 10A2);
  * Kích hoạt chế độ **Chiếu mã QR lên máy chiếu/TV lớp học** để 100% học sinh trong lớp quét tham gia chỉ trong 15 giây;
  * Giao bài tập kỹ năng tình huống định kỳ theo tuần (đặt hạn nộp bài);
  * Xem danh sách học sinh chưa nộp bài hoặc chọn sai phương án an toàn khẩn cấp, bấm nút 1-chạm gửi thông báo nhắc nhở đẩy về điện thoại học sinh và dùng dữ liệu để giải thích trong tiết sinh hoạt lớp.
  * *Lưu ý:* Giáo viên trường không có quyền sửa đổi hay can thiệp vào kho học liệu chuẩn của hệ thống do Curator biên soạn.
* **Môi trường tiếp cận:** Educator Class Management Portal trên Web/Tablet.

### ⚙️ Role 5: Chủ hệ thống / Quản trị viên Tối cao (Super Admin & System Operations)
* **Đối tượng:** Đội ngũ sáng lập, quản trị viên cấp cao của nền tảng MIMO.
* **Nhu cầu:** 
  * Kiểm soát và vận hành toàn diện hệ thống: quản lý người dùng theo 5 vai trò (RBAC);
  * Thẩm định hồ sơ công tác chuyên môn, cấp tài khoản và phân bổ chuyên đề (Domain) cho Chuyên gia Curator;
  * Đóng vai trò **Người gác cổng tối cao (The Gatekeeper)**: Ký duyệt lần cuối (Final sign-off) các bài học của Curator trước khi phát hành Live;
  * Quản lý hợp đồng đối tác trường học B2B, quản lý tài chính doanh thu định kỳ (MRR);
  * Cấu hình tham số Game hóa (Tim, XP, Chuỗi Streak, Leagues) và giám sát báo cáo rủi ro an toàn toàn quốc.
* **Môi trường tiếp cận:** Super Admin Portal nội bộ.

---

## 3. CẤU TRÚC HỌC LIỆU & BÀI HỌC (CONTENT ARCHITECTURE)

Khác với danh sách video nhàm chán, học liệu của Mimo được tổ chức theo **Cây kỹ năng (Skill Tree / Learning Path)**:

```
[Chủ đề lớn: SƠ CẤP CỨU]
    │
    ├── [Chặng 1: Xử lý Vết thương & Bỏng]
    │      ├── Nút 1.1: Bỏng nước sôi sinh hoạt (Đã mở khóa)
    │      ├── Nút 1.2: Vết thương chảy máu (Khóa - Cần hoàn thành 1.1)
    │      ├── Nút 1.3: Rương thưởng XP 🎁
    │      └── Nút 1.4: Thử thách Boss Chặng 1 (Đánh giá tổng hợp)
    │
    └── [Chặng 2: Cấp cứu Hồi sinh tim phổi (CPR)]
           ├── Nút 2.1: Nhận biết người bị ngất / ngừng thở
           └── Nút 2.2: Quy trình ép tim đúng nhịp
```

### 3.1. Danh mục 5 Chủ đề Kỹ năng cốt lõi (Core Domains)
1. **🚑 Sơ cấp cứu ban đầu (First Aid):** Xử lý bỏng, cầm máu, dị vật đường thở (hóc nghẹn), đuối nước, say nắng, đột quỵ.
2. **🔥 Thoát hiểm & PCCC (Fire Safety):** Thoát hiểm chung cư mini, xử lý rò rỉ khí gas, dùng bình cứu hỏa mini, chống ngạt khói.
3. **🚦 An toàn giao thông (Road Safety):** Kỹ năng qua đường, điểm mù xe tải, xử lý khi gặp tai nạn, an toàn xe điện.
4. **🛡️ An toàn số & Chống lừa đảo (Cyber Safety & Anti-Scam):** Nhận diện cuộc gọi mạo danh công an/ngân hàng, link độc hại, lộ lọt OTP, bảo vệ danh tính số.
5. **🤝 Phòng chống bạo lực & Sức khỏe tinh thần (Mental & Physical Safety):** Ứng phó bạo lực học đường, ranh giới cơ thể (chống xâm hại), thoát khỏi bẫy lôi kéo.

### 3.2. Ba dạng câu hỏi tương tác chuẩn Duolingo (Interactive Question Types)

| Dạng câu hỏi | Mô tả cách chơi | Ví dụ thực tế trong Mimo |
| :--- | :--- | :--- |
| **1. Quyết định tình huống (Scenario Choice)** | Đưa ra bối cảnh (kèm ảnh/ảnh động GIF). Có 3–4 lựa chọn hành động. Người học chọn cách xử lý. | *“Phát hiện chập điện bốc khói sau tủ lạnh, việc ĐẦU TIÊN bạn làm là gì?”*<br>A. Tạt xô nước vào (Sai - giật điện)<br>B. Ngắt cầu dao tổng (Đúng +10XP)<br>C. Mở toang cửa sổ gọi hàng xóm (Chưa tối ưu) |
| **2. Sắp xếp quy trình (Sequencing Steps)** | Kéo thả 4–5 thẻ hành động theo đúng thứ tự chuẩn y tế/an toàn. | *“Sắp xếp quy trình xử lý nạn nhân bị bỏng nước sôi:”*<br>① Đưa nạn nhân khỏi nguồn nhiệt ➔ ② Xả nước mát sạch 15-20p ➔ ③ Che vết bỏng bằng gạc sạch ➔ ④ Đưa tới viện. |
| **3. Vạch lá tìm sâu (Spot the Red Flag)** | Hiển thị 1 giao diện tin nhắn SMS/Zalo hoặc hợp đồng. Người học chạm vào điểm bất thường. | Người học chạm vào tên miền giả mạo `vietcombannk-otp.com` hoặc cú pháp đe dọa đòi chuyển tiền gấp để phát hiện bẫy lừa đảo. |

---

## 4. HỆ THỐNG GAMIFICATION & RETENTION (BỘ NÃO GIỮ CHÂN)

### 4.1. 🔥 Cơ chế Chuỗi ngày (Streak)
* **Quy tắc:** Mỗi ngày hoàn thành tối thiểu **01 bài học** (bất kỳ) trước 23:59:
  * Hoàn thành: Chuỗi tăng thêm +1 ngày (🔥 Day 1, Day 2...).
  * Bỏ lỡ: Chuỗi bị reset về 0.
* **Đóng băng chuỗi (Streak Freeze):** Vật phẩm bảo hiểm cho phép cứu chuỗi nếu bận 1 ngày. Người học có thể mua bằng điểm XP hoặc mở từ rương.

### 4.2. ❤️ Cơ chế Tim (Hearts / Lives)
* **Quy tắc:** Người học có tối đa **5 Tim**.
  * Chọn sai 1 quyết định: **-1 Tim**. Hệ thống rung nhẹ (haptic) và hiển thị cảnh báo đỏ + giải thích nguy hiểm.
  * Khi về 0 Tim: Không thể chơi bài mới.
* **Cách hồi phục Tim:**
  * Tự động: Hồi 1 Tim mỗi 4 tiếng.
  * Chế độ Luyện tập (Practice to Earn Heart): Làm lại các câu đã từng làm sai trong quá khứ để nhận ngay 1 Tim (biến việc thất bại thành cơ hội ôn bài).
  * Tài khoản Mimo Pro: Vô hạn Tim (Unlimited Hearts).

### 4.3. ⭐ Điểm kinh nghiệm (XP) & Cấp độ (Levels)
* **Điểm thưởng chuẩn:**
  * Đúng lần đầu: **+10 XP**
  * Đúng liên hoàn (Combo): **+2 đến +5 XP**
  * Hoàn hảo (Perfect Lesson - không mất tim nào): **+5 XP thưởng**
* **Hệ thống Cấp bậc (Tier/Rank):**
  * Level 1: Người mới bắt đầu (Tân binh an toàn)
  * Level 2: Người phòng vệ (Defender)
  * Level 3: Người giải cứu (Rescuer)
  * Level 4: Chuyên gia an toàn (Safety Guardian)
  * Level 5: Người làm chủ cuộc sống (Life Master)

### 4.4. 🏆 Bảng xếp hạng tuần (Weekly Leagues)
* **Quy chế:** Nhóm người dùng ngẫu nhiên vào 1 bảng đấu 30 người có trình độ tương đương.
* **Chu kỳ:** Từ 00:00 Thứ Hai đến 23:59 Chủ Nhật.
* **Phân hạng:**
  * Top 1–7: **Thăng hạng** lên giải cao hơn (Đồng ➔ Bạc ➔ Vàng ➔ Kim Cương).
  * Hạng 8–24: Trụ hạng.
  * Hạng 25–30: **Rớt hạng** về giải thấp hơn.
  * Top 1–3 nhận rương thưởng lớn (Rương Kim Cương chứa Huy hiệu độc quyền).

---

## 5. CHI TIẾT CHỨC NĂNG THEO TỪNG PHÂN HỆ

### 5.1. Phân hệ Học sinh (Student Web/Mobile App)
1. **Onboarding & Khảo sát cá nhân hóa:**
   * Chọn mục tiêu học tập hàng ngày (5 phút, 10 phút, 15 phút/ngày).
   * Khảo sát mức độ nhận biết rủi ro để đề xuất chặng xuất phát phù hợp.
2. **Bản đồ Lộ trình học (Gamified Skill Tree):**
   * Hiển thị cây kỹ năng cuộn dọc với các nút bài học trực quan.
   * Trạng thái bài học rõ ràng: Đã hoàn thành (vàng/xanh), Đang học (hiệu ứng nhấp nháy), Đang khóa (xám).
   * Thanh chỉ số tức thời (Top Bar): Cờ chủ đề, 🔥 Streak, ⭐ XP, ❤️ Tim.
3. **Trình chơi Tình huống tương tác (Scenario Game Engine):**
   * Thanh tiến độ bài học (Progress bar) trực quan.
   * Tình huống mô phỏng chân thực: Nội dung kịch bản cô đọng + minh họa hình ảnh/ảnh động.
   * Tương tác đa dạng: Chọn quyết định tình huống, sắp xếp quy trình cấp cứu/thoát hiểm, tìm bẫy lừa đảo.
   * Khay phản hồi tức thì (Instant Feedback Sheet): Màu xanh khen thưởng kèm giải thích y tế/an toàn chuẩn, hoặc màu đỏ cảnh báo hậu quả nguy hiểm kèm trừ Tim.
4. **Bảng xếp hạng Tuần (Weekly Leagues):**
   * Bảng đua top 30 người học cùng hạng, cập nhật thứ hạng và hiển thị đồng hồ đếm ngược kết thúc tuần.
5. **Khu vực Lớp học & Nhiệm vụ B2B (Class Assignments):**
   * Nhập mã lớp (Class Code) để tham gia lớp của trường/tổ chức.
   * Xem danh sách bài tập, thời hạn (deadline) và tiến độ hoàn thành các nhiệm vụ do tổ chức giao.
6. **Hồ sơ & Kho thành tích cá nhân (Profile & Badges Showcase):**
   * Thống kê tổng số ngày Streak, tổng XP, số bài học đã hoàn thành.
   * Bộ sưu tập danh hiệu, huy hiệu an toàn (Badges) đã mở khóa.

### 5.2. Phân hệ Chuyên gia Biên soạn Độc lập (Curator Studio Portal)
1. **Quản lý Cây Kỹ năng & Lộ trình Chuẩn mực:**
   * Thiết lập và sắp xếp các Chủ đề lớn (Domain) ➔ Chặng kỹ năng (Units) ➔ Nút bài học (Nodes) dùng chung cho toàn bộ người học trên hệ thống.
   * Cấu hình điều kiện mở khóa bài học, điểm thưởng XP và các bài kiểm tra chặng (Boss Challenge).
2. **Trình biên soạn kịch bản tình huống (Scenario Builder):**
   * Tạo câu hỏi tình huống tương tác theo các định dạng: Quyết định tình huống (Scenario Choice), Sắp xếp quy trình (Sequencing Steps), Vạch lá tìm sâu (Spot the Red Flag).
   * Tải lên hình ảnh mô phỏng, gif trực quan cho tình huống khẩn cấp.
   * Soạn thảo lời giải thích chuyên môn: Phân tích vì sao đúng, vì sao nguy hiểm theo chuẩn y khoa, an toàn PCCC hoặc an ninh mạng.
3. **Mô phỏng trải nghiệm & Hàng chờ thẩm định:**
   * Trực tiếp chơi thử bộ câu hỏi trên **Mobile Simulator** tích hợp sẵn trong Curator Studio để kiểm tra độ dài câu chữ và nhịp điệu game.
   * Gửi bài học lên Hàng chờ Phê duyệt của Super Admin; tiếp nhận phản hồi yêu cầu chỉnh sửa nếu có sai sót.
4. **Báo cáo chất lượng học liệu (Content Quality Analytics):**
   * Thống kê tỷ lệ học viên cả nước chọn sai từng câu hỏi để tối ưu hóa phác đồ và nội dung kịch bản kịp thời.

### 5.3. Phân hệ Ban Giám Hiệu / Quản lý Nhà trường (School Organization B2B Portal)
1. **Quản lý Hợp đồng & Gói bản quyền B2B (Seat Subscription):**
   * Theo dõi số lượng tài khoản (seats) đã kích hoạt / còn trống, thời hạn hợp đồng bản quyền của trường.
2. **Quản lý & Cấp Mã Bản Quyền Giáo Viên (Teacher License Provisioning):**
   * Ban Giám Hiệu sinh **Mã kích hoạt Giáo viên** (Teacher Activation Codes) kèm chỉ định các khối/lớp phân công phụ trách.
   * Bàn giao mã qua Email/Zalo nội bộ cho thầy/cô để giáo viên tự kích hoạt nhận quyền.
3. **Giám sát Chỉ số Sức khỏe An toàn Toàn trường (School Safety Index - SSI):**
   * Theo dõi bức tranh tổng thể mức độ sẵn sàng ứng phó rủi ro an toàn của toàn trường qua 5 trụ cột kỹ năng.
   * So sánh tỷ lệ hoàn thành kỹ năng giữa các khối lớp (K10, K11, K12).
4. **Xuất Hồ sơ Báo cáo An toàn Chính quy (Official Safety Audit Report):**
   * Xuất báo cáo định dạng PDF/Excel chuẩn thể thức văn bản nhà nước (có chữ ký số của Hiệu trưởng và mộc điện tử MIMO) để nộp trực tiếp cho **Sở GD&ĐT** và **Công an PCCC** khi có đợt kiểm tra an toàn học đường.

### 5.4. Phân hệ Giáo viên Trường học (School Educator Portal)
1. **Quản lý Lớp học & Học sinh (Classroom Roster):**
   * Quản lý danh sách học sinh theo các lớp được Ban Giám Hiệu phân công (vd: Lớp 10A1, 10A2).
   * **Chế độ Chiếu mã QR lên bảng (Presentation Mode):** Giáo viên chiếu mã QR của lớp lên máy chiếu/TV trong tiết sinh hoạt đầu năm để học sinh quét vào lớp trong 15 giây.
2. **Trung tâm Giao bài tập Kỹ năng tuần (Assignment Center):**
   * Lựa chọn chủ đề an toàn phù hợp với kế hoạch giáo dục của trường (ví dụ: *"Tuần 3: Thoát hiểm hỏa hoạn nhà cao tầng"*).
   * Đặt thời hạn hoàn thành (deadline) và chỉ tiêu điểm chuẩn.
3. **Bản đồ nhiệt Rủi ro & Nhắc nhở Một chạm (Risk Heatmap & 1-Click Reminder):**
   * Theo dõi tiến độ nộp bài của từng học sinh theo thời gian thực.
   * Nhận diện các câu hỏi tình huống có trên 50% học sinh trong lớp chọn sai phương án (ví dụ: bôi kem đánh răng vào vết bỏng).
   * Bấm nút gửi thông báo đẩy (push notification) nhắc nhở học sinh chưa làm bài trước hạn chót.
   * Sử dụng dữ liệu báo cáo để dành 3-5 phút giải thích lỗi sai cho cả lớp trong tiết sinh hoạt cuối tuần.

### 5.5. Phân hệ Quản trị Hệ thống (Admin Portal - Chủ hệ thống)
1. **Quản trị Tài khoản & Phân quyền RBAC 5 Roles:**
   * Quản lý danh sách toàn bộ người dùng theo 5 vai trò: Learner, Curator, Organization, Educator, Admin.
   * Khóa / mở khóa tài khoản, thẩm định hồ sơ và cấp tài khoản cho Chuyên gia Curator.
2. **Người Gác Cổng Tối Cao & Thẩm định Xuất bản (Content Approval Gatekeeper):**
   * Trực tiếp trải nghiệm kịch bản của Curator trên Giả lập; bấm ký duyệt phát hành (Publish Live) hoặc yêu cầu sửa đổi kèm ghi chú chi tiết.
   * Thu hồi khẩn cấp (Emergency Rollback) bài học khi phát hiện sai lệch phác đồ an toàn.
3. **Quản lý Đối tác Trường học B2B & Hợp đồng Doanh thu:**
   * Khởi tạo tổ chức trường học mới, cấu hình hạn mức Seats và quản lý doanh thu B2B/B2C.
4. **Cấu hình Tham số Nền tảng (Platform & Gamification Balancing):**
   * Tinh chỉnh thời gian hồi Tim, điểm XP, hệ số Combo, mốc League tuần và quản trị thông báo toàn hệ thống.

---

## 6. LUỒNG XỬ LÝ NGHIỆP VỤ (CORE PROCESS FLOWS)

### 6.1. Luồng Chơi Một Ván Bài Học (Gameplay Loop)
1. Người học bấm vào một Nút bài học trên Bản đồ lộ trình.
2. Hệ thống kiểm tra số lượng Tim:
   * Nếu Tim = 0: Thông báo hết tim, gợi ý vào phòng Ôn tập để kiếm tim hoặc chờ hồi.
   * Nếu Tim > 0: Tải bộ câu hỏi tình huống của bài học.
3. Hiển thị từng câu hỏi:
   * Người học đọc tình huống và bấm chọn quyết định.
   * Nếu ĐÚNG: Phát âm thanh chúc mừng, viền xanh lá, cộng điểm XP, hiển thị giải thích cơ sở an toàn.
   * Nếu SAI: Phát âm thanh cảnh báo, viền đỏ, trừ 1 Tim, hiển thị giải thích hậu quả tai nạn, lưu câu hỏi vào danh sách "cần ôn lại".
4. Nếu hết Tim giữa chừng: Màn hình kết thúc với thông điệp động viên từ Robot Mimo.
5. Nếu vượt qua tất cả các câu hỏi:
   * Màn hình Tổng kết: Tính toán tổng XP nhận được.
   * Cập nhật Chuỗi ngày (Streak) nếu đây là bài học đầu tiên trong ngày.
   * Kiểm tra và trao Huy hiệu mới nếu đạt mốc thành tích.

### 6.2. Luồng Kiểm Tra & Cập Nhật Chuỗi Ngày (Daily Streak Logic)
1. Khi hoàn thành bài học, hệ thống lấy ngày hiện tại (theo múi giờ địa phương YYYY-MM-DD).
2. So sánh với ngày học gần nhất (`last_active_date`):
   * Nếu trùng hôm nay: Giữ nguyên Streak (đã tính rồi).
   * Nếu là ngày hôm qua (cách đúng 1 ngày): `streak = streak + 1`.
   * Nếu cách từ 2 ngày trở lên:
     * Nếu có vật phẩm `Streak Freeze` (Bùa đóng băng): Tiêu thụ 1 bùa, giữ nguyên Streak, gửi thông báo chúc mừng thoát nạn đứt chuỗi.
     * Nếu không có bùa: Reset `streak = 1` (Bắt đầu lại chuỗi mới).
3. Lưu trạng thái mới vào cơ sở dữ liệu.

---

## 7. MÔ HÌNH KINH DOANH & DOANH THU (BUSINESS MODEL)

MIMO áp dụng mô hình doanh thu lai **B2C Freemium + B2B School Subscriptions + Sponsorship**:

| Kênh doanh thu | Đối tượng chi trả | Mô tả chi tiết | Tiềm năng quy mô |
| :--- | :--- | :--- | :--- |
| **1. B2C Mimo Pro (Subscription)** | Phụ huynh / Người học cá nhân | Gói 49.000đ/tháng hoặc 399.000đ/năm:<br>• Vô hạn tim (luyện tập thoải mái).<br>• Không quảng cáo.<br>• Mở khóa phòng luyện tập chuyên sâu các tình huống sai. | Doanh thu định kỳ (MRR) ổn định từ tệp người dùng trung thành. |
| **2. B2B School License (Trọng tâm EXE201)** | Các trường THCS, THPT, Đại học | Gói bản quyền theo năm cho trường học (15.000đ - 20.000đ/học sinh/năm):<br>• Trường có portal quản trị để giao bài kỹ năng sống.<br>• Xuất báo cáo chứng nhận an toàn định kỳ gửi Sở/Phòng GD&ĐT. | Hợp đồng giá trị lớn (B2B Contract), độ rời bỏ (churn rate) thấp. |
| **3. Brand Sponsorship (Tài trợ doanh nghiệp)** | Ngân hàng, Công ty Bảo hiểm, Bệnh viện | Doanh nghiệp tài trợ để đặt các tình huống mang tính giáo dục thương hiệu (vd: *Ngân hàng VPBank/MB tài trợ chuyên đề "Chống lừa đảo chuyển tiền"*). | Doanh thu tài trợ xã hội (CSR) và quảng cáo tự nhiên (Native Ads). |

---

## 8. LỘ TRÌNH TRIỂN KHAI MVP (EXE201 ROADMAP)

Để đảm bảo dự án vừa vặn với tiến độ môn học EXE201 và đạt điểm xuất sắc trước hội đồng, kế hoạch triển khai được chia làm 3 giai đoạn rõ ràng:

### ✅ Giai đoạn 1: Tập trung MVP phục vụ Demo & Đánh giá (Scope hiện tại)
1. **Frontend:** 
   * Giao diện Bản đồ học (Path Map) với các Nút bài học.
   * Giao diện Chơi kịch bản tình huống (Scenario Player) hỗ trợ câu hỏi trắc nghiệm, tính giờ, trừ tim, cộng XP.
   * Thanh trạng thái hiển thị Streak, XP, Tim và Avatar linh vật Mimo 🤖.
   * Màn hình Bảng xếp hạng (Leaderboard demo).
2. **Nội dung mẫu (Mock Data):**
   * Xây dựng chuẩn chỉnh **15 tình huống thực tế chất lượng cao**:
     * 5 tình huống Sơ cấp cứu (Bỏng, đuối nước, hóc dị vật, ngừng tim, gãy xương).
     * 5 tình huống PCCC & Thoát hiểm (Cháy chung cư, cháy chập điện, rò rỉ gas).
     * 5 tình huống Chống lừa đảo không gian mạng (Mạo danh công an, link trúng thưởng lừa đảo, giả mạo giọng nói Deepfake).
3. **Kịch bản Demo hội đồng EXE201:**
   * Demo trực tiếp từ lúc vào Landing Page ➔ Bấm học ngay ➔ Thử chọn sai để thấy cảnh báo nguy hiểm và mất tim ➔ Chọn đúng để nhận XP và nổ pháo hoa giữ Streak ➔ Xem vị trí trên Bảng xếp hạng.
