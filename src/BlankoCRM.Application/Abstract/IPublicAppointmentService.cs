using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract;

public interface IPublicAppointmentService
{
    Task<Appointment> CreatePendingAsync(
        CreatePublicAppointmentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetMyAppointmentsAsync(
        IReadOnlyDictionary<string, string> vkParams,
        CancellationToken cancellationToken = default);

    Task WithdrawAsync(
        Guid appointmentId,
        IReadOnlyDictionary<string, string> vkParams,
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        Guid appointmentId,
        IReadOnlyDictionary<string, string> vkParams,
        CancellationToken cancellationToken = default);
}
