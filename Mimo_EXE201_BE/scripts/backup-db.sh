#!/usr/bin/env bash
# ==============================================================================
# MIMO EDUCATION PLATFORM — AUTOMATED DATABASE BACKUP SCRIPT (BASH)
# Task: BE0-3
# Usage: ./scripts/backup-db.sh [docker|direct]
# ==============================================================================

set -euo pipefail

BACKUP_DIR="${BACKUP_DIR:-./backups}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/mimo_db_backup_${TIMESTAMP}.sql"
RETENTION_DAYS="${RETENTION_DAYS:-7}"

mkdir -p "${BACKUP_DIR}"

MODE="${1:-docker}"

echo "=================================================================="
echo ">>> BẮT ĐẦU SAO LƯU DỮ LIỆU CƠ SỞ DỮ LIỆU MIMO (POSTGRESQL 16) <<<"
echo ">>> Thời gian: $(date)"
echo ">>> Thư mục lưu trữ: ${BACKUP_DIR}"
echo ">>> Chế độ sao lưu: ${MODE}"
echo "=================================================================="

if [ "$MODE" = "docker" ]; then
    CONTAINER_NAME="${POSTGRES_CONTAINER:-mimo_postgres}"
    DB_USER="${POSTGRES_USER:-postgres}"
    DB_NAME="${POSTGRES_DB:-mimo_db}"

    echo "Đang xuất dữ liệu từ container '${CONTAINER_NAME}'..."
    docker exec -t "${CONTAINER_NAME}" pg_dump -U "${DB_USER}" -d "${DB_NAME}" --clean --if-exists > "${BACKUP_FILE}"
else
    DB_HOST="${DB_HOST:-localhost}"
    DB_PORT="${DB_PORT:-5433}"
    DB_USER="${POSTGRES_USER:-postgres}"
    DB_NAME="${POSTGRES_DB:-mimo_db}"

    echo "Đang xuất dữ liệu trực tiếp từ ${DB_HOST}:${DB_PORT}/${DB_NAME}..."
    PGPASSWORD="${POSTGRES_PASSWORD:-123456}" pg_dump -h "${DB_HOST}" -p "${DB_PORT}" -U "${DB_USER}" -d "${DB_NAME}" --clean --if-exists > "${BACKUP_FILE}"
fi

# Nén file SQL để tiết kiệm dung lượng lưu trữ
echo "Đang nén bản sao lưu..."
gzip -f "${BACKUP_FILE}"
FINAL_BACKUP="${BACKUP_FILE}.gz"

FILE_SIZE=$(du -h "${FINAL_BACKUP}" | cut -f1)
echo ">>> Sao lưu hoàn tất thành công: ${FINAL_BACKUP} (Kích thước: ${FILE_SIZE})"

# Tự động dọn dẹp các bản sao lưu cũ vượt quá số ngày lưu trữ
echo "Đang dọn dẹp các bản sao lưu cũ hơn ${RETENTION_DAYS} ngày..."
find "${BACKUP_DIR}" -type f -name "mimo_db_backup_*.sql.gz" -mtime +"${RETENTION_DAYS}" -delete
echo ">>> Quy trình sao lưu hoàn tất an toàn!"
echo "=================================================================="
