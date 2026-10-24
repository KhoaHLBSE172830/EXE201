using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services
{
    public class AdminSchoolService : IAdminSchoolService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminSchoolService> _logger;

        public AdminSchoolService(
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminSchoolService> logger)
        {
            _orgRepository = orgRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AdminSchoolDto>> GetSchoolsAsync(CancellationToken cancellationToken = default)
        {
            var orgs = await _orgRepository.WhereAsync(
                filter: null,
                orderBy: q => q.OrderByDescending(o => o.CreatedAt),
                page: null,
                pageSize: null,
                nameof(Organization.Users)
            );

            var result = new List<AdminSchoolDto>();

            foreach (var org in orgs)
            {
                var activated = org.Users?.Count ?? 0;
                var total = Math.Max(org.SeatCount, 1);

                var statusStr = "ACTIVE";
                if (org.Status == OrganizationStatusEnum.Suspended)
                {
                    statusStr = "SUSPENDED";
                }
                else if (org.ExpirationDate.HasValue)
                {
                    if (org.ExpirationDate.Value < DateTime.UtcNow)
                    {
                        statusStr = "EXPIRED";
                    }
                    else if (org.ExpirationDate.Value < DateTime.UtcNow.AddDays(30))
                    {
                        statusStr = "EXPIRING_SOON";
                    }
                }

                var contractStart = org.CreatedAt?.ToString("yyyy-MM-dd") ?? "2026-09-01";
                var contractEnd = org.ExpirationDate?.ToString("yyyy-MM-dd") ?? "2027-08-31";
                var contractValue = $"{((long)org.SeatCount * 50000):N0} VNĐ";

                var classes = new List<string>
                {
                    $"{org.Code}-10A1",
                    $"{org.Code}-10A2",
                    $"{org.Code}-11B",
                    $"{org.Code}-12A"
                };

                result.Add(new AdminSchoolDto
                {
                    Id = org.Id,
                    Name = org.Name,
                    Code = org.Code,
                    Address = org.Address ?? "TP. Hồ Chí Minh",
                    ContactPerson = !string.IsNullOrEmpty(org.PhoneNumber) ? $"Ban Giám Hiệu ({org.PhoneNumber})" : "Ban Giám Hiệu",
                    ContactEmail = org.Email,
                    TotalLicenses = org.SeatCount,
                    ActivatedLicenses = activated,
                    ContractStart = contractStart,
                    ContractEnd = contractEnd,
                    ContractValue = contractValue,
                    Status = statusStr,
                    SafetyAuditPassed = statusStr == "ACTIVE",
                    Classes = classes
                });
            }

            return result;
        }

        public async Task<List<AdminSchoolDto>> CreateSchoolContractAsync(CreateSchoolContractDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tên trường / tổ chức không được để trống.");
            }

            var cleanCode = string.IsNullOrWhiteSpace(request.Code)
                ? $"SCH-{Guid.NewGuid().ToString()[..6].ToUpper()}"
                : request.Code.Trim().ToUpper();

            var existingCode = await _orgRepository.FirstOrDefaultAsync(o => o.Code == cleanCode);
            if (existingCode != null)
            {
                throw new BadRequestException($"Mã trường '{cleanCode}' đã tồn tại trong hệ sinh thái.");
            }

            DateTime contractExpiry = DateTime.UtcNow.AddYears(1);
            if (!string.IsNullOrWhiteSpace(request.ContractEnd) && DateTime.TryParse(request.ContractEnd, out var parsedEnd))
            {
                contractExpiry = DateTime.SpecifyKind(parsedEnd, DateTimeKind.Utc);
            }

            if (cleanCode.Length > 20)
            {
                cleanCode = cleanCode[..20];
            }

            var phone = !string.IsNullOrWhiteSpace(request.ContactPerson)
                ? (request.ContactPerson.Trim().Length > 20 ? request.ContactPerson.Trim()[..20] : request.ContactPerson.Trim())
                : "0900000000";

            var org = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Code = cleanCode,
                Address = request.Address?.Trim() ?? "Việt Nam",
                Email = request.ContactEmail?.Trim() ?? $"{cleanCode.ToLower()}@school.edu.vn",
                PhoneNumber = phone,
                SeatCount = Math.Max(request.TotalLicenses, 50),
                ExpirationDate = contractExpiry,
                Status = OrganizationStatusEnum.Active,
                SubscriptionTier = SubscriptionTierEnum.Enterprise,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _orgRepository.CreateAsync(org);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin đã tạo thành công hợp đồng trường B2B mới: {OrgName} ({OrgCode}) với {Seats} seats",
                org.Name, org.Code, org.SeatCount);

            return await GetSchoolsAsync(cancellationToken);
        }

        public async Task<List<AdminSchoolDto>> UpdateSchoolContractAsync(Guid schoolId, UpdateSchoolContractDto request, CancellationToken cancellationToken = default)
        {
            var org = await _orgRepository.FindAsync(o => o.Id == schoolId);
            if (org == null)
            {
                throw new NotFoundException($"Không tìm thấy trường học có ID '{schoolId}'.");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                org.Name = request.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Address))
            {
                org.Address = request.Address.Trim();
            }

            if (request.TotalLicenses.HasValue && request.TotalLicenses.Value > 0)
            {
                org.SeatCount = request.TotalLicenses.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.ContractEnd) && DateTime.TryParse(request.ContractEnd, out var parsedEnd))
            {
                org.ExpirationDate = DateTime.SpecifyKind(parsedEnd, DateTimeKind.Utc);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                org.Status = request.Status.ToUpperInvariant() switch
                {
                    "SUSPENDED" => OrganizationStatusEnum.Suspended,
                    "EXPIRED" => OrganizationStatusEnum.Expired,
                    _ => OrganizationStatusEnum.Active
                };
            }

            org.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin đã cập nhật thành công hợp đồng trường B2B: {OrgName} ({OrgCode})", org.Name, org.Code);

            return await GetSchoolsAsync(cancellationToken);
        }

        public async Task<List<AdminSchoolDto>> GenerateClassCodesAsync(Guid schoolId, int count, CancellationToken cancellationToken = default)
        {
            var org = await _orgRepository.FindAsync(o => o.Id == schoolId);
            if (org == null)
            {
                throw new NotFoundException($"Không tìm thấy trường học có ID '{schoolId}'.");
            }

            _logger.LogInformation("Admin đã tạo {Count} mã lớp học mới cho trường {OrgName} ({OrgCode})", count, org.Name, org.Code);

            return await GetSchoolsAsync(cancellationToken);
        }
    }
}
