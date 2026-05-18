using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleParts.Application.DTOs.Sale; 
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface ISaleRepository
{
    Task<IEnumerable<SalesInvoice>> GetAllAsync();
    Task<SalesInvoice?> GetByIdAsync(int id);
    Task<IEnumerable<SalesInvoice>> GetByCustomerIdAsync(int customerId);
    Task<SalesInvoice> CreateAsync(SalesInvoice invoice);
    Task<SalesInvoice> UpdateAsync(SalesInvoice invoice);

    // Feature 9: Analytical Reports Methods 
    Task<List<RegularCustomerReportDto>> GetRegularCustomersAsync(int minPurchases);
    Task<List<HighSpenderReportDto>> GetHighSpendersAsync(int topN);
    Task<List<PendingCreditReportDto>> GetPendingCreditsAsync();
}