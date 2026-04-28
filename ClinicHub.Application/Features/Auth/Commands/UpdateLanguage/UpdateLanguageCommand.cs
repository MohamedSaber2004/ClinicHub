using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateLanguage
{
    public class UpdateLanguageCommand : IRequest<bool>
    {
        public LanguageCode Language { get; set; }
    }
}
