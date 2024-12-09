using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Questao5.Domain.Dto;
using Questao5.Domain.Exception;

namespace Questao5.Configurations
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var statusCode = context.Exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,

                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

                _ => StatusCodes.Status500InternalServerError
            };

            if(statusCode == 400)
            {
                context.Result = new ObjectResult(new ResponseErrorDto()
                {
                    Error = context.Exception.Message,
                })
                {
                    StatusCode = statusCode
                };
            }
            else
            {
                context.Result = new ObjectResult(new ResponseErrorDto()
                {
                    Error = context.Exception.Message,
                    StackTrace = context.Exception.StackTrace
                })
                {
                    StatusCode = statusCode
                };
            }
        }
    }
}
