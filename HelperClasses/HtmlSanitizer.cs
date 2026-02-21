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
//
using System.Text.RegularExpressions;

namespace BlazorRTE.HelperClasses
{
    /// <summary>
    /// HTML sanitization utility for RichTextEditor
    /// Provides XSS protection and safe HTML filtering
    /// </summary>
    public static class HtmlSanitizer
    {
        private static readonly string[] AllowedTags = new[]
        {
            "p", "br", "strong", "b", "em", "i", "u", "strike", "s",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "ul", "ol", "li",
            "a", "span", "div",
            "font", "sup", "sub",
            "hr"
        };

        private static readonly string[] DangerousTags = new[]
        {
            "script", "iframe", "object", "embed", "link", "meta", "style",
            "form", "input", "button", "textarea", "select"
        };

        /// <summary>
        /// Sanitizes HTML content by removing dangerous tags and attributes
        /// </summary>
        /// <param name="html">Raw HTML input</param>
        /// <returns>Sanitized HTML safe for rendering</returns>
        public static string Sanitize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove script tags and their content
            html = Regex.Replace(html, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase);

            // Remove dangerous tags
            foreach (var tag in DangerousTags)
            {
                html = Regex.Replace(html, $@"<{tag}\b[^>]*>.*?<\/{tag}>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                html = Regex.Replace(html, $@"<{tag}\b[^>]*\/?>", "", RegexOptions.IgnoreCase);
            }

            // Remove event handlers (onclick, onerror, etc.)
            html = Regex.Replace(html, @"\s*on\w+\s*=\s*[""'][^""']*[""']", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"\s*on\w+\s*=\s*[^\s>]*", "", RegexOptions.IgnoreCase);

            // Remove javascript: protocol
            html = Regex.Replace(html, @"javascript:", "", RegexOptions.IgnoreCase);

            // Remove data: protocol (can be used for XSS)
            html = Regex.Replace(html, @"data:text/html", "", RegexOptions.IgnoreCase);

            return html;
        }

        /// <summary>
        /// Strips all HTML tags from content, returning plain text
        /// </summary>
        /// <param name="html">HTML input</param>
        /// <returns>Plain text without HTML tags</returns>
        public static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove all HTML tags
            var plainText = Regex.Replace(html, "<.*?>", string.Empty);

            // Decode HTML entities
            plainText = System.Net.WebUtility.HtmlDecode(plainText);

            return plainText.Trim();
        }
    }
}