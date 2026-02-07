# BlazorRTE - Professional Rich Text Editor for Blazor

Current: **Native Blazor • 53 Features • Zero JavaScript Dependencies • Production-Ready**

[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE.txt)

## 🚀 Quick Start

⚠️ Important:** BlazorRTE requires interactive rendering for Blazor Server apps. Add `@rendermode InteractiveServer` to your page.

### Installation

dotnet add package BlazorRTE

### Basic Usage

```
@page "/editor" 

@using BlazorRTE.Components 
@rendermode InteractiveServer  @* Required for JS interop! *@

<RichTextEditor @bind-Value="@content" Placeholder="Start typing..." />

@code { private string content = ""; }
 ```
**Important:** BlazorRTE requires interactive rendering. Add `@rendermode InteractiveServer` to your page or component.

**Why is @rendermode required?**
- BlazorRTE uses JavaScript interop for contenteditable functionality
- Static SSR mode won't work - the component needs client-side interactivity
- Supported modes: `InteractiveServer`, `InteractiveWebAssembly`, or `InteractiveAuto`

## 🎯 Features

- ✅ Rich text formatting (Bold, Italic, Underline, Strikethrough)
- ✅ Headings, Lists, Alignment
- ✅ Text & Highlight Colors
- ✅ Links, Horizontal Rules
- ✅ Font Family & Size
- ✅ Undo/Redo
- ✅ **🎭 Emoji Picker** - 1800+ emojis with search
- ✅ **⚡ Emoji Autocomplete** - Type `:smile` for inline suggestions
- ✅ Character & Word Count
- ✅ **WCAG 2.1 AA Compliant** - Full accessibility support
- ✅ **Industry Standard UX** - Follows Word/Google Docs patterns

> 📖 See [ACCESSIBILITY.md](./ACCESSIBILITY.md) for detailed compliance information.

## 📖 Documentation

**Full API Documentation:** [Docs/FEATURES.md](Docs/FEATURES.md)

### Component Parameters
```
[Parameter] public string Value { get; set; } 
[Parameter] public EventCallback<string> ValueChanged { get; set; } 
[Parameter] public string Placeholder { get; set; } = "Type your message..."; 
[Parameter] public bool ShowToolbar { get; set; } = true; 
[Parameter] public int MaxLength { get; set; } = 5000;
[Parameter] public bool ShowCharacterCount { get; set; } = true; 
[Parameter] public string AriaLabel { get; set; } = "Rich text editor";
```
### Public Methods

```	
await ClearAsync();           // Clear all content
await FocusAsync();           // Focus the editor
string text = GetPlainText(); // Get plain text without HTML
```
### Height Control
```
<!-- Default: 200px min, 600px max (industry standard) --> <RichTextEditor @bind-Value="@content" />
<!-- Custom heights --> <RichTextEditor MinHeight="300px" MaxHeight="800px" />
<!-- Values without 'px' are auto-converted --> <RichTextEditor MinHeight="300" MaxHeight="800" />
```
--- 

## ⌨️ Keyboard Shortcuts

The Rich Text Editor supports 27+ industry-standard keyboard shortcuts:

### History
- `Ctrl+Z` - Undo
- `Ctrl+Y` or `Ctrl+Shift+Z` - Redo

### Headings
- `Ctrl+Alt+0` - Normal Text
- `Ctrl+Alt+1` - Heading 1
- `Ctrl+Alt+2` - Heading 2
- `Ctrl+Alt+3` - Heading 3

### Font Size
- `Ctrl+Shift+>` - Increase Font Size
- `Ctrl+Shift+<` - Decrease Font Size

### Text Formatting
- `Ctrl+B` - Bold
- `Ctrl+I` - Italic
- `Ctrl+U` - Underline
- `Ctrl+Shift+X` - Strikethrough
- `Ctrl+=` - Subscript
- `Ctrl+Shift+=` - Superscript

### Lists & Indentation
- `Ctrl+Shift+8` - Bullet List
- `Ctrl+Shift+7` - Numbered List
- `Ctrl+[` - Decrease Indent
- `Ctrl+]` - Increase Indent

### Alignment
- `Ctrl+L` - Align Left
- `Ctrl+E` - Align Center
- `Ctrl+R` - Align Right
- `Ctrl+J` - Justify

