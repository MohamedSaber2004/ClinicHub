using ClinicHub.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization
{
    public class CreateSpecializationCommand : IRequest<string>
    {
        public string Name { get; set; } = null!;
        public string ArName { get; set; } = null!;
        public string? Description { get; set; }
        public IFormFile? Icon { get; set; }
        public bool IsFamous { get; set; }
    }
}
