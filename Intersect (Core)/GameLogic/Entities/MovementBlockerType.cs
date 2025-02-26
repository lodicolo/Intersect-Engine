namespace Intersect.GameLogic.Entities;

public enum MovementBlockerType
{
    NotBlocked = 0,
    MapDoesNotExist,
    OutOfBounds,
    MapAttribute,
    Slide,
    Entity,
    ZDimension,
}