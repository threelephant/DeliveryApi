using System.Linq;
using System.Threading.Tasks;
using Delivery.Contracts.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errorsInModelState = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(x => x.ErrorMessage))
                    .ToArray();

                var errorResponse = new ErrorResponse();

                foreach (var (errorKey, errorValue) in errorsInModelState)
                {
                    foreach (var subError in errorValue)
                    {
                        var errorModel = new ErrorModel
                        {
                            FieldName = errorKey,
                            Message = subError
                        };
                        
                        errorResponse.Errors.Add(errorModel);
                    }
                }

                context.Result = new BadRequestObjectResult(errorResponse);
                return;
            }
            
            await next();
        }
    }
}