using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface IPurchaseInvoiceRepository
{
    Task<IEnumerable<PurchaseInvoice>> GetAllAsync();
    Task<PurchaseInvoice?> GetByIdAsync(int id);
    Task<PurchaseInvoice> CreateAsync(PurchaseInvoice invoice);
    Task<PurchaseInvoice> UpdateAsync(PurchaseInvoice invoice);
}