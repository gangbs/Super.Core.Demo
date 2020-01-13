using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Super.Core.Calc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Super.Core.Calc.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        readonly ILoggerFactory _loggerfactory;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerfactory)
        {
            _next = next;
            _loggerfactory = loggerfactory;
        }


        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                //var loggerfactory = (ILoggerFactory)context.RequestServices.GetService(typeof(ILoggerFactory));
                var logger = _loggerfactory.CreateLogger($"{e.TargetSite.DeclaringType}.{e.TargetSite.Name}");
                logger.LogError(e, e.Message);
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                var res = new ErrorResponseModel { code = (int)HttpStatusCode.BadRequest, message = e.Message, detail = e };
                await context.Response.WriteAsync(JsonConvert.SerializeObject(res));
            }
        }
    }

    public static class ExceptionMiddlewareExtension
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
