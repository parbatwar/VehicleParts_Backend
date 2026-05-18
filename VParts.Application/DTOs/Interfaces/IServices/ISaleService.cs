using VParts.Application.DTOs.Sales;

namespace VParts.Application.Interfaces.IServices;

public interface ISaleService
{
    Task<SaleDTO> ProcessSaleAsync(CreateSaleDTO dto);
    Task<SaleDTO?> GetSaleAsync(int saleId);
    Task<IEnumerable<SaleDTO>> GetCustomerSalesAsync(int customerId);
}