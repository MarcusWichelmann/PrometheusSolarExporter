namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests
{
    public class StatusDataRequest : RequestMessage
    {
        public StatusDataRequest()
        {
            // No idea what these bytes mean...
            Body = new byte[] {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x02, 0x80, 0x01, 0x03, 0xe8, 0x00, 0x4a
            };
        }
    }
}
