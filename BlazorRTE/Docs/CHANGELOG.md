# Changelog

All notable changes to BlazorRTE will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.1] - 2026-02-22

### Fixed
- 🐛 Emoji autocomplete popup now highlights the first item on open (was highlighting last item)

## [1.2.0] - 2026-02-20

### Added
- ♿ **Full keyboard accessibility for toolbar** (WCAG 2.1 compliant)
  - Arrow key navigation between toolbar buttons
  - ArrowDown opens dropdown menus
  - Enter/Space activates buttons
  - Escape closes dropdowns
  - Home/End jumps to first/last button
- ✨ **Pending format support** - Click Bold with no selection, then type
- 🦊 **Firefox Ctrl+B/I/U support** - Prevents browser bookmark/info shortcuts
- ⬆️⬇️ **Emoji autocomplete arrow key navigation** - Navigate suggestions with keyboard
- 🔗 **Source Link** for NuGet package debugging
- 📜 **GPL-3.0 license headers** in all source files

### Fixed
- Firefox browser shortcuts conflicting with editor formatting (Ctrl+B/I/U)
- Emoji autocomplete arrow keys not working
- Toolbar button focus management

### Changed
- Emoji autocomplete selection styling now matches heading/font dropdowns (blue highlight)
- Updated feature count to 39 (accurate count)
- Improved documentation accuracy (removed unverified claims)

## [1.1.4] - 2026-02-15

### Removed
- Keyboard accessibility (temporarily removed due to issues - reimplemented in 1.2.0)

### Fixed
- Dependency issues

## [1.1.1] - 2026-02-06

### Added
- ⌨️ **27 keyboard shortcuts** matching industry standards
- 🎭 **Emoji Picker** - 1800+ emojis with search (via BlazorEmo)
- ⚡ **Emoji Autocomplete** - Inline shortcode suggestions
- 🧪 **Comprehensive Unit Test Suite** (54 tests, 100% pass rate)
- (all features from both 1.1.0 and 1.1.1)

### Fixed
- (all fixes from both versions)

### Changed
- **Emoji autocomplete improvements**
  - 8px gap between cursor and popup (reduced from 24px)
  - Viewport coordinate system for accurate positioning
  - Checks against `window.innerWidth`/`window.innerHeight` for boundaries
- **EventsDemo.razor optimizations**
  - Removed `_activeFormats` display from statistics panel
  - Uses Blazor's natural render batching

### Testing
- **Run Tests:** `dotnet test`
- **With Coverage:** `dotnet test --collect:"XPlat Code Coverage"`
- **Result:** 54/54 tests passing (100%) ✅
- **Target Framework:** .NET 10
- **Dependencies:** bUnit 2.5.3, xUnit 2.9.3, coverlet.collector 6.0.4

### Documentation
- Updated README.md with emoji features and testing section
- Added `Docs/TESTING.md` with comprehensive testing guide
- Updated feature count to 53 features (added emoji picker + autocomplete)
- Documented known issues with `document.execCommand()` deprecation

## [1.1.0] - 2026-02-02

