using System.Text;

namespace Intersect.Framework.Terminal;

/// <summary>
/// The structure representing the options for console input processing.
/// </summary>
public struct ConsoleInputMode
{
    /// <summary>
    /// The input configuration for normal operation.
    /// </summary>
    public static ConsoleInputMode Normal => new(false);

    /// <summary>
    /// The input configuration for passwords, replacing key presses with asterisks.
    /// </summary>
    public static ConsoleInputMode Password => new(true, '*');

    private readonly char[] _substitution;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleInputMode"/> struct.
    /// </summary>
    /// <param name="intercept">If key presses should be intercepted instead of being processed with the default behavior.</param>
    /// <param name="substitution">What character(s) should be substituted in place of the pressed key.</param>
    public ConsoleInputMode(bool intercept, params char[] substitution)
    {
        Intercept = intercept;
        _substitution = substitution;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleInputMode"/> struct.
    /// </summary>
    /// <param name="intercept">If key pressed should be intercepted instead of printed to the screen.</param>
    /// <param name="substitution">What character(s) should be substituted in place of the pressed key.</param>
    public ConsoleInputMode(bool intercept, ReadOnlySpan<char> substitution)
    {
        Intercept = intercept;
        _substitution = substitution.ToArray();
    }

    /// <summary>
    /// If key presses should be intercepted and substituted with the contents of <see cref="Substitution"/>.
    /// </summary>
    public bool Intercept { get; }

    /// <summary>
    /// What characters should be used to substitute input key characters.
    /// </summary>
    public ReadOnlySpan<char> Substitution => new(_substitution);
}

public sealed class ConsoleInterface
{
    private readonly ConsoleInputMode _consoleInputMode;

    public ConsoleInterface(ConsoleInputMode consoleInputMode = default)
    {
        _consoleInputMode = consoleInputMode;
    }

    public string? ReadLine(CancellationToken cancellationToken) => ReadLine(_consoleInputMode, cancellationToken);

    public string? ReadLine(ConsoleInputMode consoleInputMode, CancellationToken cancellationToken)
    {
        var buffer = new StringBuilder();
        do
        {
            while (!Console.KeyAvailable)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return buffer.ToString();
                }

                Thread.Yield();
            }

            var consoleKeyInfo = Console.ReadKey(consoleInputMode.Intercept);

            switch (consoleKeyInfo.Key)
            {
                case ConsoleKey.OemClear:
                    return null;

                case ConsoleKey.Enter:
                    return buffer.ToString();

                case ConsoleKey.LeftArrow:
                    Console.CursorLeft -= 1;
                    break;

                case ConsoleKey.UpArrow:
                    Console.CursorTop -= 1;
                    break;

                case ConsoleKey.RightArrow:
                    Console.CursorTop += 1;
                    break;

                case ConsoleKey.DownArrow:
                    Console.CursorTop += 1;
                    break;

                case < (ConsoleKey)0x30:
                    continue;

                default:
                    if (consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        Console.Write('^');
                        Console.Write(consoleKeyInfo.KeyChar);
                    }
                    else if (!consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt))
                    {
                        if (consoleInputMode.Intercept)
                        {
                            Console.Write(consoleInputMode.Substitution.ToArray());
                        }
                        else
                        {
                            Console.Write(consoleKeyInfo.KeyChar);
                        }
                    }

                    break;
            }
        } while (true);
    }
}
