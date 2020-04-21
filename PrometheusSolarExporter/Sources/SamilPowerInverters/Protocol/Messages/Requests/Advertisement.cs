namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests
{
    public class Advertisement : RequestMessage
    {
        public Advertisement()
        {
            // No idea what these bytes mean...
            Body = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00 };
        }
    }
}
