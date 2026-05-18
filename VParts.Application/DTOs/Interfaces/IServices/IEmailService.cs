namespace VParts.Application.Interfaces.IServices;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(int saleId);
}