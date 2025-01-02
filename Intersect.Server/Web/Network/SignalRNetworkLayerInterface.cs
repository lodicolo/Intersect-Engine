using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using Intersect.Framework.Reflection;
using Intersect.Memory;
using Intersect.Network;
using Intersect.Server.Core;
using Intersect.Server.CustomChange;
using Intersect.Server.Networking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Intersect.Server.Web.Network;

public enum SignalRConnectionState
{
    Pending,
    Anonymous,
    Authenticated,
}

public sealed class SignalRConnection : AbstractConnection
{
    private readonly Hub _hub;

    public SignalRConnection(Hub hub, string connectionId, IPEndPoint remoteEndPoint,
        SignalRConnectionState initialState)
    {
        _hub = hub;

        ConnectionId = connectionId;
        RemoteEndPoint = remoteEndPoint;
        State = initialState;
    }

    private IClientProxy? ClientProxy => _hub.Clients.Client(ConnectionId);

    public string ConnectionId { get; }

    public IPEndPoint RemoteEndPoint { get; }

    public SignalRConnectionState State { get; internal set; }

    public override string Ip => RemoteEndPoint.Address.ToString();

    public override int Port => RemoteEndPoint.Port;

    public override bool Send(IPacket packet, TransmissionMode mode = TransmissionMode.All)
    {
        throw new NotImplementedException();
    }

    public override void Disconnect(string message = default)
    {
        throw new NotImplementedException();
    }
}

public sealed record SignalRHail(byte[] VersionData);

public sealed record SignalRHailResponse(bool Approved, string? Message, Guid Guid);

internal sealed class SignalRNetworkLayerInterface : INetworkLayerInterface
{
    internal sealed class InterfaceHub : Hub
    {
        private static ulong _nextId = 0;

        private readonly ulong _id = ++_nextId;
        private readonly ILogger<InterfaceHub> _logger;
        private readonly SignalRNetworkLayerInterface _networkLayerInterface;

        public InterfaceHub(SignalRNetworkLayerInterface networkLayerInterface, ServerContext serverContext,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<InterfaceHub>();
            if (serverContext.Network is not AbstractServerNetwork serverNetwork)
            {
                throw new InvalidOperationException();
            }

            _networkLayerInterface = networkLayerInterface;
        }

        private HttpContext HttpContext => Context.GetHttpContext() ?? throw new InvalidOperationException();

        private IPEndPoint RemoteEndPoint
        {
            get
            {
                var httpContext = HttpContext;
                return new IPEndPoint(
                    httpContext.Connection.RemoteIpAddress ?? IPAddress.None,
                    httpContext.Connection.RemotePort
                );
            }
        }

        public async Task<SignalRHailResponse> Hail(SignalRHail hail)
        {
            var httpContext = HttpContext;
            var remoteEndPoint = RemoteEndPoint;

            _logger.LogWarning(
                "Received hail on hub {HubId} from connection {ConnectionId}",
                _id,
                httpContext.Connection.Id
            );

            if (!_networkLayerInterface._pendingConnections.TryGetValue(
                    Context.ConnectionId,
                    out var pendingConnection
                ))
            {
                _logger.LogDebug("Rejecting (no pending connection) {RemoteEndPoint}", remoteEndPoint);
                httpContext.Abort();
                return default;
            }

            Debug.Assert(SharedConstants.VersionData != null, "SharedConstants.VERSION_DATA != null");
            Debug.Assert(hail.VersionData != null, "hail.VersionData != null");
            if (!SharedConstants.VersionData.SequenceEqual(hail.VersionData))
            {
                _logger.LogDebug("Rejecting (bad version) {RemoteEndPoint}", remoteEndPoint);
                _ = Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => httpContext.Abort());
                return new SignalRHailResponse(false, NetworkStatus.VersionMismatch.ToString(), default);
            }

            if (_networkLayerInterface._network.ConnectionCount >= Options.Instance.MaxClientConnections)
            {
                _logger.LogDebug("Rejecting (full) {RemoteEndPoint}", remoteEndPoint);
                httpContext.Abort();
                return new SignalRHailResponse(false, NetworkStatus.ServerFull.ToString(), default);
            }

            pendingConnection.State = SignalRConnectionState.Anonymous;

            return new SignalRHailResponse(
                true,
                default,
                pendingConnection.Guid
            );

