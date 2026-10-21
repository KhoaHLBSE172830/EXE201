using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Curriculum;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class CurriculumService : ICurriculumService
    {
        private readonly IGenericRepository<Domain> _domainRepository;
        private readonly IGenericRepository<Unit> _unitRepository;
        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IGenericRepository<UserNodeProgress> _progressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CurriculumService> _logger;

        public CurriculumService(
            IGenericRepository<Domain> domainRepository,
            IGenericRepository<Unit> unitRepository,
            IGenericRepository<Node> nodeRepository,
            IGenericRepository<UserNodeProgress> progressRepository,
            IMapper mapper,
            ILogger<CurriculumService> logger)
        {
            _domainRepository = domainRepository;
            _unitRepository = unitRepository;
            _nodeRepository = nodeRepository;
            _progressRepository = progressRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DomainOverviewDto>> GetDomainsAsync(CancellationToken cancellationToken = default)
        {
            var domains = await _domainRepository.WhereAsync(
                d => string.Equals(d.Status, "Active", StringComparison.OrdinalIgnoreCase),
                nameof(Domain.Units));

            var result = new List<DomainOverviewDto>();
            var allUnitIds = domains.SelectMany(d => d.Units.Select(u => u.Id)).Distinct().ToList();

            // BUG-09 FIX: Batch query toàn bộ published nodes 1 lần duy nhất, tránh N+1 database queries
            var publishedNodes = await _nodeRepository.WhereAsync(
                n => allUnitIds.Contains(n.UnitId) && string.Equals(n.Status, "Published", StringComparison.OrdinalIgnoreCase));
            var nodesCountByUnit = publishedNodes
                .GroupBy(n => n.UnitId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var d in domains.OrderBy(d => d.Order))
            {
                int totalNodes = d.Units.Sum(u => nodesCountByUnit.TryGetValue(u.Id, out var c) ? c : 0);

                result.Add(new DomainOverviewDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Slug = d.Slug,
                    IconUrl = d.IconUrl,
                    Order = d.Order,
                    Status = d.Status,
                    TotalUnits = d.Units.Count,
                    TotalNodes = totalNodes
                });
            }

            return result;
        }

        public async Task<DomainDto> GetDomainByIdAsync(Guid domainId, CancellationToken cancellationToken = default)
        {
            var domain = await _domainRepository.FirstOrDefaultAsync(
                d => d.Id == domainId,
                nameof(Domain.Units));

            if (domain == null)
            {
                throw new NotFoundException($"Không tìm thấy chủ đề với ID: {domainId}");
            }

            var domainDto = new DomainDto
            {
                Id = domain.Id,
                Name = domain.Name,
                Slug = domain.Slug,
                IconUrl = domain.IconUrl,
                Order = domain.Order,
                Status = domain.Status,
                Units = new List<UnitDto>()
            };

            // BUG-09 FIX: Batch query toàn bộ published nodes của domain 1 lần
            var unitIds = domain.Units.Select(u => u.Id).Distinct().ToList();
            var publishedNodes = await _nodeRepository.WhereAsync(
                n => unitIds.Contains(n.UnitId) && string.Equals(n.Status, "Published", StringComparison.OrdinalIgnoreCase));
            var nodesByUnit = publishedNodes
                .GroupBy(n => n.UnitId)
                .ToDictionary(g => g.Key, g => g.OrderBy(n => n.Order).ToList());

            foreach (var unit in domain.Units.OrderBy(u => u.Order))
            {
                var unitNodes = nodesByUnit.TryGetValue(unit.Id, out var list) ? list : new List<Node>();

                var unitDto = new UnitDto
                {
                    Id = unit.Id,
                    DomainId = domain.Id,
                    DomainName = domain.Name,
                    Title = unit.Title,
                    Description = unit.Description,
                    Order = unit.Order,
                    Nodes = unitNodes.Select(n => new NodeDto
                    {
                        Id = n.Id,
                        UnitId = n.UnitId,
                        Title = n.Title,
                        Type = n.Type,
                        Order = n.Order,
                        XpReward = n.XpReward,
                        Status = n.Status,
                        UnlockRule = n.UnlockRule,
                        UserProgressStatus = "locked"
                    }).ToList()
                };

                domainDto.Units.Add(unitDto);
            }

            return domainDto;
        }

        public async Task<UnitDto> GetUnitByIdAsync(Guid unitId, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var unit = await _unitRepository.FirstOrDefaultAsync(
                u => u.Id == unitId,
                nameof(Unit.Domain));

            if (unit == null)
            {
                throw new NotFoundException($"Không tìm thấy chặng kỹ năng với ID: {unitId}");
            }

            var publishedNodes = await _nodeRepository.WhereAsync(
                n => n.UnitId == unit.Id && string.Equals(n.Status, "Published", StringComparison.OrdinalIgnoreCase));

            var orderedNodes = publishedNodes.OrderBy(n => n.Order).ToList();
            var nodeDtos = new List<NodeDto>();

            Dictionary<Guid, UserNodeProgress> progressMap = new();
            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                var nodeIds = orderedNodes.Select(n => n.Id).ToList();
                var progresses = await _progressRepository.WhereAsync(p => p.UserId == userId.Value && nodeIds.Contains(p.NodeId));
                progressMap = progresses.ToDictionary(p => p.NodeId, p => p);
            }

            bool foundCurrent = false;
            for (int i = 0; i < orderedNodes.Count; i++)
            {
                var node = orderedNodes[i];
                string status = "locked";
                DateTime? completedAt = null;

                if (progressMap.TryGetValue(node.Id, out var prog))
                {
                    status = prog.Status;
                    completedAt = prog.CompletedAt;
                    if (string.Equals(status, "current", StringComparison.OrdinalIgnoreCase))
                    {
                        foundCurrent = true;
                    }
                }
                else if (!foundCurrent && (i == 0 || (i > 0 && nodeDtos[i - 1].UserProgressStatus == "done")))
                {
                    status = "current";
                    foundCurrent = true;
                }

                nodeDtos.Add(new NodeDto
                {
                    Id = node.Id,
                    UnitId = node.UnitId,
                    Title = node.Title,
                    Type = node.Type,
                    Order = node.Order,
                    XpReward = node.XpReward,
                    Status = node.Status,
                    UnlockRule = node.UnlockRule,
                    UserProgressStatus = status,
                    CompletedAt = completedAt
                });
            }

            return new UnitDto
            {
                Id = unit.Id,
                DomainId = unit.DomainId,
                DomainName = unit.Domain?.Name,
                Title = unit.Title,
                Description = unit.Description,
                Order = unit.Order,
                Nodes = nodeDtos
            };
        }

        public async Task<LearningPathDto> GetLearningPathAsync(Guid? domainId, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            Domain? domain;

            if (domainId.HasValue && domainId.Value != Guid.Empty)
            {
                domain = await _domainRepository.FirstOrDefaultAsync(d => d.Id == domainId.Value);
                if (domain == null)
                {
                    throw new NotFoundException($"Không tìm thấy chủ đề với ID: {domainId.Value}");
                }
            }
            else
            {
                var activeDomains = await _domainRepository.WhereAsync(d => string.Equals(d.Status, "Active", StringComparison.OrdinalIgnoreCase));
                domain = activeDomains.OrderBy(d => d.Order).FirstOrDefault();
                if (domain == null)
                {
                    throw new NotFoundException("Hệ thống chưa có chủ đề học tập nào đang kích hoạt.");
                }
            }

            // Lấy toàn bộ Units thuộc Domain, sắp xếp theo Order ASC
            var units = await _unitRepository.WhereAsync(u => u.DomainId == domain.Id);
            var orderedUnits = units.OrderBy(u => u.Order).ToList();
            var unitIds = orderedUnits.Select(u => u.Id).ToList();

            // Lấy toàn bộ Published Nodes thuộc các Units trên (AGENT.md Section 5.5)
            var publishedNodes = await _nodeRepository.WhereAsync(
                n => unitIds.Contains(n.UnitId) && string.Equals(n.Status, "Published", StringComparison.OrdinalIgnoreCase));

            // Nhóm Nodes theo UnitId và sắp xếp theo Order
            var nodesByUnit = publishedNodes
                .GroupBy(n => n.UnitId)
                .ToDictionary(g => g.Key, g => g.OrderBy(n => n.Order).ToList());

            // Lấy tiến trình người dùng nếu có userId
            Dictionary<Guid, UserNodeProgress> userProgressMap = new();
            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                var allNodeIds = publishedNodes.Select(n => n.Id).ToList();
                var progresses = await _progressRepository.WhereAsync(p => p.UserId == userId.Value && allNodeIds.Contains(p.NodeId));
                userProgressMap = progresses.ToDictionary(p => p.NodeId, p => p);
            }

            // Danh sách tuần tự tất cả các nodes trên lộ trình (theo thứ tự Unit.Order -> Node.Order)
            var chronologicalNodes = new List<Node>();
            foreach (var u in orderedUnits)
            {
                if (nodesByUnit.TryGetValue(u.Id, out var uNodes))
                {
                    chronologicalNodes.AddRange(uNodes);
                }
            }

            // Xác định trạng thái locked | current | done cho từng Node
            var nodeStatusMap = new Dictionary<Guid, (string Status, DateTime? CompletedAt)>();
            bool hasAssignedCurrent = false;

            for (int i = 0; i < chronologicalNodes.Count; i++)
            {
                var node = chronologicalNodes[i];

                if (userProgressMap.TryGetValue(node.Id, out var prog))
                {
                    if (string.Equals(prog.Status, "done", StringComparison.OrdinalIgnoreCase))
                    {
                        nodeStatusMap[node.Id] = ("done", prog.CompletedAt);
                        continue;
                    }

                    if (string.Equals(prog.Status, "current", StringComparison.OrdinalIgnoreCase))
                    {
                        nodeStatusMap[node.Id] = ("current", prog.CompletedAt);
                        hasAssignedCurrent = true;
                        continue;
                    }
                }

                // Nếu chưa gán 'current' và (là node đầu tiên HOẶC node liền trước đã hoàn thành)
                if (!hasAssignedCurrent)
                {
                    bool isFirstNode = (i == 0);
                    bool previousIsDone = (i > 0 && nodeStatusMap[chronologicalNodes[i - 1].Id].Status == "done");

                    if (isFirstNode || previousIsDone)
                    {
                        nodeStatusMap[node.Id] = ("current", null);
                        hasAssignedCurrent = true;
                        continue;
                    }
                }

                // Các trường hợp còn lại đều bị khóa
                nodeStatusMap[node.Id] = ("locked", null);
            }

            // Build kết quả LearningPathDto
            var unitDtos = new List<UnitDto>();
            int totalNodesCount = chronologicalNodes.Count;
            int completedNodesCount = 0;

            foreach (var u in orderedUnits)
            {
                var unitNodeDtos = new List<NodeDto>();
                if (nodesByUnit.TryGetValue(u.Id, out var uNodes))
                {
                    foreach (var n in uNodes)
                    {
                        var (status, completedAt) = nodeStatusMap.TryGetValue(n.Id, out var s) ? s : ("locked", null);
                        if (status == "done")
                        {
                            completedNodesCount++;
                        }

                        unitNodeDtos.Add(new NodeDto
                        {
                            Id = n.Id,
                            UnitId = n.UnitId,
                            Title = n.Title,
                            Type = n.Type,
                            Order = n.Order,
                            XpReward = n.XpReward,
                            Status = n.Status,
                            UnlockRule = n.UnlockRule,
                            UserProgressStatus = status,
                            CompletedAt = completedAt
                        });
                    }
                }

                unitDtos.Add(new UnitDto
                {
                    Id = u.Id,
                    DomainId = domain.Id,
                    DomainName = domain.Name,
                    Title = u.Title,
                    Description = u.Description,
                    Order = u.Order,
                    Nodes = unitNodeDtos
                });
            }

            double progressPercent = totalNodesCount > 0 
                ? Math.Round((double)completedNodesCount / totalNodesCount * 100, 1) 
                : 0.0;

            return new LearningPathDto
            {
                DomainId = domain.Id,
                DomainName = domain.Name,
                DomainSlug = domain.Slug,
                DomainIconUrl = domain.IconUrl,
                TotalNodes = totalNodesCount,
                CompletedNodes = completedNodesCount,
                ProgressPercent = progressPercent,
                Units = unitDtos
            };
        }

        public async Task<NodeDto> GetNodeDetailAsync(Guid nodeId, Guid? userId = null, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(
                n => n.Id == nodeId && string.Equals(n.Status, "Published", StringComparison.OrdinalIgnoreCase));

            if (node == null)
            {
                throw new NotFoundException($"Không tìm thấy bài học (hoặc chưa được xuất bản) với ID: {nodeId}");
            }

            string userStatus = "locked";
            DateTime? completedAt = null;

            if (userId.HasValue && userId.Value != Guid.Empty)
            {
                var prog = await _progressRepository.FirstOrDefaultAsync(p => p.UserId == userId.Value && p.NodeId == nodeId);
                if (prog != null)
                {
                    userStatus = prog.Status;
                    completedAt = prog.CompletedAt;
                }
            }

            return new NodeDto
            {
                Id = node.Id,
                UnitId = node.UnitId,
                Title = node.Title,
                Type = node.Type,
                Order = node.Order,
                XpReward = node.XpReward,
                Status = node.Status,
                UnlockRule = node.UnlockRule,
                UserProgressStatus = userStatus,
                CompletedAt = completedAt
            };
        }
    }
}
