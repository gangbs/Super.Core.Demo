using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Super.Core.Calc.Middleware
{
    public class WsHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public WsHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/ws"))
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    //处理ws请求
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next.Invoke(context);
            }

        }
    }


    public static class WsHandlerMiddlewareExtension
    {
        public static IApplicationBuilder UseWsHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WsHandlerMiddleware>();
        }
    }
}
