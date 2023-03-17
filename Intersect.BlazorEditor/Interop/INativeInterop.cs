namespace Intersect.BlazorEditor.Interop;

public interface INativeInterop
{
    Task<string> RuntimeVersion { get; }

    Task Close();

    Task Open(string url);

    Task SendNotification(string message);
}