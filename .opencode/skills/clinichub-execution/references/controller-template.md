# Controller Template

## File: `ClinicHub.API/Controllers/Version1/<EntityName>Controller.cs`

```csharp
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.<Feature>.Commands.<Action>;
using ClinicHub.Application.Features.<Feature>.Queries.<Action>;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/<resource>")]
[ApiController]
[Authorize]
public class <EntityName>Controller : BaseApiController
{
    [HttpPost]
    [RoleAuthorize(UserType.Doctor | UserType.ClinicOwner)]
    public async Task<IActionResult> Create([FromBody] <Action>Command command)
    {
        var result = await Mediator.Send(command);
        return Created(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new <Action>Query(id);
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [RoleAuthorize(UserType.Doctor | UserType.ClinicOwner)]
    public async Task<IActionResult> Update(Guid id, [FromBody] <Action>Command command)
    {
        command = command with { Id = id };
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RoleAuthorize(UserType.Doctor | UserType.ClinicOwner)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new <Action>Command(id);
        var result = await Mediator.Send(command);
        return Deleted(result);
    }
}
```

## Key Points

- Inherit `BaseApiController` — provides `Mediator` property and response helpers
- Use `[ApiVersion("1.0")]` attribute
- Use `[Route("api/v{version:apiVersion}/<resource>")]` — version in URL
- Use `[RoleAuthorize(UserType.X)]` for authorization or `[AllowAnonymous]`
- Always call `await Mediator.Send()` — never direct service calls
- Use response helpers: `Ok(result)`, `Created(result)`, `Deleted(result)`, `Accepted(result)`