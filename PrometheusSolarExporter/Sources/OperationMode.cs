namespace PrometheusSolarExporter.Sources
{
    public enum OperationMode : byte
    {
        Wait = 0,
        Normal = 1,
        PvPowerOff = 2
    }

    public static class OperationModeExtensions
    {
        public static string ToFriendlyString(this OperationMode operationMode)
            => operationMode switch {
                OperationMode.Wait       => "Wait",
                OperationMode.Normal     => "Normal",
                OperationMode.PvPowerOff => "PV Power Off",
                var unknown              => $"Unknown: {(byte)unknown}"
            };
    }
}
