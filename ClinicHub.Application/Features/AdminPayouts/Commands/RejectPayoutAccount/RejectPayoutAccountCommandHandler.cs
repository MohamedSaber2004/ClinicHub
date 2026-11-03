using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.RejectPayoutAccount;

public class RejectPayoutAccountCommandHandler
    : IRequestHandler<RejectPayoutAccountCommand, PayoutAccountAdminDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFcmService _fcmService;
    private readonly ILogger<RejectPayoutAccountCommandHandler> _logger;

    public RejectPayoutAccountCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFcmService fcmService,
        ILogger<RejectPayoutAccountCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fcmService = fcmService;
        _logger = logger;
    }

    public async Task<PayoutAccountAdminDto> Handle(
        RejectPayoutAccountCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BadRequestException(LocalizationKeys.PayoutMessages.RejectionReasonRequired.Value);

        var repository = _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>();

        var account = await repository
            .GetAllAsync(a => a.Id == request.AccountId)
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.AccountNotFound.Value);

        var reason = request.Reason.Trim();
        account.Reject(reason, _currentUserService.UserId);
        repository.Update(account);

        var clinicName = account.Clinic?.Name ?? string.Empty;
        if (account.Clinic?.ClinicAdminId is Guid adminId)
        {
            try
            {
                await _fcmService.SendToUserAsync(adminId, NotificationType.PayoutAccountRejected, new()
                {
                    ["clinicName"] = clinicName,
                    ["reason"] = reason
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payout-account-rejected notification for clinic {ClinicId}.", account.ClinicId);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return PayoutAccountAdminDto.From(account, clinicName);
    }
}
