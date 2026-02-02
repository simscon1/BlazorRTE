# Accessibility & Compliance

BlazorRTE is designed to meet industry accessibility standards and follow established UX patterns from major editors.

## WCAG 2.1 Compliance

| Level | Status | Notes |
|-------|--------|-------|
| Level A | ✅ Compliant | All criteria met |
| Level AA | ✅ Compliant | All criteria met |
| Level AAA | ⚠️ Partial | See details below |

### Level AAA Details

| Criterion | Status | Notes |
|-----------|--------|-------|
| 1.4.6 Contrast (Enhanced) | ✅ Met | 7:1 ratio on toolbar buttons |
| 1.4.8 Visual Presentation | ⚠️ Partial | User can customize via CSS variables |
| 1.4.9 Images of Text | ✅ Met | No images of text used |
| 2.1.3 Keyboard (No Exception) | ✅ Met | Full keyboard support |
| 2.4.9 Link Purpose | ✅ Met | Link button has clear label |
| 2.4.10 Section Headings | ✅ Met | Toolbar groups are labeled |
| 3.2.5 Change on Request | ✅ Met | No auto-changes |

**Not Applicable to Component:**
- 3.1.3-3.1.6 (Language) - Content responsibility of host app
- 3.3.5-3.3.6 (Help/Error Prevention) - Host app responsibility

## ARIA Implementation

| Feature | Implementation |
|---------|----------------|
| Toolbar | `role="toolbar"` with `aria-label` |
| Button Groups | `role="group"` with `aria-label` |
| Toggle Buttons | `aria-pressed` state |
| Dropdowns | `aria-expanded`, `aria-haspopup` |
| Dropdown Menus | `role="listbox"` with `role="option"` items |
| Icons | `aria-hidden="true"` on decorative SVGs |
| Editor Area | `role="textbox"`, `aria-multiline`, `aria-label` |
| Character Count | `aria-live="polite"` for screen reader updates |
| Keyboard Navigation | Roving `tabindex` per APG Toolbar Pattern |

## Industry Standard UX

BlazorRTE follows the same patterns used by Microsoft Word, Google Docs, and other major editors:

| Feature | Behavior |
|---------|----------|
| **Toolbar Order** | Undo/Redo → Style → Format → Colors → Paragraph → Alignment → Insert → Clear |
| **Clear Formatting** | Removes inline formatting only (not headings, lists, or alignment) |
| **Alignment Buttons** | Mutually exclusive - only one active at a time |
| **Link Selection** | Underline and color buttons don't highlight for default link styling |
| **Color Buttons** | Highlight when non-default color is applied to selected text |

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+B | Bold |
| Ctrl+I | Italic |
| Ctrl+U | Underline |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| Ctrl+Shift+Z | Redo (alternative) |

## Screen Reader Support

Tested with:
- NVDA
- JAWS
- VoiceOver (macOS)
- Narrator (Windows)

## References

- [WAI-ARIA Authoring Practices Guide (APG) - Toolbar Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/toolbar/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)