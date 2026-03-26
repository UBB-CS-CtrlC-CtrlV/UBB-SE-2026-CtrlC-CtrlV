using MovieApp.Ui.Converters;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class NegativeBooleanConverterTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Convert_InvertsBooleanValues(bool value, bool expected)
    {
        var converter = new NegativeBooleanConverter();

        var result = converter.Convert(value, typeof(bool), null!, string.Empty);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void ConvertBack_InvertsBooleanValues(bool value, bool expected)
    {
        var converter = new NegativeBooleanConverter();

        var result = converter.ConvertBack(value, typeof(bool), null!, string.Empty);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_ReturnsOriginalValueWhenInputIsNotBoolean()
    {
        var converter = new NegativeBooleanConverter();

        var result = converter.Convert("text", typeof(string), null!, string.Empty);

        Assert.Equal("text", result);
    }
}
