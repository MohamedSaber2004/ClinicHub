namespace ClinicHub.Application.Common.Interfaces
{
    public interface IIdProtectorService
    {
        string Protect(Guid id, string? purpose = null);
        Guid? Unprotect(string token, string? purpose = null);
    }
}
