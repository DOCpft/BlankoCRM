using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Обрабатывает истёкшие заявки небольшими транзакционными пакетами.
/// </summary>
public sealed class AppointmentExpirationService : IAppointmentExpirationService
{
    private const int BatchSize = 100;

    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentExpirationService(
        IAppointmentRepository appointmentRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CancelExpiredPendingAsync(CancellationToken cancellationToken = default)
    {
        var totalCancelled = 0;

        while (true)
        {
            var ownsTransaction = !_unitOfWork.HasActiveTransaction;
            await using var transaction = ownsTransaction
                ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
                : null;

            int cancelled;
            try
            {
                cancelled = await _appointmentRepository.CancelExpiredPendingBatchAsync(
                    DateTime.UtcNow,
                    BatchSize,
                    cancellationToken);

                if (ownsTransaction)
                {
                    await transaction!.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                if (ownsTransaction && _unitOfWork.HasActiveTransaction)
                {
                    await transaction!.RollbackAsync(cancellationToken);
                }

                throw;
            }

            totalCancelled += cancelled;
            if (cancelled < BatchSize)
            {
                return totalCancelled;
            }
        }
    }
}
