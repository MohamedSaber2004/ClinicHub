namespace ClinicHub.Application.Features.Payment.DTOs;

/// <summary>
/// Result of verifying the clinic's most recent subscription payment.
/// Returned to the MVC frontend right after the user comes back from the
/// payment gateway, so it can show a real status instead of guessing.
/// </summary>
public class VerifySubscriptionPaymentResponseDto
{
    /// <summary>"paid" | "pending" | "failed" | "none"</summary>
    public string Status { get; set; } = "none";

    public bool SubscriptionActive { get; set; }

    public DateTime? EndDate { get; set; }

    public string? PlanName { get; set; }
}
