using System;
using System.Text;

namespace PrometheusSolarExporter
{
    public static class Utils
    {
        public static string ReadNullTerminatedString(ReadOnlySpan<byte> data)
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == 0)
                    break;
                stringBuilder.Append((char)b);
            }

            return stringBuilder.ToString();
        }
    }
}
