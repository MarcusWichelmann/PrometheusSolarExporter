using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Responses
{
    public class InverterInfoResponse : ResponseMessage
    {
        public DeviceType DeviceType { get; private set; }

        public int WattageRating { get; private set; }

        public string? Model { get; private set; }

        public string? Vendor { get; private set; }

        public string? SerialNumber { get; private set; }

        public string ModelDescription => $"{Vendor} {Model} ({WattageRating}W {DeviceType.ToFriendlyString()})";

        public override void SetBytes(ReadOnlySpan<byte> data)
        {
            base.SetBytes(data);

            string ReadString(ReadOnlySpan<byte> data) => Utils.ReadNullTerminatedString(data).Trim(' ');

            DeviceType = (DeviceType)(byte)ReadString(Body[16..19])[0];

            string modelType = ReadString(Body[19..28]);
            WattageRating = int.Parse(modelType[..^2]);
            Model = ResolveModelAbbreviation(modelType[^2..]) + " " + ReadString(Body[28..55]);

            Vendor = ReadString(Body[55..87]);
            SerialNumber = ReadString(Body[87..]);

            // No idea which version number is which. Skipped for now.
        }

        public override string? ToString() => $"{ModelDescription}, SN: {SerialNumber}";

        private static string ResolveModelAbbreviation(string abb)
            => abb switch {
                "SL" => "SolarLake",
                _    => abb
            };
    }
}
