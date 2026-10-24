using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class HandbookService : IHandbookService
    {
        private readonly IGenericRepository<HandbookArticle> _articleRepository;
        private readonly ILogger<HandbookService> _logger;

        public HandbookService(
            IGenericRepository<HandbookArticle> articleRepository,
            ILogger<HandbookService> logger)
        {
            _articleRepository = articleRepository;
            _logger = logger;
        }

        public async Task<HandbookResponseDto> GetHandbookAsync(string? pillar, string? search)
        {
            var allArticles = await _articleRepository.ListAsync();

            var query = allArticles.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(pillar) && !pillar.Equals("Tất cả", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(a => string.Equals(a.Pillar, pillar, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a => a.Title.ToLower().Contains(term) ||
                                         a.KeyProtocol.ToLower().Contains(term) ||
                                         a.Pillar.ToLower().Contains(term));
            }

            var filtered = query.OrderBy(a => a.Order).ToList();

            var allPillars = allArticles.Select(a => a.Pillar).Distinct().ToList();

            var hotlines = new List<EmergencyDialDto>
            {
                new EmergencyDialDto { Name = "Tổng đài Quốc gia Bảo vệ Trẻ em", Dial = "111" },
                new EmergencyDialDto { Name = "Công an phản ứng nhanh", Dial = "113" },
                new EmergencyDialDto { Name = "Cứu nạn cứu hộ & PCCC", Dial = "114" },
                new EmergencyDialDto { Name = "Cấp cứu y tế", Dial = "115" }
            };

            return new HandbookResponseDto
            {
                Pillars = allPillars,
                EmergencyHotlines = hotlines,
                Articles = filtered.Select(MapToDto).ToList()
            };
        }

        public async Task<HandbookArticleDto> GetArticleByIdAsync(string id)
        {
            var article = await _articleRepository.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
            {
                throw new NotFoundException($"Không tìm thấy bài viết cẩm nang với mã '{id}'.");
            }

            return MapToDto(article);
        }

        private static HandbookArticleDto MapToDto(HandbookArticle a)
        {
            List<EmergencyDialDto>? dials = null;
            if (!string.IsNullOrWhiteSpace(a.EmergencyDials))
            {
                try
                {
                    dials = JsonSerializer.Deserialize<List<EmergencyDialDto>>(a.EmergencyDials, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch
                {
                    dials = null;
                }
            }

            return new HandbookArticleDto
            {
                Id = a.Id,
                Pillar = a.Pillar,
                Title = a.Title,
                Badge = a.Badge,
                ReadTime = a.ReadTime,
                EmergencyDials = dials,
                KeyProtocol = a.KeyProtocol,
                LegalBasis = a.LegalBasis,
                ContentMarkdown = a.ContentMarkdown,
                Order = a.Order
            };
        }
    }
}
