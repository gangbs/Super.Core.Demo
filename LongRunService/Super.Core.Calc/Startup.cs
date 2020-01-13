using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Super.Core.Calc.CalcService;
using Super.Core.Calc.Filter;
using Super.Core.Calc.Middleware;
using Super.Core.Infrastruct.Configuration;
using Super.Core.Infrastruct.Logger;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Super.Core.Calc
{
    public class Startup
    {
        readonly ILogger _logger;
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            this._logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = "SuperCoreTemplate",
                    Version = "v1"
                });



                // 为 Swagger JSON and UI设置xml文档注释路径
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var xmlPath = Path.Combine(basePath, "Config/Super.Core.Calc.xml");
                c.IncludeXmlComments(xmlPath);


                c.DescribeAllEnumsAsStrings();
                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });



                // 在过滤器中为需要认证的API添加对应参数
                // 过滤器的定义见下文
                c.OperationFilter<AuthorizationHeaderOperationFilter>();


            });

            #endregion

            #region 添加cache

            services.AddMemoryCache();

            #endregion

            #region 模型验证

            services.Configure<ApiBehaviorOptions>(options =>
            {
                //阻止自动的模型验证处理，改由自定义拦截器去统一处理
                options.SuppressModelStateInvalidFilter = true;
            });

            #endregion

            #region Mvc、拦截器设置

            services.AddMvc(options =>
            {
                options.AllowValidatingTopLevelNodes = true;
                options.Filters.Add<ModelValidateFilterAttribute>();//模型验证拦截器
                options.Filters.Add<ApiExceptionFilterAttribute>();//系统异常拦截器 
                options.Filters.Add<ApiLogFilterAttribute>();//api日志拦截器 
                options.Filters.Add<ApiAuthorizationFilterAttribute>();//身份验证拦截器
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            #endregion

            #region 跨域设置

            services.AddCors();//貌似这个不要也行，只要中间件中添加了就可以

            #endregion

            #region jwt验证

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,//是否验证Issuer
                    ValidateAudience = false,//是否验证Audience
                    ValidateLifetime = false,//是否验证失效时间
                    ValidateIssuerSigningKey = true,//是否验证SecurityKey
                    //ValidAudience = JwtConst.Audience,//Audience
                    //ValidIssuer = JwtConst.Issuer,//Issuer，这两项和前面签发jwt的设置一致
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfiguration.Instance.GetValue("Jwt:SecurityKey")))//拿到SecurityKey
                };
            });

            #endregion

            //服务注入
            this.ExtraServiceRegist(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime, ILoggerFactory loggerFactory)
        {
            #region 注册logger,便于非依赖注入、或其它程序集获取logger

            LoggerBuilder.Instance.Register(loggerFactory);

            #endregion

            #region 程序的开启和关闭事件注册

            appLifetime.ApplicationStarted.Register(() =>
            {
                this._logger.LogInformation("application started!");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                this._logger.LogInformation("application is stopping!");
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                this._logger.LogInformation("application stopped!");
            });

            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region swagger

            //启用中间件服务生成Swagger作为JSON终结点
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SuperCoreTemplate");
            });

            #endregion

            #region Cors跨域设置

            app.UseCors(
                builder =>
                {
                    builder.AllowAnyHeader();//任何http头信息
                    builder.AllowAnyMethod();//任何http方法
                    builder.AllowAnyOrigin();//任何源
                });//CORS 中间件必须位于之前定义的任何终结点应用程序中你想要支持跨域请求

            #endregion

            #region websocket,要添加在usemvc之前

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),//向客户端发送“ping”帧的频率，以确保代理保持连接处于打开状态。 默认值为 2 分钟
                ReceiveBufferSize = 4 * 1024//用于接收数据的缓冲区的大小。 高级用户可能需要对其进行更改，以便根据数据大小调整性能。 默认值为 4 KB               
            });

            app.UseWsHandlerMiddleware();

            #endregion

            #region 静态文件发布

#if DEBUG
#else

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider($"{AppContext.BaseDirectory}/Logs"),
                RequestPath = new PathString("/log")
            });

#endif

            #endregion

            app.UseAuthentication();//启用验证
            app.UseMvc();
        }


        /// <summary>
        /// 本地IOC容器注册
        /// </summary>
        /// <param name="services"></param>
        public void ExtraServiceRegist(IServiceCollection services)
        {
            //services.AddTransient<IYygTest, YygTest>();
            services.AddSingleton<ICalcTaskQueue, CalcTaskQueue>();
        }


        /// <summary>
        /// 判断是否需要添加Authorize Header
        /// </summary>
        public class AuthorizationHeaderOperationFilter : IOperationFilter
        {
            /// <summary>
            /// 为需要认证的Operation添加认证参数
            /// </summary>
            /// <param name="operation">The Swashbuckle operation.</param>
            /// <param name="context">The Swashbuckle operation filter context.</param>
            public void Apply(Operation operation, OperationFilterContext context)
            {
                // 获取对应方法的过滤器描述
                // 应该也就是所添加的Attribute
                var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
                // 判断是否添加了AuthorizeFilter
                // 也就是[Authorize]
                var isAuthorized = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is AuthorizeFilter);
                // 判断是否添加了IAllowAnonymousFilter
                // 也就是[AllowAnonymous]
                var allowAnonymous = filterPipeline.Select(filterInfo => filterInfo.Filter).Any(filter => filter is IAllowAnonymousFilter);

                // 仅当需要认证且不是AllowAnonymous的情况下，添加认证参数
                if (isAuthorized && !allowAnonymous)
                {
                    // 若该Operation不存在认证参数的话，
                    // 这个Security将是null，而不是空的List
                    if (operation.Security == null)
                        operation.Security = new List<IDictionary<string, IEnumerable<string>>>();

                    // 添加认证参数
                    operation.Security.Add(new Dictionary<string, IEnumerable<string>>
                    {
                      { JwtBearerDefaults.AuthenticationScheme, new string[] { } }
                    });
                }

            }
        }
    }
}
