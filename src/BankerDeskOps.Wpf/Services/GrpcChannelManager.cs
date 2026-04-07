using System.Net;
using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    /// <summary>
    /// gRPC channel manager for API communication.
    /// </summary>
    public class GrpcChannelManager : IDisposable
    {
        private readonly string _address;
        private readonly ILogger<GrpcChannelManager> _logger;
        private GrpcChannel? _channel;

        public GrpcChannelManager(string address, ILogger<GrpcChannelManager> logger)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or creates the gRPC channel.
        /// </summary>
        public GrpcChannel Channel
        {
            get
            {
                if (_channel == null)
                {
                    _logger.LogInformation("Creating new gRPC channel to {Address}", _address);
                    
                    // For local development, allow unencrypted HTTP/2 and suppress certificate validation
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    
                    _channel = GrpcChannel.ForAddress(_address, new GrpcChannelOptions
                    {
                        HttpHandler = handler
                    });
                }
                return _channel;
            }
        }

        /// <summary>
        /// Closes the gRPC channel.
        /// </summary>
        public async Task CloseAsync()
        {
            if (_channel != null)
            {
                await _channel.ShutdownAsync();
            }
        }

        /// <summary>
        /// Disposes the gRPC channel.
        /// </summary>
        public void Dispose()
        {
            CloseAsync().Wait();
        }
    }
}
