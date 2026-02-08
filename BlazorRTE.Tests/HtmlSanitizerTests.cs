using BlazorRTE.HelperClasses;

namespace BlazorRTE.Tests;

public class HtmlSanitizerTests
{
    #region XSS Prevention Tests

    [Fact]
    public void XSS01_ScriptTagsRemoved()
    {
        // Arrange
        var input = "<p>Hello</p><script>alert('XSS')</script>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("alert", result);
    }

    [Fact]
    public void XSS02_OnEventAttributesRemoved()
    {
        // Arrange
        var input = "<p onclick=\"alert('XSS')\">Click me</p>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.DoesNotContain("onclick", result);
    }

    [Fact]
    public void XSS03_JavaScriptUrlsRemoved()
    {
        // Arrange
        var input = "<a href=\"javascript:alert('XSS')\">Bad Link</a>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.DoesNotContain("javascript:", result);
    }

    [Fact]
    public void XSS04_OnErrorAttributeRemoved()
    {
        // Arrange
        var input = "<img src=x onerror='alert(1)'>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.DoesNotContain("onerror", result);
    }

    #endregion

    #region Allowed Tags Tests

    [Fact]
    public void TAG01_StrongTagPreserved()
    {
        // Arrange
        var input = "<p><strong>Bold</strong></p>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Contains("<strong>", result);
    }

    [Fact]
    public void TAG02_EmTagPreserved()
    {
        // Arrange
        var input = "<p><em>Italic</em></p>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Contains("<em>", result);
    }

    [Fact]
    public void TAG03_UnderlineTagPreserved()
    {
        // Arrange
        var input = "<p><u>Underline</u></p>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Contains("<u>", result);
    }

    [Fact]
    public void TAG04_LinksPreserved()
    {
        // Arrange
        var input = "<a href=\"https://example.com\">Link</a>";

        // Act
        var result = HtmlSanitizer.Sanitize(input);

        // Assert
        Assert.Contains("<a", result);
        Assert.Contains("href", result);
    }

    #endregion

    #region Strip HTML Tests

    [Fact]
    public void STRIP01_AllTagsRemoved()
    {
        // Arrange
        var input = "<p><strong>Bold</strong> text</p>";

        // Act
        var result = HtmlSanitizer.StripHtml(input);

        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.Contains("Bold", result);
        Assert.Contains("text", result);
    }

    [Fact]
    public void STRIP02_NestedTagsHandled()
    {
        // Arrange
        var input = "<div><p><span>Nested</span></p></div>";

        // Act
        var result = HtmlSanitizer.StripHtml(input);

        // Assert
        Assert.Equal("Nested", result.Trim());
    }

    #endregion
}