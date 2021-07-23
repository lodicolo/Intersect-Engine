using Intersect.Enums;
using Intersect.GameObjects.Events;
using Intersect.Network.Packets.Server;
using Intersect.Server.Framework.Maps;

namespace Intersect.Server.Framework.Entities.Events
{
    public interface IEventPageInstance : IEntity
    {
        EventBase BaseEvent { get; set; }
        bool DisablePreview { get; set; }
        IEventPageInstance GlobalClone { get; set; }
        EventMovementFrequency MovementFreq { get; set; }
        EventMovementSpeed MovementSpeed { get; set; }
        EventMovementType MovementType { get; set; }
        IEvent MyEventIndex { get; set; }
        EventGraphic MyGraphic { get; set; }
        EventPage MyPage { get; set; }
        string Param { get; set; }
        IPlayer Player { get; set; }
        int Speed { get; set; }
        EventTrigger Trigger { get; set; }
        IMapInstance Map { get; }

        int CanMove(int moveDir);
        EntityPacket EntityPacket(EntityPacket packet = null, IPlayer forPlayer = null);
        EntityTypes GetEntityType();
        float GetMovementTime();
        int[] GetStatValues();
        void Move(int moveDir, IPlayer forPlayer, bool doNotUpdate = false, bool correction = false);
        void SendToPlayer();
        void SetMovementSpeed(EventMovementSpeed speed);
        bool ShouldDespawn(IMapInstance map);
        void TurnTowardsPlayer();
        void Update(bool isActive, long timeMs);
    }
}