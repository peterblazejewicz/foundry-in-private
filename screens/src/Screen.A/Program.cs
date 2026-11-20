using Screen.Core;

namespace Screen.A;

/// <summary>Screen A: the read-only perception monitor (board A, HDMI).</summary>
internal static class Program
{
    [STAThread]
    public static int Main(string[] args) => ScreenHost.Run(args, "Screen A");
}
