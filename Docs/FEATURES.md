# BlazorRTE - Feature List

**Professional Rich Text Editor for Blazor**  
Zero JavaScript dependencies • 51 Features • Production-Ready

---

## 🎯 Current Version: v1.1

**Total Features:** 51 ✅  
**Visual Quality:** Commercial-grade ✅  
**License:** GPL v3 (Community) / Commercial (Paid)  
**Status:** Ready for separation & NuGet publication

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
- ✅ **Font family selector** - 10 web-safe fonts
  - Sans-serif: Arial, Helvetica, Tahoma, Trebuchet MS, Verdana
  - Serif: Garamond, Georgia, Times New Roman
  - Monospace: Courier New
  - Display: Impact
- ✅ **Font size selector** - 6 preset sizes
  - Small (10px), Normal (14px), Medium (16px)
  - Large (18px), X-Large (24px), XX-Large (32px)

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
- ✅ **Smart color picker positioning** - Auto-detects edges, repositions
- ✅ **Compact toolbar design** - Fits in one row on desktop
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
[Parameter] public string Placeholder { get; set; } 
[Parameter] public bool ShowToolbar { get; set; } 
[Parameter] public bool ShowCharacterCount { get; set; } 
[Parameter] public int MaxLength { get; set; } 
[Parameter] public string AriaLabel { get; set; }

// Public Methods await ClearAsync();           
// Clear all content await FocusAsync();           
// Focus the editor string text = GetPlainText(); 
// Get text without HTML
```

---

## 🚀 Roadmap

### v1.0 (Current - Ship First!):
- ✅ All 51 features working
- ✅ SVG icons integrated
- ✅ Project separated into BlazorRTE
- ⏳ Add README, LICENSE, NuGet metadata
- ⏳ Commit & tag v1.0.0
- ⏳ Publish to NuGet

### Version 1.1 (Code Quality & Refactoring)
- [ ] Code refactoring - Improve code organization and maintainability
  - Extract FormatCommand enum to separate FormatCommand.cs file (58 values)
  - Create RichTextEditorHelpers.cs for shared utility methods
  - Extract color definitions to reduce duplication
  - Improve testability with isolated helper classes
	
### Version 1.2 (Project Separation) - IN PROGRESS
- [ ] **Separate BlazorRTE project** - Extract into standalone NuGet package
  - Independent versioning
  - Publish as `BlazorRTE` on NuGet
  - Broader market reach (blogs, CMS, forms, email)
- [ ] **GPL v3 licensing + commercial tiers** - License infrastructure
  - Community (Free - GPL v3): All features, GitHub support
  - Professional ($79-99/year): Commercial license, email support
  - Business ($149-199/year): Priority support, phone/video
  - Enterprise ($499+/year): Source code, custom dev, SLA
- [ ] **Code refactoring** - Improve organization
  - Extract FormatCommand enum (58 values)
  - Create helper classes for shared utilities
  - Extract color definitions


### Version 2.0 (Premium Features) - FUTURE
- [ ] **Collaborative editing** - Real-time multi-user editing
- [ ] **Advanced themes** - Pre-built professional themes
- [ ] **Image upload** - Built-in image handling with optimization
- [ ] **Tables** - Advanced table editing
- [ ] **Find & replace** - Search and replace text
- [ ] **Spell check** - Built-in spell checking
- [ ] **Auto-save** - Cloud storage integration
- [ ] **Custom toolbar builder** - User-configurable toolbars
- [ ] **Pre-built integrations** - Azure Blob, S3, etc.

### Version 2.5 (Advanced Features) - FUTURE
- [ ] **Extended font library** - Additional fonts based on user feedback
  - Specialty fonts (Arial Black, Book Antiqua, Palatino)
  - Fun fonts (Comic Sans MS)
  - Additional monospace (Lucida Console)
  - Custom web fonts (Google Fonts integration)
- [ ] **Markdown support** - Import/export Markdown
- [ ] **Templates** - Pre-built document templates
- [ ] **Comments & annotations** - Document review features
- [ ] **Version history** - Track document changes

---

## 📦 Installation

bash dotnet add package BlazorRTE

## 🚀 Quick Start

### Register Services

```
// Program.cs - Community (GPL v3) builder.Services.AddBlazorRTE();
// Or with Commercial License builder.Services.AddBlazorRTE(options => { options.LicenseKey = "PRO-XXXX-XXXX-XXXX"; });
```

---

## 📜 License

### Community Edition (Free - GPL v3)
- All 51 features included
- Free for open-source projects
- GitHub support
- Must open-source your application under GPL v3

### Commercial License (Paid)
For proprietary/closed-source applications:
- **Professional** ($79-99/year): Commercial license + email support
- **Business** ($149-199/year): Priority support + phone/video
- **Enterprise** ($499+/year): Source code + custom development + SLA

**Purchase:** https://blazorrte.dev/pricing

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


