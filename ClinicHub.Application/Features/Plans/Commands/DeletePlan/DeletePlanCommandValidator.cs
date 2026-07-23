using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Plans.Commands.DeletePlan
{
    public class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
    {
        private readonly IUnitOfWork _ctx;

        public DeletePlanCommandValidator(IStringLocalizer<Messages> localizer, IUnitOfWork ctx)
        {
            _ctx = ctx;

            RuleFor(v => v.Id)
                .NotEmpty().WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.ValidationMessages.Required.Value]))
                .MustAsync(async (id, ct) =>
                    await _ctx.GetRepository<Plan, Guid>().ExistsAsync(p => p.Id == id && !p.IsDeleted, ct))
                .WithMessage(JsonLocalizationProvider.GetLocalizedString(localizer[LocalizationKeys.PlanMessages.NotFound.Value]));
        }
    }
}
