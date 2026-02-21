# BlazorRTE - Feature List

**Professional Rich Text Editor for Blazor**  
Zero JavaScript dependencies • 51 Features • Production-Ready

---

## 🎯 Current Version: v1.1.2 (Bug Fix & UX Release)

**Release Status:** Ready for NuGet publication ✅  
**Release Date:** February 2026  
**Total Features:** 51 ✅  
**Visual Quality:** Commercial-grade ✅  
**Code Quality:** Refactored & optimized ✅  
**Accessibility:** Full ARIA Implementation ✅  
**License:** GPL v3 (Community Edition Only)  
**No service registration required!** BlazorRTE works out of the box.

---

## ✅ Core Features (51 Total)

### Text Formatting ✅
- ✅ **Bold** (Ctrl+B) - Strong emphasis, SVG icon
- ✅ **Italic** (Ctrl+I) - Emphasis, SVG icon
- ✅ **Underline** (Ctrl+U) - Underlined text, SVG icon
- ✅ **Strikethrough** - Crossed-out text, SVG icon
- ✅ **Subscript** (x₂) - Chemical formulas, mathematical notation
- ✅ **Superscript** (x²) - Exponents, footnotes

### Headings & Blocks ✅
- ✅ **Heading 1** - Large section headers
- ✅ **Heading 2** - Subsection headers
- ✅ **Heading 3** - Minor headers
- ✅ **Paragraph** - Normal text blocks
- ✅ **Heading selector dropdown** - Easy switching between text styles

### Lists ✅
- ✅ **Bullet list** - Unordered lists, distinct SVG icon
- ✅ **Numbered list** - Ordered lists, distinct SVG icon
- ✅ **Indent** - Increase indentation, clear right arrow SVG
- ✅ **Outdent** - Decrease indentation, clear left arrow SVG
- ✅ **Horizontal rule** - Insert divider line, SVG icon

### Typography ✅
- ✅ **Font family picker** - 10 web-safe fonts, button+popup interface
  - Button shows "Aa" icon
  - Dropdown palette with font previews (each option shows in its font)
  - Sans-serif: Arial, Helvetica, Tahoma, Trebuchet MS, Verdana
  - Serif: Garamond, Georgia, Times New Roman
  - Monospace: Courier New
  - Display: Impact
  - Smart edge detection (auto-repositions to stay visible)
  - Saves ~140px toolbar space vs dropdown
- ✅ **Font size picker** - 6 preset sizes, button+popup interface
  - Button shows current size ("14")
  - Dropdown palette with size options
  - Small (10px), Normal (14px), Medium (16px)
  - Large (18px), X-Large (24px), XX-Large (32px)
  - Smart edge detection (auto-repositions to stay visible)
  - Saves ~100px toolbar space vs dropdown

### Colors & Highlighting ✅
- ✅ **Text color dropdown** - 9 preset colors
  - Black, Red, Orange, Yellow, Green, Blue, Purple, Gray, White
  - Professional dropdown palette interface
  - Visual indicator: Black A with red underline
  - Smart edge detection and auto-positioning
  - Click outside to close
- ✅ **Background/Highlight color dropdown** - 7 preset colors
  - Yellow, Green, Blue, Pink, Orange, Gray, None
  - Professional dropdown palette interface
  - Visual indicator: Black A on yellow background
  - Auto-repositions to stay visible in container
  - Click outside to close
- ✅ **Space-efficient design** - Dropdowns save ~330px toolbar width
- ✅ **Smart positioning** - Detects container edges, aligns automatically

### Alignment & Layout ✅
- ✅ **Align left** - Left-align text, distinct SVG icon
- ✅ **Align center** - Center text, distinct SVG icon
- ✅ **Align right** - Right-align text, distinct SVG icon
- ✅ **Justify** - Justify text (full width), distinct SVG icon

### Links ✅
- ✅ **Insert link** - Button with SVG icon
- ✅ **Smart URL detection** - Automatically adds https://
- ✅ **Auto-link pasted URLs** - Detects and formats

### Document Management ✅
- ✅ **Undo** (Ctrl+Z) - Curved left arrow SVG icon
- ✅ **Redo** (Ctrl+Y, Ctrl+Shift+Z) - Curved right arrow SVG icon
- ✅ **Character count** - Real-time counter
- ✅ **Word count** - Real-time counter
- ✅ **Max length enforcement** - Prevents input beyond limit
- ✅ **Clear formatting** - Remove all formatting, X mark SVG icon

