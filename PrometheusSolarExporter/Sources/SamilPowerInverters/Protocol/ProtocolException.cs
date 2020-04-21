using System;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol
{
    public class ProtocolException : Exception
    {
        public ProtocolException() { }
        public ProtocolException(string? message) : base(message) { }
        public ProtocolException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
