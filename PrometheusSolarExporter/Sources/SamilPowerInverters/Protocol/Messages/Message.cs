using System;
using System.Buffers.Binary;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages
{
    public abstract class Message
    {
        // Always the same
        protected byte[] Prefix { get; } = { 0x55, 0xaa };

        protected byte[] Body { get; set; } = Array.Empty<byte>();

        protected ushort Checksum => CalculateChecksum(GetBytes(false));

        protected ushort BodyAndChecksumLength => (ushort)(Body.Length + sizeof(ushort));

        public ushort Length => (ushort)(Prefix.Length + sizeof(ushort) + BodyAndChecksumLength);

        protected void SetBytes(ReadOnlySpan<byte> data)
        {
            // Validate prefix
            ReadOnlySpan<byte> prefix = data[..2];
            if (!prefix.SequenceEqual(Prefix))
                throw new ArgumentException($"Message prefix {BitConverter.ToString(prefix.ToArray())} is invalid.",
                    nameof(data));

            // Set body
            Range bodyRange = (Prefix.Length + sizeof(ushort))..^sizeof(ushort);
            Body = data[bodyRange].ToArray();

            // Validate length
            ushort bodyAndChecksumLength = BinaryPrimitives.ReadUInt16BigEndian(data[Prefix.Length..]);
            ushort actualBodyAndChecksumLength = BodyAndChecksumLength;
            if (bodyAndChecksumLength != actualBodyAndChecksumLength)
                throw new ArgumentException(
                    $"Body length {bodyAndChecksumLength} is invalid. Actual length is {actualBodyAndChecksumLength}",
                    nameof(data));

            // Validate checksum
            ushort checksum = BinaryPrimitives.ReadUInt16BigEndian(data[^sizeof(short)..]);
            ushort actualChecksum = Checksum;
            if (checksum != actualChecksum)
                throw new ArgumentException($"Checksum {checksum} is invalid. Actual checksum is {actualChecksum}",
                    nameof(data));
        }

        public ReadOnlySpan<byte> GetBytes(bool includeChecksum = true)
        {
            var data = new Span<byte>(new byte[Length]);

            // Every message starts with the prefix
            Prefix.CopyTo(data);

            // Write count of following bytes (body and checksum)
            BinaryPrimitives.WriteUInt16BigEndian(data[sizeof(short)..], BodyAndChecksumLength);

            // Write body
            Body.CopyTo(data[(2 * sizeof(short))..]);

            // Return early in case the checksum should not be included
            Range dataWithoutChecksum = ..^sizeof(short);
            if (!includeChecksum)
                return data[dataWithoutChecksum];

            // Write checksum of all previous bytes
            ushort checksum = CalculateChecksum(data[dataWithoutChecksum]);
            BinaryPrimitives.WriteUInt16BigEndian(data[^sizeof(short)..], checksum);

            return data;
        }

        private static ushort CalculateChecksum(ReadOnlySpan<byte> messageWithoutChecksum)
        {
            ushort sum = 0;
            for (int i = 0; i < messageWithoutChecksum.Length; i++)
                sum += messageWithoutChecksum[i];
            return sum;
        }
    }
}
