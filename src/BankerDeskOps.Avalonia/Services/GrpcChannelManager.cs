using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.Services;

/// <summary>
/// Manages the lifecycle of a single shared gRPC channel.
/// Uses <see cref="HttpClientHandler.DangerousAcceptAnyServerCertificateValidator"/>
/// for development convenience; replace with proper cert validation in production.
/// </summary>
public sealed class GrpcChannelManager : IDisposable
{
    private readonly string _address;
    private readonly ILogger<GrpcChannelManager> _logger;
    private GrpcChannel? _channel;

    public GrpcChannelManager(string address, ILogger<GrpcChannelManager> logger)
    {
        _address = address ?? throw new ArgumentNullException(nameof(address));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets or lazily creates the gRPC channel.</summary>
    public GrpcChannel Channel
    {
        get
        {
            if (_channel is null)
            {
                _logger.LogInformation("Creating gRPC channel → {Address}", _address);

#if DEBUG
                // Accept self-signed dev-certs on macOS / Windows without Keychain trust
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                _channel = GrpcChannel.ForAddress(_address, new GrpcChannelOptions
                {
                    HttpHandler = handler
                });
#else
                _channel = GrpcChannel.ForAddress(_address);
#endif
            }

            return _channel;
        }
    }

    public async Task ShutdownAsync()
    {
        if (_channel is not null)
            await _channel.ShutdownAsync();
    }

    public void Dispose()
    {
        ShutdownAsync().GetAwaiter().GetResult();
        _channel?.Dispose();
    }
}
