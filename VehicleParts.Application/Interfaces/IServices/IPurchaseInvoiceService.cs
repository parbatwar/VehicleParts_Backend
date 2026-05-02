using VehicleParts.Application.DTOs.PurchaseInvoice;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IPurchaseInvoiceService
{
    Task<IEnumerable<PurchaseInvoiceResponseDto>> GetAllAsync();
    Task<PurchaseInvoiceResponseDto> GetByIdAsync(int id);
    Task<PurchaseInvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto);
}