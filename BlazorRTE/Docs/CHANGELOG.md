# Changelog

All notable changes to BlazorRTE will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[1.0.2]: https://github.com/simscon1/BlazorRTE/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/simscon1/BlazorRTE/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/simscon1/BlazorRTE/releases/tag/v1.0.0