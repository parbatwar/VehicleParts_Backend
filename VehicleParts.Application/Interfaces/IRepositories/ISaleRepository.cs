using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface ISaleRepository
{
    Task<IEnumerable<SalesInvoice>> GetAllAsync();
    Task<SalesInvoice?> GetByIdAsync(int id);
    Task<IEnumerable<SalesInvoice>> GetByCustomerIdAsync(int customerId);
    Task<SalesInvoice> CreateAsync(SalesInvoice invoice);
}