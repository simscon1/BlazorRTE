// ===== PENDING FORMAT SUPPORT =====
// Handles keyboard-only formatting scenarios

let pendingFormatState = {
    formats: [],
    textColor: null,
    backgroundColor: null,
    fontSize: null,
    fontFamily: null,
    isApplying: false
};

/**
 * Checks if there's actual text selected (not just collapsed cursor).
 */
export function hasTextSelection() {
    const selection = window.getSelection();
    return selection && selection.rangeCount > 0 && !selection.isCollapsed;
}

/**
 * Keeps the selection highlighted after applying a format.
 * Allows multiple formats to be applied without losing selection.
 */
export function keepSelectionAfterFormat() {
    // Selection should already be maintained by saveSelection/restoreSelection
    // This ensures the visual highlight remains
    const selection = window.getSelection();
    if (selection && selection.rangeCount > 0) {
        // Force a re-render of the selection
        const range = selection.getRangeAt(0);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

/**
 * Moves cursor to end of selection and collapses it.
 * Called when tabbing back to editor after formatting selected text.
 */
export function collapseSelectionToEnd(element) {
    const selection = window.getSelection();
    if (selection && selection.rangeCount > 0) {
        const range = selection.getRangeAt(0);
        range.collapse(false); // false = collapse to end
        selection.removeAllRanges();
        selection.addRange(range);
    }
    element.focus();
}

/**
 * Applies pending formats when user starts typing.
 * Wraps the next typed character in appropriate formatting elements.
 */
export function applyPendingFormats(pendingData) {
    if (!pendingData) return;
    
    pendingFormatState = {
        formats: pendingData.formats || [],
        textColor: pendingData.textColor,
        backgroundColor: pendingData.backgroundColor,
        fontSize: pendingData.fontSize,
        fontFamily: pendingData.fontFamily,
        isApplying: true
    };
}

/**
 * Called on keypress to wrap typed character with pending formats.
 * Should be called from the editor's keypress handler.
 */
export function handleKeyPressWithPendingFormats(element, event, dotNetRef) {
    if (!pendingFormatState.isApplying) return false;
    
    // Only handle printable characters
    if (event.key.length !== 1 || event.ctrlKey || event.metaKey || event.altKey) {
        return false;
    }
    
    event.preventDefault();
    
    const char = event.key;
    let html = escapeHtml(char);
    
    // Build nested formatting tags
    // Order: font properties first, then text decorations
    if (pendingFormatState.fontFamily) {
        html = `<font face="${pendingFormatState.fontFamily}">${html}</font>`;
    }
    if (pendingFormatState.fontSize) {
        html = `<font size="${pendingFormatState.fontSize}">${html}</font>`;
    }
    if (pendingFormatState.textColor) {
        html = `<font color="${pendingFormatState.textColor}">${html}</font>`;
    }
    if (pendingFormatState.backgroundColor) {
        html = `<span style="background-color: ${pendingFormatState.backgroundColor}">${html}</span>`;
    }
    
    // Apply text decoration formats
    for (const format of pendingFormatState.formats) {
        switch (format) {
            case 'bold':
                html = `<strong>${html}</strong>`;
                break;
            case 'italic':
                html = `<em>${html}</em>`;
                break;
            case 'underline':
                html = `<u>${html}</u>`;
                break;
            case 'strikeThrough':
                html = `<del>${html}</del>`;
                break;
            case 'subscript':
                html = `<sub>${html}</sub>`;
                break;
            case 'superscript':
                html = `<sup>${html}</sup>`;
                break;
        }
    }
    
    // Insert the formatted HTML
    insertHtmlAtCursor(html);
    
    // Keep pending formats active for subsequent characters
    // (User can type multiple characters with same format)
    
    // Notify Blazor of content change
    dotNetRef.invokeMethodAsync('HandleContentChangedFromJs', element.innerHTML)
        .catch(err => console.error('Content change error:', err));
    
    return true;
}

/**
 * Clears pending format state.
 * Called when selection changes or cursor moves.
 */
export function clearPendingFormats() {
    pendingFormatState = {
        formats: [],
        textColor: null,
        backgroundColor: null,
        fontSize: null,
        fontFamily: null,
        isApplying: false
    };
}

/**
 * Returns current pending format state for debugging.
 */
export function getPendingFormatState() {
    return pendingFormatState;
}

// ===== HELPER FUNCTIONS =====

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function insertHtmlAtCursor(html) {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;
    
    const range = selection.getRangeAt(0);
    range.deleteContents();
    
    const fragment = document.createRange().createContextualFragment(html);
    const lastNode = fragment.lastChild;
    range.insertNode(fragment);
    
    // Move cursor after inserted content
    if (lastNode) {
        const newRange = document.createRange();
        newRange.setStartAfter(lastNode);
        newRange.collapse(true);
        selection.removeAllRanges();
        selection.addRange(newRange);
    }
}

/**
 * Handles Tab key to return focus to editor from toolbar.
 */
export function handleTabBackToEditor(element, hadSelection) {
    element.focus();
    
    if (hadSelection) {
        // Collapse selection to end when returning from toolbar
        collapseSelectionToEnd(element);
    }
    
    // Pending formats will be applied on next keypress
}