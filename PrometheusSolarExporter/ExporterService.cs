using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using PrometheusSolarExporter.Options;

namespace PrometheusSolarExporter
{
    public class ExporterService : IHostedService
    {
        private readonly ILogger<ExporterService> _logger;

        private readonly MetricServer _server;
        private readonly string _listeningUrl;

        public ExporterService(ILogger<ExporterService> logger, IOptions<ServerOptions> serverOptions)
        {
            _logger = logger;

            _server = new MetricServer(serverOptions.Value.Hostname, serverOptions.Value.Port,
                serverOptions.Value.Path);
            _listeningUrl = new UriBuilder("http", serverOptions.Value.Hostname, serverOptions.Value.Port,
                serverOptions.Value.Path).ToString();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start the HTTP Server
            _logger.LogInformation("Starting metrics server on {listeningUrl}", _listeningUrl);
            _server.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop server
            _logger.LogInformation("Stopping metrics server...");
            return _server.StopAsync();
        }
    }
}
