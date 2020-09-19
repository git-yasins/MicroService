using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using Consul;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using User.Api.Dtos;
using User.API.Data;

namespace User.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddDbContext<UserContext> (
                options => {
                    options.UseMySql (Configuration.GetConnectionString ("MySqlUser"));
                }
            );

            //获取Consul配置,映射为ServiceDisvoveryOptions对象
            services.Configure<ServiceDiscoveryOptions> (Configuration.GetSection ("ServiceDiscovery"));

            //注册Consul客户端
            services.AddSingleton<IConsulClient> (provider => new ConsulClient (cfg => {
                var serviceConfiguration = provider.GetRequiredService<IOptions<ServiceDiscoveryOptions>> ().Value;
                if (!string.IsNullOrEmpty (serviceConfiguration.Consul.HttpEndpoint)) {
                    cfg.Address = new Uri (serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

            //注册JWT验证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear ();

            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme).AddJwtBearer (Options => {
                Options.RequireHttpsMetadata = false;
                Options.Audience = "user_api";
                Options.Authority = "http://localhost:8003";
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

            //CAP
            services.AddCap (options => {
                options
                    .UseMySql (Configuration.GetConnectionString ("MySqlUser"))
                    .UseRabbitMQ (mq => { //发布|订阅 rabbitMQ主机地址
                        mq.HostName = "10.211.55.5";
                        mq.UserName = "admin";
                        mq.Password = "admin";
                    })
                    .UseDashboard (); //Cap的可视化管理界面；默认地址:http://localhost:8002/cap

                //注册Consul
                options.UseDiscovery (d => {
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 5800;
                    d.NodeId = "1";
                    d.NodeName = "CAP No.1 Node";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime hostApplicationLifetime, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consulClient) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            #region Consul    
            //启动时注册Consul服务
            hostApplicationLifetime.ApplicationStarted.Register (() => {
                RegisterService (app, serviceOptions, consulClient);
            });

            //停止时移除Consul服务
            hostApplicationLifetime.ApplicationStopped.Register (() => {
                DeRegisterService (app, serviceOptions, consulClient);
            });
            #endregion

            //使用权限验证
            app.UseAuthentication ();

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }

        /// <summary>
        /// 注册Consul服务发现
        /// </summary>
        /// <param name="app"></param>
        /// <param name="serviceOptions"></param>
        private void RegisterService (IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consulClient) {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature> ().Addresses.Select (p => new Uri (p));

            foreach (var address in addresses) {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes (1),
                    Interval = TimeSpan.FromSeconds (30),
                    HTTP = new Uri (address, "HealthCheck").OriginalString //HealthCheck Controller Name 健康检查
                };

                var registration = new AgentServiceRegistration {
                    Checks = new [] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consulClient.Agent.ServiceRegister (registration).GetAwaiter ().GetResult ();
            }
        }

        //停止Consul服务
        private void DeRegisterService (IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consulClient) {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature> ().Addresses.Select (p => new Uri (p));

            foreach (var address in addresses) {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";
                consulClient.Agent.ServiceDeregister (serviceId).GetAwaiter ().GetResult ();
            }
        }
    }
}