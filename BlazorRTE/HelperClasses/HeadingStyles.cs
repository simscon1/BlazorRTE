namespace BlazorRTE.HelperClasses;

/// <summary>
/// Predefined heading styles with their display properties
/// </summary>
public static class HeadingStyles
{
    /// <summary>
    /// Represents a heading style option
    /// </summary>
    public record HeadingOption(
        string Tag,           // h1, h2, h3, h4, h5, h6, p
        string Label,         // "Heading 1", "Heading 2", etc.
        string FontSize,      // CSS font-size value
        string FontWeight,    // CSS font-weight value
        string Shortcut       // Keyboard shortcut hint
    );

    /// <summary>
    /// All available heading options
    /// </summary>
    public static readonly HeadingOption[] Options =
    [
        new("p",  "Normal",    "14px", "400", "Ctrl+Alt+0"),
        new("h1", "Heading 1", "32px", "700", "Ctrl+Alt+1"),
        new("h2", "Heading 2", "24px", "700", "Ctrl+Alt+2"),
        new("h3", "Heading 3", "18px", "600", "Ctrl+Alt+3"),
        new("h4", "Heading 4", "16px", "600", "Ctrl+Alt+4"),
        new("h5", "Heading 5", "14px", "600", "Ctrl+Alt+5"),
        new("h6", "Heading 6", "12px", "600", "Ctrl+Alt+6")
    ];

    /// <summary>
    /// Gets the display label for a heading tag (e.g., "h1" → "H1", "p" → "¶")
    /// </summary>
    public static string GetButtonLabel(string tag) => tag switch
    {
        "h1" => "H1",
        "h2" => "H2",
        "h3" => "H3",
        "h4" => "H4",
        "h5" => "H5",
        "h6" => "H6",
        _ => "¶"
    };
}