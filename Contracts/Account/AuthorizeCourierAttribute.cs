using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Contracts.Account
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeCourierAttribute : Attribute, IAsyncActionFilter
    {
        private deliveryContext db;
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var login = context.HttpContext.User.Identity?.Name;
            
            db = new deliveryContext();
            var isCourier = db.Couriers.Any(c => c.UserLogin == login);
            await db.DisposeAsync();
            
            if (!isCourier)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}