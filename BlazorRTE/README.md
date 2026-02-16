# BlazorRTE - Professional Rich Text Editor for Blazor

**Native Blazor • 51 Features • Zero JavaScript Dependencies • Production-Ready**

[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE.txt)
[![.NET 8+](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

## 🚀 Quick Start

**⚠️ Important:** For Blazor Server, BlazorRTE requires interactive rendering. Add `@rendermode InteractiveServer` to your page.

### Installation

````````dotnetcli
dotnet add package BlazorRTE
````````

### Basic Usage

```razor
@page "/editor" 

@using BlazorRTE.Components
@rendermode InteractiveServer 

@* Required for JS interop! *@
<RichTextEditor @bind-Value="@content" Placeholder="Start typing..." />

@code { private string content = ""; }
 ```
**Important:** BlazorRTE requires interactive rendering. Add `@rendermode InteractiveServer` to your page or component.

**Why is @rendermode required?**
- BlazorRTE uses JavaScript interop for contenteditable functionality
- Static SSR mode won't work - the component needs client-side interactivity
- Supported modes: `InteractiveServer`, `InteractiveWebAssembly`, or `InteractiveAuto`

---

## 🎯 Features

### Text Formatting
- ✅ Rich text formatting (Bold, Italic, Underline, Strikethrough)
- ✅ Subscript & Superscript
- ✅ Headings (H1, H2, H3), Lists, Alignment
- ✅ Text & Highlight Colors (9 + 7 preset colors)
- ✅ Font Family (10 web-safe fonts) & Font Size (6 sizes)

### Functionality
- ✅ Links (with URL prompts), Horizontal Rules
- ✅ Undo/Redo (Ctrl+Z/Y)
- ✅ **🎭 Emoji Picker** - 1800+ emojis with search
- ✅ **⚡ Emoji Autocomplete** - Type `:smile` for inline suggestions
- ✅ Character & Word Count
- ✅ Max length enforcement
- ✅ HTML sanitization (XSS protection)

### Dynamic Toolbar Indicators ✨
- ✅ **Font size button** - Shows current size (10, 14, 16, 18, 24, 32)
- ✅ **Heading button** - Shows current level (¶, H1, H2, H3)
- ✅ **Text color button** - Shows current color with colored underline
- ✅ **Highlight button** - Shows current color with colored background

### Developer Experience
- ✅ **Two-way data binding** (`@bind-Value`)
- ✅ **Comprehensive API** - 20+ event callbacks, methods, parameters
- ✅ **Dark mode support** - Automatic theme switching
- ✅ **Responsive design** - Works on desktop, tablet, mobile
- ✅ **Zero dependencies** - ~25KB, fully self-contained 

---

## 💬 Chat Integration - Enter Key Bypass (NEW in v1.1.4)

Perfect for chat applications where Enter sends the message:

- **Auto-submit on Enter** - Send messages with the Enter key
- **Shift+Enter** - Insert newline
- **Control over behavior** - `EnterKeyBehavior` parameter:
  - `EnterKeyBehavior.AlwaysSubmit` - Always send on Enter
  - `EnterKeyBehavior.WithModifier` - Send only with Shift/Ctrl
  - `EnterKeyBehavior.NeverSubmit` - Disable Enter to send

**Example:**
```razor
<RichTextEditor @bind-Value="@message" EnterKeyBehavior="EnterKeyBehavior.WithModifier" />
```

---
 
## 📖 Documentation

**Full API Documentation:** [Docs/FEATURES.md](Docs/FEATURES.md)

### Component Parameters
```razor
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
```razor
<!-- Default: 200px min, 600px max (industry standard) --> <RichTextEditor @bind-Value="@content" />
<!-- Custom heights --> <RichTextEditor MinHeight="300px" MaxHeight="800px" />
<!-- Values without 'px' are auto-converted --> <RichTextEditor MinHeight="300" MaxHeight="800" />
```
--- 

 


 ## 🎭 Emoji Support

BlazorRTE includes **two ways** to insert emojis:

### 1. Emoji Picker (Toolbar Button)
Click the 😀 button in the toolbar to open a searchable emoji picker with:
- ✅ **1800+ emojis** organized by category
- ✅ Search by name or keyword
- ✅ Recently used emojis (persisted)
- ✅ Full keyboard navigation
- ✅ Smart positioning (viewport-aware) 

**Keyboard Shortcut:** `Ctrl+Shift+E`

### 2. Emoji Autocomplete (Inline Shortcodes)

**Quick Emoticons** (`:` + 1 character) - Instant replacement:
:)  → 😊 :(  → 😔 :D  → 😃 ;)  → 😉 <3  → ❤️ :P  → 😛

**Autocomplete Search** (`:` + 2+ characters) - Shows popup with suggestions:
:smile  → 😊 (+ 9 more matches) :heart  → ❤️ (+ 9 more matches) :rocket → 🚀 (+ 9 more matches) :thumbs → 👍 (+ 9 more matches)

**Autocomplete Features:**
- ✅ Appears at cursor position
- ✅ Shows 10 best matches
- ✅ Keyboard navigation (`↑` `↓` `Enter` `Esc`)
- ✅ Click to select
- ✅ Auto-positioning (stays on screen)
- ✅ Fuzzy matching on emoji names and keywords

> **Note:** Emoji data is embedded (no external dependencies). Works offline!

---

## 🧪 Testing

BlazorRTE includes comprehensive unit tests using **bUnit** and **xUnit**.

**Test Coverage:**
- ✅ **54 Unit Tests** covering all major functionality
- ✅ **100% Pass Rate** (54/54 tests passing) ⭐
- ✅ Component rendering & initialization 
- ✅ Security (XSS prevention, HTML sanitization)
- ✅ UI components (buttons, pickers, toolbar)
- ✅ Event handling and state management

**Test Breakdown:**
- **RichTextEditorTests** (41 tests) - Component functionality
- **HtmlSanitizerTests** (10 tests) - XSS prevention & sanitization
- **Additional Tests** (3 tests) - Integration & utilities

---

## 🔒 Security

BlazorRTE includes **enterprise-grade XSS protection**:

- ✅ **Whitelist-based HTML sanitization**
- ✅ **Script tag removal** (`<script>`, event handlers)
- ✅ **Dangerous tag filtering** (`<iframe>`, `<object>`, `<embed>`)
- ✅ **JavaScript protocol blocking** (`javascript:` URLs)
- ✅ **Attribute sanitization** (removes `onclick`, `onerror`, etc.)

**Allowed tags:** `p`, `br`, `strong`, `em`, `u`, `s`, `h1-h3`, `ul`, `ol`, `li`, `a`, `hr`, `sub`, `sup`, `span`, `font`

---

## 📜 License

### Community Edition (Free - GPL v3)
- ✅ All 51 features included
- ✅ Free for open-source projects
- ✅ Community support via GitHub Issues
- ⚠️ **GPL v3 Requirement:** Your application must also be open-source under GPL v3

**See [LICENSE.txt](LICENSE.txt) for full GPL v3 terms.**
 
---

## 🙏 Acknowledgments

- [Heroicons](https://heroicons.com/) - Beautiful SVG icons (MIT License)
- [Material Icons](https://fonts.google.com/icons) - Link icon (Apache 2.0)
- [BlazorEmo](https://github.com/simscon1/BlazorEmo) - Emoji picker component

---
 