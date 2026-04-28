using ClinicHub.Domain.Common;
using NetTopologySuite.Geometries;

namespace ClinicHub.Domain.Entities
{
    public class Clinic : BaseEntity<Guid>
    {
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Address { get; set; }
        public string? AddressAr { get; set; }
        public string? Phone { get; set; }
        
        // Location stored as a Point (SRID 4326 for WGS84)
        public Point Location { get; set; } = null!;
        
        public bool IsRegistered { get; set; }
        
        public Guid SpecializationId { get; set; }
        public Specialization Specialization { get; set; } = null!;
    }
}
