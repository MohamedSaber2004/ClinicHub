# Query Handler Template

## File: `ClinicHub.Application/Features/<Feature>/Queries/<Action>/<Action>Query.cs`

```csharp
using MediatR;

namespace ClinicHub.Application.Features.<Feature>.Queries.<Action>;

public record <Action>Query(<Parameters>) : IRequest<<ResponseType>>;
```

## File: `ClinicHub.Application/Features/<Feature>/Queries/<Action>/<Action>QueryHandler.cs`

```csharp
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Repositories.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.<Feature>.Queries.<Action>;

public class <Action>QueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper
) : IRequestHandler<<Action>Query, <ResponseType>>
{
    public async Task<<ResponseType>> Handle(<Action>Query request, CancellationToken cancellationToken)
    {
        var result = await unitOfWork.<Entity>Repository
            .GetAllWithIncluding()
            .ProjectTo<<Dto>>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return mapper.Map<<ResponseType>>(result);
    }
}
```

## Common Patterns

- **Single item**: Use `unitOfWork.Repo.GetByIdAsync(id)` 
- **Pagginated**: Use `.ProjectTo<TDto>().ToPagginatedListAsync(request.PageNumber, request.PageSize)` with `PagginatedResult<T>`
- **With filtering**: Use `.WhereIf(!string.IsNullOrEmpty(request.Search), x => x.Name.Contains(request.Search))`
- **With includes**: Use `IGenericRepository.GetAllWithIncluding(Expression<Func<T, object>>[] includes)`