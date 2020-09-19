using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using Consul;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Project.API.Applications.Queries;
using Project.API.Applications.Service;
using Project.API.Dto;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using Project.Infrastructure;
using Project.Infrastructure.Repositories;

namespace Project.API {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddDbContext<ProjectContext> (options => {
                options.UseMySql (Configuration.GetConnectionString ("MySqlProject"), mySql => {
                    mySql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                });
            });

            services.AddMediatR (typeof (Startup));
            //services.AddMediatR (typeof (Domain.AggregatesModel.Project).GetType().Assembly);

            services.AddScoped<IRecommendService, RecommendService> ()
                .AddScoped<IProjectRepository, ProjectRepository> (sp => {
                    var projectContext = sp.GetRequiredService<ProjectContext> ();
                    return new ProjectRepository (projectContext);
                });

            services.AddScoped<IProjectQueries, ProjectQueries> (sp => {
                return new ProjectQueries (Configuration.GetConnectionString ("MySqlProject"));
            });

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
                Options.Audience = "project_api";
                Options.Authority = "http://localhost:8003";
            });

            services.AddControllers ();
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