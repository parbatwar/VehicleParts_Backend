using VehicleParts.Application.DTOs.Sale;

namespace VehicleParts.Application.Interfaces.IServices;

public interface ISaleService
{
    Task<SaleResponseDto> ProcessSaleAsync(CreateSaleDto dto);
    Task<SaleResponseDto?> GetSaleByIdAsync(int saleId);
    Task<List<SaleResponseDto>> GetAllSalesAsync();
    Task<List<SaleResponseDto>> GetSalesByCustomerAsync(int customerId);
}