using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class ShopService : IShopService
    {
        private readonly IGenericRepository<ShopItem> _itemRepository;
        private readonly IGenericRepository<ShopPurchase> _purchaseRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShopService> _logger;

        public ShopService(
            IGenericRepository<ShopItem> itemRepository,
            IGenericRepository<ShopPurchase> purchaseRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IUnitOfWork unitOfWork,
            ILogger<ShopService> logger)
        {
            _itemRepository = itemRepository;
            _purchaseRepository = purchaseRepository;
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _xpEventRepository = xpEventRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ShopItemsResponseDto> GetShopItemsAsync(Guid userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (stats == null)
            {
                stats = new UserStats { UserId = userId, Hearts = 5, HeartsUpdatedAt = DateTime.UtcNow };
                await _statsRepository.CreateAsync(stats);
                await _unitOfWork.SaveChangesAsync();
            }

            // Lazy regen check
            stats.CalculateCurrentHearts(DateTime.UtcNow);

            var items = await _itemRepository.WhereAsync(i => i.Status == "Active");

            return new ShopItemsResponseDto
            {
                TotalXp = user.TotalXp,
                Hearts = stats.Hearts,
                StreakFreezeCount = stats.StreakFreezeCount,
                IsPro = stats.IsPro,
                Items = items.Select(i => new ShopItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    CostXp = i.CostXp,
                    Icon = i.Icon,
                    Color = i.Color,
                    BgColor = i.BgColor,
                    CanAfford = user.TotalXp >= i.CostXp
                }).ToList()
            };
        }

        public async Task<PurchaseItemResponseDto> PurchaseItemAsync(Guid userId, string itemId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (stats == null)
            {
                stats = new UserStats { UserId = userId, Hearts = 5, HeartsUpdatedAt = DateTime.UtcNow };
                await _statsRepository.CreateAsync(stats);
            }

            var item = await _itemRepository.FirstOrDefaultAsync(i => i.Id == itemId && i.Status == "Active");
            if (item == null)
            {
                throw new NotFoundException("Vật phẩm không tồn tại hoặc đã ngừng cung cấp.");
            }

            if (user.TotalXp < item.CostXp)
            {
                throw new BadRequestException($"Bạn không đủ điểm XP để đổi vật phẩm này. Cần {item.CostXp} XP, hiện có {user.TotalXp} XP.");
            }

            string message;
            switch (item.Id)
            {
                case "freeze":
                    if (stats.StreakFreezeCount >= 2)
                    {
                        throw new BadRequestException("Bạn đã tích trữ tối đa 2 Bùa Đóng Băng Streak. Hãy sử dụng trước khi đổi thêm.");
                    }
                    stats.StreakFreezeCount += 1;
                    message = "Đổi thành công +1 Bùa Đóng Băng Streak!";
                    break;

                case "hearts_refill":
                    stats.CalculateCurrentHearts(DateTime.UtcNow);
                    if (stats.Hearts >= 5)
                    {
                        throw new BadRequestException("Số tim của bạn đã đầy (5/5). Bạn không cần đổi gói hồi tim lúc này.");
                    }
                    stats.Hearts = 5;
                    stats.HeartsUpdatedAt = DateTime.UtcNow;
                    message = "Đã hồi đầy 5 Trái Tim thành công!";
                    break;

                case "pro_trial_7d":
                    stats.IsPro = true;
                    stats.ProExpiresAt = stats.ProExpiresAt.HasValue && stats.ProExpiresAt.Value > DateTime.UtcNow
                        ? stats.ProExpiresAt.Value.AddDays(7)
                        : DateTime.UtcNow.AddDays(7);
                    message = "Kích hoạt 7 ngày trải nghiệm Pro thành công!";
                    break;

                default:
                    message = $"Đổi thành công {item.Title}!";
                    break;
            }

            // XP Ledger deduction
            user.TotalXp -= item.CostXp;
            await _userRepository.UpdateAsync(user);
            await _statsRepository.UpdateAsync(stats);

            await _xpEventRepository.CreateAsync(new XpEvent
            {
                UserId = userId,
                Amount = -item.CostXp,
                Source = "shop_purchase",
                CreatedAt = DateTime.UtcNow
            });

            // Record purchase history
            await _purchaseRepository.CreateAsync(new ShopPurchase
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ItemId = item.Id,
                CostXp = item.CostXp,
                PurchasedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            return new PurchaseItemResponseDto
            {
                ItemId = item.Id,
                CostXp = item.CostXp,
                NewTotalXp = user.TotalXp,
                Hearts = stats.Hearts,
                StreakFreezeCount = stats.StreakFreezeCount,
                IsPro = stats.IsPro,
                Message = message
            };
        }
    }
}
