using VParts.Application.DTOs.Sales;

namespace VParts.Application.Interfaces.IRepositories;

public interface ISaleRepository
{
    Task<SaleDTO> CreateSaleAsync(CreateSaleDTO dto);
    Task<SaleDTO?> GetSaleByIdAsync(int saleId);
    Task<IEnumerable<SaleDTO>> GetSalesByCustomerIdAsync(int customerId);
}