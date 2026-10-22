# ==============================================================================
# MIMO EDUCATION PLATFORM — AUTOMATED DATABASE BACKUP SCRIPT (POWERSHELL)
# Task: BE0-3
# Usage: .\scripts\backup-db.ps1 [-Mode docker|direct] [-RetentionDays 7]
# ==============================================================================

param (
    [string]$Mode = "docker",
    [int]$RetentionDays = 7,
    [string]$BackupDir = ".\backups",
    [string]$ContainerName = "mimo_postgres",
    [string]$DbUser = "postgres",
    [string]$DbName = "mimo_db",
    [string]$DbHost = "localhost",
    [string]$DbPort = "5433",
    [string]$DbPassword = "postgrespassword"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir | Out-Null
}

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$SqlFile = Join-Path $BackupDir "mimo_db_backup_$Timestamp.sql"
$ZipFile = Join-Path $BackupDir "mimo_db_backup_$Timestamp.zip"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ">>> BẮT ĐẦU SAO LƯU DỮ LIỆU CƠ SỞ DỮ LIỆU MIMO (POSTGRESQL 16) <<<" -ForegroundColor Green
Write-Host ">>> Thời gian: $(Get-Date)"
Write-Host ">>> Thư mục lưu trữ: $BackupDir"
Write-Host ">>> Chế độ sao lưu: $Mode"
Write-Host "==================================================================" -ForegroundColor Cyan

try {
    if ($Mode -eq "docker") {
        Write-Host "Đang xuất dữ liệu từ container '$ContainerName'..." -ForegroundColor Yellow
        docker exec -t $ContainerName pg_dump -U $DbUser -d $DbName --clean --if-exists | Out-File -FilePath $SqlFile -Encoding utf8
    } else {
        Write-Host "Đang xuất dữ liệu trực tiếp từ ${DbHost}:${DbPort}/${DbName}..." -ForegroundColor Yellow
        $env:PGPASSWORD = $DbPassword
        pg_dump -h $DbHost -p $DbPort -U $DbUser -d $DbName --clean --if-exists | Out-File -FilePath $SqlFile -Encoding utf8
    }

    Write-Host "Đang nén file SQL thành định dạng ZIP..." -ForegroundColor Yellow
    Compress-Archive -Path $SqlFile -DestinationPath $ZipFile -Force
    Remove-Item -Path $SqlFile -Force

    $FileSize = (Get-Item $ZipFile).Length / 1MB
    Write-Host (">>> Sao lưu thành công: {0} ({1:N2} MB)" -f $ZipFile, $FileSize) -ForegroundColor Green

    # Tự động dọn dẹp các file sao lưu cũ
    $CutoffDate = (Get-Date).AddDays(-$RetentionDays)
    Get-ChildItem -Path $BackupDir -Filter "mimo_db_backup_*.zip" | Where-Object { $_.LastWriteTime -lt $CutoffDate } | ForEach-Object {
        Write-Host "Xóa bản sao lưu cũ: $($_.Name)" -ForegroundColor DarkGray
        Remove-Item $_.FullName -Force
    }

    Write-Host "==================================================================" -ForegroundColor Cyan
    Write-Host ">>> QUY TRÌNH SAO LƯU DATABASE HOÀN TẤT AN TOÀN!" -ForegroundColor Green
    Write-Host "==================================================================" -ForegroundColor Cyan
} catch {
    Write-Host "LỖI KHI SAO LƯU DATABASE: $_" -ForegroundColor Red
    exit 1
}
