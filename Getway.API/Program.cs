using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Getway {
    public class Program {
        public static void Main (string[] args) {
            CreateHostBuilder (args).Build ().Run ();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder (args)
            .ConfigureAppConfiguration ((host, builder) => {
                builder
                    .SetBasePath (host.HostingEnvironment.ContentRootPath)
                    .AddJsonFile ("ocelot.json");
            })
            .ConfigureWebHostDefaults (webBuilder => {
                webBuilder.UseUrls ("http://+:80");
                webBuilder.UseStartup<Startup> ();
            });
    }
}