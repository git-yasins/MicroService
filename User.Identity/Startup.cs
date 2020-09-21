using DnsClient;
using IdentityServer4.Services;
using IdentityServerCenter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using User.Identity.Authentication;
using User.Identity.Dtos;
using User.Identity.Infrastructure;
using User.Identity.Services;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace User.Identity {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            //配置IdentityServer
            services.AddIdentityServer ()
                .AddExtensionGrantValidator<Authentication.SmsAuthCodeValidator> ()
                .AddDeveloperSigningCredential ()
                .AddInMemoryIdentityResources (Config.GetIdentityResources ())
                .AddInMemoryClients (Config.GetClients ())
                .AddInMemoryApiResources (Config.GetApiResource ());

            //获取Consul配置,映射为ServiceDisvoveryOptions对象
            services.Configure<ServiceDiscoveryOptions> (Configuration.GetSection ("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery> (p => {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>> ().Value;
                return new LookupClient (serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint ()); //IPAddress.Parse("127.0.0.1"), 8600
            });

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
            services.AddScoped<IUserService, UserService> ();
            services.AddScoped<IAuthCodeService, AuthCodeService> ();
            //用户配置Claims注册
            services.AddTransient<IProfileService, ProfileService> ();

            services.AddControllers ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }
            app.UseIdentityServer ();

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            RegisterZipkinTrace (app, loggerFactory, applicationLifetime);

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
        /// <summary>
        /// 请求跟踪
        /// </summary>
        /// <param name="application"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="applicationLifetime"></param>
        public void RegisterZipkinTrace (IApplicationBuilder application, ILoggerFactory loggerFactory, IHostApplicationLifetime applicationLifetime) {
            applicationLifetime.ApplicationStarted.Register (() => {
                TraceManager.SamplingRate = 1.0f; //记录数据粒度 全部记录
                var logger = new TracingLogger (loggerFactory, "zipkin4net");
                var httpSender = new HttpZipkinSender ("http://10.211.55.5:9411", "application/json");
                var tracer = new ZipkinTracer (httpSender, new JSONSpanSerializer (), new Statistics ()); //序列化 统计
                var consoleTracer = new zipkin4net.Tracers.ConsoleTracer ();
                TraceManager.RegisterTracer (tracer);
                TraceManager.RegisterTracer (consoleTracer);
                TraceManager.Start (logger);
            });

            applicationLifetime.ApplicationStopped.Register (() => {
                application.UseTracing ("identity_api");
            });
        }
    }
}