using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO đối tác trường học B2B và bản quyền Seat
    /// </summary>
    public class AdminSchoolDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string ContactPerson { get; set; } = "Ban Giám Hiệu";
        public string ContactEmail { get; set; } = string.Empty;
        public int TotalLicenses { get; set; }
        public int ActivatedLicenses { get; set; }
        public string ContractStart { get; set; } = string.Empty;
        public string ContractEnd { get; set; } = string.Empty;
        public string ContractValue { get; set; } = string.Empty;
        public string Status { get; set; } = "ACTIVE"; // ACTIVE | EXPIRING_SOON | EXPIRED | SUSPENDED
        public bool SafetyAuditPassed { get; set; } = true;
        public List<string> Classes { get; set; } = new();
    }

    /// <summary>
    /// DTO tạo mới hợp đồng trường học B2B
    /// </summary>
    public class CreateSchoolContractDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public int TotalLicenses { get; set; } = 1000;
        public string? ContractStart { get; set; }
        public string? ContractEnd { get; set; }
        public string? ContractValue { get; set; }
    }

    /// <summary>
    /// DTO cập nhật hợp đồng trường B2B
    /// </summary>
    public class UpdateSchoolContractDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int? TotalLicenses { get; set; }
        public string? ContractEnd { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO sinh mã lớp / dải mã kích hoạt
    /// </summary>
    public class GenerateClassCodesDto
    {
        public int Count { get; set; } = 3;
    }
}
