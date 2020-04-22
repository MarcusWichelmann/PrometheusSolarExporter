using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages;
using PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol.Messages.Requests;

namespace PrometheusSolarExporter.Sources.SamilPowerInverters.Protocol
{
    public class SamilPowerProtocol : IDisposable
    {
        private const int AdvertisementBroadcastPort = 60000;

        private readonly ILogger<SamilPowerProtocol> _logger;

        private readonly UdpClient _udpBroadcastClient = new UdpClient();

        public SamilPowerProtocol(ILogger<SamilPowerProtocol> logger)
        {
            _logger = logger;
        }

        public Task BroadcastAdvertisementAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] advertisement = new Advertisement().GetBytes().ToArray();

            _logger.LogDebug("Sending advertisement broadcast on port {port}: {advertisement}",
                AdvertisementBroadcastPort, BitConverter.ToString(advertisement));

            return _udpBroadcastClient.SendAsync(advertisement, advertisement.Length,
                new IPEndPoint(IPAddress.Broadcast, AdvertisementBroadcastPort));
        }

        public async Task<TResponse> SendRequestAsync<TResponse>(Socket clientSocket, RequestMessage requestMessage,
            CancellationToken cancellationToken = default) where TResponse : ResponseMessage, new()
        {
            byte[] requestBytes = requestMessage.GetBytes().ToArray();

            // Send request
            _logger.LogDebug("Sending {requestType} to {remoteEndpoint}: {requestBytes}", requestMessage.GetType().Name,
                clientSocket.RemoteEndPoint, BitConverter.ToString(requestBytes));
            await clientSocket.SendAsync(requestBytes, SocketFlags.None, cancellationToken).ConfigureAwait(false);

            // Wait for response
            var responseBytes = new byte[1024];
            int responseLength;
            do
            {
                responseLength = await clientSocket.ReceiveAsync(responseBytes, SocketFlags.None, cancellationToken)
                    .ConfigureAwait(false);
            }
            while (responseLength == 0);

            responseBytes = responseBytes[..responseLength];

            _logger.LogDebug("Received {responseLength} bytes from {remoteEndpoint}: {responseBytes}", responseLength,
                clientSocket.RemoteEndPoint, BitConverter.ToString(responseBytes));

            // Create response message object
            object? responseInstance = Activator.CreateInstance(typeof(TResponse));
            Debug.Assert(responseInstance != null, nameof(responseInstance) + " != null");

            var responseMessage = (TResponse)responseInstance;
            responseMessage.SetBytes(responseBytes);
            return responseMessage;
        }

        public void Dispose()
        {
            _udpBroadcastClient.Dispose();
        }
    }
}
