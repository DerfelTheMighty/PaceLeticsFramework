using PaceLetics.CoreModule.Infrastructure.Converter;

namespace PaceLetics.Tests;

public class PaceFormattingTests
{
    [Theory]
    [InlineData(4.0, "4:10 min/km")]
    [InlineData(3.36, "4:58 min/km")]
    public void FormatFromSpeed_ReturnsPacePerKilometer(double speedMps, string expected)
    {
        Assert.Equal(expected, PaceFormatting.FormatFromSpeed(speedMps));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FormatFromSpeed_ReturnsPlaceholderForInvalidSpeed(double speedMps)
    {
        Assert.Equal("-", PaceFormatting.FormatFromSpeed(speedMps));
    }
}
