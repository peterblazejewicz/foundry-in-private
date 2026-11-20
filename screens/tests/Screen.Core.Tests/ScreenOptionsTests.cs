using Xunit;

namespace Screen.Core.Tests;

public sealed class ScreenOptionsTests
{
    [Fact]
    public void NoArgumentsGiveTheDesktopDefaults()
    {
        Assert.Equal(ScreenOptions.Default, ScreenOptions.Parse([]));
    }

    [Fact]
    public void DrmFlagsAreRead()
    {
        ScreenOptions options = ScreenOptions.Parse(["--drm", "--card", "/dev/dri/card0", "--force-probe", "--scaling", "1.25"]);

        Assert.Equal(new ScreenOptions(UseDrm: true, Card: "/dev/dri/card0", ForceProbe: true, Scaling: 1.25), options);
    }

    [Theory]
    [InlineData("--card")]
    [InlineData("--scaling")]
    public void AFlagWithNoValueIsRefused(string flag)
    {
        Assert.Throws<ArgumentException>(() => ScreenOptions.Parse([flag]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("big")]
    public void ABadScalingIsRefused(string value)
    {
        Assert.Throws<ArgumentException>(() => ScreenOptions.Parse(["--scaling", value]));
    }
}
