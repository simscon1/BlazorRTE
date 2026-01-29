# BlazorRTE - Professional Rich Text Editor for Blazor

**Native Blazor • 51 Features • Zero JavaScript Dependencies • Production-Ready**

[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE.txt)

## 🚀 Quick Start

### Installation

dotnet add package BlazorRTE

### Basic Usage

@using BlazorRTE.Components
<RichTextEditor @bind-Value="@content" Placeholder="Start typing..." />
@code { private string content = ""; }

## ✨ Features

✅ **51 Features** - Complete formatting toolkit
- Text formatting (bold, italic, underline, strikethrough, sub/superscript)
- Typography (10 fonts, 6 sizes)
- Professional color pickers (9 text colors, 7 highlight colors)
- Alignment (left, center, right, justify)
- Lists (bullet, numbered) with indent/outdent
- Links, horizontal rules, clear formatting
- Undo/redo with keyboard shortcuts
- Character/word count
- Built-in XSS protection

✅ **Professional UI**
- Pixel-perfect SVG icons
- Smart color picker positioning
- Dark mode support
- Responsive toolbar

✅ **Zero Dependencies**
- Native Blazor component (~25KB)
- No JavaScript libraries required
- No external icon fonts

See [FEATURES.md](Docs/FEATURES.md) for complete feature list.

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

## 📜 License

### Community Edition (Free - GPL v3)
- ✅ All 51 features included
- ✅ Free for open-source projects
- ✅ GitHub support
- ⚠️ **GPL v3 License:** Your application must also be GPL v3

### Commercial License (Paid)
For proprietary/closed-source applications:
- **Professional** ($79-99/year): Commercial license + email support
- **Business** ($149-199/year): Priority support + phone/video
- **Enterprise** ($499+/year): Source code + custom development + SLA

**Purchase:** [https://blazorrte.dev/pricing](https://blazorrte.dev/pricing)

See [LICENSE.txt](LICENSE.txt) for full GPL v3 terms.

## 🛠️ Development

### Build from Source

git clone https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorRTE cd BlazorRTE dotnet build

## 🤝 Contributing

Contributions are welcome! Please submit pull requests to our repository.

## 📞 Support

- **Community (GPL v3):** [Azure DevOps Issues](https://dev.azure.com/LoneWorxLLC/LoneWorx/_git/BlazorRTE)
- **Commercial:** support@loneworx.com

## 🙏 Acknowledgments

- [Heroicons](https://heroicons.com/) - Beautiful SVG icons (MIT License)
- [Material Icons](https://fonts.google.com/icons) - Link icon (Apache 2.0)

---

**Built with ❤️ for the Blazor community by [LoneWorx LLC](https://loneworx.com)**

