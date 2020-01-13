using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Super.Core.Calc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Super.Core.Calc.Filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ApiAuthorizationFilterAttribute : Attribute, IAuthorizationFilter
    {
        readonly ILoggerFactory _loggerfactory;

        public ApiAuthorizationFilterAttribute(ILoggerFactory loggerfactory)
        {
            this._loggerfactory = loggerfactory;
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //模块授权
            //if (!LicMng.Instance.HasLicense && LicMng.Instance.DemoRestTime <= 0)
            //{
            //    context.Result = new ForbidResult();
            //}

            //身份验证
            var filterPipeline = context.Filters;
            var hasAuthorized = filterPipeline.Any(filter => filter is AuthorizeFilter);
            var allowAnonymous = filterPipeline.Any(filter => filter is IAllowAnonymousFilter);
            var needAuth = hasAuthorized && !allowAnonymous;

            if (needAuth && !context.HttpContext.User.Identity.IsAuthenticated)
            {
                WriteLog(context);
                context.Result = new UnauthorizedObjectResult(new { message = "用户身份信息验证失败" });
            }



            //用户权限

        }


        private void WriteLog(AuthorizationFilterContext context)
        {
            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            ApiErrorLogModel msg = new ApiErrorLogModel
            {
                HttpMethod = context.HttpContext.Request.Method,
                ControllerName = action.ControllerName,
                ActionName = action.ActionName,
                Path = context.HttpContext.Request.Path,
                Msg = "authentication failed!"
            };
            var logger = _loggerfactory.CreateLogger(context.HttpContext.Request.Path);
            logger.LogWarning(msg.ToString());
        }
    }
}
