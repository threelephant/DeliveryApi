using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Delivery.Domain.Account
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeOwnerAttribute : Attribute, IAsyncActionFilter
    {
        private deliveryContext db;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            db = new deliveryContext();
            
            var login = context.HttpContext.User.Identity?.Name;
            var isCourier = db.Stores.Any(c => c.OwnerLogin == login);
            await db.DisposeAsync();
            
            if (!isCourier)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}