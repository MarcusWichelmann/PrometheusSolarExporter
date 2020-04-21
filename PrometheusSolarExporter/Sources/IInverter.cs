namespace PrometheusSolarExporter.Sources
{
    public interface IInverter
    {
        string Model { get; }

        string Identification { get; }
    }
}
