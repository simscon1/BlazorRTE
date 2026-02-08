using System.Collections.Generic;
using System.Globalization;

namespace BlazorRTE.HelperClasses
{
    /// <summary>
    /// Provides color palettes and utility methods for the Rich Text Editor.
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// A standard palette of colors for text.
        /// </summary>
        public static readonly List<string> StandardColors = new()
        {
            "#000000", "#4B5563", "#6B7280", "#EF4444", "#F97316", "#F59E0B",
            "#84CC16", "#10B981", "#06B6D4", "#3B82F6", "#8B5CF6", "#EC4899"
        };

        /// <summary>
        /// A standard palette of colors for highlighting text backgrounds.
        /// </summary>
        public static readonly List<string> HighlightColors = new()
        {
            "#FFFFFF", "#FEE2E2", "#FFEDD5", "#FEF3C7", "#D9F99D", "#A7F3D0",
            "#A5F3FC", "#DBEAFE", "#E9D5FF", "#FCE7F3", "#F3F4F6", "#E5E7EB"
        };

        /// <summary>
        /// Determines if a given hex color is "dark" based on its calculated luminance.
        /// Used to decide whether to place light or dark text on top of the color.
        /// </summary>
        /// <param name="hexColor">The color in hex format (e.g., "#RRGGBB").</param>
        /// <returns>True if the color is dark, false otherwise.</returns>
        public static bool IsDarkColor(string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor) || hexColor.Length < 7)
            {
                return false; // Default to not dark for invalid colors
            }

            try
            {
                // Remove '#' and parse R, G, B
                var colorString = hexColor.Substring(1);
                var r = int.Parse(colorString.Substring(0, 2), NumberStyles.HexNumber);
                var g = int.Parse(colorString.Substring(2, 2), NumberStyles.HexNumber);
                var b = int.Parse(colorString.Substring(4, 2), NumberStyles.HexNumber);

                // Calculate luminance using the standard formula
                var luminance = (0.299 * r + 0.587 * g + 0.114 * b);

                // Colors with luminance below 128 are generally considered dark
                return luminance < 128;
            }
            catch
            {
                return false; // If parsing fails, assume not dark
            }
        }
    }
}