### Security Features ✅
- ✅ **XSS protection** - HTML sanitization
- ✅ **Whitelist-based filtering** - Only safe tags allowed
- ✅ **Script tag removal** - Automatic stripping
- ✅ **Event handler removal** - No onclick, onerror, etc.
- ✅ **JavaScript protocol blocking** - Prevents javascript: URLs
- ✅ **Dangerous tag filtering** - Removes iframe, object, embed, etc.

### UX Features ✅
- ✅ **Active state indicators** - Toolbar buttons show current format
- ✅ **Selection preservation** - Formatting works when clicking toolbar
- ✅ **Professional SVG icons** - 17 pixel-perfect vector icons
- ✅ **Compact toolbar with 4 pickers** - Button+popup for colors and fonts
  - Text color picker (9 colors)
  - Background color picker (7 colors)
  - Font size picker (6 sizes)
  - Font family picker (10 fonts)
- ✅ **Smart popup positioning** - All 4 pickers auto-detect container edges
  - Centers by default
  - Aligns left when near left edge
  - Aligns right when near right edge
  - Works across toolbar wrapping scenarios
- ✅ **Toolbar space optimization** - Compact design saves ~224px width
  - Font size dropdown → button (saves ~100px)
  - Font family dropdown → button (saves ~140px)
  - Total: Fits in one row on most screens
- ✅ **Click-outside-to-close** - All popups close when clicking elsewhere
- ✅ **Paste as plain text** - Strips external formatting automatically
- ✅ **Placeholder text** - Customizable empty state
- ✅ **Two-way data binding** - `@bind-Value` support
- ✅ **Focus management** - Programmatic focus control
- ✅ **Toolbar toggle** - Show/hide formatting toolbar
- ✅ **Responsive design** - Works on desktop, tablet, mobile
- ✅ **Dark mode support** - Automatic styling, icons adapt via currentColor

### Visual Design ✅
- ✅ **Professional SVG icons** - Vector icons for all toolbar buttons
  - Heroicons for most buttons (outline style)
  - Material Icons for link icon
  - Consistent 18px sizing via CSS
  - Icons: Bold, Italic, Underline, Strikethrough
  - Icons: Undo, Redo
  - Icons: Bullet List, Numbered List (clearly distinct)
  - Icons: Indent, Outdent (directional arrows)
  - Icons: Align Left, Center, Right, Justify (all different)
  - Icons: Horizontal Rule, Insert Link, Clear Formatting
  - Unicode kept for: Subscript (x₂), Superscript (x²)
- ✅ **Dark mode compatibility** - All icons use `stroke="currentColor"`
- ✅ **Compact toolbar** - Single row, responsive wrapping

### Keyboard Shortcuts ✅
- ✅ **Ctrl+B** - Bold
- ✅ **Ctrl+I** - Italic
- ✅ **Ctrl+U** - Underline
- ✅ **Ctrl+Z** - Undo
- ✅ **Ctrl+Y** - Redo
- ✅ **Ctrl+Shift+Z** - Redo (alternate)

### Component API ✅

```
// Parameters 
[Parameter] public string Value { get; set; } 
[Parameter] public EventCallback<string> ValueChanged { get; set; } 
[Parameter] public string Placeholder { get; set; } = "Type your message..."; 
[Parameter] public bool ShowToolbar { get; set; } = true; 
[Parameter] public bool ShowCharacterCount { get; set; } = true; 
[Parameter] public int MaxLength { get; set; } = 5000; 
[Parameter] public string MinHeight { get; set; } = "200px"; 

// NEW in v1.0.1 
[Parameter] public string MaxHeight { get; set; } = "600px";  
[Parameter] public string AriaLabel { get; set; } = "Rich text editor";

// Public Methods await ClearAsync();           
// Clear all content await FocusAsync();           
// Focus the editor string text = GetPlainText(); 
// Get text without HTML
```

---

**Height Behavior (Industry Standard):**
- Default: 200px minimum, 600px maximum (matches TinyMCE/CKEditor)
- Content scrolls internally when exceeding max height
- Supports CSS units: px, em, rem, vh, %
- Auto-adds 'px' if no unit specified

---

