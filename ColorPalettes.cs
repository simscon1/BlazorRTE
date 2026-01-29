namespace BlazorRTE
{
    /// <summary>
    /// Predefined color palettes for text and background colors
    /// </summary>
    public static class ColorPalettes
    {
        /// <summary>
        /// Text color options (9 colors)
        /// </summary>
        public static readonly Dictionary<string, string> TextColors = new()
        {
            { "#000000", "Black" },
            { "#FF0000", "Red" },
            { "#FF8800", "Orange" },
            { "#FFFF00", "Yellow" },
            { "#00AA00", "Green" },
            { "#0066CC", "Blue" },
            { "#9933FF", "Purple" },
            { "#666666", "Gray" },
            { "#FFFFFF", "White" }
        };

        /// <summary>
        /// Background/Highlight color options (7 colors)
        /// </summary>
        public static readonly Dictionary<string, string> BackgroundColors = new()
        {
            { "#FFFF00", "Yellow" },
            { "#CCFFCC", "Green" },
            { "#CCE5FF", "Blue" },
            { "#FFCCFF", "Pink" },
            { "#FFD699", "Orange" },
            { "#E0E0E0", "Gray" },
            { "transparent", "None" }
        };
    }
}