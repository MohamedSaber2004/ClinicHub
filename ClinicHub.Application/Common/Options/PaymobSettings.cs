namespace ClinicHub.Application.Common.Options;

public class PaymobSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string IntegrationId { get; set; } = string.Empty;
    public string WalletIntegrationId { get; set; } = string.Empty;
    public string HmacSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://accept.paymob.com/api";
}