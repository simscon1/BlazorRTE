# BlazorRTE - Rich Text Editor for Blazor

**Native Blazor • Keyboard Accessible • Zero External Dependencies**

[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE.txt)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

## 🌐 Browser Support

| Browser | Status |
|---------|--------|
| Chrome | ✅ Tested |
| Edge | ✅ Tested |
| Firefox | ✅ Tested |
| Safari | ⚠️ Not tested |

## 🚀 Quick Start

**⚠️ Important:** BlazorRTE requires interactive rendering.

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
- Supported modes: `InteractiveServer`, `InteractiveWebAssembly`, `InteractiveAuto`

---

## 🎯 Features

### Text Formatting
- ✅ Bold, Italic, Underline, Strikethrough
- ✅ Subscript & Superscript
- ✅ Headings (H1, H2, H3)
- ✅ Bulleted & Numbered Lists
- ✅ Text Alignment (Left, Center, Right, Justify)
- ✅ Indent / Outdent
- ✅ Text & Highlight Colors (preset palette + custom picker)
- ✅ Font Family (10 web-safe fonts)
- ✅ Font Size (6 sizes)

### Functionality
- ✅ Links with URL prompts
- ✅ Horizontal Rules
- ✅ Undo/Redo
- ✅ **Emoji Picker** - 1800+ emojis via BlazorEmo
- ✅ **Emoji Autocomplete** - Type `:smile` for suggestions
- ✅ Character & Word Count
- ✅ Max length enforcement
- ✅ HTML sanitization (XSS protection)
- ✅ Dark mode support

### Accessibility (v1.2.0)
- ✅ Full keyboard navigation for toolbar
- ✅ ARIA labels on all controls
- ✅ Pending format support (click Bold with no selection, then type)

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action | Chrome | Edge | Firefox |
|----------|--------|--------|------|---------|
| `Ctrl+B` | Bold | ✅ | ✅ | ✅ |
| `Ctrl+I` | Italic | ✅ | ✅ | ✅ |
| `Ctrl+U` | Underline | ✅ | ✅ | ✅ |
| `Ctrl+Z` | Undo | ✅ | ✅ | ✅ |
| `Ctrl+Y` | Redo | ✅ | ✅ | ✅ |
| `Ctrl+Alt+1` | Heading 1 | ✅ | ✅ | ✅ |
| `Ctrl+Alt+2` | Heading 2 | ✅ | ✅ | ✅ |
| `Ctrl+Alt+3` | Heading 3 | ✅ | ✅ | ✅ |
| `Ctrl+L` | Align Left | ✅ | ✅ | ❌ |
| `Ctrl+Enter` | Horizontal Rule | ✅ | ✅ | ❌ |

### Known Issues
- **Firefox `Ctrl+Shift+X`**: Applies strikethrough but also right-aligns text

### Toolbar Navigation (v1.2.0)

| Key | Action |
|-----|--------|
| `←` `→` | Move between buttons |
| `↓` | Open dropdown |
| `Enter` / `Space` | Activate button |
| `Escape` | Close dropdown |
| `Home` | First button |
| `End` | Last button |

---

## 💬 Chat Mode (Enter to Send)

For chat applications where Enter sends the message:

```
<RichTextEditor @bind-Value="@message" 
				BypassEnterKey="true" O
				nEnterKeyPressed="SendMessage" />

@code { 
		private string message = "";
		private async Task SendMessage()
		{
		    // Send the message
		    await SendAsync(message);
		    message = "";
		}
}
```

- `BypassEnterKey="true"` - Enter triggers `OnEnterKeyPressed` instead of newline
- `Shift+Enter` - Insert newline when bypass is enabled

---

## 📖 Component Parameters

---
  
### Component Parameters
```razor 
[Parameter] public string Value { get; set; } 
[Parameter] public EventCallback<string> ValueChanged { get; set; } 
[Parameter] public string Placeholder { get; set; } = "Type your message..."; 
[Parameter] public bool ShowToolbar { get; set; } = true; 
[Parameter] public int MaxLength { get; set; } = 5000; 
[Parameter] public bool ShowCharacterCount { get; set; } = true; 
[Parameter] public string MinHeight { get; set; } = "200px"; 
[Parameter] public string MaxHeight { get; set; } = "600px"; 
[Parameter] public bool DarkMode { get; set; } = false; 
[Parameter] public bool EnableEmojiShortcodes { get; set; } = true; 
[Parameter] public bool BypassEnterKey { get; set; } = false; 
[Parameter] public string AriaLabel { get; set; } = "Rich text editor";

```
### Public Methods

```	
await ClearAsync();           // Clear all content
await FocusAsync();           // Focus the editor
string text = GetPlainText(); // Get plain text without HTML
``` 


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

**Allowed tags:** `p`, `br`, `strong`, `em`, `u`, `s`, `h1-h6`, `ul`, `ol`, `li`, `a`, `hr`, `sub`, `sup`, `span`, `font`

---

## 📜 License

### Community Edition (Free - GPL v3)
- ✅ All 39 features included
- ✅ Free for open-source projects
- ✅ Community support via GitHub Issues
- ⚠️ **GPL v3 Requirement:** Your application must also be open-source under GPL v3

**See [LICENSE.txt](LICENSE.txt) for full GPL v3 terms.**
 
---

## 🙏 Acknowledgments

- [Heroicons](https://heroicons.com/) - Beautiful SVG icons (MIT License)
- [Material Icons](https://fonts.google.com/icons) - Link icon (Apache 2.0)
- [BlazorEmo](https://github.com/simscon1/BlazorEmo) - Emoji picker component