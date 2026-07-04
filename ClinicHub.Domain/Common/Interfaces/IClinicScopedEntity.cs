namespace ClinicHub.Domain.Common.Interfaces
{
    public interface IClinicScopedEntity
    {
        Guid? ClinicId { get; }
    }
}
