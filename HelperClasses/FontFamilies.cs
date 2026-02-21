// ============================================================================
// BlazorRTE - Rich Text Editor for Blazor
// Copyright (C) 2024-2026 LoneWorx LLC
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.
// ============================================================================

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
        new("Segoe UI", "'Segoe UI', sans-serif", "Sans-serif"),
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