using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Responses;
using Timer = System.Timers.Timer;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters
{
    public class SamilPowerInverterListener : BackgroundService
    {
        private const int TcpListenPort = 60001;
        private const int AdvertisementInterval = 15 * 1000;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<SamilPowerInverterListener> _logger;
        private readonly SamilPowerProtocol _protocol;
        private readonly InverterMetricCollection _inverterMetricCollection;

        private readonly TcpListener _tcpListener;

        private readonly Timer _advertisementTimer;

        private readonly List<SamilPowerInverter> _connectedInverters = new List<SamilPowerInverter>();

        public SamilPowerInverterListener(ILoggerFactory loggerFactory, SamilPowerProtocol protocol,
            InverterMetricCollection inverterMetricCollection)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SamilPowerInverterListener>();
            _protocol = protocol;
            _inverterMetricCollection = inverterMetricCollection;

            // Don't care about dual stack here, these inverters don't support it anyway... :(
            _tcpListener = new TcpListener(IPAddress.Any, TcpListenPort);

            _advertisementTimer = new Timer(AdvertisementInterval);
            _advertisementTimer.Elapsed += OnAdvertisementTimerElapsed;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Broadcasting advertisements and listening for incoming inverter connections on port {port}...",
                TcpListenPort);

            // Start listening for TCP connections from the inverters
            _tcpListener.Start();

            // Ensure that AcceptTcpClientAsync fails on cancellation
            stoppingToken.Register(() => _tcpListener.Stop());

            // Send advertisement
            await _protocol.BroadcastAdvertisementAsync(stoppingToken).ConfigureAwait(false);

            // Start sending frequent advertisements in the background
            _advertisementTimer.Start();

            // Accept connections until stopping is requested...
            await AcceptConnectionsAsync(stoppingToken).ConfigureAwait(false);

            _logger.LogInformation("Stopping listening and closing all inverter connections...");

            _advertisementTimer.Stop();
            _tcpListener.Stop();

            foreach (SamilPowerInverter inverter in _connectedInverters)
            {
                await inverter.StopMonitoringAsync().ConfigureAwait(false);
                inverter.Dispose();
            }

            _connectedInverters.Clear();
        }

        private async Task AcceptConnectionsAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Don't crash on communication errors
                TcpClient? client = null;
                try
                {
                    client = await _tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);

                    _logger.LogInformation(
                        "New incoming SamilPower inverter connection from {remoteEndpoint}, requesting info...",
                        client.Client.RemoteEndPoint);

                    // Wait for initial communication before accepting inverter connection
                    InverterInfoResponse inverterInfo = await _protocol
                        .SendRequestAsync<InverterInfoResponse>(client.Client, new InverterInfoRequest(), stoppingToken)
                        .ConfigureAwait(false);

                    _logger.LogInformation("SamilPower Inverter connected successfully: {details}",
                        inverterInfo.ToString());

                    // Register and monitor inverter
                    var inverter = new SamilPowerInverter(_loggerFactory.CreateLogger<SamilPowerInverter>(),
                        _inverterMetricCollection, _protocol, client, inverterInfo);
                    await inverter.StartMonitoringAsync(stoppingToken).ConfigureAwait(false);
                    _connectedInverters.Add(inverter);
                }
                catch (Exception ex)
                {
                    client?.Dispose();

                    if (!stoppingToken.IsCancellationRequested)
                        _logger.LogWarning(ex, "Accepting inverter connection failed.");
                }
            }
        }

        private async void OnAdvertisementTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _protocol.BroadcastAdvertisementAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Sending advertisement failed.");
            }
        }
    }
}
