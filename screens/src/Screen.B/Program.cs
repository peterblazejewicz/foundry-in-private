using Screen.Core;

namespace Screen.B;

/// <summary>Screen B: the operator console (board B, DisplayLink).</summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args) => ScreenHost.Run(args, "Screen B");
}
