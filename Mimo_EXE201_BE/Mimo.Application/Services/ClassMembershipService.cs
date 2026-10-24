using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Business;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services
{
    public class ClassMembershipService : IClassMembershipService
    {
        private readonly IGenericRepository<Class> _classRepository;
        private readonly IGenericRepository<ClassMember> _classMemberRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Assignment> _assignmentRepository;
        private readonly IGenericRepository<AssignmentProgress> _assignmentProgressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClassMembershipService(
            IGenericRepository<Class> classRepository,
            IGenericRepository<ClassMember> classMemberRepository,
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Assignment> assignmentRepository,
            IGenericRepository<AssignmentProgress> assignmentProgressRepository,
            IUnitOfWork unitOfWork)
        {
            _classRepository = classRepository;
            _classMemberRepository = classMemberRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _assignmentRepository = assignmentRepository;
            _assignmentProgressRepository = assignmentProgressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<StudentClassroomDto> GetMyClassroomAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            var memberships = await _classMemberRepository.WhereAsync(cm => cm.StudentId == studentId && cm.Status == "Enrolled");
            var activeMembership = memberships.OrderByDescending(cm => cm.JoinedAt).FirstOrDefault();

            if (activeMembership == null)
            {
                return new StudentClassroomDto { Joined = false };
            }

            var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == activeMembership.ClassId, "Organization", "Educator", "Educator.Profile");
            if (cls == null)
            {
                return new StudentClassroomDto { Joined = false };
            }

            var allClassMembers = await _classMemberRepository.WhereAsync(cm => cm.ClassId == cls.Id && cm.Status == "Enrolled");
            var assignments = await _assignmentRepository.WhereAsync(a => a.ClassId == cls.Id);
            var studentProgresses = await _assignmentProgressRepository.WhereAsync(ap => ap.StudentId == studentId);

            var now = DateTime.UtcNow;
            var assignmentDtos = new List<StudentAssignmentItemDto>();

            foreach (var a in assignments.OrderBy(a => a.Deadline))
            {
                var prog = studentProgresses.FirstOrDefault(p => p.AssignmentId == a.Id);
                int bestScore = prog?.BestScore ?? 0;
                bool isPassed = prog?.IsPassed ?? false;

                string status;
                if (isPassed)
                {
                    status = "Đạt";
                }
                else if (now > a.Deadline)
                {
                    status = "Quá hạn";
                }
                else if (bestScore > 0)
                {
                    status = "Chưa đạt";
                }
                else
                {
                    status = "Chưa làm";
                }

                assignmentDtos.Add(new StudentAssignmentItemDto
                {
                    AssignmentId = a.Id,
                    NodeId = a.NodeId,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    PassScoreThreshold = a.PassScoreThreshold,
                    RewardXp = a.RewardXp,
                    BestScore = bestScore,
                    IsPassed = isPassed,
                    Status = status
                });
            }

            string teacherName = cls.Educator?.Profile?.FullName ?? cls.Educator?.Name ?? "Chưa phân công";

            return new StudentClassroomDto
            {
                Joined = true,
                ClassInfo = new StudentClassInfoDto
                {
                    Id = cls.Id,
                    Code = cls.ClassCode,
                    ClassName = cls.Name,
                    School = cls.Organization?.Name ?? "Trường học đối tác",
                    Topic = cls.FocusTopic ?? "Kỹ năng sống & An toàn",
                    Teacher = teacherName,
                    TeacherRole = "Giáo viên bộ môn",
                    TotalStudents = allClassMembers.Count
                },
                Assignments = assignmentDtos
            };
        }

        public async Task<StudentClassroomDto> JoinClassByCodeAsync(Guid studentId, string classCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(classCode))
            {
                throw new BadRequestException("Vui lòng nhập mã lớp học.");
            }

            var normalizedCode = classCode.Trim().ToUpperInvariant();
            var cls = await _classRepository.FirstOrDefaultAsync(c => c.ClassCode == normalizedCode && c.Status == "Active");
            if (cls == null)
            {
                throw new NotFoundException("Mã lớp học không hợp lệ hoặc lớp học không tồn tại trong hệ thống liên kết.");
            }

            // Kiểm tra đã vào lớp này chưa (Idempotent)
            var existingMembership = await _classMemberRepository.FirstOrDefaultAsync(cm => cm.ClassId == cls.Id && cm.StudentId == studentId);
            if (existingMembership != null && existingMembership.Status == "Enrolled")
            {
                return await GetMyClassroomAsync(studentId, cancellationToken);
            }

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == studentId);
            if (user == null)
            {
                throw new UnauthorizedException("Người dùng không tồn tại.");
            }

            if (user.OrganizationId.HasValue && user.OrganizationId.Value != cls.OrganizationId)
            {
                throw new BadRequestException("Bạn đang thuộc về một trường học khác. Vui lòng rời trường học cũ trước khi tham gia lớp của trường mới.");
            }

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var currentMembers = await _classMemberRepository.WhereAsync(cm => cm.ClassId == cls.Id && cm.Status == "Enrolled");
            if (currentMembers.Count >= cls.MaxStudents)
            {
                throw new BadRequestException($"Lớp học đã đủ sĩ số tối đa ({cls.MaxStudents} học sinh).");
            }

            // Nếu học sinh chưa thuộc tổ chức này, kiểm tra và tăng số lượng ghế bản quyền (Seat Count)
            if (!user.OrganizationId.HasValue)
            {
                var org = await _organizationRepository.FirstOrDefaultAsync(o => o.Id == cls.OrganizationId);
                if (org == null || org.Status != OrganizationStatusEnum.Active || (org.ExpirationDate.HasValue && org.ExpirationDate.Value < DateTime.UtcNow))
                {
                    throw new BadRequestException("Trường học liên kết hiện không hoạt động hoặc gói bản quyền đã hết hạn.");
                }

                if (org.UsedSeats >= org.SeatCount)
                {
                    throw new BadRequestException($"Nhà trường đã sử dụng hết số lượng ghế bản quyền ({org.UsedSeats}/{org.SeatCount} seats). Vui lòng thông báo giáo viên hoặc ban giám hiệu.");
                }

                org.UsedSeats += 1;
                await _organizationRepository.UpdateAsync(org);

                user.OrganizationId = org.Id;
                await _userRepository.UpdateAsync(user);
            }

            if (existingMembership != null)
            {
                existingMembership.Status = "Enrolled";
                existingMembership.JoinedAt = DateTime.UtcNow;
                await _classMemberRepository.UpdateAsync(existingMembership);
            }
            else
            {
                await _classMemberRepository.CreateAsync(new ClassMember
                {
                    ClassId = cls.Id,
                    StudentId = studentId,
                    Status = "Enrolled",
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return await GetMyClassroomAsync(studentId, cancellationToken);
        }

        public async Task LeaveClassAsync(Guid studentId, Guid classId, CancellationToken cancellationToken = default)
        {
            var membership = await _classMemberRepository.FirstOrDefaultAsync(cm => cm.ClassId == classId && cm.StudentId == studentId && cm.Status == "Enrolled");
            if (membership == null)
            {
                return;
            }

            var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == classId);
            if (cls == null)
            {
                return;
            }

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            membership.Status = "Dropped";
            await _classMemberRepository.UpdateAsync(membership);

            // Kiểm tra xem học sinh có còn lớp nào khác trong cùng Organization không
            var otherClasses = await _classRepository.WhereAsync(c => c.OrganizationId == cls.OrganizationId && c.Id != classId);
            var otherClassIds = otherClasses.Select(c => c.Id).ToList();

            var otherMemberships = await _classMemberRepository.WhereAsync(cm =>
                cm.StudentId == studentId &&
                otherClassIds.Contains(cm.ClassId) &&
                cm.Status == "Enrolled");

            if (otherMemberships.Count == 0)
            {
                // Trả lại 1 seat cho tổ chức
                var org = await _organizationRepository.FirstOrDefaultAsync(o => o.Id == cls.OrganizationId);
                if (org != null && org.UsedSeats > 0)
                {
                    org.UsedSeats -= 1;
                    await _organizationRepository.UpdateAsync(org);
                }

                var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == studentId);
                if (user != null)
                {
                    user.OrganizationId = null;
                    await _userRepository.UpdateAsync(user);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
}
