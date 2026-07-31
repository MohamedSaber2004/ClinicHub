namespace ClinicHub.Application.Common.Interfaces;

public interface IInvoiceNumberService
{
    Task<string> GenerateNextAsync(Guid clinicId, CancellationToken cancellationToken);
}
