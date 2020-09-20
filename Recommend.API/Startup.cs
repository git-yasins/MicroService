using System.IdentityModel.Tokens.Jwt;
using DnsClient;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recommend.API.Data;
using Recommend.API.Dtos;
using Recommend.API.Infrastructure;
using Recommend.API.IntegrationEventHandlers;
using Recommend.API.Services;
using Resilience;

namespace Recommend.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            //注册JWT验证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear ();
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme).AddJwtBearer (Options => {
                Options.RequireHttpsMetadata = false;
                Options.Audience = "recommend_api";
                Options.Authority = "http://localhost:8003";
            });

            //获取Consul配置,映射为ServiceDisvoveryOptions对象
            services.Configure<ServiceDiscoveryOptions> (Configuration.GetSection ("ServiceDiscovery"));
            
            services.AddDbContext<RecommendDbContext> (options => {
                    options.UseMySql (Configuration.GetConnectionString ("MySqlRecommend"));
                })
                .AddSingleton<IDnsQuery> (p => {
                    var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>> ().Value;
                    return new LookupClient (serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint ()); //IPAddress.Parse("127.0.0.1"), 8600
                })
                .AddScoped<IUserService,UserService>()
                .AddScoped<IContactService,ContactService>()
                .AddScoped<ProjectCretedIntegrationEventHandler> () //注册CAP RabbitMQ 消息订阅;
                .AddHttpContextAccessor ()
                .AddSingleton (typeof (ResilienceClientFactory), sp => { //注册全局单例 重试融断工厂
                    var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>> ();
                    var httpContextAccesser = sp.GetRequiredService<IHttpContextAccessor> ();
                    var retryCount = 5;
                    var exceptionCountAllowedBeforeBreaking = 5;
                    return new ResilienceClientFactory (logger, httpContextAccesser, retryCount, exceptionCountAllowedBeforeBreaking);
                })
                .AddSingleton<IHttpClient> (sp => {
                    return sp.GetRequiredService<ResilienceClientFactory> ().GetResilienceHttpClient (); //注册全局单例IHttpClient
                })
                .AddControllers ();

            services.AddCap (options => { //CAP
                options.UseMySql (Configuration.GetConnectionString ("MySqlRecommend"))
                    .UseRabbitMQ (mq => { //发布|订阅 rabbitMQ主机地址
                        mq.HostName = "10.211.55.5";
                        mq.UserName = "admin";
                        mq.Password = "admin";
                    })
                    .UseDashboard (); //Cap的可视化管理界面；默认地址:http://localhost:端口/cap

                //注册Consul
                options.UseDiscovery (d => {
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 8006;
                    d.NodeId = "4";
                    d.NodeName = "CAP RecommendAPI Node";
                });
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

            app.UseAuthentication ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}