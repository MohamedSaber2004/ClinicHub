namespace ClinicHub.Application.Common.Interfaces
{
    public interface IDeepLinkService
    {
        string GenerateClinicApprovalLink(Guid clinicId, Guid userId);
        string GeneratePostLink(Guid postId);
    }
}
