using System.IdentityModel.Tokens.Jwt;
using Contact.API.Data;
using Contact.API.Dtos;
using Contact.API.Infrastructure;
using Contact.API.IntegrationEvents;
using Contact.API.Service;
using DnsClient;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
namespace Contact.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            //获取MongoDB配置
            services.Configure<AppSettings> (Configuration.GetSection ("MongoDB"));

            //注册JWT验证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear ();
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme).AddJwtBearer (Options => {
                Options.RequireHttpsMetadata = false;
                Options.Audience = "contact_api";
                Options.Authority = "http://localhost:8003";
            });

            //获取Consul配置,映射为ServiceDisvoveryOptions对象
            services.Configure<ServiceDiscoveryOptions> (Configuration.GetSection ("ServiceDiscovery"));

            services.AddSingleton<ContactContext> ()
                .AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository> ()
                .AddScoped<IContactRepository, MongoContactRepository> ()
                .AddScoped<IUserService, UserService> ()
                .AddScoped<UserProfileChangedEventHandler> (); //注册CAP RabbitMQ 消息订阅

            services.AddSingleton<IDnsQuery> (p => {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>> ().Value;
                return new LookupClient (serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint ()); //IPAddress.Parse("127.0.0.1"), 8600
            });

            services.AddHttpContextAccessor ();
            //注册全局单例 重试融断工厂
            services.AddSingleton (typeof (ResilienceClientFactory), sp => {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>> ();
                var httpContextAccesser = sp.GetRequiredService<IHttpContextAccessor> ();
                var retryCount = 5;
                var exceptionCountAllowedBeforeBreaking = 5;
                return new ResilienceClientFactory (logger, httpContextAccesser, retryCount, exceptionCountAllowedBeforeBreaking);
            });

            //注册全局单例IHttpClient
            services.AddSingleton<IHttpClient> (sp => {
                return sp.GetRequiredService<ResilienceClientFactory> ().GetResilienceHttpClient ();
            });

            services.AddControllers ();

            //CAP
            services.AddCap (options => {
                options.UseMySql (Configuration.GetConnectionString ("MySqlUser"))
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
                    d.CurrentNodePort = 5801;
                    d.NodeId = "2";
                    d.NodeName = "CAP No.2 Node";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }
            //添加JWT验证
            app.UseAuthentication ();

            #region Consul    
            //启动时注册Consul服务
            // hostApplicationLifetime.ApplicationStarted.Register (() => {
            //     RegisterService (app, serviceOptions, consulClient);
            // });

            // //停止时移除Consul服务
            // hostApplicationLifetime.ApplicationStopped.Register (() => {
            //     DeRegisterService (app, serviceOptions, consulClient);
            // });
            #endregion

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }

        // /// <summary>
        // /// 注册Consul服务发现
        // /// </summary>
        // /// <param name="app"></param>
        // /// <param name="serviceOptions"></param>
        // private void RegisterService (IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consulClient) {
        //     var features = app.Properties["server.Features"] as FeatureCollection;
        //     var addresses = features.Get<IServerAddressesFeature> ().Addresses.Select (p => new Uri (p));

        //     foreach (var address in addresses) {
        //         var serviceId = $"{serviceOptions.Value.UserServiceName}_{address.Host}:{address.Port}";

        //         var httpCheck = new AgentServiceCheck {
        //             DeregisterCriticalServiceAfter = TimeSpan.FromMinutes (1),
        //             Interval = TimeSpan.FromSeconds (30),
        //             HTTP = new Uri (address, "HealthCheck").OriginalString //HealthCheck Controller Name 健康检查
        //         };

        //         var registration = new AgentServiceRegistration {
        //             Checks = new [] { httpCheck },
        //             Address = address.Host,
        //             ID = serviceId,
        //             Name = serviceOptions.Value.UserServiceName,
        //             Port = address.Port
        //         };

        //         consulClient.Agent.ServiceRegister (registration).GetAwaiter ().GetResult ();
        //     }
        // }

        // //停止Consul服务
        // private void DeRegisterService (IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consulClient) {
        //     var features = app.Properties["server.Features"] as FeatureCollection;
        //     var addresses = features.Get<IServerAddressesFeature> ().Addresses.Select (p => new Uri (p));

        //     foreach (var address in addresses) {
        //         var serviceId = $"{serviceOptions.Value.UserServiceName}_{address.Host}:{address.Port}";
        //         consulClient.Agent.ServiceDeregister (serviceId).GetAwaiter ().GetResult ();
        //     }
        // }
    }
}