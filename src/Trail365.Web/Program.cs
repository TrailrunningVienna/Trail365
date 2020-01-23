using System;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Trail365.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (System.Linq.Enumerable.Contains(args.Select(a => a.ToLowerInvariant()), "--version"))
            {
                Console.WriteLine(string.Format("{0}", Helper.GetProductVersionFromEntryAssembly()));
                Environment.Exit(0);
            }

            if (System.Linq.Enumerable.Contains(args.Select(a => a.ToLowerInvariant()), "--minor"))
            {
                Console.WriteLine(string.Format("{0}", Helper.GetMinorProductVersionFromEntryAssembly()));
                Environment.Exit(0);
            }

            if (System.Linq.Enumerable.Contains(args.Select(a => a.ToLowerInvariant()), "--major"))
            {
                Console.WriteLine(string.Format("{0}", Helper.GetMajorProductVersionFromEntryAssembly()));
                Environment.Exit(0);
            }

            Console.WriteLine(string.Format("{0} {1}", "Trail 365", Helper.GetProductVersionFromEntryAssembly()));
            Console.WriteLine(string.Format("  {0}", Helper.GetProcessInfo()));
            Console.WriteLine();

            if (System.Linq.Enumerable.Contains(args.Select(a => a.ToLowerInvariant()), "--info"))
            {
                Environment.Exit(0);
            }

            var builder = CreateWebHostBuilder(args);

            builder.ConfigureLogging(factory =>
            {
                factory.AddConsole();
                factory.SetMinimumLevel(LogLevel.Trace);
                // Turn off Info logging for EF commands
                //factory.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            });

            var webHost = builder.Build();
            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             .ConfigureLogging((context, builder) =>
             {
                 builder.AddApplicationInsights();
             })
             .UseKestrel(serverOptions =>
                {
                    // Set properties and call methods on options
                })
            .UseIISIntegration()
            .UseStartup<Startup>();
    }
}