## 🚀 Roadmap

### Version 1.0.0 (Initial Release) - ✅ PUBLISHED
**Release Date:** January 2026

**Core Features:**
- ✅ All 51 core features
- ✅ Professional SVG icons (17 icons)
- ✅ Smart color & font pickers (4 total)
- ✅ Compact toolbar (fits in one row)
- ✅ Built-in XSS protection
- ✅ Dark mode support

**Code Quality:**
- ✅ Refactored architecture (FormatCommand enum, ColorPalettes, HtmlSanitizer)
- ✅ Clean separation of concerns
- ✅ Helper classes at root level

### Version 1.0.1 (Bug Fix Release) - ✅ PUBLISHED
**Release Date:** January 2026

**Fixes:**
- ✅ Subscript/superscript toggle now properly removes formatting
  - Added `IsFormatActiveAsync` to detect active formats
  - Automatically removes opposite format when switching
  - Clicking again properly returns text to normal
  
**Improvements:**
- ✅ Industry-standard height management (200px-600px default)
- ✅ `MinHeight` and `MaxHeight` parameters with flexible CSS unit support
	
**Documentation:**
- ✅ Added `@rendermode InteractiveServer` requirement
- ✅ Explained why interactive rendering is needed

### Version 1.0.2 (Bug Fix Release) - ✅ PUBLISHED
**Release Date:** February 2026

**Fixes:**
- ✅ Minor bug fixes and improvements

### Version 1.1.1 (Emoji Release) - ⏭️ SKIPPED
**Note:** Version 1.1.1 was developed but not published. Features merged into v1.1.2.

**Features developed:**
- Emoji Picker with 1800+ searchable emojis
- Emoji Autocomplete (type `:smile` for suggestions)
- 27 keyboard shortcuts
- 54 unit tests with 100% pass rate

### Version 1.1.2 (Bug Fix & UX Release) - 🎯 CURRENT / READY TO PUBLISH
**Release Date:** February 2026

**Added:**
- ✅ **Emoji Picker** - 1800+ searchable emojis with categories
  - Search by name or keyword
  - Recently used emojis (persisted)
  - Full keyboard navigation
  - Dark mode support
- ✅ **Emoji Autocomplete** - Type `:smile` for inline suggestions
  - 10 best matches shown
  - Keyboard navigation (↑ ↓ Enter Esc)
  - Auto-positioning (viewport-aware)
- ✅ **27 Keyboard Shortcuts** - Full Ctrl+Key support
  - Formatting (Ctrl+B, Ctrl+I, Ctrl+U, etc.)
  - Headings (Ctrl+Alt+0-3)
  - Font size (Ctrl+Shift+> / <)
  - Lists, alignment, links, emoji picker
- ✅ **54 Unit Tests** - bUnit + xUnit
  - 100% pass rate
  - Component, accessibility, security testing
- ✅ **Dynamic font size button** - Shows current size at cursor position
  - Displays: 10, 14, 16, 18, 24, or 32
  - Updates in real-time when moving cursor
  - Matches selected text's font size

**Fixed:**
- ✅ **Link button functionality** - Now prompts user for URL (was hardcoded)
  - Validates text selection before allowing link creation
  - Auto-adds `https://` prefix if missing
  - Shows "Remove link?" dialog when cursor in existing link
  - Properly raises `OnLinkCreated` events
- ✅ **Mouse click fixes for all pickers** - Added `@onmousedown:stopPropagation`
  - Heading picker clicks now work
  - Font family picker clicks now work
  - Font size picker clicks now work
  - Text color picker clicks now work
  - Background color picker clicks now work
  - (Keyboard navigation was already working)
- ✅ **ARIA improvements** - Correct semantics for link button
  - Changed from `aria-pressed` (toggle) to `aria-haspopup="dialog"` (opens prompt)
  - Dynamic `aria-label` based on context ("Insert link" vs "Edit link")

**Accessibility:**
- ✅ Complete ARIA implementation following WAI-ARIA 1.2 patterns
  - All pickers have proper role attributes (listbox/grid)
  - All palette titles have id + aria-labelledby relationships
  - Color swatches use role="gridcell" with aria-label
  - Emoji autocomplete has role="listbox" with aria-selected states
  - Character count uses aria-live="polite"

