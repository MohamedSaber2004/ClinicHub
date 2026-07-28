# Command Handler Template

## File: `ClinicHub.Application/Features/<Feature>/Commands/<Action>/<Action>Command.cs`

```csharp
using MediatR;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Repositories.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.<Feature>.Commands.<Action>;

public record <Action>Command(<Parameters>) : IRequest<string>;

public class <Action>CommandValidator : AbstractValidator<<Action>Command>
{
    public <Action>CommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Property)
            .NotEmpty()
            .WithMessage(localizer[LocalizationKeys.SomeKey]);
    }
}
```

## File: `ClinicHub.Application/Features/<Feature>/Commands/<Action>/<Action>CommandHandler.cs`

```csharp
using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.<Feature>.Commands.<Action>;

public class <Action>CommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<<Action>Command, string>
{
    public async Task<string> Handle(<Action>Command request, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Entity>(request);
        entity.MarkAsCreated();

        await unitOfWork.<Entity>Repository.AddAsync(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return localizer[LocalizationKeys.SuccessCreated];
    }
}
```

## Common Patterns

- **Simple creation**: Create entity, add to repo, save, return localized success message
- **With clinic scoping**: Set `entity.ClinicId = currentUserService.CurrentClinicId` before saving
- **With ownership**: Set `entity.OwnerId = currentUserService.UserId` for user-owned entities
- **Soft delete**: Call `entity.MarkAsDeleted()`, repo handles the rest
- **With validation**: Validator auto-runs via `ValidationBehaviour` — no manual call needed