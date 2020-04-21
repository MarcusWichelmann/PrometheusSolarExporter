namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Responses
{
    public enum DeviceType : byte
    {
        // Taken from https://github.com/mhvis/solar/wiki/Communication-protocol#messages
        SinglePhaseInverter = (byte)'1',
        ThreePhaseInverter = (byte)'2',
        SolarEnviMonitor = (byte)'3'
    }

    public static class DeviceTypeExtensions
    {
        public static string ToFriendlyString(this DeviceType deviceType)
            => deviceType switch {
                DeviceType.SinglePhaseInverter => "Single-phase Inverter",
                DeviceType.ThreePhaseInverter  => "Three-phase Inverter",
                DeviceType.SolarEnviMonitor    => "SolarEnvi Monitor",
                var unknown                    => $"Unknown: {(byte)unknown}"
            };
    }
}
