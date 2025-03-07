using Intersect.Core;
using Intersect.Editor.Content;
using Intersect.Editor.Core;
using Intersect.Editor.General;
using Intersect.Editor.Localization;
using Intersect.Editor.Maps;
using Intersect.Enums;
using Intersect.Framework.Core.GameObjects.Animations;
using Intersect.Framework.Core.GameObjects.Crafting;
using Intersect.Framework.Core.GameObjects.Events;
using Intersect.Framework.Core.GameObjects.Items;
using Intersect.Framework.Core.GameObjects.Mapping.Tilesets;
using Intersect.Framework.Core.GameObjects.Maps;
using Intersect.Framework.Core.GameObjects.Maps.MapList;
using Intersect.Framework.Core.GameObjects.NPCs;
using Intersect.Framework.Core.GameObjects.PlayerClass;
using Intersect.Framework.Core.GameObjects.Resources;
using Intersect.Framework.Core.GameObjects.Variables;
using Intersect.GameObjects;
using Intersect.Network;
using Intersect.Network.Packets.Server;
using Microsoft.Extensions.Logging;
using ApplicationContext = Intersect.Core.ApplicationContext;

namespace Intersect.Editor.Networking;

internal sealed partial class PacketHandler
{
    private sealed partial class VirtualPacketSender : IPacketSender
    {
        public IApplicationContext ApplicationContext { get; }

        public INetwork Network => Networking.Network.EditorLidgrenNetwork;

