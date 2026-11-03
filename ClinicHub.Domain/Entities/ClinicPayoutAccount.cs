using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

/// <summary>
/// Bank destination for a clinic's payouts. The account number is stored encrypted;
/// only the last four digits are kept in clear text for display.
/// </summary>
public class ClinicPayoutAccount : BaseEntity<Guid>, IClinicScopedEntity
{
    public Guid ClinicId { get; private set; }
    public Clinic Clinic { get; private set; } = null!;
    Guid? IClinicScopedEntity.ClinicId => ClinicId;

    public string HolderName { get; private set; } = null!;
    public string BankCode { get; private set; } = null!;
    public string AccountNumberEncrypted { get; private set; } = null!;
    public string AccountLast4 { get; private set; } = null!;

    /// <summary>Paymob requires the recipient's national id on every disbursement.</summary>
    public string NationalIdEncrypted { get; private set; } = null!;
    public string NationalIdLast4 { get; private set; } = null!;

    public string BankTransactionType { get; private set; } = PayoutBankTransactionTypes.CashTransfer;

    public PayoutAccountStatus Status { get; private set; } = PayoutAccountStatus.PendingVerification;
    public Guid? VerifiedByUserId { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private ClinicPayoutAccount() { }

    public ClinicPayoutAccount(
        Guid clinicId,
        string holderName,
        string nationalIdEncrypted,
        string nationalIdLast4,
        string bankCode,
        string accountNumberEncrypted,
        string accountLast4,
        string bankTransactionType)
    {
        ClinicId = clinicId;
        ApplyDetails(
            holderName,
            nationalIdEncrypted,
            nationalIdLast4,
            bankCode,
            accountNumberEncrypted,
            accountLast4,
            bankTransactionType);
    }

    public void UpdateDetails(
        string holderName,
        string nationalIdEncrypted,
        string nationalIdLast4,
        string bankCode,
        string accountNumberEncrypted,
        string accountLast4,
        string bankTransactionType)
    {
        ApplyDetails(
            holderName,
            nationalIdEncrypted,
            nationalIdLast4,
            bankCode,
            accountNumberEncrypted,
            accountLast4,
            bankTransactionType);

        // Any change re-opens verification: money must never go to an unverified destination.
        Status = PayoutAccountStatus.PendingVerification;
        VerifiedByUserId = null;
        VerifiedAt = null;
        RejectionReason = null;
    }

    public void Verify(Guid verifiedByUserId)
    {
        Status = PayoutAccountStatus.Verified;
        VerifiedByUserId = verifiedByUserId;
        VerifiedAt = DateTime.Now;
        RejectionReason = null;
    }

    public void Reject(string reason, Guid reviewedByUserId)
    {
        Status = PayoutAccountStatus.Rejected;
        VerifiedByUserId = reviewedByUserId;
        VerifiedAt = DateTime.Now;
        RejectionReason = reason;
    }

    public void Disable()
    {
        Status = PayoutAccountStatus.Disabled;
    }

    private void ApplyDetails(
        string holderName,
        string nationalIdEncrypted,
        string nationalIdLast4,
        string bankCode,
        string accountNumberEncrypted,
        string accountLast4,
        string bankTransactionType)
    {
        HolderName = holderName.Trim();
        NationalIdEncrypted = nationalIdEncrypted;
        NationalIdLast4 = nationalIdLast4;
        BankCode = PayoutBankCodes.Normalize(bankCode);
        AccountNumberEncrypted = accountNumberEncrypted;
        AccountLast4 = accountLast4;
        BankTransactionType = PayoutBankTransactionTypes.Normalize(bankTransactionType);
    }
}
