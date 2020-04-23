using Prometheus;
using PrometheusSolarExporter.Sources;

namespace PrometheusSolarExporter
{
    public class InverterMetricCollection
    {
        private const string Prefix = "solar_inverter";

        private const string InverterModelLabel = "model";
        private const string InverterIdentificationLabel = "identification";
        private const string PvNumberLabel = "pv";

        private readonly Gauge _ambientTemp = Metrics.CreateGauge($"{Prefix}_ambient_temp", "Ambient temperature",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _pvVoltage = Metrics.CreateGauge($"{Prefix}_pv_voltage", "PV voltage",
            InverterModelLabel, InverterIdentificationLabel, PvNumberLabel);

        private readonly Gauge _pvCurrent = Metrics.CreateGauge($"{Prefix}_pv_current", "PV current in amperes",
            InverterModelLabel, InverterIdentificationLabel, PvNumberLabel);

        private readonly Gauge _pvPower = Metrics.CreateGauge($"{Prefix}_pv_power", "PV power in watts",
            InverterModelLabel, InverterIdentificationLabel, PvNumberLabel);

        private readonly Gauge _inverterTemp = Metrics.CreateGauge($"{Prefix}_inverter_temp", "Inverter temperature",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _gridVoltage = Metrics.CreateGauge($"{Prefix}_grid_voltage", "Grid voltage",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _gridFrequency = Metrics.CreateGauge($"{Prefix}_grid_frequency", "Grid frequency in Hz",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _gridCurrent = Metrics.CreateGauge($"{Prefix}_grid_current", "Grid current in amperes",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Counter _energyTotal = Metrics.CreateCounter($"{Prefix}_energy_total",
            "Total generated energy in kWh", InverterModelLabel, InverterIdentificationLabel);

        private readonly Counter _totalOperationHours = Metrics.CreateCounter($"{Prefix}_total_operation_hours",
            "Total operation hours", InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _energyToday = Metrics.CreateGauge($"{Prefix}_energy_today",
            "Generated energy today in kWh", InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _outputPower = Metrics.CreateGauge($"{Prefix}_output_power", "Output power in watts",
            InverterModelLabel, InverterIdentificationLabel);

        private readonly Gauge _operationMode = Metrics.CreateGauge($"{Prefix}_operation_mode",
            "Operation mode (0: Wait, 1: Normal, 2: PV Power Off)", InverterModelLabel, InverterIdentificationLabel);

        public void RecordAmbientTemp(IInverter inverter, double temp)
            => _ambientTemp.WithLabels(inverter.Model, inverter.Identification).Set(temp);

        public void RecordPvVoltage(IInverter inverter, int pv, double voltage)
            => _pvVoltage.WithLabels(inverter.Model, inverter.Identification, pv.ToString()).Set(voltage);

        public void RecordPvCurrent(IInverter inverter, int pv, double current)
            => _pvCurrent.WithLabels(inverter.Model, inverter.Identification, pv.ToString()).Set(current);

        public void RecordPvPower(IInverter inverter, int pv, double power)
            => _pvPower.WithLabels(inverter.Model, inverter.Identification, pv.ToString()).Set(power);

        public void RecordInverterTemp(IInverter inverter, double temp)
            => _inverterTemp.WithLabels(inverter.Model, inverter.Identification).Set(temp);

        public void RecordGridVoltage(IInverter inverter, double voltage)
            => _gridVoltage.WithLabels(inverter.Model, inverter.Identification).Set(voltage);

        public void RecordGridFrequency(IInverter inverter, double frequency)
            => _gridFrequency.WithLabels(inverter.Model, inverter.Identification).Set(frequency);

        public void RecordGridCurrent(IInverter inverter, double current)
            => _gridCurrent.WithLabels(inverter.Model, inverter.Identification).Set(current);

        public void RecordEnergyTotal(IInverter inverter, double energy)
            => _energyTotal.WithLabels(inverter.Model, inverter.Identification).IncTo(energy);

        public void RecordTotalOperationHours(IInverter inverter, double hours)
            => _totalOperationHours.WithLabels(inverter.Model, inverter.Identification).IncTo(hours);

        public void RecordEnergyToday(IInverter inverter, double energy)
            => _energyToday.WithLabels(inverter.Model, inverter.Identification).Set(energy);

        public void RecordOutputPower(IInverter inverter, double power)
            => _outputPower.WithLabels(inverter.Model, inverter.Identification).Set(power);

        public void RecordOperationMode(IInverter inverter, OperationMode operationMode)
            => _operationMode.WithLabels(inverter.Model, inverter.Identification).Set((int)operationMode);
    }
}
