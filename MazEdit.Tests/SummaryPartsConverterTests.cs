using System.Globalization;
using System.Windows.Data;
using Xunit;

namespace MazEdit.Tests;

public class SummaryPartsConverterTests
{
    private readonly SummaryPartsConverter _converter = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Convert_Blank_ReturnsEmptyList(string? summary)
    {
        var parts = ((IEnumerable<SummaryPart>)_converter.Convert(summary!, typeof(SummaryPart[]), parameter: null!, CultureInfo.InvariantCulture)).ToArray();

        Assert.Empty(parts);
    }

    [Fact]
    public void Convert_NameValuePairs_SplitsOnFirstEquals()
    {
        var parts = Convert("MAT=CST IRN  INITIAL-Z=200  MULTI MODE=OFFSET TYPE");

        Assert.Equal(3, parts.Length);
        Assert.Equal("MAT=", parts[0].Name);
        Assert.Equal("CST IRN", parts[0].Value);
        Assert.Equal("INITIAL-Z=", parts[1].Name);
        Assert.Equal("200", parts[1].Value);
        Assert.Equal("MULTI MODE=", parts[2].Name);
        Assert.Equal("OFFSET TYPE", parts[2].Value);
    }

    [Fact]
    public void Convert_TokenWithoutEquals_IsValueOnly()
    {
        var parts = Convert("END MILL  Φ=63  J  No=3");

        Assert.Equal("", parts[0].Name);
        Assert.Equal("END MILL", parts[0].Value);
        Assert.Equal("Φ=", parts[1].Name);
        Assert.Equal("63", parts[1].Value);
        Assert.Equal("", parts[2].Name);
        Assert.Equal("J", parts[2].Value);
        Assert.Equal("No=", parts[3].Name);
        Assert.Equal("3", parts[3].Value);
    }

    [Fact]
    public void ConvertBack_Throws()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack("", typeof(string), parameter: null!, CultureInfo.InvariantCulture));
    }

    private SummaryPart[] Convert(string summary)
        => ((IEnumerable<SummaryPart>)_converter.Convert(summary, typeof(IEnumerable<SummaryPart>), parameter: null!, CultureInfo.InvariantCulture)).ToArray();
}
