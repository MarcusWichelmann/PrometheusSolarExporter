using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PrometheusSolarExporter.Options;
using PrometheusSolarExporter.Sources.SamilPowerInverters;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol;

namespace PrometheusSolarExporter
{
    public static class Program
    {
        private static readonly Dictionary<string, string> SwitchMappings = new Dictionary<string, string> {
            { "--host", "Server:Hostname" },
            { "--port", "Server:Port" },
            { "--path", "Server:Path" },
            { "--log-level", "Logging:LogLevel:Default" }
        };

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config => config.AddCommandLine(args, SwitchMappings))
                .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ServerOptions>(context.Configuration.GetSection("Server"));

            services.AddSingleton<InverterMetricCollection>();
            services.AddHostedService<ExporterService>();

            // Add support for Samil Power inverters
            services.Configure<SamilPowerOptions>(context.Configuration.GetSection("SamilPower"));
            services.AddSingleton<SamilPowerProtocol>();
            services.AddHostedService<SamilPowerInverterListener>();

            // Add other data sources here...
        }
    }
}
