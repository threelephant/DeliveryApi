using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Domain.Account
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByLoginAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.ActionArguments.TryGetValue("login", out var login);
            if (context.HttpContext.User.Identity?.Name != login as string)
            {
                context.Result = new ForbidResult();
                return;
            }
            
            await next();
        }
    }
}