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

namespace BlazorRTE.Demo.SvgIcons
{
    public class SvgIcons
    {
        // Logo aspect ratio (200:90)
        private const double LogoAspectRatio = 200.0 / 90.0;

        public static string GetBadge(int size)
        {
            return $@"<svg width=""{size}"" height=""{size}"" viewBox=""0 0 80 80"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- Rounded square background -->
  <rect x=""8"" y=""8"" width=""64"" height=""64"" rx=""12"" fill=""url(#icon4-bg)"" stroke=""#5B21B6"" stroke-width=""2.5"" />
  
  <!-- Document icon -->
  <g>
    <path
      d=""M25 22 L25 58 L55 58 L55 35 L46 22 Z""
      fill=""white""
      stroke=""white""
      stroke-width=""1.5""
    />
    <path d=""M46 22 L46 35 L55 35"" fill=""#E9D5FF"" stroke=""white"" stroke-width=""1.5"" stroke-linejoin=""round"" />
    
    <!-- Text lines in document -->
    <line x1=""30"" y1=""40"" x2=""50"" y2=""40"" stroke=""#7C3AED"" stroke-width=""2"" stroke-linecap=""round"" />
    <line x1=""30"" y1=""45"" x2=""48"" y2=""45"" stroke=""#7C3AED"" stroke-width=""2"" stroke-linecap=""round"" />
    <line x1=""30"" y1=""50"" x2=""50"" y2=""50"" stroke=""#7C3AED"" stroke-width=""2"" stroke-linecap=""round"" />
    
    <!-- Small flame accent -->
    <path
      d=""M50 50 C50 50 47 54 47 57 C47 60 49 62 52 62 C55 62 57 60 57 57 C57 55 55 54 55 54 C55 54 54 55 53 56 C53 54 55 51 55 50 C55 50 53 53 53 55 C53 53 50 50 50 50Z""
      fill=""#FCD34D""
    />
  </g>
  
  <defs>
    <linearGradient id=""icon4-bg"" x1=""40"" y1=""8"" x2=""40"" y2=""72"" gradientUnits=""userSpaceOnUse"">
      <stop offset=""0%"" stop-color=""#A78BFA"" />
      <stop offset=""100%"" stop-color=""#7C3AED"" />
    </linearGradient>
  </defs>
</svg>";
        }

        /// <summary>
        /// Gets the logo SVG with width only - height is calculated to maintain aspect ratio (200:90)
        /// </summary>
        public static string GetLogo(int width)
        {
            int height = (int)Math.Round(width / LogoAspectRatio);
            return GetLogo(width, height);
        }

        /// <summary>
        /// Gets the logo SVG with custom width and height
        /// </summary>
        public static string GetLogo(int width, int height)
        {
            return $@"<svg width=""{width}"" height=""{height}"" viewBox=""0 0 184 58"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- Content with proper centering -->
  <g transform=""translate(-13, -13)"">
    <!-- Document icon -->
    <g>
      <path
        d=""M20 20 L20 60 L52 60 L52 32 L40 20 Z""
        fill=""white""
        stroke=""#7C3AED""
        stroke-width=""2.5""
      />
      <path d=""M40 20 L40 32 L52 32"" fill=""#E9D5FF"" stroke=""#7C3AED"" stroke-width=""2"" stroke-linejoin=""round"" />
      
      <!-- Text lines in document -->
      <line x1=""26"" y1=""38"" x2=""46"" y2=""38"" stroke=""#9CA3AF"" stroke-width=""2"" stroke-linecap=""round"" />
      <line x1=""26"" y1=""43"" x2=""44"" y2=""43"" stroke=""#9CA3AF"" stroke-width=""2"" stroke-linecap=""round"" />
      <line x1=""26"" y1=""48"" x2=""46"" y2=""48"" stroke=""#9CA3AF"" stroke-width=""2"" stroke-linecap=""round"" />
      <line x1=""26"" y1=""53"" x2=""40"" y2=""53"" stroke=""#9CA3AF"" stroke-width=""2"" stroke-linecap=""round"" />
      
      <!-- Small flame accent -->
      <path
        d=""M48 48 C48 48 46 51 46 53 C46 55 47 56 49 56 C51 56 52 55 52 53 C52 52 51 51 51 51 C51 51 50 52 50 52 C50 51 51 49 51 48 C51 48 50 50 50 51 C50 50 48 48 48 48Z""
        fill=""url(#doc-flame)"">
      </path>
    </g>
    
    <text x=""65"" y=""40"" font-family=""Arial, sans-serif"" font-size=""26"" font-weight=""bold"" fill=""#1F2937"">
      Blazor<tspan fill=""#7C3AED"">RTE</tspan>
    </text>
    
    <!-- Separator line -->
    <line x1=""65"" y1=""46"" x2=""170"" y2=""46"" stroke=""#D1D5DB"" stroke-width=""1"" />
    
    <text x=""65"" y=""58"" font-family=""Arial, sans-serif"" font-size=""11"" font-style=""italic"" fill=""#6B7280"">
      Rich Text Editor
    </text>
  </g>
  
  <defs>
    <linearGradient id=""doc-flame"" x1=""49"" y1=""48"" x2=""49"" y2=""56"" gradientUnits=""userSpaceOnUse"">
      <stop offset=""0%"" stop-color=""#FBBF24"" />
      <stop offset=""100%"" stop-color=""#F59E0B"" />
    </linearGradient>
  </defs>
</svg>";
        }
    }
}