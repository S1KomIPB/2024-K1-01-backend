using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Middleware
{
    public class AdminRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.IsInRole("Admin"))
            {
                context.Result = new UnauthorizedObjectResult(new { Message = "Admin privileges required" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}