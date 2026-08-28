using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayments;

public static class PaymentMethodMapper
{
    public static PaymentMethod ToEnum(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
            return PaymentMethod.PaymobWallet;

        return method.Trim().ToLowerInvariant() switch
        {
            "cash" => PaymentMethod.Cash,
            "creditcard" or "card" or "credit_card" => PaymentMethod.PaymobCreditCard,
            _ => PaymentMethod.PaymobWallet
        };
    }

    public static string ToDbString(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "cash",
        PaymentMethod.PaymobCreditCard => "paymob_card",
        _ => "paymob"
    };

    public static PaymentStatus ToUiStatus(PaymentStatus status) =>
        status == PaymentStatus.Processing ? PaymentStatus.Pending : status;
}
