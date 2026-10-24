# SỔ TAY VẬN HÀNH DEVOPS — HỆ THỐNG MIMO BACK-END (.NET 8)

> **Dự án:** MIMO — Nền tảng EdTech Giáo dục Kỹ năng Sinh tồn & Phòng chống Bắt nạt Học đường  
> **Kiến trúc:** Clean Onion Architecture (.NET 8 Web API + PostgreSQL 16 + Redis + Docker + Supabase)  
> **Đáp ứng các task:** BE0-1, BE0-2, BE0-3, BE0-4, BE0-5 ([Plan&Task.md](file:///d:/Full_Library/FPT_Library/SE_8/EXE201/EXE201/Mimo_EXE201_BE/Plan&Task.md))

---

## 1. TỔNG QUAN HẠ TẦNG & CÁC THÀNH PHẦN

Hệ thống được thiết kế theo chuẩn Cloud-Native, hỗ trợ chạy mượt mà ở cả **Local Docker Compose** (phát triển, demo offline) và **Cloud Managed** (Staging/Production với chi phí $0 cho nhóm startup sinh viên).

```
[ Khách hàng / Mobile / Web ]
             │
             ▼
     [ Cloudflare / Edge ] (SSL Termination)
             │
             ▼
[ Render / Docker Container (.NET 8 API) ] ── (Port 8080)
   ├── /health (Liveness / Readiness Probe)
   ├── /swagger (Tài liệu OpenAPI)
   ├── Dynamic Migration & Seeder (24 tình huống an toàn)
   │
   ├── [ Database: Supabase PostgreSQL (Port 5432 / 6543) ]
   ├── [ Storage: Supabase S3-compatible Storage (mimo-assets) ]
   └── [ Cache: Redis 7 In-Memory Caching (Optional) ]
```

---

## 2. CHẠY CỤC BỘ BẰNG DOCKER COMPOSE (LOCAL DEVELOPMENT)

Hệ thống đã tích hợp sẵn trọn bộ container trong [`docker-compose.yml`](file:///d:/Full_Library/FPT_Library/SE_8/EXE201/EXE201/Mimo_EXE201_BE/docker-compose.yml):
- **`postgres`:** PostgreSQL 16 Alpine (Port `5433:5432`)
- **`pgadmin`:** Giao diện quản trị Database trực quan (Port `5050:80`)
- **`redis`:** Redis 7 Alpine (Port `6379:6379`)
- **`mimo-api`:** Ứng dụng .NET 8 Web API (Port `8080:8080`)

### Lệnh khởi chạy:
```bash
# 1. Tạo file cấu hình từ template mẫu
cp .env.example .env

# 2. Khởi chạy toàn bộ hệ thống ở chế độ nền
docker compose up -d --build

# 3. Theo dõi nhật ký khởi động và quá trình seed dữ liệu
docker compose logs -f mimo-api

# 4. Khi cần dừng toàn bộ hệ sinh thái
docker compose down
```

### Các đường dẫn kiểm tra nhanh:
- **Swagger UI:** `http://localhost:8080/swagger`
- **Health Check RFC:** `http://localhost:8080/health`
- **pgAdmin 4:** `http://localhost:5050`
  - *Tài khoản:* `admin@mimo.app`
  - *Mật khẩu:* `Admin123!`
  - *Kết nối DB:* Host `postgres`, Port `5432`, User `postgres`, Password `postgrespassword`, DB `mimo_db`

---

## 3. HƯỚNG DẪN TRIỂN KHAI CLOUD (PRODUCTION / STAGING MIỄN PHÍ)

Mô hình tối ưu nhất cho nhóm sinh viên khởi nghiệp (6 thành viên):
- **Cơ sở dữ liệu:** Supabase PostgreSQL (Managed Cloud, tự động sao lưu, tích hợp S3 Storage `mimo-assets`)
- **Máy chủ Web API:** Render Web Service (Free Tier, tự động build từ Dockerfile, hỗ trợ HTTPS tự động)

### Bước 1: Lấy thông tin kết nối Supabase PostgreSQL
1. Truy cập [Supabase Dashboard](https://supabase.com/dashboard/project/axbejjdzsuelscpkuwab).
2. Vào **Project Settings** -> **Database**.
3. Tại mục **Connection string**, chọn tab **URI** hoặc **Connection pooling** (chọn **Session** mode port `5432` hoặc **Transaction** mode port `6543`).
4. Chuỗi kết nối có dạng:
   ```text
   Host=aws-0-ap-southeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.axbejjdzsuelscpkuwab;Password=YOUR_DATABASE_PASSWORD;Timeout=300;CommandTimeout=300
   ```

### Bước 2: Triển khai lên Render bằng `render.yaml` (1-Click)
1. Đẩy code lên GitHub repository của nhóm.
2. Đăng nhập [Render.com](https://render.com), chọn **Blueprints** -> **New Blueprint Instance**.
3. Chọn repo `Mimo_EXE201_BE`. Render sẽ tự động đọc cấu hình file [`render.yaml`](file:///d:/Full_Library/FPT_Library/SE_8/EXE201/EXE201/Mimo_EXE201_BE/render.yaml).
4. Điền các biến môi trường bảo mật:
   - `ConnectionStrings__DefaultConnection`: Chuỗi kết nối Supabase lấy ở Bước 1.
   - `Supabase__ApiKey`: Anon key của dự án Supabase.
   - `APPLY_MIGRATIONS`: `true` (Hệ thống sẽ tự động chạy toàn bộ Migration và Seeding 24 tình huống ngay khi container khởi động lần đầu).
   - `ENABLE_SWAGGER`: `true` (Cho phép mở Swagger UI trên Cloud để nhóm Frontend kiểm thử trực tiếp).
5. Bấm **Apply**. Ứng dụng sẽ được build và online trong 3-5 phút với tên miền `https://mimo-be-api.onrender.com`.

---

## 4. TỰ ĐỘNG HÓA CI/CD VỚI GITHUB ACTIONS

Hệ thống đã cấu hình pipeline tại [`.github/workflows/ci-cd.yml`](file:///d:/Full_Library/FPT_Library/SE_8/EXE201/EXE201/Mimo_EXE201_BE/.github/workflows/ci-cd.yml):

```
[ Developer Push Code ]
          │
          ▼
   [ Stage 1: Build & Verify ] ──> dotnet restore & build Release
          │
          ▼
   [ Stage 2: Docker Build ]   ──> Kiểm tra multi-stage build & Healthcheck
          │
          ▼
   [ Stage 3: Auto Deploy ]    ──> Gửi Webhook kích hoạt Render cập nhật (khi merge vào main)
```

### Cách thiết lập Auto-Deploy khi đẩy code vào `main`:
1. Trên Render Dashboard của service `mimo-be-api`, vào **Settings** -> sao chép đường link **Deploy Hook**.
2. Trên GitHub repo, vào **Settings** -> **Secrets and variables** -> **Actions**.
3. Thêm secret mới:
   - Tên: `RENDER_DEPLOY_HOOK_URL`
   - Giá trị: Đường link Deploy Hook vừa copy.
4. Từ nay, mỗi khi thành viên trong nhóm merge PR vào nhánh `main`, hệ thống sẽ tự động kiểm tra code, đóng gói Docker và kích hoạt cập nhật bản mới nhất lên Cloud mà không cần thao tác tay.

---

## 5. CƠ CHẾ SAO LƯU DỰ PHÒNG CƠ SỞ DỮ LIỆU (DISASTER RECOVERY)

Thực thi yêu cầu task **BE0-3** về tự động hóa sao lưu định kỳ:

### Cách chạy trên Windows (PowerShell):
```powershell
# Sao lưu từ Docker cục bộ:
.\scripts\backup-db.ps1 -Mode docker -RetentionDays 7

# Hoặc sao lưu từ máy chủ Cloud từ xa:
.\scripts\backup-db.ps1 -Mode direct -DbHost "aws-0-ap-southeast-1.pooler.supabase.com" -DbPort 5432 -DbName "postgres" -DbUser "postgres.axbejjdzsuelscpkuwab" -DbPassword "YOUR_PASSWORD"
```

### Cách chạy trên Linux / macOS / Server CI:
```bash
chmod +x ./scripts/backup-db.sh

# Chạy sao lưu cục bộ:
./scripts/backup-db.sh docker

# Chạy sao lưu từ xa:
DB_HOST="aws-0-ap-southeast-1.pooler.supabase.com" DB_PORT=5432 DB_NAME="postgres" DB_USER="postgres.axbejjdzsuelscpkuwab" POSTGRES_PASSWORD="YOUR_PASSWORD" ./scripts/backup-db.sh direct
```

- Các bản sao lưu được lưu trữ tại thư mục `./backups/` dưới dạng file nén `.zip` / `.gz`.
- Script tự động kích hoạt tính năng **Retention Cleanup**: Tự động xóa các bản sao lưu cũ quá 7 ngày để tránh đầy bộ nhớ.
