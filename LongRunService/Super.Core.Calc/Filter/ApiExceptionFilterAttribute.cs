using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Super.Core.Calc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Super.Core.Calc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        readonly ILoggerFactory _loggerfactory;

        public ApiExceptionFilterAttribute(ILoggerFactory loggerfactory)
        {
            this._loggerfactory = loggerfactory;
        }


        public override void OnException(ExceptionContext context)
        {
            //如果该异常已被处理则跳过
            if (context.ExceptionHandled) return;

            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            var reqParam = context.Filters.FirstOrDefault(x => x.GetType() == typeof(ModelValidateFilterAttribute));

            var model = new ApiErrorLogModel
            {
                HttpMethod = context.HttpContext.Request.Method,
                ControllerName = action.ControllerName,
                ActionName = action.ActionName,
                Path = context.HttpContext.Request.Path,
                User = context.HttpContext.User.Identity.Name,
                Msg = context.Exception.Message,
                RequestJson = reqParam != null ? JsonConvert.SerializeObject(((ModelValidateFilterAttribute)reqParam).RequestArguments) : null
            };

            var logger = _loggerfactory.CreateLogger($"{context.Exception.TargetSite.DeclaringType}.{context.Exception.TargetSite.Name}");
            logger.LogError(context.Exception, model.ToString());


            context.Result = new BadRequestObjectResult(new ErrorResponseModel { code = (int)HttpStatusCode.BadRequest, message = context.Exception.Message, detail = context.Exception });
        }
    }
}