**Quality:**
- ✅ All 54 unit tests passing
- ✅ Both keyboard and mouse interaction verified
- ✅ Event callbacks properly wired
- ✅ Focus management confirmed

### Version 1.2.0 (Planned - UX Improvements) 📋
**Status:** Planned based on user feedback

**Planned Features:**
- [ ] Dynamic font family button (show current font, not just "Aa")
- [ ] Custom link dialog (replace browser prompt)
  - Link preview
  - Link validation
  - Recent links
  - "Open in new tab" option
  - Edit existing links

**Documentation (as needed):**
- [ ] Security documentation (SECURITY.md)
- [ ] Additional accessibility testing (upon request)

**Note:** This release focuses on UX improvements. Additional testing and certification will be conducted if requested by enterprise customers or if specific issues are reported.

### Version 1.3.0 (Planned - Commercial Licensing) 💼
**Target:** Q3 2026

**Licensing Infrastructure:**
- [ ] License validation service (non-enforcing)
- [ ] License tiers:
  - Community (Free - GPL v3)
  - Professional (~$79-99/year)
  - Business (~$149-199/year)
  - Enterprise (~$499+/year)
- [ ] License management dashboard
- [ ] Email support system

### Version 1.5.0 (Future - Premium Features) 🌟
**Premium features for paid tiers:**
- [ ] Collaborative editing - Real-time multi-user
- [ ] Advanced themes - Professional pre-built themes
- [ ] Image upload - Built-in with optimization
- [ ] Tables - Advanced table editing
- [ ] Find & replace
- [ ] Spell check
- [ ] Custom toolbar configuration

### Version 2.0.0 (Future - Major Features) 🚀
- [ ] Extended font library (Google Fonts integration)
- [ ] Markdown support (import/export)
- [ ] Document templates
- [ ] Comments & annotations
- [ ] Version history
- [ ] Plugin system
- [ ] AI-powered features (grammar, summarization)

---

## 📦 Installation

bash dotnet add package BlazorRTE

## 🚀 Quick Start

### Requirements
- **Blazor Interactive rendering** - Static SSR not supported
- Supports: Server, WebAssembly, or Auto render modes
- .NET 8.0 or higher

### Basic Usage

**⚠️ Important:** Add `@rendermode InteractiveServer` to your page or component.

**Why InteractiveServer is required:**
- BlazorRTE uses JavaScript interop for contenteditable functionality
- Static SSR renders HTML but toolbar buttons won't work
- Component needs client-side event handling and DOM manipulation

**Supported Render Modes:**
- ✅ `@rendermode InteractiveServer` (recommended for most apps)
- ✅ `@rendermode InteractiveWebAssembly` (runs entirely in browser)
- ✅ `@rendermode InteractiveAuto` (starts as Server, upgrades to WASM)
- ❌ Static SSR (not supported - component requires JS interop)

````````

## 📜 License

### Community Edition (Free - GPL v3)
**v1.0.0 is GPL v3 ONLY**

- ✅ All 51 features included
- ✅ Free for open-source projects
- ✅ Community support via GitHub Issues
- ⚠️ **GPL v3 Requirement:** Your application must also be open-source under GPL v3

**See [LICENSE.txt](LICENSE.txt) for full terms.**

### Commercial License (Coming in v1.1.0)
For proprietary/closed-source applications, commercial licensing will be available in version 1.1.0.

**Planned pricing tiers:**
- **Professional** (~$79-99/year): Commercial license + email support
- **Business** (~$149-199/year): Priority support + phone/video
- **Enterprise** (~$499+/year): Source code + custom development + SLA

**For early access or enterprise licensing inquiries:**  
📧 Email: licensing@loneworx.com  
🌐 Website: https://www.loneworx.com  (Coming soon)
📁 GitHub: https://github.com/simscon1/BlazorRTE

---

## 🎯 Why BlazorRTE?

✅ **Native Blazor** - Not a JavaScript wrapper  
✅ **51 Features** - Complete formatting toolkit  
✅ **Zero Dependencies** - ~25KB, no external libraries  
✅ **Security-First** - Built-in XSS protection  
✅ **Professional UI** - SVG icons, smart positioning  
✅ **Dark Mode** - Automatic theme adaptation  
✅ **GPL v3 Free** - Full features for open-source  
✅ **Fair Commercial Pricing** - $79-499/year  

---

**Built with ❤️ for the Blazor community**










