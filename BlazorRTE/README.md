# BlazorRTE - Professional Rich Text Editor for Blazor

**Native Blazor • 51 Features • Zero JavaScript Dependencies • Production-Ready**

[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE.txt)

## 🚀 Quick Start

**⚠️ Important:** BlazorRTE requires interactive rendering for Blazer Server apps. Add `@rendermode InteractiveServer` to your page.

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

## 📜 License

### Community Edition (Free - GPL v3)
- ✅ All 51 features included
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

