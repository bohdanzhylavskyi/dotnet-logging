using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.EmailPickup;

namespace BrainstormSessions
{
    public class Program
    {
        private static readonly string EmailPickupDirectory = @"c:\logs\emailpickup";

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}")
                .WriteTo.EmailPickup(
                    fromEmail: "logging-app@gmail.com",
                    toEmail: "logging-app-admin@gmail.com",
                    pickupDirectory: EmailPickupDirectory,
                    subject: "Logging",
                    fileExtension: ".email",
                    restrictedToMinimumLevel: LogEventLevel.Warning
                )
                .CreateLogger();


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((logging) =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                });
    }
}
