using System;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages
{
    public abstract class ResponseMessage : Message
    {
        // Make the SetBytes method publicly available
        public new virtual void SetBytes(ReadOnlySpan<byte> data) => base.SetBytes(data);

        public abstract override string? ToString();
    }
}
