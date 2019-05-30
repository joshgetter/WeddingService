using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UploadService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 5001);  // http:localhost:5001
                options.Listen(IPAddress.Any, 5000, listenOptions => // https:localhost:5000
                {
                    var certificateCredential = File.ReadLines("CertificateCredential.txt").First();
                    listenOptions.UseHttps("hounvs.ddns.net.pfx", certificateCredential);
                });
            })
                .UseStartup<Startup>();
    }
}
