using System.Threading.Tasks;
using Mimo.Application.DTOs.Gamification;

namespace Mimo.Application.Services
{
    public interface IHandbookService
    {
        Task<HandbookResponseDto> GetHandbookAsync(string? pillar, string? search);
        Task<HandbookArticleDto> GetArticleByIdAsync(string id);
    }
}
