namespace Intersect.Framework.Eventing;

public interface IAggregateSender
{
    object CurrentSender { get; }

    object? OriginalSender { get; }
}

public interface IAggregateSender<out T> : IAggregateSender where T : notnull
{
    new T CurrentSender { get; }

    object IAggregateSender.CurrentSender => CurrentSender;
}

public sealed record AggregateSender : IAggregateSender
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AggregateSender(object currentSender, object? originalSender)
    {
        CurrentSender = currentSender;
        OriginalSender = originalSender;
    }

    /// <inheritdoc />
    public object CurrentSender { get; }

    /// <inheritdoc />
    public object? OriginalSender { get; }
}

public sealed class AggregateSender<T> : IAggregateSender<T> where T : notnull
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public AggregateSender(T currentSender, object? originalSender)
    {
        CurrentSender = currentSender;
        OriginalSender = originalSender;
    }

    /// <inheritdoc />
    public T CurrentSender { get; }

    /// <inheritdoc />
    public object? OriginalSender { get; }
}