            // if (_network.ConnectionCount >= Options.Instance.MaxClientConnections)
            // {
            //     Log.Debug($"Rejecting (full) {request.RemoteEndPoint}");
            //     response.Put((ushort)503);
            //     response.Put(NetworkStatus.ServerFull.ToString());
            //     request.Reject(response);
            //     return;
            // }
            //
            // _logger.LogDebug("CONN-SIGNALR {RemoteEndPoint}", remoteEndPoint);
            //
            //
            // if (peer.Tag is not Guid guid)
            // {
            //     Log.Diagnostic("Peer tag is not a guid");
            //     return;
            // }
            //
            // if (!_connectionIdLookup.TryAdd(peer.Id, guid))
            // {
            //     Log.Diagnostic($"Failed to add connection ID {guid} for peer {peer.Id}");
            //     return;
            // }
            //
            // OnConnected?.Invoke(this, new ConnectionEventArgs
            // {
            //     Connection = _network.FindConnection(guid),
            //     NetworkStatus = NetworkStatus.Online,
            // });

            var reconnectionToken = httpContext.Request.Query["access_token"].ToString();

            // anounymous player, we accept the connection, but we don't do anything with it
            if (string.IsNullOrWhiteSpace(reconnectionToken))
            {
                return default;
            }

            // base.Clients.Client("").SendAsync()
            return default;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == default)
            {
                _logger.LogWarning($"No {nameof(HttpContext)} for SignalR context, aborting");
                Context.Abort();
                return;
            }

            var connectionId = Context.ConnectionId;
            IPEndPoint remoteEndPoint = new(
                httpContext.Connection.RemoteIpAddress ?? IPAddress.None,
                httpContext.Connection.RemotePort
            );

            var rawAccessToken = httpContext.Request.Query["access_token"].ToString();
            if (rawAccessToken.Length < 1)
            {
                _logger.LogDebug("Missing access token from {ConnectionId}", connectionId);
                Context.Abort();
                return;
            }

