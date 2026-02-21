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
/// Predefined font size options for the rich text editor
/// </summary>
public static class FontSizes
{
    /// <summary>
    /// Represents a font size option
    /// </summary>
    /// <param name="Value">The execCommand font size value (1-7)</param>
    /// <param name="Label">Display label</param>
    /// <param name="PixelSize">Approximate pixel size for preview</param>
    public record FontSizeOption(string Value, string Label, string PixelSize);

    /// <summary>
    /// All available font size options
    /// </summary>
    public static readonly FontSizeOption[] Options =
    [
        new("1", "Small", "10px"),
        new("3", "Normal", "14px"),
        new("4", "Medium", "16px"),
        new("5", "Large", "18px"),
        new("6", "X-Large", "24px"),
        new("7", "XX-Large", "32px")
    ];

    /// <summary>
    /// Gets a display label for the toolbar button
    /// </summary>
    public static string GetButtonLabel(string sizeValue) => sizeValue switch
    {
        "1" => "10",
        "3" => "14",
        "4" => "16",
        "5" => "18",
        "6" => "24",
        "7" => "32",
        _ => "14"
    };
}