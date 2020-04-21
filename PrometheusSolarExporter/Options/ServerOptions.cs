namespace PrometheusSolarExporter.Options
{
    public class ServerOptions
    {
        public string Hostname { get; set; } = "*";

        public int Port { get; set; } = 80;

        public string Path { get; set; } = "metrics/";
    }
}
