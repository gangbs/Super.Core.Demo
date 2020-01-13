using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Super.Core.Infrastruct.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Super.Core.Calc.Middleware
{
    public class IpSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        readonly ILoggerFactory _loggerfactory;

        public IpSafeListMiddleware(RequestDelegate next, ILoggerFactory loggerfactory)
        {
            _next = next;
            _loggerfactory = loggerfactory;
            InitIpList();
        }


        public async Task Invoke(HttpContext context)
        {
            if (!_ipList.Contains("*"))
            {
                var remoteIp = context.Connection.RemoteIpAddress.ToString();
                if (!_ipList.Contains(remoteIp))
                {
                    var logger = _loggerfactory.CreateLogger(typeof(IpSafeListMiddleware));
                    logger.LogWarning("the client ip is not allowed!");
                    context.Response.StatusCode = 401;
                    return;
                }
            }


            await _next.Invoke(context);
        }


        static List<string> _ipList = new List<string>();
        static object _locker = new object();
        static void InitIpList()
        {
            if (_ipList.Count < 1)
            {
                lock (_locker)
                {
                    if (_ipList.Count < 1)
                    {
                        _ipList = GetIpSafeList();
                    }
                }
            }
        }
        static List<string> GetIpSafeList()
        {
            var lst = new List<string>();
            lst.Add("127.0.0.1");
            lst.Add("::1");
            string ips = AppConfiguration.Instance.GetValue("AllowedHosts");
            if (!string.IsNullOrEmpty(ips))
                lst.AddRange(ips.Split(';'));
            return lst;
        }

    }


    public static class IpSafeListMiddlewareExtension
    {
        public static IApplicationBuilder UseIpSafeListMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IpSafeListMiddleware>();
        }
    }
}
