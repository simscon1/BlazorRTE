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
/// Predefined color palettes for text and background colors
/// </summary>
public static class ColorPalette
{
    /// <summary>
    /// Text color options (24 colors - 8 columns x 3 rows)
    /// </summary>
    public static readonly Dictionary<string, string> TextColors = new()
    {
        // Row 1 - Grayscale + Primary
        { "#000000", "Black" },
        { "#434343", "Dark Gray" },
        { "#666666", "Gray" },
        { "#999999", "Light Gray" },
        { "#B7B7B7", "Silver" },
        { "#CCCCCC", "Pale Gray" },
        { "#EFEFEF", "Near White" },
        { "#FFFFFF", "White" },
        
        // Row 2 - Vibrant colors
        { "#980000", "Dark Red" },
        { "#FF0000", "Red" },
        { "#FF9900", "Orange" },
        { "#FFFF00", "Yellow" },
        { "#00FF00", "Lime" },
        { "#00FFFF", "Cyan" },
        { "#4A86E8", "Cornflower Blue" },
        { "#0000FF", "Blue" },
        
        // Row 3 - Extended palette
        { "#9900FF", "Purple" },
        { "#FF00FF", "Magenta" },
        { "#E06666", "Salmon" },
        { "#F6B26B", "Peach" },
        { "#93C47D", "Sage" },
        { "#76A5AF", "Teal" },
        { "#8E7CC3", "Lavender" },
        { "#C27BA0", "Pink" }
    };

    /// <summary>
    /// Background/Highlight color options (24 colors - 8 columns x 3 rows)
    /// </summary>
    public static readonly Dictionary<string, string> BackgroundColors = new()
    {
        // Row 1 - Bright + None
        { "transparent", "None" },
        { "#FFFF00", "Yellow" },
        { "#00FF00", "Lime" },
        { "#00FFFF", "Cyan" },
        { "#FF00FF", "Magenta" },
        { "#FF0000", "Red" },
        { "#FF9900", "Orange" },
        { "#FFFFFF", "White" },
        
        // Row 2 - Warm pastels
        { "#FCE5CD", "Peach" },
        { "#FFF2CC", "Cream" },
        { "#FFE599", "Butter" },
        { "#F4CCCC", "Blush" },
        { "#EAD1DC", "Rose" },
        { "#D5A6BD", "Mauve" },
        { "#E6B8AF", "Coral" },
        { "#F9CB9C", "Apricot" },
        
        // Row 3 - Cool pastels
        { "#D0E0E3", "Mist" },
        { "#C9DAF8", "Periwinkle" },
        { "#CFE2F3", "Ice Blue" },
        { "#D9EAD3", "Mint" },
        { "#B6D7A8", "Sage" },
        { "#A2C4C9", "Seafoam" },
        { "#B4A7D6", "Violet" },
        { "#9FC5E8", "Powder Blue" }
    };
}