using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace SerilogWapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ConfigureLogger(app, env);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureLogger(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var assemblyData = Assembly.GetExecutingAssembly().GetName();

            Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Env Filename", env.EnvironmentName)
                    .Enrich.WithProperty("Env Name", Configuration.GetSection("EnvironmentSettings").GetValue<string>("AppEnvironment"))
                    .Enrich.WithProperty("Assembly Name", assemblyData.Name)
                    .Enrich.WithProperty("Assembly Version", assemblyData.Version)
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentUserName()
                    .WriteTo.File($"SerilogWapp-{env.EnvironmentName}-Log-.txt", // Write a different log name depending o the environment
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} Properties: {Properties} {Exception}{NewLine}", // output format
                        rollingInterval: RollingInterval.Day, // Create files per day
                        retainedFileCountLimit: 7, // Keep just the last 6 files
                        rollOnFileSizeLimit: true) // If file size exceeds their size, create a new file for the same day;
                    .CreateLogger();

            app.UseSerilogRequestLogging(options =>
            {
                // Emit debug-level events instead of the defaults
                options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });
        }
    }
}