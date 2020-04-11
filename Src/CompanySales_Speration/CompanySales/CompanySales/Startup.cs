using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CompanySales
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // 注入跨域设置
            services.AddCors(opt =>
            {
                // 增加全局跨域规则，global_cors 是规则名称，UseCors需要使用到
                opt.AddPolicy("global_cors", cor =>
                {
                    // 允许所有来源的 CORS 请求和任何方案（http 或 https）
                    cor.AllowAnyOrigin();
                });
            });

            // 加密的秘钥，不能少于16位，密钥在【appsettings.json】配置文件中
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["token:key"]));

            // 添加认证服务，用于对用户验证，相当于登录拦截
            services.AddAuthentication("Bearer").AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,

                    // 是否验证颁发者
                    ValidateIssuer = true,
                    // 颁发者的名称，在【appsettings.json】配置文件中
                    ValidIssuer = Configuration["token:issuer"],

                    // 是否验证接收者
                    ValidateAudience = true,
                    // 接受者名称，在【appsettings.json】配置文件中
                    ValidAudience = Configuration["token:audience"],

                    // 是否必须具有“过期”值。
                    RequireExpirationTime = true,
                    // 是否在令牌验证期间验证生存期
                    ValidateLifetime = true
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 按照规则名称应用跨域规则，必须在 UseRouting 前应用
            app.UseCors("global_cors");

            app.UseRouting();

            // 管道中应用认证中间件
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
