using MediatR;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiBaseController : ControllerBase
{
    private ISender? _sender;
    protected ISender Mediator => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        var error = result.TopError;
        return error.Type switch
        {
            ErrorKind.NotFound => NotFound(new { code = error.Code, message = error.Description }),
            ErrorKind.Validation => BadRequest(new { code = error.Code, message = error.Description, errors = result.Errors }),
            ErrorKind.Conflict => Conflict(new { code = error.Code, message = error.Description }),
            ErrorKind.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { code = error.Code, message = error.Description }),
            ErrorKind.Unauthorized => Unauthorized(new { code = error.Code, message = error.Description }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { code = error.Code, message = error.Description })
        };
    }
}
