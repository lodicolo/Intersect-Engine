using Intersect.Core;
using Intersect.Network;
using Intersect.Server.Core;

namespace Intersect.Server.Networking;

internal abstract class AbstractServerNetwork : AbstractNetwork, IServer
{
    protected IServerContext Context { get; }

    protected AbstractServerNetwork(
        IServerContext context,
        IApplicationContext applicationContext,
        NetworkConfiguration configuration
    ) : base(applicationContext, configuration)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));

        Id = Guid.NewGuid();
    }

    public abstract bool Listen();

    internal new void AddNetworkLayerInterface(INetworkLayerInterface networkLayerInterface)
    {
        base.AddNetworkLayerInterface(networkLayerInterface);
    }
}