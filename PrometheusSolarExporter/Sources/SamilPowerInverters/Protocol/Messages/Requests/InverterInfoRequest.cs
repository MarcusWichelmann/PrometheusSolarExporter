namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests
{
    public class InverterInfoRequest : RequestMessage
    {
        public InverterInfoRequest()
        {
            // No idea what these bytes mean...
            Body = new byte[] {
                0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x80, 0x01, 0x00, 0x00, 0x00, 0x7a
            };
        }
    }
}