            if (rawAccessToken.Contains(' '))
            {
                var parts = rawAccessToken.Split(' ', 2);
                var accessTokenType = parts.FirstOrDefault();
                if (string.Equals(
                        "Hail",
                        accessTokenType,
                        StringComparison.OrdinalIgnoreCase
                    ))
                {
                    Debug.Assert(SharedConstants.VersionData != null, "SharedConstants.VERSION_DATA != null");
                    var hailVersionText = parts.Skip(1).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(hailVersionText))
                    {
                        _logger.LogDebug("Rejecting (bad version) {RemoteEndPoint}", remoteEndPoint);
                        await Clients.Client(connectionId).SendAsync(
                            "NetworkStatus",
                            NetworkStatus.VersionMismatch.ToString()
                        );
                        Context.Abort();
                        return;
                    }

                    var hailVersionData = Encoding.UTF8.GetBytes(hailVersionText);
                    if (!SharedConstants.VersionData.SequenceEqual(hailVersionData))
                    {
                        _logger.LogDebug("Rejecting (bad version) {RemoteEndPoint}", remoteEndPoint);
                        await Clients.Client(connectionId).SendAsync(
                            "NetworkStatus",
                            NetworkStatus.VersionMismatch.ToString()
                        );
                        Context.Abort();
                        return;
                    }
                }
                else
                {
                    _logger.LogDebug(
                        "Invalid access token type {AccessTokenType} from {ConnectionId}",
                        accessTokenType,
                        connectionId
                    );
                    await Clients.Client(connectionId).SendAsync(
                        "NetworkStatus",
                        NetworkStatus.HandshakeFailure.ToString()
                    );
                    Context.Abort();
                    return;
                }
            }
            else
            {
                await Clients.Client(connectionId).SendAsync(
                    "NetworkStatus",
                    NetworkStatus.Unknown.ToString()
                );
                throw new NotImplementedException();
            }

            if (_networkLayerInterface._network.ConnectionCount >= Options.Instance.MaxClientConnections)
            {
                _logger.LogDebug("Rejecting (full) {RemoteEndPoint}", remoteEndPoint);
                await Clients.Client(connectionId).SendAsync(
                    "NetworkStatus",
                    NetworkStatus.ServerFull.ToString()
                );
                httpContext.Abort();
                return;
            }

            SignalRConnection connection = new(
                this,
                connectionId,
                remoteEndPoint,
                SignalRConnectionState.Pending
            );

            if (!_networkLayerInterface._network.AddConnection(connection))
            {
                _logger.LogError(
                    "Failed to add connection {ConnectionId}",
                    connectionId
                );
                Context.Abort();
                return;
            }

            _logger.LogWarning(
                "Added connection to hub {HubId}: {ConnectionId} ({Connections})",
                _id,
                connectionId,
                _networkLayerInterface._network.ConnectionCount
            );

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = RoomHandler.FindPlayerByConnectionId(Context.ConnectionId);
            if (player != default)
            {
                RoomHandler.AddDisconnectedPlayer(player);
            }

            await base.OnDisconnectedAsync(exception);

            var connectionId = Context.ConnectionId;
            if (_networkLayerInterface._pendingConnections.Remove(connectionId, out _))
            {
                _logger.LogWarning(
                    "Removed pending connection {ConnectionId} ({Connections})",
                    connectionId,
                    _networkLayerInterface._pendingConnections.Count
                );
            }
        }
    }

    public event HandleConnectionEvent OnConnected;
    public event HandleConnectionEvent OnConnectionApproved;
    public event HandleConnectionEvent OnConnectionDenied;
    public event HandleConnectionRequest OnConnectionRequested;
    public event HandleConnectionEvent OnDisconnected;
    public event HandlePacketAvailable OnPacketAvailable;
    public event HandleUnconnectedMessage OnUnconnectedMessage;

    private readonly ConcurrentDictionary<string, Guid> _connectionIdLookup = [];
    private readonly ConcurrentDictionary<string, SignalRConnection> _pendingConnections = [];

    private readonly ILogger<SignalRNetworkLayerInterface> _logger;
    private readonly INetwork _network;

    public SignalRNetworkLayerInterface(ServerContext serverContext,
        ILoggerFactory loggerFactory, INetwork network)
    {
        _network = network;
        _logger = loggerFactory.CreateLogger<SignalRNetworkLayerInterface>();
        if (serverContext.Network is not AbstractServerNetwork serverNetwork)
        {
            throw new InvalidOperationException();
        }

        serverNetwork.AddNetworkLayerInterface(this);
    }

    public bool TryGetInboundBuffer(out IBuffer buffer, out IConnection connection)
    {
        throw new NotImplementedException();
    }

    public void ReleaseInboundBuffer(IBuffer buffer)
    {
        // Nothing to do
    }

    public bool SendPacket(IPacket packet, IConnection connection = null,
        TransmissionMode transmissionMode = TransmissionMode.All)
    {
        if (connection == default)
        {
            _logger.LogDebug("No active connection, ignoring packet");
            return false;
        }

        if (connection is not SignalRConnection signalRConnection)
        {
            _logger.LogWarning(
                $"{nameof(SignalRNetworkLayerInterface)}.{nameof(SendPacket)}({nameof(IPacket)}, {nameof(IConnection)}, {nameof(TransmissionMode)}) called with invalid connection type {{ConnectionTypeName}}",
                connection.GetQualifiedTypeName()
            );
            return false;
        }

        return signalRConnection.Send(packet, transmissionMode);
    }

    public bool SendPacket(IPacket packet, ICollection<IConnection> connections,
        TransmissionMode transmissionMode = TransmissionMode.All)
    {
        return connections.OfType<SignalRConnection>().Aggregate(
            true,
            (current, connection) => current & connection.Send(packet, transmissionMode)
        );
    }

    public bool SendUnconnectedPacket(IPEndPoint target, ReadOnlySpan<byte> data)
    {
        throw new NotSupportedException();
    }

    public bool SendUnconnectedPacket(IPEndPoint target, UnconnectedPacket packet)
    {
        throw new NotSupportedException();
    }

    public void Start()
    {
        // Does nothing
    }

    public void Stop(string reason = "stopping")
    {
        // TODO: Is this necessary? How would we go about removing SignalR from a running app?
    }

    public bool Connect()
    {
        throw new NotImplementedException();
    }

    public void Disconnect(IConnection connection, string message)
    {
        if (connection is not SignalRConnection signalRConnection)
        {
            _logger.LogWarning(
                $"{nameof(SignalRNetworkLayerInterface)}.{nameof(Disconnect)}() called with invalid connection type {{ConnectionTypeName}}",
                connection.GetQualifiedTypeName()
            );
            return;
        }

        signalRConnection.Disconnect(message);
    }

    public void Disconnect(ICollection<IConnection> connections, string message)
    {
        foreach (var connection in connections)
        {
            if (connection is not SignalRConnection)
            {
                continue;
            }

            Disconnect(connection, message);
        }
    }

    public void Dispose()
    {
    }
}