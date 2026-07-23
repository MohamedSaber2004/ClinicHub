using ClinicHub.Application.Features.Plans.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Plans.Commands.CreatePlan
{
    public class CreatePlanCommand : IRequest<PlanDto>
    {
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int? MaxDoctors { get; set; }
        public int? MaxStaff { get; set; }
        public string? Features { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
    }
}
