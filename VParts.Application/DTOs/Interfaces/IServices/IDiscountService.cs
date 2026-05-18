using VParts.Application.DTOs.Sales;

namespace VParts.Application.Interfaces.IServices;

public interface IDiscountService
{
    DiscountResultDTO ApplyLoyaltyDiscount(decimal subtotal);
}