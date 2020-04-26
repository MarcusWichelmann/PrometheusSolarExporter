using System;
using System.Buffers.Binary;
using System.Text;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Responses
{
    public class StatusDataResponse : ResponseMessage
    {
        public double AmbientTemp { get; private set; } // Degrees

        public double Pv1Voltage { get; private set; } // Volts

        public double Pv2Voltage { get; private set; } // Volts

        public double Pv1Current { get; private set; } // Amperes

        public double Pv2Current { get; private set; } // Amperes

        public double Pv1Power => Pv1Voltage * Pv1Current; // VA

        public double Pv2Power => Pv2Voltage * Pv2Current; // VA

        public double InverterTemp { get; private set; } // Degrees

        public double GridVoltage { get; private set; } // Volts

        public double GridFrequency { get; private set; } // Hz

        public double GridCurrent { get; private set; } // Amperes

        public double EnergyTotal { get; private set; } // kWh

        public uint TotalOperationHours { get; private set; } // Hours

        public double EnergyToday { get; private set; } // kWh

        public double OutputPower { get; private set; } // Watts

        public OperationMode OperationMode { get; private set; }

        public override void SetBytes(ReadOnlySpan<byte> data)
        {
            base.SetBytes(data);

            ReadOnlySpan<byte> content = Body[15..];

            // Based on https://github.com/semonet/solar/blob/master/samil.py#L122
            AmbientTemp = BinaryPrimitives.ReadUInt16BigEndian(content[..2]) / 10.0;
            Pv1Voltage = BinaryPrimitives.ReadUInt16BigEndian(content[2..4]) / 10.0;
            Pv2Voltage = BinaryPrimitives.ReadUInt16BigEndian(content[4..6]) / 10.0;
            Pv1Current = BinaryPrimitives.ReadUInt16BigEndian(content[6..8]) / 10.0;
            Pv2Current = BinaryPrimitives.ReadUInt16BigEndian(content[8..10]) / 10.0;
            InverterTemp = BinaryPrimitives.ReadUInt16BigEndian(content[14..16]) / 10.0;
            GridVoltage = BinaryPrimitives.ReadUInt16BigEndian(content[18..20]) / 10.0;
            GridFrequency = BinaryPrimitives.ReadUInt16BigEndian(content[20..22]) / 100.0;
            GridCurrent = BinaryPrimitives.ReadUInt16BigEndian(content[22..24]) / 10.0;
            EnergyTotal = BinaryPrimitives.ReadUInt32BigEndian(content[34..38]) / 100.0;
            TotalOperationHours = BinaryPrimitives.ReadUInt32BigEndian(content[38..42]);
            EnergyToday = BinaryPrimitives.ReadUInt16BigEndian(content[42..44]) / 100.0;
            OutputPower = BinaryPrimitives.ReadUInt32BigEndian(content[44..48]);
            OperationMode = (OperationMode)BinaryPrimitives.ReadUInt16BigEndian(content[48..50]);
        }

        public override string? ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"Ambient Temp: {AmbientTemp}, ");
            stringBuilder.Append($"Pv1 Voltage: {Pv1Voltage}, ");
            stringBuilder.Append($"Pv2 Voltage: {Pv2Voltage}, ");
            stringBuilder.Append($"Pv1 Current: {Pv1Current}, ");
            stringBuilder.Append($"Pv2 Current: {Pv2Current}, ");
            stringBuilder.Append($"Inverter Temp: {InverterTemp}, ");
            stringBuilder.Append($"Grid Voltage: {GridVoltage}, ");
            stringBuilder.Append($"Grid Frequency: {GridFrequency}, ");
            stringBuilder.Append($"Grid Current: {GridCurrent}, ");
            stringBuilder.Append($"Energy Total: {EnergyTotal}, ");
            stringBuilder.Append($"Total Operation Hours: {TotalOperationHours}, ");
            stringBuilder.Append($"Energy Today: {EnergyToday}, ");
            stringBuilder.Append($"Output Power: {OutputPower}, ");
            stringBuilder.Append($"Operation Mode: {OperationMode}");
            return stringBuilder.ToString();
        }
    }
}
