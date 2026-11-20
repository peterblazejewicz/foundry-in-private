using System.Globalization;

namespace Screen.Core;

/// <summary>The start options of a screen client, read from its command line.</summary>
/// <param name="UseDrm">True: draw on a DRM card with no desktop (the boards). False: open a desktop window (development).</param>
/// <param name="Card">The DRM card path, for example <c>/dev/dri/card0</c>. Null: Avalonia picks the card.</param>
/// <param name="ForceProbe">True: probe the connectors at start. The evdi (DisplayLink) card needs it.</param>
/// <param name="Scaling">The DRM output scaling.</param>
public sealed record ScreenOptions(bool UseDrm, string? Card, bool ForceProbe, double Scaling)
{
    /// <summary>The options when the command line gives none.</summary>
    public static ScreenOptions Default { get; } = new(UseDrm: false, Card: null, ForceProbe: false, Scaling: 1.0);

    /// <summary>Reads <c>--drm</c>, <c>--card &lt;path&gt;</c>, <c>--force-probe</c> and <c>--scaling &lt;n&gt;</c>.</summary>
    /// <exception cref="ArgumentException">A value is missing or is not valid.</exception>
    public static ScreenOptions Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        ScreenOptions options = Default;
        for (int i = 0; i < args.Count; i++)
        {
            switch (args[i])
            {
                case "--drm":
                    options = options with { UseDrm = true };
                    break;
                case "--force-probe":
                    options = options with { ForceProbe = true };
                    break;
                case "--card":
                    options = options with { Card = ValueAt(args, ++i, "--card") };
                    break;
                case "--scaling":
                    options = options with { Scaling = ScalingAt(args, ++i) };
                    break;
                default:
                    break;
            }
        }

        return options;
    }

    private static double ScalingAt(IReadOnlyList<string> args, int index)
    {
        string raw = ValueAt(args, index, "--scaling");
        return double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double scaling) && scaling > 0
            ? scaling
            : throw new ArgumentException($"--scaling needs a positive number, not '{raw}'.", nameof(args));
    }

    private static string ValueAt(IReadOnlyList<string> args, int index, string flag) =>
        index < args.Count && !args[index].StartsWith("--", StringComparison.Ordinal)
            ? args[index]
            : throw new ArgumentException($"{flag} needs a value.", nameof(args));
}
