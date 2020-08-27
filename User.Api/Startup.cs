using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using User.API.Data;
using User.API.Filters;
namespace User.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddDbContext<UserContext> (options => {
                options.UseMySql (Configuration.GetConnectionString ("MySqlUser"));
            });
  
            services.AddControllers (
                //options => options.Filters.Add (typeof (GlobalExceptionFilter))
                )
                .AddNewtonsoftJson (setup => { //添加[patch]局部更新JSON序列化  JsonPatchDocument 支持
                    //串行化设置
                    setup.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver ();
                }).AddXmlDataContractSerializerFormatters () //3.X支持的输入输出XML
                .ConfigureApiBehaviorOptions (setupAction => { //7867规范 实体验证错误,自定义详细错误422状态码和消息内容
                    setupAction.InvalidModelStateResponseFactory = context => {
                    var problemDetails = new ValidationProblemDetails (context.ModelState) {
                    Type = "www.baidu.com",
                    Title = "错误!!!",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = "查看详细错误",
                    Instance = context.HttpContext.Request.Path
                        };
                        problemDetails.Extensions.Add ("traceId", context.HttpContext.TraceIdentifier);
                        return new UnprocessableEntityObjectResult (problemDetails) {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}