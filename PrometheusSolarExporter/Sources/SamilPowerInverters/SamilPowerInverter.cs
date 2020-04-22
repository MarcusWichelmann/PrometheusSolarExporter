using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Responses;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters
{
    public class SamilPowerInverter : IInverter, IDisposable
    {
        private static readonly TimeSpan MinimumUpdateInterval = TimeSpan.FromSeconds(1);

        private readonly ILogger<SamilPowerInverter> _logger;
        private readonly InverterMetricCollection _inverterMetricCollection;
        private readonly SamilPowerProtocol _protocol;
        private readonly TcpClient _tcpClient;

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        private Task? _updateTask;

        public string Model { get; }

        public string Identification { get; }

        public bool Disconnected { get; private set; }

        public SamilPowerInverter(ILogger<SamilPowerInverter> logger, InverterMetricCollection inverterMetricCollection,
            SamilPowerProtocol protocol, TcpClient tcpClient, InverterInfoResponse inverterInfo)
        {
            _logger = logger;
            _inverterMetricCollection = inverterMetricCollection;
            _protocol = protocol;
            _tcpClient = tcpClient;

            Model = inverterInfo.ModelDescription;
            Identification = inverterInfo.SerialNumber ?? "unknown";
        }

        public Task StartMonitoringAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _updateTask = ExecuteUpdatesAsync(_stoppingCts.Token);

            return _updateTask.IsCompleted ? _updateTask : Task.CompletedTask;
        }

        public async Task StopMonitoringAsync()
        {
            if (_updateTask == null)
                return;

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await _updateTask.ConfigureAwait(false);
            }
        }

        private async Task ExecuteUpdatesAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateDataAsync(stoppingToken).ConfigureAwait(false);

                    // Wait before next update
                    await Task.Delay(MinimumUpdateInterval, stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (!stoppingToken.IsCancellationRequested)
                        _logger.LogWarning(ex, $"Updating status data for inverter {Identification} failed.");

                    Disconnected = true;
                    break;
                }
            }
        }

        private async Task UpdateDataAsync(CancellationToken cancellationToken = default)
        {
            if (Disconnected)
                return;
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogDebug("Updating status data for inverter {identification}...", Identification);

            // Request current status
            StatusDataResponse statusData = await _protocol
                .SendRequestAsync<StatusDataResponse>(_tcpClient.Client, new StatusDataRequest(), cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Status data of {identification} updated successfully: {details}", Identification,
                statusData.ToString());

            _inverterMetricCollection.RecordAmbientTemp(this, statusData.AmbientTemp);
            _inverterMetricCollection.RecordPvVoltage(this, 1, statusData.Pv1Voltage);
            _inverterMetricCollection.RecordPvVoltage(this, 2, statusData.Pv2Voltage);
            _inverterMetricCollection.RecordPvCurrent(this, 1, statusData.Pv1Current);
            _inverterMetricCollection.RecordPvCurrent(this, 2, statusData.Pv2Current);
            _inverterMetricCollection.RecordPvPower(this, 1, statusData.Pv1Power);
            _inverterMetricCollection.RecordPvPower(this, 2, statusData.Pv2Power);
            _inverterMetricCollection.RecordInverterTemp(this, statusData.InverterTemp);
            _inverterMetricCollection.RecordGridVoltage(this, statusData.GridVoltage);
            _inverterMetricCollection.RecordGridFrequency(this, statusData.GridFrequency);
            _inverterMetricCollection.RecordGridCurrent(this, statusData.GridCurrent);
            _inverterMetricCollection.RecordEnergyTotal(this, statusData.EnergyTotal);
            _inverterMetricCollection.RecordTotalOperationHours(this, statusData.TotalOperationHours);
            _inverterMetricCollection.RecordEnergyToday(this, statusData.EnergyToday);
            _inverterMetricCollection.RecordOutputPower(this, statusData.OutputPower);
            _inverterMetricCollection.RecordOperationMode(this, statusData.OperationMode);
        }

        public void Dispose()
        {
            _tcpClient.Dispose();
            _stoppingCts.Dispose();
        }
    }
}
