# BlazorRTE Manual Testing Guide

This document provides comprehensive manual testing steps to verify all features of the Rich Text Editor.

---

## 🧪 Test Categories

1. [Basic Text Formatting](#1-basic-text-formatting)
2. [Color Pickers](#2-color-pickers)
3. [Headings & Typography](#3-headings--typography)
4. [Lists & Alignment](#4-lists--alignment)
5. [Links](#5-links)
6. [Emoji](#6-emoji)
7. [Undo/Redo](#7-undoredo)
8. [Keyboard Accessibility](#8-keyboard-accessibility)
9. [Dark Mode](#9-dark-mode)
10. [Edge Cases](#10-edge-cases)
11. [Multiple Instances](#11-multiple-editor-instances)
12. [WASM Toolbar Protection](#12-wasm-toolbar-protection)

---

## 1. Basic Text Formatting

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Bold | Select text → Click Bold (or Ctrl+B) | Text becomes bold, button shows active state | ☐ |
| Italic | Select text → Click Italic (or Ctrl+I) | Text becomes italic | ☐ |
| Underline | Select text → Click Underline (or Ctrl+U) | Text becomes underlined | ☐ |
| Strikethrough | Select text → Click Strikethrough (or Ctrl+Shift+X) | Text has line through | ☐ |
| Subscript | Select text → Click Subscript (or Ctrl+=) | Text moves below baseline | ☐ |
| Superscript | Select text → Click Superscript (or Ctrl+Shift+=) | Text moves above baseline | ☐ |
| Toggle off | Apply format → Click same button again | Format is removed | ☐ |
| Multiple formats | Apply Bold + Italic together | Both formats applied | ☐ |
| Clear formatting | Apply formats → Click Clear Formatting (Ctrl+\) | All formatting removed | ☐ |

---

## 2. Color Pickers

### Text Color

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open palette | Click text color button | Palette opens with 24 colors + custom input | ☐ |
| Select preset | Click any color swatch | Color applied to selected text, palette closes | ☐ |
| Custom color | Click custom color input → Select color | Custom color applied | ☐ |
| Selected indicator | Open with colored text selected | Current color has blue outline | ☐ |
| Click outside | Click anywhere outside palette | Palette closes | ☐ |
| Keyboard: L/R | Arrow Left/Right in grid | Focus moves horizontally | ☐ |
| Keyboard: U/D | Arrow Up/Down in grid | Focus moves vertically by row | ☐ |
| Keyboard: Enter | Focus color → Press Enter | Color applied | ☐ |
| Keyboard: Escape | Press Escape | Closes palette, returns focus to button | ☐ |

### Highlight Color

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open palette | Click highlight button | Palette opens | ☐ |
| Select preset | Click any color swatch | Highlight applied | ☐ |
| "None" option | Click ✕ swatch | Removes highlight (transparent) | ☐ |
| Custom color | Use color input picker | Custom highlight applied | ☐ |
| All keyboard tests | Repeat keyboard tests above | Same behavior as text color | ☐ |

---

## 3. Headings & Typography

### Headings

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open picker | Click heading button (¶) | Dropdown opens | ☐ |
| Heading 1 | Select H1 (or Ctrl+Alt+1) | Large heading applied | ☐ |
| Heading 2 | Select H2 (or Ctrl+Alt+2) | Medium heading applied | ☐ |
| Heading 3 | Select H3 (or Ctrl+Alt+3) | Small heading applied | ☐ |
| Normal/Paragraph | Select Normal (or Ctrl+Alt+0) | Returns to paragraph | ☐ |
| Button label | Apply heading | Button shows H1/H2/H3/¶ | ☐ |

### Font Family

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open picker | Click Aa button | Font list opens | ☐ |
| Select font | Click any font | Font applied to selection | ☐ |
| Font preview | View dropdown | Each option shows in its font | ☐ |

### Font Size

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open picker | Click size button | Size list opens | ☐ |
| Select size | Click any size | Size applied | ☐ |
| Increase | Ctrl+Shift+> | Font gets larger | ☐ |
| Decrease | Ctrl+Shift+< | Font gets smaller | ☐ |
| Button label | Change size | Button shows current size (10/14/16/18/24/32) | ☐ |

---

## 4. Lists & Alignment

### Lists

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Bullet list | Click bullet (or Ctrl+Shift+8) | Creates bulleted list | ☐ |
| Numbered list | Click number (or Ctrl+Shift+7) | Creates numbered list | ☐ |
| Toggle off | Click list button again | Removes list | ☐ |
| Nested list | In list item → Click indent | Creates nested list | ☐ |

### Indentation

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Indent | Click indent (or Ctrl+]) | Content indents right | ☐ |
| Outdent | Click outdent (or Ctrl+[) | Content outdents left | ☐ |

### Alignment

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Align Left | Click align left (or Ctrl+L) | Text left-aligned, button active | ☐ |
| Align Center | Click center (or Ctrl+E) | Text centered | ☐ |
| Align Right | Click right (or Ctrl+R) | Text right-aligned | ☐ |
| Justify | Click justify (or Ctrl+J) | Text justified | ☐ |
| Mutually exclusive | Click different alignment | Only one active at a time | ☐ |

---

## 5. Links

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| No selection | Click link with no text selected | Alert: "Please select text first" | ☐ |
| Create link | Select text → Click link → Enter URL | Link created, text is blue/underlined | ☐ |
| Auto https | Enter URL without protocol | https:// added automatically | ☐ |
| Edit/Remove | Place cursor in link → Click link | Prompt to remove link | ☐ |
| Keyboard | Ctrl+K | Opens link dialog | ☐ |
| Remove shortcut | Ctrl+Shift+K | Removes link | ☐ |

---

## 6. Emoji

### Emoji Picker

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open picker | Click 😀 button (or Ctrl+Shift+E) | Emoji picker opens | ☐ |
| Insert emoji | Click any emoji | Emoji inserted at cursor | ☐ |
| Search | Type in search box | Filters emojis | ☐ |
| Categories | Click category tabs | Shows emojis from category | ☐ |
| Close on click outside | Click outside picker | Picker closes | ☐ |

### Emoji Shortcodes

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| `:)` | Type `:)` | Converts to 😊 | ☐ |
| `:D` | Type `:D` | Converts to 😀 | ☐ |
| `:(` | Type `:(` | Converts to 😞 | ☐ |
| `:P` | Type `:P` | Converts to 😛 | ☐ |

### Emoji Autocomplete

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Trigger | Type `:sm` (2+ chars after colon) | Autocomplete popup appears | ☐ |
| Navigate | Arrow Up/Down | Highlights different suggestions | ☐ |
| Select | Press Enter | Inserts selected emoji | ☐ |
| Dismiss | Press Escape or Backspace | Closes autocomplete | ☐ |

---

## 7. Undo/Redo

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Undo | Make changes → Ctrl+Z | Changes reverted | ☐ |
| Redo | Undo → Ctrl+Y | Changes restored | ☐ |
| Multiple undo | Make 3 changes → Ctrl+Z 3 times | All reverted in order | ☐ |
| Undo button | Click Undo button | Same as Ctrl+Z | ☐ |
| Redo button | Click Redo button | Same as Ctrl+Y | ☐ |

---

## 8. Keyboard Accessibility

### Toolbar Navigation

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Tab into toolbar | Tab from outside editor | First button (Undo) focused | ☐ |
| Arrow Right | Press Arrow Right | Focus moves to next button | ☐ |
| Arrow Left | Press Arrow Left | Focus moves to previous button | ☐ |
| Home | Press Home | Focus moves to first button | ☐ |
| End | Press End | Focus moves to last button | ☐ |
| Wrap around | Arrow Right from last | Returns to first button | ☐ |

### Dropdown Navigation

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Open with Enter | Focus dropdown button → Enter | Dropdown opens | ☐ |
| Open with Arrow Down | Focus dropdown button → Arrow Down | Dropdown opens, first item focused | ☐ |
| Navigate | Arrow Up/Down in dropdown | Focus moves through options | ☐ |
| Select | Enter on option | Option selected, dropdown closes | ☐ |
| Escape | Press Escape | Closes dropdown, returns to button | ☐ |

### Focus Indicators

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Visible focus | Tab through toolbar | Blue outline visible on focused button | ☐ |
| Focus in dropdown | Navigate dropdown | Focus indicator visible | ☐ |
| Dark mode focus | Enable dark mode → Tab | Focus still clearly visible | ☐ |

---

## 9. Dark Mode

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Enable | Set `DarkMode="true"` | Editor switches to dark theme | ☐ |
| Toolbar | Check toolbar | Dark background, light icons | ☐ |
| Content area | Check content area | Dark background, light text | ☐ |
| Color palettes | Open color picker | Dark background on palette | ☐ |
| Emoji picker | Open emoji picker | Dark theme applied | ☐ |
| Footer | Check character count | Visible in dark mode | ☐ |
| Active buttons | Toggle Bold | Active state visible | ☐ |
| Focus indicators | Tab through toolbar | Visible focus rings | ☐ |

---

## 10. Edge Cases

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Empty editor format | Click Bold with no text/selection | No crash, pending format applied | ☐ |
| Max length | Type beyond MaxLength | Stops accepting input, event fired | ☐ |
| Character count | Type text | Count updates in real-time | ☐ |
| Word count | Type words | Word count accurate | ☐ |
| Paste plain text | Copy rich text → Paste in editor | Only plain text inserted | ☐ |
| Drag and drop | Try to drag content into editor | Prevented (no drop) | ☐ |
| Horizontal rule | Ctrl+Enter | HR inserted with new paragraph | ☐ |
| Selection loss | Click toolbar button | Selection preserved after format | ☐ |

---

## 11. Multiple Editor Instances

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Two editors | Add 2 editors on same page | Both render correctly | ☐ |
| Independent content | Type in each | Content stays separate | ☐ |
| Independent formatting | Format text in each | Formatting independent | ☐ |
| Focus switch | Click between editors | Each maintains own state | ☐ |

---

## 12. WASM Toolbar Protection

### Key Repeat Prevention

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Hold Enter on Indent | Focus Indent button → Hold Enter key | Only one indent applied, no freeze/hang | ☐ |
| Hold Enter on Bold | Focus Bold button → Hold Enter key | Only one toggle, no rapid flashing | ☐ |
| Hold Space on Indent | Focus Indent button → Hold Space key | Only one indent applied | ☐ |
| Hold Enter on Undo | Focus Undo button → Hold Enter key | Only one undo applied | ☐ |
| Single press still works | Focus Indent → Press Enter once | Indent applied normally | ☐ |
| Single click still works | Click Indent button once | Indent applied normally | ☐ |
| Rapid single clicks | Click Indent 5 times quickly | 5 indents applied (not dropped) | ☐ |

### Command Interleave Prevention

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| No double execution | Click Bold while another command is running | Second click ignored, no crash | ☐ |
| Flag resets | Execute command → Wait → Execute again | Second command works normally | ☐ |
| Error recovery | Trigger error during command | `_commandInProgress` resets, next command works | ☐ |

### Deferred Toolbar Update (Repeatable Commands)

| Test | Steps | Expected Result | Pass |
|------|-------|-----------------|------|
| Indent toolbar sync | Click Indent → Wait 200ms | Toolbar state updates correctly | ☐ |
| Outdent toolbar sync | Click Outdent → Wait 200ms | Toolbar state updates correctly | ☐ |
| Undo toolbar sync | Click Undo → Wait 200ms | Toolbar state reflects undone content | ☐ |
| Redo toolbar sync | Click Redo → Wait 200ms | Toolbar state reflects redone content | ☐ |
| Value binding | Indent text → Check @bind-Value | Value updated with indented HTML | ☐ |

## 🔧 Quick Test Page

Use this Razor page to test all scenarios:

```
@page "/test"
<h2>Editor Test Page</h2>
<h3>1. Standard Editor (Light Mode)</h3> <RichTextEditor @bind-Value="content1" ShowCharacterCount="true" MaxLength="500" Placeholder="Test formatting here..." /> <p><strong>Value:</strong> @content1</p>
<hr />
<h3>2. Dark Mode Editor</h3> <RichTextEditor @bind-Value="content2" DarkMode="true" ShowCharacterCount="true" />
<hr />
<h3>3. Chat Mode (Enter sends)</h3> <RichTextEditor @bind-Value="content3" BypassEnterKey="true" OnEnterKeyPressed="HandleSend" MinHeight="100px" MaxHeight="200px" Placeholder="Press Enter to send..." /> <p>Messages sent: @messageCount</p>
<hr />
<h3>4. Minimal Editor</h3> <RichTextEditor @bind-Value="content4" ShowCharacterCount="false" ShowToolbar="true" MinHeight="80px" />
@code { string content1 = ""; string content2 = "<p>Pre-loaded <strong>dark mode</strong> content</p>"; string content3 = ""; string content4 = ""; int messageCount = 0;

void HandleSend()
{
    if (!string.IsNullOrWhiteSpace(content3))
    {
        messageCount++;
        Console.WriteLine($"Message {messageCount}: {content3}");
        content3 = "";
    }
}
}
```

---

## ✅ Test Summary

| Category | Total Tests | Passed | Failed |
|----------|-------------|--------|--------|
| Basic Formatting | 9 | ☐ | ☐ |
| Color Pickers | 18 | ☐ | ☐ |
| Headings & Typography | 14 | ☐ | ☐ |
| Lists & Alignment | 11 | ☐ | ☐ |
| Links | 6 | ☐ | ☐ |
| Emoji | 12 | ☐ | ☐ |
| Undo/Redo | 5 | ☐ | ☐ |
| Keyboard Accessibility | 14 | ☐ | ☐ |
| Dark Mode | 8 | ☐ | ☐ |
| Edge Cases | 10 | ☐ | ☐ |
| Multiple Instances | 4 | ☐ | ☐ |
| WASM Toolbar Protection | 15 | ☐ | ☐ |
| **TOTAL** | **126** | ☐ | ☐ |

---

## 📝 Notes

_Use this section to document any issues found during testing:_

| Issue # | Description | Severity | Status |
|---------|-------------|----------|--------|
| 1 | Ctrl+K (Link) not working in any browser | Medium | Known |
| 2 | Ctrl+L (Align Left) not working in Firefox | Low | Known |
| 3 | Ctrl+Enter (HR) not working in Firefox | Low | Known |
| 4 | Ctrl+Shift+X causes right-align in Firefox | Low | Known |
| 5 | Ctrl+Shift+E (Emoji picker) not verified | Low | Needs Test |

---

**Tested by:** ________________  
**Date:** ________________  
**Version:** 1.2.0
