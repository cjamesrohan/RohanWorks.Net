using RohanWorks.Net.Options.Validation;

namespace RohanWorks.Net.Options.Validation.Tests;

public class HttpUrlAttributeTests
{
    private readonly HttpUrlAttribute _sut = new();

    [Theory]
    [InlineData(null, true)]
    [InlineData(5, false)]
    [InlineData("google", false)]
    [InlineData("https://google.com", true)]
    [InlineData("http://google.com", true)]
    [InlineData("http:///google.com", false)]
    [InlineData("htp://google.com", false)]
    [InlineData("http:://google.com", false)]
    [InlineData("ftp://example.com", false)]
    public void IsValid_ReturnsExpected(object? value, bool expected)
    {
        _sut.IsValid(value).Should().Be(expected);
    }

    [Fact]
    public void ErrorMessage_ContainsFieldPlaceholder()
    {
        _sut.ErrorMessage.Should().Contain("{0}");
    }
}
