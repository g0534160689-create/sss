using DaatAharon.Core.Links;
using Xunit;

namespace DaatAharon.Tests;

public sealed class LinkAddressTests
{
    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com/path?q=1", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("file:///C:/Windows/win.ini", false)]
    [InlineData("https://user:pass@example.com", false)]
    [InlineData("", false)]
    [InlineData("not a url", false)]
    public void TryNormalize_ValidatesSchemesAndCredentials(string input, bool expected)
    {
        Assert.Equal(expected, LinkAddress.TryNormalize(input, out _));
    }
}
