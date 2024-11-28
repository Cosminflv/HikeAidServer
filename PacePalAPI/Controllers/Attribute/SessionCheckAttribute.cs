using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PacePalAPI.Controllers.Attribute
{
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        private readonly string _sessionKey;

        public SessionCheckAttribute(string sessionKey)
        {
            _sessionKey = sessionKey;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userJson = session.GetString(_sessionKey);

            if (userJson == null)
            {
                // Return 404 or any other status code with a message
                //context.Result = new NotFoundObjectResult("No active session found.");
                context.Result = new UnauthorizedObjectResult("Unauthorized: Session expired or not found.");
            }
        }
    }
}
