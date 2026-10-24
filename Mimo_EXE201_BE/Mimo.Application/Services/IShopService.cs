using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Gamification;

namespace Mimo.Application.Services
{
    public interface IShopService
    {
        Task<ShopItemsResponseDto> GetShopItemsAsync(Guid userId);
        Task<PurchaseItemResponseDto> PurchaseItemAsync(Guid userId, string itemId);
    }
}
