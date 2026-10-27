using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.AdminPayments;

public static class PaymentMethodMapper
{
    public static PaymentMethod ToEnum(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
            return PaymentMethod.PaymobWallet;

        // Normalize so "PaymobWallet", "paymob_wallet", "Paymob Wallet" all map identically.
        var normalized = method.Trim().ToLowerInvariant().Replace("_", "").Replace(" ", "");
        return normalized switch
        {
            "cash" => PaymentMethod.Cash,
            "creditcard" or "card" or "paymobcard" or "paymobcreditcard" or "visa" or "mastercard" => PaymentMethod.PaymobCreditCard,
            "wallet" or "paymobwallet" or "paymob" => PaymentMethod.PaymobWallet,
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