### Insert
- `Ctrl+K` - Insert Link
- `Ctrl+Shift+K` - Remove Link
- `Ctrl+Shift+E` - **Toggle Emoji Picker** 🆕
- `Ctrl+Enter` - Horizontal Rule

### Utility
- `Ctrl+\` - Clear Formatting

> **Note:** On macOS, use `Cmd` instead of `Ctrl`.

[See full documentation](docs/keyboard-shortcuts.md)

## 🎭 Emoji Support

BlazorRTE includes **two ways** to insert emojis:

### 1. Emoji Picker (Toolbar Button)
Click the 😀 button in the toolbar to open a searchable emoji picker with:
- ✅ **1800+ emojis** organized by category
- ✅ Search by name or keyword
- ✅ Recently used emojis
- ✅ Full keyboard navigation
- ✅ Smart positioning (viewport-aware)

**Keyboard Shortcut:** `Ctrl+Shift+E` - Toggle emoji picker

:smile → 😊 :heart → ❤️ :rocket → 🚀 :thumbs → 👍


### 2. Emoji Autocomplete (Inline Shortcodes)
Type `:` followed by 2+ characters to trigger inline autocomplete:

**Features:**
- ✅ Appears at cursor position
- ✅ 10 best matches shown
- ✅ Keyboard navigation (`↑` `↓` `Enter` `Esc`)
- ✅ Click to select
- ✅ Auto-positioning (stays on screen)

**Quick Emoticons** (single character):
- `:)` → 😊
- `:(` → 😔
- `:D` → 😃
- `;)` → 😉
- `<3` → ❤️
- `:P` → 😛

> **Note:** Emoji data is embedded (no external dependencies). Works offline!


## 🧪 Testing

BlazorRTE includes comprehensive unit tests using **bUnit** and **xUnit**.

```
dotnet test

```

**Test Coverage:**
- ✅ **54 Unit Tests** covering all major functionality
- ✅ **100% Pass Rate** (54/54 tests passing) ⭐
- ✅ Component rendering & initialization
- ✅ Accessibility (ARIA attributes, keyboard navigation)
- ✅ Security (XSS prevention, HTML sanitization)
- ✅ UI components (buttons, dropdowns, toolbar)
- ✅ Event handling and state management

**Test Breakdown:**
- **RichTextEditorTests** (41 tests) - Component functionality
- **HtmlSanitizerTests** (10 tests) - XSS prevention & sanitization
- **Additional Tests** (3 tests) - Integration & utilities
 

## 📜 License

### Community Edition (Free - GPL v3)
- ✅ All 53 features included
- ✅ Free for open-source projects
- ✅ Community support via GitHub Issues
- ⚠️ **GPL v3 Requirement:** Your application must also be open-source under GPL v3

**See [LICENSE.txt](LICENSE.txt) for full GPL v3 terms.**

### Commercial License (Coming in v1.1.0)
For proprietary/closed-source applications, commercial licensing will be available in version 1.1.0.

**Planned pricing tiers:**
- **Professional** (~$79-99/year): Commercial license + email support
- **Business** (~$149-199/year): Priority support + phone/video
- **Enterprise** (~$499+/year): Source code + custom development + SLA

**For early access or enterprise licensing inquiries:**  
- 📧 Email: licensing@loneworx.com  
- 🌐 Website: https://www.loneworx.com  (Coming Soon)
- 📁 GitHub: https://github.com/simscon1/BlazorRTE

## 🛠️ Development

### Build from Source

git clone https://github.com/simscon1/BlazorRTE.git cd BlazorRTE dotnet build

## 🤝 Contributing

Contributions are welcome! Please submit pull requests to our GitHub repository.

## 📞 Support

- **Community (GPL v3):** [GitHub Issues](https://github.com/simscon1/BlazorRTE/issues)
- **Commercial Inquiries:** licensing@loneworx.com

## 🙏 Acknowledgments

- [Heroicons](https://heroicons.com/) - Beautiful SVG icons (MIT License)
- [Material Icons](https://fonts.google.com/icons) - Link icon (Apache 2.0)

---

**Built with ❤️ for the Blazor community by [LoneWorx LLC](https://loneworx.com)**

