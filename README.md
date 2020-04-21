# Prometheus Solar Exporter

![Build](https://github.com/MarcusWichelmann/PrometheusSolarExporter/workflows/Build/badge.svg)

This small tool polls a solar inverter, queries its metrics and exports them in a prometheus-compatible format. I developed this for recording and visualizing statistics of my PV setup using [Grafana](https://grafana.com/).

## Device support

The codebase is structured a way that makes it easy to add support for solar inverters from other vendors. Currently supported inverters (all I have access to) are:

- **Samil Power**
    - SolarLake TL-PM series
        - Based on some reverse-engineering fun and [this great work by *semonet*](https://github.com/semonet/solar)
        - Tested with **SolarLake 8500TL-PM**
    - Support for other Samil Power inverters like SolarRiver TD, TL-D should be [fairly easy to add](https://github.com/mhvis/solar), but has not happened yet, due to me not having access to any such device.
 - **Other vendors**
    - Pull requests welcome!

## How to run

### Docker

There are multiple [Docker images](https://github.com/MarcusWichelmann/PrometheusSolarExporter/packages) attached to this repository for Linux on AMD64, ARM32 and ARM64 architectures, for the case that you want to run this on a Raspberry Pi, like me:
- `docker.pkg.github.com/marcuswichelmann/prometheussolarexporter/prometheus-solar-exporter:latest-amd64`
- `docker.pkg.github.com/marcuswichelmann/prometheussolarexporter/prometheus-solar-exporter:latest-arm32v7`
- `docker.pkg.github.com/marcuswichelmann/prometheussolarexporter/prometheus-solar-exporter:latest-arm64v8`

The `:latest` tag is broken right now, please don't use it, see [docker/cli#2396](https://github.com/docker/cli/issues/2396).

Because of the protocol being based on IPv4 UDP broadcasts, you need to start the container with the `host` network mode:

```
docker run -it --rm --name solar-exporter --net host docker.pkg.github.com/marcuswichelmann/prometheussolarexporter/prometheus-solar-exporter:latest-amd64 --port 8080
```

Much like the `--port` option, these arguments are supported:
- `--host`: The hostname/address the server should listen on (default: `*`)
- `--port`: The port of the HTTP server (default: `80`)
- `--path`: URL, where the metrics can be found (default: `metrics/`, results in `http://host:port/metrics/`)
- `--log-level`: Log level like `trace`, `debug`, `info`, ... (default: `info`)

Finer grained configuration and environment variables are supported. Please consult the official documentation regarding the configuration pattern: https://docs.microsoft.com/de-de/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1

### Manually

`dotnet run` should do the job.

## Contributing

Contributions like fixes or support for additional inverters are always welcome. For other vendors, just add a new source implementation to the `PrometheusSolarExporter/Sources/` directory and initialize it in `Program.cs`.

In case you have access to another, not yet supported, Samil Power inverter, feel free to enhance the existing `SamilPowerInverters` source and add some switches for the slightly different message formats.

#### Have fun!
