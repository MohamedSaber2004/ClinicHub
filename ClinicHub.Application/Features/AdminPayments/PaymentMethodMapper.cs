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
            "creditcard" or "card" or "credit_card" or "paymob_card" or "visa" or "mastercard" => PaymentMethod.PaymobCreditCard,
            "wallet" or "paymob_wallet" or "paymob" or "paymobwallet" => PaymentMethod.PaymobWallet,
            _ => PaymentMethod.PaymobWallet
        };
    }

    public static string ToDbString(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "cash",
        PaymentMethod.PaymobCreditCard => "paymob_card",
        _ => "paymob_wallet"
    };

    public static PaymentStatus ToUiStatus(PaymentStatus status) =>
        status == PaymentStatus.Processing ? PaymentStatus.Pending : status;
}
