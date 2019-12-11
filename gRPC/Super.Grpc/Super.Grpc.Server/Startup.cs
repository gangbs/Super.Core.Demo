using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Super.Grpc.Server.Services;

namespace Super.Grpc.Server
{
    public class Startup
    {
       public static SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567812345678")); 


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options => 
            {
                options.EnableDetailedErrors = true;  
            });

            #region jwt验证

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                });
            });

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
                    IssuerSigningKey = SecurityKey//拿到SecurityKey
                };
            });

            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<TypicalService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });

                endpoints.MapGet("/jwt", context =>
                {
                    return context.Response.WriteAsync(GenerateJwtToken("yyg"));
                });

            });
                       
        }

        private string GenerateJwtToken(string name)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, name) };
            var creds = new SigningCredentials(Startup.SecurityKey, SecurityAlgorithms.HmacSha256);
            var head = new JwtHeader(creds);
            var payload = new JwtPayload(claims);
            var tokenObj = new JwtSecurityToken(head, payload);
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObj);
            return token;
        }


    }
}
