namespace BlazorRTE.HelperClasses;

/// <summary>
/// Predefined font family options for the rich text editor
/// </summary>
public static class FontFamilies
{
    /// <summary>
    /// Represents a font family option
    /// </summary>
    /// <param name="Name">Display name of the font</param>
    /// <param name="CssValue">Full CSS font-family value with fallbacks</param>
    /// <param name="Category">Font category (Sans-serif, Serif, Monospace, etc.)</param>
    public record FontOption(string Name, string CssValue, string Category);

    /// <summary>
    /// All available font family options
    /// </summary>
    public static readonly FontOption[] Options =
    [
        // Sans-serif fonts
        new("Arial", "Arial, sans-serif", "Sans-serif"),
        new("Calibri", "Calibri, sans-serif", "Sans-serif"),
        new("Helvetica", "Helvetica, sans-serif", "Sans-serif"),
        new("Tahoma", "Tahoma, sans-serif", "Sans-serif"),
        new("Trebuchet MS", "Trebuchet MS, sans-serif", "Sans-serif"),
        new("Verdana", "Verdana, sans-serif", "Sans-serif"),
        
        // Serif fonts
        new("Georgia", "Georgia, serif", "Serif"),
        new("Times New Roman", "Times New Roman, serif", "Serif"),
        
        // Display fonts
        new("Comic Sans MS", "Comic Sans MS, cursive", "Display"),
        new("Impact", "Impact, sans-serif", "Display"),
        
        // Monospace fonts
        new("Courier New", "Courier New, monospace", "Monospace"),
        new("Lucida Console", "Lucida Console, monospace", "Monospace")
    ];

    /// <summary>
    /// Gets a font option by name (case-insensitive)
    /// </summary>
    public static FontOption? GetByName(string name) =>
        Options.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}