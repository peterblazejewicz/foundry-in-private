using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.LinuxFramebuffer;
using Avalonia.Media;
using Avalonia.Themes.Fluent;

namespace Screen.Core;

/// <summary>Starts a thin screen client: DRM with no desktop on a board, a window on a development machine.</summary>
public static class ScreenHost
{
    /// <summary>Runs the screen until it closes, and returns the exit code.</summary>
    public static int Run(string[] args, string title)
    {
        ScreenOptions options = ScreenOptions.Parse(args);
        AppBuilder builder = AppBuilder.Configure(() => new ScreenApp(title))
            .UsePlatformDetect()
            .LogToTrace();

        return options.UseDrm
            ? builder.StartLinuxDrm(
                args,
                card: options.Card,
                connectorsForceProbe: options.ForceProbe,
                options: new DrmOutputOptions { Scaling = options.Scaling })
            : builder.StartWithClassicDesktopLifetime(args);
    }
}

/// <summary>The placeholder application. The IPC client and the views replace the text when the spec is ready.</summary>
internal sealed class ScreenApp(string title) : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());

    public override void OnFrameworkInitializationCompleted()
    {
        var view = new TextBlock
        {
            Text = title,
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White,
        };
        var root = new Border { Background = Brushes.Black, Child = view };

        switch (ApplicationLifetime)
        {
            // DRM: never make a Window under a single-view lifetime.
            case ISingleViewApplicationLifetime single:
                single.MainView = root;
                break;
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new Window { Title = title, Width = 1280, Height = 800, Content = root };
                break;
            default:
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
