using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SerilogWapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Log just to get startup errors. The 'most used' log configuration is located at the Startup class.
            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName()
                    .WriteTo.File("SerilogWappLog.txt",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} Properties: {Properties} {Exception}{NewLine}", // output format
                        rollingInterval: RollingInterval.Day, // Create files per day
                        retainedFileCountLimit: 6, // Keep just the last 6 files
                        rollOnFileSizeLimit: true) // If file size exceeds their size, create a new file for the same day;
                    .CreateBootstrapLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}