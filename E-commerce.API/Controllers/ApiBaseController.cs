using E_commerce.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
        // Errors -> Http Respone
        protected static ObjectResult ToProblem(IReadOnlyList<Error> errors)
        {
            var first = errors[0];
            var status = first.type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = first.code,
                Detail = first.Description,
                Extensions = { ["errors"] = errors }
            };

            return new ObjectResult(problem) { StatusCode = status };
        }


        public static ActionResult<T> ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Data);
            }

            return ToProblem(result.Errors);
        }



        public static ActionResult ToActionResult(Result result)
        {
            if (result.IsSuccess)
                return new OkResult();

            return ToProblem(result.Errors);
        }
    }
}
