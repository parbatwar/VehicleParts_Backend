using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> UpdateAsync(Appointment appointment);
}