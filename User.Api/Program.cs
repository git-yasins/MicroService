using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Elasticsearch;

namespace User.API {
    public class Program {

        public static void Main (string[] args) {
            CreateHostBuilder (args).Build ().Run ();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder (args)
            // .UseSerilog ((ctx, config) => {
            //     config.ReadFrom.Configuration (ctx.Configuration);
            //     config.WriteTo.Console (new ElasticsearchJsonFormatter ());
            // })
            .ConfigureWebHostDefaults (webBuilder => {
                webBuilder.UseStartup<Startup> ();
            });
    }
}