        public VirtualPacketSender(IApplicationContext applicationContext) =>
            ApplicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));

        #region Implementation of IPacketSender

        /// <inheritdoc />
        public bool Send(IPacket packet)
        {
            if (packet is IntersectPacket intersectPacket)
            {
                Networking.Network.SendPacket(intersectPacket);

                return true;
            }

            return false;
        }

        #endregion
    }

    public delegate void GameObjectUpdated(GameObjectType type);

    public delegate void MapUpdated();

    public static GameObjectUpdated GameObjectUpdatedDelegate;

    public static MapUpdated MapUpdatedDelegate;

    public IApplicationContext Context { get; }

    public ILogger Logger { get; }

    public PacketHandlerRegistry Registry { get; }

    public IPacketSender VirtualSender { get; }

    public PacketHandler(IApplicationContext context, PacketHandlerRegistry packetHandlerRegistry)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Registry = packetHandlerRegistry ?? throw new ArgumentNullException(nameof(packetHandlerRegistry));

        if (!Registry.TryRegisterAvailableMethodHandlers(GetType(), this, false) || Registry.IsEmpty)
        {
            throw new InvalidOperationException("Failed to register method handlers, see logs for more details.");
        }

        VirtualSender = new VirtualPacketSender(context);
    }

    public bool HandlePacket(IConnection connection, IPacket packet)
    {
        if (!(packet is IntersectPacket))
        {
            return false;
        }

        if (!Registry.TryGetHandler(packet, out HandlePacketGeneric handler))
        {
            Logger.LogError($"No registered handler for {packet.GetType().FullName}!");

            return false;
        }

        if (Registry.TryGetPreprocessors(packet, out var preprocessors))
        {
            if (!preprocessors.All(preprocessor => preprocessor.Handle(VirtualSender, packet)))
            {
                // Preprocessors are intended to be silent filter functions
                return false;
            }
        }

        if (Registry.TryGetPreHooks(packet, out var preHooks))
        {
            if (!preHooks.All(hook => hook.Handle(VirtualSender, packet)))
            {
                // Hooks should not fail, if they do that's an error
                Logger.LogError($"PreHook handler failed for {packet.GetType().FullName}.");
                return false;
            }
        }

        if (!handler(VirtualSender, packet))
        {
            return false;
        }

        if (Registry.TryGetPostHooks(packet, out var postHooks))
        {
            if (!postHooks.All(hook => hook.Handle(VirtualSender, packet)))
            {
                // Hooks should not fail, if they do that's an error
                Logger.LogError($"PostHook handler failed for {packet.GetType().FullName}.");
                return false;
            }
        }

        return true;
    }

    //PingPacket
    public void HandlePacket(IPacketSender packetSender, PingPacket packet)
    {
        if (packet.RequestingReply)
        {
            PacketSender.SendPing();
        }
    }

    //ConfigPacket
    public void HandlePacket(IPacketSender packetSender, ConfigPacket packet)
    {
        Options.LoadFromServer(packet.Config);
        try
        {
            Strings.Load();
        } catch (Exception exception)
        {
            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to load server options");
            throw;
        }
    }

    //JoinGamePacket
    public void HandlePacket(IPacketSender packetSender, JoinGamePacket packet)
    {
        Globals.LoginForm.TryRemembering();
        Globals.LoginForm.HideSafe();
    }

    //MapPacket
    public void HandlePacket(IPacketSender packetSender, MapPacket packet)
    {
        if (packet.Deleted)
        {
            if (!MapInstance.TryGet(packet.MapId, out var existingMapToDelete))
            {
                return;
            }

            if (Globals.CurrentMap == existingMapToDelete)
            {
                Globals.MainForm.EnterMap(MapList.List.FindFirstMap());
            }

            existingMapToDelete.Delete();
            return;
        }

        var receivedMap = new MapInstance(packet.MapId);
        receivedMap.Load(packet.Data);
        receivedMap.LoadTileData(packet.TileData);
        receivedMap.AttributeData = packet.AttributeData;
        receivedMap.MapGridX = packet.GridX;
        receivedMap.MapGridY = packet.GridY;
        receivedMap.SaveStateAsUnchanged();
        receivedMap.InitAutotiles();
        receivedMap.UpdateAdjacentAutotiles();
        
        if (MapInstance.TryGet(packet.MapId, out var existingMap))
        {
            lock (existingMap.MapLock)
            {
                if (Globals.CurrentMap == existingMap)
                {
                    Globals.CurrentMap = receivedMap;
                }

                existingMap.Delete();
            }
        }

        MapInstance.Lookup.Set(packet.MapId, receivedMap);
        if (!Globals.InEditor && Globals.HasGameData)
        {
            Globals.CurrentMap = receivedMap;
            Globals.LoginForm.BeginInvoke(Globals.LoginForm.EditorLoopDelegate);
        }
        else if (Globals.InEditor)
        {
            if (Globals.FetchingMapPreviews || Globals.CurrentMap == receivedMap)
            {
                var currentMapId = Globals.CurrentMap.Id;
                var img = Database.LoadMapCacheLegacy(packet.MapId, receivedMap.Revision);
                if (img == null && !Globals.MapsToScreenshot.Contains(packet.MapId))
                {
                    Globals.MapsToScreenshot.Add(packet.MapId);
                }

                img?.Dispose();
                if (Globals.FetchingMapPreviews)
                {
                    if (Globals.MapsToFetch.Contains(packet.MapId))
                    {
                        Globals.MapsToFetch.Remove(packet.MapId);
                        if (Globals.MapsToFetch.Count == 0)
                        {
                            Globals.FetchingMapPreviews = false;
                            Globals.PreviewProgressForm.BeginInvoke(
                                (MethodInvoker) delegate { Globals.PreviewProgressForm.Dispose(); }
                            );
                        }
                        else
                        {
                            //TODO Localize
                            Globals.PreviewProgressForm.SetProgress(
                                "Fetching Maps: " +
                                (Globals.FetchCount - Globals.MapsToFetch.Count) +
                                "/" +
                                Globals.FetchCount,
                                (int) ((float) (Globals.FetchCount - Globals.MapsToFetch.Count) /
                                       (float) Globals.FetchCount *
                                       100f), false
                            );
                        }
                    }
                }

                Globals.CurrentMap = MapInstance.Get(currentMapId);
            }

            if (packet.MapId != Globals.LoadingMap)
            {
                return;
            }

            Globals.CurrentMap = MapInstance.Get(Globals.LoadingMap);
            MapUpdatedDelegate();
            if (receivedMap.Up != Guid.Empty)
            {
                PacketSender.SendNeedMap(receivedMap.Up);
            }

            if (receivedMap.Down != Guid.Empty)
            {
                PacketSender.SendNeedMap(receivedMap.Down);
            }

            if (receivedMap.Left != Guid.Empty)
            {
                PacketSender.SendNeedMap(receivedMap.Left);
            }

            if (receivedMap.Right != Guid.Empty)
            {
                PacketSender.SendNeedMap(receivedMap.Right);
            }
        }

        if (Globals.CurrentMap is not { } currentMap)
        {
            throw new InvalidOperationException(
                $"Received packet for map {packet.MapId} but the current map was null"
            );
        }

        if (Globals.MapGrid is not { } mapGrid)
        {
            ApplicationContext.CurrentContext.Logger.LogError(
                "Received packet for map {MapId} before the map grid was initialized",
                packet.MapId
            );
            return;
        }

        if (!mapGrid.Loaded)
        {
            ApplicationContext.CurrentContext.Logger.LogError(
                "Received packet for map {MapId} before the map grid was loaded",
                packet.MapId
            );
            return;
        }

        if (currentMap.Id != packet.MapId)
        {
            ApplicationContext.CurrentContext.Logger.LogError(
                "Received packet for map {MapId} but the current map is {CurrentMapId} ({CurrentMapName})",
                packet.MapId,
                currentMap.Id,
                currentMap.Name
            );
            return;
        }

        for (var y = currentMap.MapGridY + 1; y >= currentMap.MapGridY - 1; y--)
        {
            if (y < 0 || y >= mapGrid.GridHeight)
            {
                continue;
            }
                
            for (var x = currentMap.MapGridX - 1; x <= currentMap.MapGridX + 1; x++)
            {
                if (x < 0 || x >= mapGrid.GridWidth)
                {
                    continue;
                }

                var gridMapId = mapGrid.Grid[x, y].MapId;
                if (gridMapId == Guid.Empty)
                {
                    continue;
                }

                if (MapInstance.TryGet(gridMapId, out _))
                {
                    continue;
                }
                    
                PacketSender.SendNeedMap(gridMapId);
            }
        }
    }

    //GameDataPacket
    public void HandlePacket(IPacketSender packetSender, GameDataPacket packet)
    {
        foreach (var obj in packet.GameObjects)
        {
            HandlePacket(packetSender, obj);
        }

        Globals.HasGameData = true;
        if (!Globals.InEditor && Globals.HasGameData && Globals.CurrentMap != null)
        {
            Globals.LoginForm.BeginInvoke(Globals.LoginForm.EditorLoopDelegate);
        }

        GameContentManager.LoadTilesets();
    }

    //EnterMapPacket
    public void HandlePacket(IPacketSender packetSender, EnterMapPacket packet)
    {
        Globals.MainForm.BeginInvoke((Action) (() => Globals.MainForm.EnterMap(packet.MapId)));
    }

    //MapListPacket
    public void HandlePacket(IPacketSender packetSender, MapListPacket packet)
    {
        MapList.List.JsonData = packet.MapListData;
        MapList.List.PostLoad(MapDescriptor.Lookup, false, true);

        // If our current map is null, load our previous map as per our stored Id or the first available map.
        if (Globals.CurrentMap == null)
        {
            // Check if we have a map we left off at before proceeding.
            var firstMap = MapList.List.FindFirstMap();
            var storedPreference = Preferences.LoadPreference("LastMapOpened");
            if (!string.IsNullOrWhiteSpace(storedPreference))
            {
                // Check if the map Id isn't empty and if this map still exists.
                var storedMapId = Guid.Parse(storedPreference);
                if (storedMapId != Guid.Empty && MapList.List.FindMap(storedMapId) != null)
                {
                    Globals.MainForm.EnterMap(storedMapId);
                }
                // No longer exists, so just load the first.
                else
                {
                    Globals.MainForm.EnterMap(firstMap);
                }
            }
            // Invalid Id, so load the first map.
            else
            {
                Globals.MainForm.EnterMap(firstMap);
            }
            
        }

        Globals.MapListWindow.BeginInvoke(Globals.MapListWindow.mapTreeList.MapListDelegate, Guid.Empty, null);
        Globals.MapPropertiesWindow?.BeginInvoke(Globals.MapPropertiesWindow.UpdatePropertiesDelegate);
    }

    //ErrorMessagePacket
    public void HandlePacket(IPacketSender packetSender, ErrorMessagePacket packet)
    {
        MessageBox.Show(packet.Error, packet.Header, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    //MapGridPacket
    public void HandlePacket(IPacketSender packetSender, MapGridPacket packet)
    {
        if (Globals.MapGrid != null)
        {
            Globals.MapGrid.Load(packet.EditorGrid);
            if (Globals.CurrentMap != null && Globals.MapGrid != null && Globals.MapGrid.Loaded)
            {
                for (var y = Globals.CurrentMap.MapGridY + 1; y >= Globals.CurrentMap.MapGridY - 1; y--)
                {
                    for (var x = Globals.CurrentMap.MapGridX - 1; x <= Globals.CurrentMap.MapGridX + 1; x++)
                    {
                        if (x >= 0 && x < Globals.MapGrid.GridWidth && y >= 0 && y < Globals.MapGrid.GridHeight)
                        {
                            var needMap = MapInstance.Get(Globals.MapGrid.Grid[x, y].MapId);
                            if (needMap == null && Globals.MapGrid.Grid[x, y].MapId != Guid.Empty)
                            {
                                PacketSender.SendNeedMap(Globals.MapGrid.Grid[x, y].MapId);
                            }
                        }
                    }
                }
            }
        }
    }

    //GameObjectPacket
    public void HandlePacket(IPacketSender packetSender, GameObjectPacket packet)
    {
        var id = packet.Id;
        var deleted = packet.Deleted;
        var json = string.Empty;
        if (!packet.Deleted)
        {
            json = packet.Data;
        }

        switch (packet.Type)
        {
            case GameObjectType.Animation:
                if (deleted)
                {
                    var anim = AnimationDescriptor.Get(id);
                    anim.Delete();
                }
                else
                {
                    var anim = new AnimationDescriptor(id);
                    anim.Load(json);
                    try
                    {
                        AnimationDescriptor.Lookup.Set(id, anim);
                    }
                    catch (Exception exception)
                    {
                        Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError($"Another mystery NPE. [Lookup={AnimationDescriptor.Lookup}]");
                        if (exception.InnerException != null)
                        {
                            Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(exception.InnerException, "Mystery NPE");
                        }

                        Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError(exception, "Failed to load animation base");
                        Intersect.Core.ApplicationContext.Context.Value?.Logger.LogError($"{nameof(id)}={id},{nameof(anim)}={anim}");

                        throw;
                    }
                }

                break;

            case GameObjectType.Class:
                if (deleted)
                {
                    var cls = ClassDescriptor.Get(id);
                    cls.Delete();
                }
                else
                {
                    var cls = new ClassDescriptor(id);
                    cls.Load(json);
                    ClassDescriptor.Lookup.Set(id, cls);
                }

                break;

            case GameObjectType.Item:
                if (deleted)
                {
                    var itm = ItemDescriptor.Get(id);
                    itm.Delete();
                }
                else
                {
                    var itm = new ItemDescriptor(id);
                    itm.Load(json);
                    ItemDescriptor.Lookup.Set(id, itm);
                }

                break;
            case GameObjectType.Npc:
                if (deleted)
                {
                    var npc = NPCDescriptor.Get(id);
                    npc.Delete();
                }
                else
                {
                    var npc = new NPCDescriptor(id);
                    npc.Load(json);
                    NPCDescriptor.Lookup.Set(id, npc);
                }

                break;

            case GameObjectType.Projectile:
                if (deleted)
                {
                    var proj = ProjectileDescriptor.Get(id);
                    proj.Delete();
                }
                else
                {
                    var proj = new ProjectileDescriptor(id);
                    proj.Load(json);
                    ProjectileDescriptor.Lookup.Set(id, proj);
                }

                break;

            case GameObjectType.Quest:
                if (deleted)
                {
                    var qst = QuestDescriptor.Get(id);
                    qst.Delete();
                }
                else
                {
                    var qst = new QuestDescriptor(id);
                    qst.Load(json);
                    foreach (var tsk in qst.Tasks)
                    {
                        qst.OriginalTaskEventIds.Add(tsk.Id, tsk.CompletionEventId);
                    }

                    QuestDescriptor.Lookup.Set(id, qst);
                }

                break;

            case GameObjectType.Resource:
                if (deleted)
                {
                    var res = ResourceDescriptor.Get(id);
                    res.Delete();
                }
                else
                {
                    var res = new ResourceDescriptor(id);
                    res.Load(json);
                    ResourceDescriptor.Lookup.Set(id, res);
                }

                break;

            case GameObjectType.Shop:
                if (deleted)
                {
                    var shp = ShopDescriptor.Get(id);
                    shp.Delete();
                }
                else
                {
                    var shp = new ShopDescriptor(id);
                    shp.Load(json);
                    ShopDescriptor.Lookup.Set(id, shp);
                }

                break;

            case GameObjectType.Spell:
                if (deleted)
                {
                    var spl = SpellDescriptor.Get(id);
                    spl.Delete();
                }
                else
                {
                    var spl = new SpellDescriptor(id);
                    spl.Load(json);
                    SpellDescriptor.Lookup.Set(id, spl);
                }

                break;

            case GameObjectType.CraftTables:
                if (deleted)
                {
                    var cft = CraftingTableDescriptor.Get(id);
                    cft.Delete();
                }
                else
                {
                    var cft = new CraftingTableDescriptor(id);
                    cft.Load(json);
                    CraftingTableDescriptor.Lookup.Set(id, cft);
                }

                break;

            case GameObjectType.Crafts:
                if (deleted)
                {
                    var cft = CraftingRecipeDescriptor.Get(id);
                    cft.Delete();
                }
                else
                {
                    var cft = new CraftingRecipeDescriptor(id);
                    cft.Load(json);
                    CraftingRecipeDescriptor.Lookup.Set(id, cft);
                }

                break;

            case GameObjectType.Map:
                //Handled in a different packet
                break;

            case GameObjectType.Event:
                var wasCommon = false;
                if (deleted)
                {
                    var evt = EventDescriptor.Get(id);
                    wasCommon = evt.CommonEvent;
                    evt.Delete();
                }
                else
                {
                    var evt = new EventDescriptor(id);
                    evt.Load(json);
                    wasCommon = evt.CommonEvent;
                    EventDescriptor.Lookup.Set(id, evt);
                }

                if (!wasCommon)
                {
                    return;
                }

                break;

            case GameObjectType.PlayerVariable:
                if (deleted)
                {
                    var pvar = PlayerVariableDescriptor.Get(id);
                    pvar.Delete();
                }
                else
                {
                    var pvar = new PlayerVariableDescriptor(id);
                    pvar.Load(json);
                    PlayerVariableDescriptor.Lookup.Set(id, pvar);
                }

                break;

            case GameObjectType.ServerVariable:
                if (deleted)
                {
                    var svar = ServerVariableDescriptor.Get(id);
                    svar.Delete();
                }
                else
                {
                    var svar = new ServerVariableDescriptor(id);
                    svar.Load(json);
                    ServerVariableDescriptor.Lookup.Set(id, svar);
                }

                break;

            case GameObjectType.Tileset:
                var obj = new TilesetDescriptor(id);
                obj.Load(json);
                TilesetDescriptor.Lookup.Set(id, obj);
                if (Globals.HasGameData && !packet.AnotherFollowing)
                {
                    GameContentManager.LoadTilesets();
                }

                break;

            case GameObjectType.GuildVariable:
                if (deleted)
                {
                    var pvar = GuildVariableDescriptor.Get(id);
                    pvar.Delete();
                }
                else
                {
                    var pvar = new GuildVariableDescriptor(id);
                    pvar.Load(json);
                    GuildVariableDescriptor.Lookup.Set(id, pvar);
                }

                break;

            case GameObjectType.UserVariable:
                if (deleted)
                {
                    var pvar = UserVariableDescriptor.Get(id);
                    pvar.Delete();
                }
                else
                {
                    var pvar = new UserVariableDescriptor(id);
                    pvar.Load(json);
                    UserVariableDescriptor.Lookup.Set(id, pvar);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        GameObjectUpdatedDelegate?.Invoke(packet.Type);
    }

    //OpenEditorPacket
    public void HandlePacket(IPacketSender packetSender, OpenEditorPacket packet)
    {
        Globals.MainForm.BeginInvoke(Globals.MainForm.EditorDelegate, packet.Type);
    }

    //TimeDataPacket
    public void HandlePacket(IPacketSender packetSender, TimeDataPacket packet)
    {
        DaylightCycleDescriptor.Instance.LoadFromJson(packet.TimeJson);
    }
}
