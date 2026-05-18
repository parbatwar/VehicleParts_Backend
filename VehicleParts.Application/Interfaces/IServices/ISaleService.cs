using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleParts.Application.DTOs.Sale;

namespace VehicleParts.Application.Interfaces.IServices;

public interface ISaleService
{
    // Core Actions 
    Task<SaleResponseDto> ProcessSaleAsync(CreateSaleDto dto);
    Task<SaleResponseDto?> GetSaleByIdAsync(int saleId);
    Task<List<SaleResponseDto>> GetAllSalesAsync();
    Task<List<SaleResponseDto>> GetSalesByCustomerAsync(int customerId);
    Task<SaleResponseDto> MarkAsPaidAsync(int saleId);
    Task<bool> SendInvoiceEmailAsync(int saleId); 

    // Feature 9 Analytical Reports Methods 
    Task<List<RegularCustomerReportDto>> GetRegularCustomersAsync(int minPurchases = 3);
    Task<List<HighSpenderReportDto>> GetHighSpendersAsync(int topN = 10);
    Task<List<PendingCreditReportDto>> GetPendingCreditsAsync();

    // Khalti Payment Gateway Integration Methods 
    Task<KhaltiInitializeApiResponse> InitiateKhaltiPaymentAsync(KhaltiInitiateRequestDto dto);
    Task<KhaltiLookupApiResponse> VerifyKhaltiPaymentAsync(KhaltiVerifyRequestDto dto);
}