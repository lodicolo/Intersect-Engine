namespace Intersect.Client.Interface;

public delegate void InterfaceStateChangedHandler(
    object? sender,
    InterfaceState state,
    IMutableInterface mutableInterface
);