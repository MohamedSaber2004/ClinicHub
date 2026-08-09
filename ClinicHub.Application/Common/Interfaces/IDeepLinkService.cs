namespace ClinicHub.Application.Common.Interfaces
{
    public interface IDeepLinkService
    {
        string GenerateClinicApprovalLink(Guid clinicId, Guid userId);
        string GeneratePostLink(Guid postId);
        string GenerateVerificationApprovedLink(string userId, string role, string status);
        string GenerateLink(string path);
        string GenerateGoLink(string path);
        bool VerifyToken(string data, string token);
    }
}
