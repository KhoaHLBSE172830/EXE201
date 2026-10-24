using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminSchoolService
    {
        Task<List<AdminSchoolDto>> GetSchoolsAsync(CancellationToken cancellationToken = default);
        Task<List<AdminSchoolDto>> CreateSchoolContractAsync(CreateSchoolContractDto request, CancellationToken cancellationToken = default);
        Task<List<AdminSchoolDto>> UpdateSchoolContractAsync(Guid schoolId, UpdateSchoolContractDto request, CancellationToken cancellationToken = default);
        Task<List<AdminSchoolDto>> GenerateClassCodesAsync(Guid schoolId, int count, CancellationToken cancellationToken = default);
    }
}
