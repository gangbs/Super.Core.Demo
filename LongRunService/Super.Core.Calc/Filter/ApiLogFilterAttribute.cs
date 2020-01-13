using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Super.Core.Calc.Models;
using Super.Core.Infrastruct.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Super.Core.Calc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ApiLogFilterAttribute : ActionFilterAttribute
    {
        readonly ILoggerFactory _loggerfactory;

        public ApiLogFilterAttribute(ILoggerFactory loggerfactory)
        {
            this._loggerfactory = loggerfactory;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            WriteInfoToLog(context);
            // do something before the action executes
            var resultContext = await next();
            // do something after the action executes; resultContext.Result will be set
        }


        private void WriteInfoToLog(ActionExecutingContext context)
        {
            var msg = new ApiInfoLogModel();
            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            msg.ControllerName = action.ControllerName;
            msg.ActionName = action.ActionName;
            msg.HttpMethod = context.HttpContext.Request.Method;
            msg.Path = context.HttpContext.Request.Path;
            msg.User = context.HttpContext.User.Identity.Name;
            msg.RequestJson = context.ActionArguments.ToJson();
            //msg.ResponseJson = context.HttpContext.Response.ToString();
            var logger = _loggerfactory.CreateLogger(context.Controller.ToString());
            logger.LogTrace(msg.ToString());
        }
    }
}