### Added
- ⌨️ **27 keyboard shortcuts** matching industry standards (Word, Google Docs)
  - Text formatting: `Ctrl+B` (Bold), `Ctrl+I` (Italic), `Ctrl+U` (Underline), `Ctrl+Shift+X` (Strikethrough)
  - Subscript/Superscript: `Ctrl+=`, `Ctrl+Shift+=`
  - Alignment: `Ctrl+L/E/R/J` (Left/Center/Right/Justify)
  - Lists: `Ctrl+Shift+8/7` (Bullet/Numbered), `Ctrl+[/]` (Indent/Outdent)
  - Headings: `Ctrl+Alt+0/1/2/3` (Normal/H1/H2/H3)
  - Font size: `Ctrl+Shift+>/<` (Increase/Decrease)
  - Insert: `Ctrl+K` (Link), `Ctrl+Shift+K` (Remove Link), `Ctrl+Shift+E` (Emoji Picker), `Ctrl+Enter` (Horizontal Rule)
  - Utility: `Ctrl+\` (Clear Formatting)
- ♿ **Voice control support** (WCAG 2.5.3 compliance)
  - Added `.sr-only` labels to all 15 icon-only buttons
  - Screen reader-friendly button names match aria-labels
  - Dragon NaturallySpeaking and Windows Speech Recognition compatible
- **Heading picker UI redesign**
  - Replaced native `<select>` with custom button + palette
  - Matches font/size picker visual pattern
  - Shows current heading level (H1/H2/H3/¶)
  - Full keyboard navigation (arrows, Home, End, Escape)
- 🎭 **Emoji Picker** - 1800+ emojis with search
  - Searchable emoji picker in toolbar
  - Recently used emojis tracking
  - Category organization
  - Full keyboard navigation
- ⚡ **Emoji Autocomplete** - Inline shortcode suggestions
  - Type `:smile` for autocomplete popup
  - 10 best matches shown
  - Cursor-aware positioning
  - Quick emoticons (`:)`, `:(`, `:D`, etc.)

### Fixed
- 🐛 Keyboard shortcuts (Ctrl+B/I/U/Z/Y) not working when typing
  - JavaScript now intercepts and prevents browser defaults
- 🐛 Cursor jumping to end of editor after applying formatting
  - Smart selection restoration only when needed
- 🐛 Font family and font size pickers closing immediately on click
  - Added `@onclick:stopPropagation` to prevent parent handlers
- 🐛 Selection lost when using toolbar buttons vs keyboard shortcuts
  - Unified selection management logic

### Changed
- Removed 8 unused accessibility tracking fields
  - Deleted `isBold`, `isItalic`, `isUnderline`, `isSubscript`, `isSuperscript`, `alignment` (write-only)
  - Removed `IsFormatActiveAsync()` method (replaced by synchronous version)
  - Removed duplicate `SelectHeadingLevel()` method
- Improved selection preservation logic
  - Only restores saved selection when no active selection exists
  - Prevents overwriting current user selection with stale data
- Updated all tooltips to include keyboard shortcuts
  - Example: "Bold (Ctrl+B)", "Strikethrough (Ctrl+Shift+X)"

### Documentation
- Added [keyboard-shortcuts.md](keyboard-shortcuts.md) reference guide
- Updated README.md with keyboard shortcuts section
- Added comparison table with MS Word and Google Docs

## [1.0.2] - 2026-02-01

### Added
- Full keyboard navigation for toolbar following WAI-ARIA Toolbar Pattern
- Arrow key navigation between all 24 toolbar buttons
- Home/End keys to jump to first/last toolbar button
- Roving tabindex implementation for accessibility compliance
- `data-toolbar-item` attributes for reliable element targeting
- `Ctrl+K` keyboard shortcut for Insert Link (browser may intercept)

### Fixed
- Tab key now properly enters toolbar at Undo button
- Tab key exits toolbar to editor content area
- Left/Right arrow keys no longer change heading dropdown value
- Focus index resets when re-entering toolbar via Tab

### Changed
- Highlight color button icon now uses fixed 18x18px square dimensions

### Documentation
- Added Keyboard Navigation section to ACCESSIBILITY.md
- Added Keyboard Shortcuts table to README.md
- Updated toolbar navigation documentation

## [1.0.1] - 2026-01-29

### Fixed
- Subscript/superscript toggle now properly removes formatting when clicked again
- Subscript and superscript can no longer be applied simultaneously
- Added proper state detection to prevent conflicting sub/super tags

### Changed
- Default `MaxHeight` changed from `300px` to `600px` (matches industry standard)
- Height management now follows TinyMCE/CKEditor behavior (200-600px range)

### Added
- `MinHeight` parameter for customizable minimum editor height (default: 200px)
- `MaxHeight` parameter for customizable maximum editor height (default: 600px)
- Automatic CSS unit detection - supports px, em, rem, vh, % (auto-adds 'px' if omitted)
- Internal scrollbar appears when content exceeds `MaxHeight`

### Documentation
- Added `@rendermode InteractiveServer` requirement to README and FEATURES
- Documented why interactive rendering is required (JavaScript interop dependency)
- Listed all supported render modes (InteractiveServer, InteractiveWebAssembly, InteractiveAuto)
- Clarified that Static SSR is not supported
- Added height control parameter documentation with examples
- Updated Component API section in FEATURES.md

## [1.0.0] - 2026-01-28

### Added
- Initial release with 51 formatting features
- **Text Formatting:** Bold, Italic, Underline, Strikethrough, Subscript, Superscript
- **Headings:** H1, H2, H3, Paragraph with dropdown selector
- **Lists:** Bulleted lists, Numbered lists, Indent, Outdent
- **Typography:** 10 web-safe fonts, 6 font sizes (10px-32px)
- **Colors:** Text color picker (9 colors), Background/highlight color picker (7 colors)
- **Alignment:** Left, Center, Right, Justify
- **Links:** Smart URL detection, auto-linking
- **Document Management:** Undo, Redo, Character count, Word count, Max length enforcement
- **Security:** Built-in XSS protection with HTML sanitization
- **UI Features:** Professional SVG icons (17 icons), Dark mode support, Smart color picker positioning
- **Keyboard Shortcuts:** Ctrl+B (Bold), Ctrl+I (Italic), Ctrl+U (Underline), Ctrl+Z (Undo), Ctrl+Y (Redo)
- **Component API:** Two-way data binding, Programmatic control (ClearAsync, FocusAsync, GetPlainText)
- **GPL v3 License** (Community Edition)

### Features
- Zero JavaScript library dependencies (~25KB)
- Native Blazor implementation (not a wrapper)
- Scoped CSS (no conflicts with host app)
- Responsive design (desktop, tablet, mobile)
- Compact toolbar (fits in one row)
- Click-outside-to-close for all popup pickers
- Paste as plain text (strips external formatting)
- Accessible (ARIA labels, keyboard navigation)

[1.2.1]: https://github.com/simscon1/BlazorRTE/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/simscon1/BlazorRTE/compare/v1.1.4...v1.2.0
[1.1.4]: https://github.com/simscon1/BlazorRTE/compare/v1.1.1...v1.1.4
[1.1.1]: https://github.com/simscon1/BlazorRTE/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/simscon1/BlazorRTE/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/simscon1/BlazorRTE/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/simscon1/BlazorRTE/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/simscon1/BlazorRTE/releases/tag/v1.0.0