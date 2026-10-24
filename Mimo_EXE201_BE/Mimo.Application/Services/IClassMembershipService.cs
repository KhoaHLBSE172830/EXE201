using System;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Business;

namespace Mimo.Application.Services
{
    public interface IClassMembershipService
    {
        Task<StudentClassroomDto> GetMyClassroomAsync(Guid studentId, CancellationToken cancellationToken = default);
        Task<StudentClassroomDto> JoinClassByCodeAsync(Guid studentId, string classCode, CancellationToken cancellationToken = default);
        Task LeaveClassAsync(Guid studentId, Guid classId, CancellationToken cancellationToken = default);
    }
}
